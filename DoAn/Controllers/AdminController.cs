using DoAn.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Neo4jClient;

namespace DoAn.Controllers
{
    public class AdminController : Controller
    {
        // GET: Admin
        QL_BanHoaEntities2 db = new QL_BanHoaEntities2();
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // Kiểm tra xem Session "UserType" có phải là "Admin" không
            // (Session này đã được tạo ở TaiKhoanController khi Admin đăng nhập)
            if (Session["UserType"] == null || Session["UserType"].ToString() != "Admin")
               
            {
                // Nếu không phải Admin, đá về trang Đăng nhập
                filterContext.Result = new RedirectToRouteResult(
                    new System.Web.Routing.RouteValueDictionary(new
                    {
                        controller = "TaiKhoan",
                        action = "DangNhap"
                    })
                );
            }
            base.OnActionExecuting(filterContext);
        }

        // === HẾT HÀM BẢO MẬT ===




        // Mở file AdminController.cs, thay thế hàm Index cũ bằng hàm này:
        public ActionResult Index(int? year, int? month)
        {
            ViewBag.Title = "Tổng quan";

            // 1. Kiểm tra quyền Admin
            int vaiTro = 0;
            if (Session["VaiTro"] != null) vaiTro = Convert.ToInt32(Session["VaiTro"]);
            bool isAdmin = (vaiTro == 1);
            ViewBag.IsAdmin = isAdmin;

            // Các thẻ thống kê chung
            ViewBag.SoDonHangMoi = db.tblHoaDon.Count(d => d.TinhTrang == 1);

            if (isAdmin)
            {
                ViewBag.TongDoanhThu = db.tblHoaDon.Where(d => d.DaThanhToan == true).Sum(d => (decimal?)d.TongTien) ?? 0;
                ViewBag.SoKhachHang = db.tblKhachHang.Count();

                // --- BỘ LỌC THÔNG MINH ---
                int selectedYear = year ?? DateTime.Now.Year;
                int? selectedMonth = month;

                // Lấy danh sách năm có đơn hàng để đổ vào Dropdown
                var years = db.tblHoaDon.Where(d => d.NgayLap.HasValue)
                                        .Select(d => d.NgayLap.Value.Year)
                                        .Distinct().OrderByDescending(y => y).ToList();
                if (!years.Contains(selectedYear)) years.Insert(0, selectedYear);

                ViewBag.YearList = years;
                ViewBag.SelectedYear = selectedYear;
                ViewBag.SelectedMonth = selectedMonth;

                // Chuẩn bị dữ liệu vẽ Biểu đồ
                var query = db.tblHoaDon.Where(d => d.DaThanhToan == true && d.NgayLap.HasValue && d.NgayLap.Value.Year == selectedYear);

                List<string> labels = new List<string>();
                List<decimal> data = new List<decimal>();
                string chartTitle = "";

                if (selectedMonth.HasValue)
                {
                    // --- CHẾ ĐỘ XEM TỪNG NGÀY TRONG THÁNG ---
                    chartTitle = $"Biểu đồ doanh thu Tháng {selectedMonth}/{selectedYear}";
                    int daysInMonth = DateTime.DaysInMonth(selectedYear, selectedMonth.Value);
                    decimal[] dailyData = new decimal[daysInMonth];

                    var revenueQuery = query.Where(d => d.NgayLap.Value.Month == selectedMonth.Value)
                                            .GroupBy(d => d.NgayLap.Value.Day)
                                            .Select(g => new { Day = g.Key, Total = g.Sum(d => d.TongTien ?? 0) })
                                            .ToList();

                    foreach (var item in revenueQuery) dailyData[item.Day - 1] = item.Total;
                    for (int i = 1; i <= daysInMonth; i++) labels.Add(i.ToString());
                    data = dailyData.ToList();
                }
                else
                {
                    // --- CHẾ ĐỘ XEM 12 THÁNG TRONG NĂM ---
                    chartTitle = $"Biểu đồ doanh thu Năm {selectedYear}";
                    decimal[] monthlyData = new decimal[12];

                    var revenueQuery = query.GroupBy(d => d.NgayLap.Value.Month)
                                            .Select(g => new { Month = g.Key, Total = g.Sum(d => d.TongTien ?? 0) })
                                            .ToList();

                    foreach (var item in revenueQuery) monthlyData[item.Month - 1] = item.Total;

                    labels = new List<string> { "T1", "T2", "T3", "T4", "T5", "T6", "T7", "T8", "T9", "T10", "T11", "T12" };
                    data = monthlyData.ToList();
                }

                // --- QUAN TRỌNG: FIX LỖI DẤU PHẨY VIỆT NAM ---
                // Sử dụng CultureInfo.InvariantCulture để ép số về dạng 1000.00 (dấu chấm) thay vì dấu phẩy
                ViewBag.ChartData = string.Join(",", data.Select(v => v.ToString(System.Globalization.CultureInfo.InvariantCulture)));

                // Gửi nhãn (labels) sang View
                ViewBag.ChartLabels = string.Join(",", labels.Select(l => $"'{l}'"));
                ViewBag.ChartTitle = chartTitle;
            }

            return View();
        }



        // GET: /Admin/SanPham

        public ActionResult SanPham()
        {
            // === BẮT ĐẦU KIỂM TRA VAI TRÒ ===
            int vaiTro = (Session["VaiTro"] != null) ? (int)Session["VaiTro"] : 0;
            if (vaiTro != 1) // Nếu KHÔNG PHẢI Admin
            {
                // "Đá" về trang Tổng quan
                return RedirectToAction("Index", "Admin");
            }
            // === KẾT THÚC KIỂM TRA ===

            ViewBag.Title = "Quản lý Sản phẩm";
            // Dùng .Include() để lấy thông tin Tên Danh Mục (loại) từ bảng liên kết 
            //Tui kết 2 bảng tblHoa và tblDanhMucHoa với nhau để lấy luôn tên danh mục
            var sanPhamList = db.tblHoa.Include(h => h.tblDanhMucHoa).ToList();

            // Lấy danh sách Danh mục và Loại hoa để nhét vào dropdown (select)
            ViewBag.DanhMucList = db.tblDanhMucHoa.ToList();
            ViewBag.LoaiHoaList = db.tblLoaiHoaChinh.ToList();
            return View(sanPhamList);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ThemSanPham(FormCollection collection, HttpPostedFileBase AnhDaiDien_Add)
        {
            // 1. Tạo đối tượng Hoa
            tblHoa newHoa = new tblHoa();
            newHoa.TenHoa = collection["TenHoa_Add"];
            newHoa.GiaBan = decimal.Parse(collection["GiaBan_Add"]);
            newHoa.MoTa = collection["MoTa_Add"];
            newHoa.DonViTinh = collection["DonViTinh_Add"];
            newHoa.MauSacChuDao = collection["MauSacChuDao_Add"];
            newHoa.MaDM = int.Parse(collection["MaDM_Add"]);
            newHoa.MaLoaiChinh = int.Parse(collection["MaLoaiChinh_Add"]);

            // 2. Xử lý Upload file ảnh
            if (AnhDaiDien_Add != null && AnhDaiDien_Add.ContentLength > 0)
            {
                // Tạo tên file duy nhất (tránh trùng)
                string fileName = Path.GetFileNameWithoutExtension(AnhDaiDien_Add.FileName);
                string extension = Path.GetExtension(AnhDaiDien_Add.FileName);
                fileName = fileName.Replace(" ", "-") + "-" + System.Guid.NewGuid().ToString().Substring(0, 4) + extension;
                //Cái Replace(" ", "-") là thay khoảng trắng thành dấu gạch ngang để tên file đẹp hơn, cái Guid.NewGuid() là tạo chuỗi ngẫu nhiên để tránh trùng tên file (0,4) là lấy 4 ký tự đầu tiên của chuỗi ngẫu nhiên đó
                //Ví dụ như mình up lên web "hoa hồng.jpg" sẽ thành "hoa-hồng-1a2b.jpg"
                //+entension là để nối lại phần mở rộng file có lấy ở cái Path.GetExtension ở trên
                // Lưu tên file vào CSDL
                newHoa.AnhDaiDien = fileName;

                // Lưu file ảnh vào thư mục /Content/Images/
                string savePath = Path.Combine(Server.MapPath("~/Content/HinhAnh"), fileName);
                AnhDaiDien_Add.SaveAs(savePath);
            }

            // 3. Thêm vào CSDL
            db.tblHoa.Add(newHoa);
            db.SaveChanges();
            var neo4j = new Neo4jService();
            var client = await neo4j.GetClient();

            await client.Cypher
                .Merge("(f:Flower {id: $id})")
                .WithParam("id", newHoa.MaHoa)
                .Set("f.name = $name")
                .WithParam("name", newHoa.TenHoa)
                .ExecuteWithoutResultsAsync();

            return RedirectToAction("SanPham");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SuaSanPham(FormCollection collection, HttpPostedFileBase AnhDaiDien_Edit)
        {
            int maHoa = int.Parse(collection["MaHoa_Edit"]);

            // Tìm hoa cần sửa
            tblHoa hoaToUpdate = db.tblHoa.Find(maHoa);

            if (hoaToUpdate != null)
            {
                // Cập nhật thông tin
                hoaToUpdate.TenHoa = collection["TenHoa_Edit"];
                hoaToUpdate.GiaBan = decimal.Parse(collection["GiaBan_Edit"]);
                hoaToUpdate.MoTa = collection["MoTa_Edit"];
                hoaToUpdate.DonViTinh = collection["DonViTinh_Edit"];
                hoaToUpdate.MauSacChuDao = collection["MauSacChuDao_Edit"];
                hoaToUpdate.MaDM = int.Parse(collection["MaDM_Edit"]);
                hoaToUpdate.MaLoaiChinh = int.Parse(collection["MaLoaiChinh_Edit"]);

                // Kiểm tra xem có upload ảnh MỚI không
                if (AnhDaiDien_Edit != null && AnhDaiDien_Edit.ContentLength > 0)
                {
                    // Xóa ảnh CŨ (nếu có)
                    if (!string.IsNullOrEmpty(hoaToUpdate.AnhDaiDien))
                    {
                        string oldPath = Path.Combine(Server.MapPath("~/Content/HinhAnh"), hoaToUpdate.AnhDaiDien);
                        if (System.IO.File.Exists(oldPath))
                        {
                            System.IO.File.Delete(oldPath);
                        }
                    }

                    // Lưu ảnh MỚI
                    string fileName = Path.GetFileNameWithoutExtension(AnhDaiDien_Edit.FileName);
                    string extension = Path.GetExtension(AnhDaiDien_Edit.FileName);
                    fileName = fileName.Replace(" ", "-") + "-" + System.Guid.NewGuid().ToString().Substring(0, 4) + extension;

                    hoaToUpdate.AnhDaiDien = fileName; // Gán tên file MỚI
                    string savePath = Path.Combine(Server.MapPath("~/Content/HinhAnh"), fileName);
                    AnhDaiDien_Edit.SaveAs(savePath);
                }
                // Nếu không upload ảnh mới, thì cứ giữ nguyên ảnh cũ, không làm gì cả

                db.Entry(hoaToUpdate).State = EntityState.Modified;
                db.SaveChanges();
            }

            return RedirectToAction("SanPham");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult XoaSanPham(FormCollection collection)
        {
            int maHoa = int.Parse(collection["MaHoa_Delete"]);
            tblHoa hoaToDelete = db.tblHoa.Find(maHoa);

            if (hoaToDelete != null)
            {
                // 1. Kiểm tra xem hoa này có trong đơn hàng nào không
                bool daCoNguoiMua = db.tblChiTietHoaDon.Any(ct => ct.MaHoa == maHoa);

                if (daCoNguoiMua)
                {
                    // Nếu đã có người mua, KHÔNG ĐƯỢC XÓA. 
                    TempData["LoiXoa"] = "Không thể xóa sản phẩm này vì đã có trong lịch sử đơn hàng!";
                    return RedirectToAction("SanPham");
                }

                // 2. Nếu chưa ai mua thì mới được xóa ảnh và xóa database
                if (!string.IsNullOrEmpty(hoaToDelete.AnhDaiDien))
                {
                    string path = Path.Combine(Server.MapPath("~/Content/HinhAnh"), hoaToDelete.AnhDaiDien);
                    if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
                }

                db.tblHoa.Remove(hoaToDelete);
                db.SaveChanges();
            }
            return RedirectToAction("SanPham");
        }


        public ActionResult DonHang()
        {
            // Trang này Nhân viên (vaiTro == 2) cũng được xem
            ViewBag.Title = "Quản lý Đơn hàng";

            // === SỬA LẠI ===
            // Lấy tất cả đơn hàng, sắp xếp mới nhất lên trên
            // Dùng .Include() để lấy Tên Khách Hàng và Tên Tình Trạng
            var donHangList = db.tblHoaDon
                .Include(d => d.tblKhachHang)
                .Include(d => d.tblTinhTrang)
                .OrderByDescending(d => d.NgayLap)
                .ToList();

            // Gửi danh sách TẤT CẢ TÌNH TRẠNG sang View
            // (Để làm dropdown "Sửa trạng thái")
            ViewBag.TinhTrangList = db.tblTinhTrang.ToList();

            return View(donHangList);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CapNhatTrangThai(FormCollection collection)
        {
            // Lấy 2 giá trị từ form
            int maHD = int.Parse(collection["MaHD_Update"]);
            int newStatusID = int.Parse(collection["TinhTrang_Update"]);

            // Tìm đơn hàng
            tblHoaDon donHang = db.tblHoaDon.Find(maHD);

            if (donHang != null)
            {
                // Cập nhật trạng thái mới
                donHang.TinhTrang = newStatusID;

                // LOGIC QUAN TRỌNG:
                // Nếu trạng thái là "Đã giao hàng" (ID = 3)
                // thì tự động cập nhật là "Đã thanh toán"
                if (newStatusID == 3) // Giả sử 3 = Đã giao hàng
                {
                    donHang.DaThanhToan = true;
                }

                db.Entry(donHang).State = EntityState.Modified;
                db.SaveChanges();
            }

            // Tải lại trang
            return RedirectToAction("DonHang");
        }



        public ActionResult DanhMuc()
        {
            // === BẮT ĐẦU KIỂM TRA VAI TRÒ ===
            int vaiTro = (Session["VaiTro"] != null) ? (int)Session["VaiTro"] : 0;
            if (vaiTro != 1) // Nếu KHÔNG PHẢI Admin
            {
                return RedirectToAction("Index", "Admin");
            }
            // === KẾT THÚC KIỂM TRA ===

            ViewBag.Title = "Quản lý Danh mục";
            var danhMucList = db.tblDanhMucHoa.ToList();
            return View(danhMucList);
            
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ThemDanhMuc(FormCollection collection)
        {
            // Lấy Tên và Ghi chú từ form
            string tenDM = collection["TenDM_Add"];
            string ghiChu = collection["GhiChu_Add"];

            // Tạo đối tượng Danh mục mới
            tblDanhMucHoa newDanhMuc = new tblDanhMucHoa();
            newDanhMuc.TenDM = tenDM;
            newDanhMuc.GhiChu = ghiChu;

            // Thêm vào CSDL
            db.tblDanhMucHoa.Add(newDanhMuc);
            db.SaveChanges();

            // Quay lại trang Danh mục
            return RedirectToAction("DanhMuc");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SuaDanhMuc(FormCollection collection)
        {
            // Lấy thông tin từ form SỬA
            int maDM = int.Parse(collection["MaDM_Edit"]);
            string tenDM = collection["TenDM_Edit"];
            string ghiChu = collection["GhiChu_Edit"];

            // Tìm danh mục trong CSDL
            tblDanhMucHoa dmToUpdate = db.tblDanhMucHoa.Find(maDM);

            if (dmToUpdate != null)
            {
                // Cập nhật thông tin
                dmToUpdate.TenDM = tenDM;
                dmToUpdate.GhiChu = ghiChu;

                // Đánh dấu là đã sửa
                db.Entry(dmToUpdate).State = EntityState.Modified;
                db.SaveChanges();
            }

            // Quay lại trang Danh mục
            return RedirectToAction("DanhMuc");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult XoaDanhMuc(FormCollection collection)
        {
            int maDM = int.Parse(collection["MaDM_Delete"]);

            // Tìm danh mục
            tblDanhMucHoa dmToDelete = db.tblDanhMucHoa.Find(maDM);

            if (dmToDelete != null)
            {
                // (Cần kiểm tra xem có sản phẩm nào thuộc danh mục này không TRƯỚC KHI XÓA)
                // (Tạm thời cho xóa luôn)

                db.tblDanhMucHoa.Remove(dmToDelete);
                db.SaveChanges();
            }

            return RedirectToAction("DanhMuc");
        }

        public ActionResult LoaiHoa()
        {
            // Kiểm tra quyền (chỉ Admin)
            int vaiTro = (Session["VaiTro"] != null) ? (int)Session["VaiTro"] : 0;
            if (vaiTro != 1) { return RedirectToAction("Index", "Admin"); }

            ViewBag.Title = "Quản lý Loại hoa";

            // Lấy toàn bộ Loại hoa chính
            var loaiHoaList = db.tblLoaiHoaChinh.ToList();
            return View(loaiHoaList);
        }

        // POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ThemLoaiHoa(FormCollection collection)
        {
            // Lấy Tên và Mô tả từ form
            tblLoaiHoaChinh newLoaiHoa = new tblLoaiHoaChinh();
            newLoaiHoa.TenLoaiChinh = collection["TenLoai_Add"];
            newLoaiHoa.MoTa = collection["MoTa_Add"];

            db.tblLoaiHoaChinh.Add(newLoaiHoa);
            db.SaveChanges();

            return RedirectToAction("LoaiHoa");
        }

        // POST: 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SuaLoaiHoa(FormCollection collection)
        {
            // Lấy thông tin từ form SỬA
            int maLoai = int.Parse(collection["MaLoai_Edit"]);
            string tenLoai = collection["TenLoai_Edit"];
            string moTa = collection["MoTa_Edit"];

            // Tìm trong CSDL
            tblLoaiHoaChinh loaiToUpdate = db.tblLoaiHoaChinh.Find(maLoai);

            if (loaiToUpdate != null)
            {
                loaiToUpdate.TenLoaiChinh = tenLoai;
                loaiToUpdate.MoTa = moTa;
                db.Entry(loaiToUpdate).State = EntityState.Modified;
                db.SaveChanges();
            }

            return RedirectToAction("LoaiHoa");
        }

        // POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult XoaLoaiHoa(FormCollection collection)
        {
            int maLoai = int.Parse(collection["MaLoai_Delete"]);

            tblLoaiHoaChinh loaiToDelete = db.tblLoaiHoaChinh.Find(maLoai);

            if (loaiToDelete != null)
            {
                // (Tương tự, cần kiểm tra xem có Hoa nào thuộc loại này không)
                db.tblLoaiHoaChinh.Remove(loaiToDelete);
                db.SaveChanges();
            }

            return RedirectToAction("LoaiHoa");
        }

        public ActionResult DanhMucApiJson()
        {
            // 1. Kiểm tra quyền Admin (nên giữ lại)
            int vaiTro = (Session["VaiTro"] != null) ? (int)Session["VaiTro"] : 0;
            if (vaiTro != 1)
            {
                // Trả về lỗi 403 (Forbidden) nếu không phải Admin
                return new HttpStatusCodeResult(403, "Access Denied");
            }

            // 2. Lấy dữ liệu (giống y chang hàm cũ)
            var danhMucList = db.tblDanhMucHoa.Select(d => new
            {
                MaDM = d.MaDM,
                TenDM = d.TenDM,
                GhiChu = d.GhiChu
            }).ToList();

            
            // Thay vì return View(), ní return Json()
            return Json(danhMucList, JsonRequestBehavior.AllowGet);
        }


        // GET: /Admin/KhachHang
        public ActionResult KhachHang()
        {
            // Kiểm tra quyền Admin (chỉ Admin mới được coi list khách hàng)
            int vaiTro = (Session["VaiTro"] != null) ? (int)Session["VaiTro"] : 0;
            if (vaiTro != 1)
            {
                // Nếu không phải Admin, chuyển về trang Tổng quan
                return RedirectToAction("Index", "Admin");
            }

            ViewBag.Title = "Danh sách Khách hàng";

            // Lấy toàn bộ danh sách khách hàng
            var khachHangList = db.tblKhachHang.ToList();

            return View(khachHangList);
        }

        // (Hàm này để giải phóng DbContext) cái dbcontext là  cái kết nối csdl á (QL_En.....db)
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}