using Cassandra;
using DoAn.Services;
using Microsoft.VisualBasic;
using MongoDB.Driver.Core.Servers;
using Neo4jClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
//Quy tắc đặt code cho khôi phục:
//1.Đối với SỬA(UPDATE) &XÓA(DELETE)
//Cứ theo đúng 3 bước như Bình nói:
//    Lưu oldDataJson trước (để chộp lấy dữ liệu cũ trước khi nó bị sửa/xóa).
//    Gọi db.SaveChanges() (để SQL Server cập nhật dữ liệu mới/xóa hẳn).
//    Gọi cassService.LogAdminAction(...) (để bắn dữ liệu cũ sang Cassandra làm log Undo).

//2. Riêng đối với THÊM MỚI (CREATE)
//Chỉ khác một xíu ở bước 1 và 2:
//    Gọi db.SaveChanges() TRƯỚC(để SQL Server tự tạo ra cái MaHoa / MaDM tự tăng).
//    Lưu newDataJson SAU (vì lúc này mới có cái MaID vừa sinh ra để mà đóng gói chuỗi).
//    Gọi cassService.LogAdminAction(...).

//Đã fix lỗi cập nhật trạng thái đơn hàng không thành công [!]

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

            // 3. Thêm vào CSDL SQL Server
            db.tblHoa.Add(newHoa);
            db.SaveChanges(); // Luôn gọi SaveChanges() trước để CSDL cấp tự động MaHoa!

            // 4. Lưu dữ liệu vừa tạo vào oldDataJson (Dùng ký tự | phân cách cho an toàn)
            string newDataJson = $"{newHoa.MaHoa}|{newHoa.TenHoa}|{newHoa.GiaBan}|{newHoa.AnhDaiDien}|{newHoa.MaDM}|{newHoa.MaLoaiChinh}";

            // 5. Ghi log vào Cassandra (cột old_data sẽ chứa newDataJson)
            var cassService = new CassandraService();
            string adminName = Session["TenNV"] != null ? Session["TenNV"].ToString() : "Admin";
            cassService.LogAdminAction(adminName, "CREATE", "tblHoa", newHoa.MaHoa, $"Thêm sản phẩm: {newHoa.TenHoa}", newDataJson);
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

                // 3. Lưu dữ liệu vừa tạo vào oldDataJson (Dùng ký tự ### phân cách cho an toàn)
                string newDataJson = $"{hoaToUpdate.MaHoa}|{hoaToUpdate.TenHoa}|{hoaToUpdate.GiaBan}|{hoaToUpdate.AnhDaiDien}|{hoaToUpdate.MaDM}|{hoaToUpdate.MaLoaiChinh}";

                // 4. Thêm vào CSDL SQL Server
                db.Entry(hoaToUpdate).State = EntityState.Modified;
                db.SaveChanges();

                // 5. Ghi log vào Cassandra (cột old_data sẽ chứa newDataJson)
                var cassService = new CassandraService();
                string adminName = Session["TenNV"] != null ? Session["TenNV"].ToString() : "Admin";
                cassService.LogAdminAction(adminName, "UPDATE", "tblHoa", maHoa, $"Cập nhật sản phẩm: {hoaToUpdate.TenHoa}", newDataJson);
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
                // Lưu tên sản phẩm để ghi log
                string tenHoa = hoaToDelete.TenHoa;

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

                // Lưu chuỗi dữ liệu cũ để phục vụ khôi phục (Undo)
                string oldDataJson = $"{hoaToDelete.TenHoa}|{hoaToDelete.GiaBan}|{hoaToDelete.AnhDaiDien}|{hoaToDelete.MaDM}|{hoaToDelete.MaLoaiChinh}|{hoaToDelete.MoTa}";

                db.tblHoa.Remove(hoaToDelete);
                db.SaveChanges(); //

                // Ghi log vào Cassandra kèm old_data
                var cassService = new CassandraService(); 
                string adminName = Session["TenNV"] != null ? Session["TenNV"].ToString() : "Admin"; 
                cassService.LogAdminAction(adminName, "DELETE", "tblHoa", maHoa, $"Xóa sản phẩm: {tenHoa}", oldDataJson); 
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
            int maHD = int.Parse(collection["MaHD_Update"]);
            int newStatusID = int.Parse(collection["TinhTrang_Update"]);

            tblHoaDon donHang = db.tblHoaDon.Find(maHD);

            if (donHang != null)
            {
                // 1. LẤY TRẠNG THÁI CŨ TRONG CSDL TRƯỚC KHI SỬA (Để làm Undo)
                // Cú pháp: MaHD|TinhTrangCu|DaThanhToanCu
                string oldDataJson = $"{donHang.MaHD}|{donHang.TinhTrang}|{(donHang.DaThanhToan == true ? 1 : 0)}";

                // 2. MỚI GÁN TRẠNG THÁI MỚI VÀO MODEL
                donHang.TinhTrang = newStatusID;

                if (newStatusID == 3) // Đã giao hàng -> Đã thanh toán
                {
                    donHang.DaThanhToan = true;
                }

                db.Entry(donHang).State = EntityState.Modified;
                db.SaveChanges();

                // 3. Ghi log sang Cassandra
                var cassService = new CassandraService();
                string adminName = Session["TenNV"] != null ? Session["TenNV"].ToString() : "Admin";
                cassService.LogAdminAction(adminName, "UPDATE", "tblHoaDon", maHD, $"Cập nhật trạng thái đơn hàng #{maHD} sang ID: {newStatusID}", oldDataJson);
            }

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

            // Lưu chuỗi dữ liệu cũ để phục vụ khôi phục (Undo)
            db.tblDanhMucHoa.Add(newDanhMuc);
            db.SaveChanges();

            string oldDataJson = $"{newDanhMuc.MaDM}|{newDanhMuc.TenDM}|{newDanhMuc.GhiChu}";

            // Ghi log vào Cassandra kèm old_data
            var cassService = new CassandraService();
            string adminName = Session["TenNV"] != null ? Session["TenNV"].ToString() : "Admin";
            cassService.LogAdminAction(adminName, "CREATE", "tblDanhMucHoa", newDanhMuc.MaDM, $"Thêm danh mục: {tenDM}", oldDataJson);

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

                // Lưu chuỗi dữ liệu cũ để phục vụ khôi phục (Undo)
                string oldDataJson = $"{dmToUpdate.MaDM}|{dmToUpdate.TenDM}|{dmToUpdate.GhiChu}";

                db.Entry(dmToUpdate).State = EntityState.Modified;
                db.SaveChanges();

                // Ghi log vào Cassandra kèm old_data
                var cassService = new CassandraService();
                string adminName = Session["TenNV"] != null ? Session["TenNV"].ToString() : "Admin";
                cassService.LogAdminAction(adminName, "UPDATE", "tblDanhMucHoa", maDM, $"Cập nhật danh mục: {tenDM}", oldDataJson);
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
                // Lưu tên danh mục để ghi log
                string tenDM = dmToDelete.TenDM;

                // (Cần kiểm tra xem có sản phẩm nào thuộc danh mục này không TRƯỚC KHI XÓA)
                // (Tạm thời cho xóa luôn)

                // Lưu chuỗi dữ liệu cũ để phục vụ khôi phục (Undo)
                string oldDataJson = $"{dmToDelete.MaDM}|{dmToDelete.TenDM}|{dmToDelete.GhiChu}";

                db.tblDanhMucHoa.Remove(dmToDelete);
                db.SaveChanges();

                // Ghi log vào Cassandra kèm old_data
                var cassService = new CassandraService();
                string adminName = Session["TenNV"] != null ? Session["TenNV"].ToString() : "Admin";
                cassService.LogAdminAction(adminName, "DELETE", "tblDanhMucHoa", maDM, $"Xóa danh mục: {tenDM}", oldDataJson);
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

            // Lưu chuỗi dữ liệu cũ để phục vụ khôi phục (Undo)
            db.tblLoaiHoaChinh.Add(newLoaiHoa);
            db.SaveChanges();

            string oldDataJson = $"{newLoaiHoa.MaLoaiChinh}|{newLoaiHoa.TenLoaiChinh}|{newLoaiHoa.MoTa}";

            // Ghi log vào Cassandra kèm old_data
            var cassService = new CassandraService();
            string adminName = Session["TenNV"] != null ? Session["TenNV"].ToString() : "Admin";
            cassService.LogAdminAction(adminName, "CREATE", "tblLoaiHoaChinh", newLoaiHoa.MaLoaiChinh, $"Thêm loại hoa: {newLoaiHoa.TenLoaiChinh}", oldDataJson);
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

                // Lưu chuỗi dữ liệu cũ để phục vụ khôi phục (Undo)
                string oldDataJson = $"{loaiToUpdate.MaLoaiChinh}|{loaiToUpdate.TenLoaiChinh}|{loaiToUpdate.MoTa}";

                db.Entry(loaiToUpdate).State = EntityState.Modified;
                db.SaveChanges();

                // Ghi log vào Cassandra kèm old_data
                var cassService = new CassandraService();
                string adminName = Session["TenNV"] != null ? Session["TenNV"].ToString() : "Admin";
                cassService.LogAdminAction(adminName, "UPDATE", "tblLoaiHoaChinh", maLoai, $"Cập nhật loại hoa: {tenLoai}", oldDataJson);
                return RedirectToAction("LoaiHoa");
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
                // Lưu tên loại hoa để ghi log
                string tenLoai = loaiToDelete.TenLoaiChinh;

                // (Tương tự, cần kiểm tra xem có Hoa nào thuộc loại này không)
                // Lưu chuỗi dữ liệu cũ để phục vụ khôi phục (Undo)
                string oldDataJson = $"{loaiToDelete.MaLoaiChinh}|{loaiToDelete.TenLoaiChinh}|{loaiToDelete.MoTa}";

                db.tblLoaiHoaChinh.Remove(loaiToDelete);
                db.SaveChanges();

                // Ghi log vào Cassandra kèm old_data
                var cassService = new CassandraService();
                string adminName = Session["TenNV"] != null ? Session["TenNV"].ToString() : "Admin";
                cassService.LogAdminAction(adminName, "DELETE", "tblLoaiHoaChinh", maLoai, $"Xóa loại hoa: {tenLoai}", oldDataJson);
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
        // 1. Trang xem Clickstream dành cho Admin
        public ActionResult Clickstream()
        {
            var listTopClick = new List<Tuple<tblHoa, int>>();
            var top10HotIds = new List<int>();

            try
            {
                var cass = DoAn.MvcApplication.CassandraSession;
                if (cass != null)
                {
                    // Query lấy tất cả event view từ Cassandra
                    var rs = cass.Execute("SELECT product_id FROM web_ban_hoa.user_events");
                    var counts = rs.Select(r => r.GetValue<int>("product_id"))
                                   .Where(id => id > 0)
                                   .GroupBy(id => id)
                                   .Select(g => new { ProductId = g.Key, Count = g.Count() })
                                   .OrderByDescending(x => x.Count)
                                   .ToList();

                    // Lấy Top 10 ID có lượt click cao nhất làm danh sách "Bán chạy" tự động
                    top10HotIds = counts.Take(10).Select(c => c.ProductId).ToList();

                    // Lấy toàn bộ sản phẩm từ SQL Server để hiển thị danh sách cho Admin theo dõi
                    var flowers = db.tblHoa.ToList();

                    foreach (var hoa in flowers)
                    {
                        var clickInfo = counts.FirstOrDefault(c => c.ProductId == hoa.MaHoa);
                        int clickCount = clickInfo != null ? clickInfo.Count : 0;
                        listTopClick.Add(new Tuple<tblHoa, int>(hoa, clickCount));
                    }

                    // Sắp xếp danh sách cho Admin xem: Sản phẩm click nhiều nằm lên đầu
                    listTopClick = listTopClick.OrderByDescending(x => x.Item2).ToList();
                }
            }
            catch { }

            // Lưu Top 10 Bán chạy vào Session để trang chủ Khách hàng đọc tự động
            Session["Top10BanChayIds"] = top10HotIds;

            return View(listTopClick);
        }

        // 2. Admin bấm nút Đề xuất sản phẩm
        [HttpPost]
        public ActionResult ToggleDeXuat(int maHoa)
        {
            var listDeXuat = Session["DeXuatProductIds"] as List<int> ?? new List<int>();
            if (listDeXuat.Contains(maHoa))
            {
                listDeXuat.Remove(maHoa); // Bỏ đề xuất
            }
            else
            {
                listDeXuat.Add(maHoa); // Thêm đề xuất
            }
            Session["DeXuatProductIds"] = listDeXuat;
            return RedirectToAction("Clickstream");
        }
        // 1. Xem danh sách Nhật ký thao tác của Admin
        public ActionResult NhatKyThaoTac()
        {
            var logs = new List<dynamic>();
            try
            {
                var cass = DoAn.MvcApplication.CassandraSession;
                if (cass != null)
                {
                    var service = new CassandraService();
                    service.InitAuditLogTable();

                    var rs = cass.Execute("SELECT log_id, admin_name, action_type, target_table, target_id, description, old_data, created_at FROM web_ban_hoa.admin_audit_logs");
                    foreach (var r in rs)
                    {
                        // Dùng ExpandoObject thay cho Anonymous Type để View đọc dynamic mượt mà
                        IDictionary<string, object> logItem = new System.Dynamic.ExpandoObject();

                        logItem["LogId"] = r.GetValue<TimeUuid>("log_id").ToGuid();
                        logItem["AdminName"] = r.GetValue<string>("admin_name") ?? "Admin";
                        logItem["ActionType"] = r.GetValue<string>("action_type") ?? "LOG";
                        logItem["TargetTable"] = r.GetValue<string>("target_table") ?? "";
                        logItem["TargetId"] = r.GetValue<int>("target_id");
                        logItem["Description"] = r.GetValue<string>("description") ?? "";
                        logItem["OldData"] = r.GetValue<string>("old_data") ?? "";

                        // Ép kiểu thời gian an toàn
                        DateTimeOffset createdAt = r.GetValue<DateTimeOffset>("created_at");
                        logItem["CreatedAt"] = createdAt.ToLocalTime().ToString("dd/MM/yyyy HH:mm:ss");

                        logs.Add(logItem);
                    }
                }
            }
            catch { }

            return View(logs);
        }

        // Khôi phục thao tác (Undo) từ Log Cassandra về SQL Server
        // 1. HAM MAIN: Nhận request từ View
        [HttpPost]
        public ActionResult KhoiPhucThaoTac(Guid logId)
        {
            try
            {
                var cass = DoAn.MvcApplication.CassandraSession;
                if (cass != null)
                {
                    var cql = "SELECT action_type, target_table, target_id, old_data FROM web_ban_hoa.admin_audit_logs WHERE log_id = ? ALLOW FILTERING";
                    var stmt = new SimpleStatement(cql, TimeUuid.Parse(logId.ToString()));
                    var row = cass.Execute(stmt).FirstOrDefault();

                    if (row != null)
                    {
                        string actionType = (row.GetValue<string>("action_type") ?? "").Trim().ToUpper();
                        string targetTable = (row.GetValue<string>("target_table") ?? "").Trim();
                        int targetId = row.GetValue<int>("target_id");
                        string oldDataJson = row.GetValue<string>("old_data") ?? "";

                        // --- A. HOÀN TÁC THÊM MỚI (CREATE) -> XÓA DỮ LIỆU VỪA TẠO ---
                        if (actionType == "CREATE")
                        {
                            UndoCreateGeneric(targetTable, targetId);
                            TempData["ThongBao"] = $"Đã hoàn tác: Xóa bản ghi (ID: {targetId}) khỏi {targetTable}!";
                        }
                        // --- B. HOÀN TÁC SỬA (UPDATE) VÀ XÓA (DELETE) -> PHỤC HỒI TỪ JSON ---
                        else if (actionType == "UPDATE" || actionType == "DELETE")
                        {
                            UndoUpdateOrDeleteGeneric(actionType, targetTable, targetId, oldDataJson);
                            TempData["ThongBao"] = $"Thành công: Đã khôi phục dữ liệu cho {targetTable} (ID: {targetId})!";
                        }
                        // --- C. HOÀN TÁC ĐỀ XUẤT (PROMOTE/UNPROMOTE) ---
                        else if (actionType == "PROMOTE" || actionType == "UNPROMOTE")
                        {
                            var listDeXuat = Session["DeXuatProductIds"] as List<int> ?? new List<int>();
                            if (actionType == "PROMOTE") listDeXuat.Remove(targetId);
                            else if (!listDeXuat.Contains(targetId)) listDeXuat.Add(targetId);

                            Session["DeXuatProductIds"] = listDeXuat;
                            TempData["ThongBao"] = "Đã hoàn tác trạng thái Đề xuất sản phẩm!";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ThongBao"] = "Lỗi khi khôi phục: " + ex.Message;
            }

            return RedirectToAction("NhatKyThaoTac");
        }


        // 2. HAM XỬ LÝ UNDO DÙNG GENERIC + REFLECTION (Cho Update & Delete)
        private void UndoUpdateOrDeleteGeneric(string actionType, string tableName, int targetId, string oldDataJson)
        {
            if (string.IsNullOrEmpty(oldDataJson)) return;

            var dbSet = GetDbSetByTableName(tableName);
            if (dbSet == null) return;

            Type entityType = GetEntityTypeByTableName(tableName);
            if (entityType == null) return;

            string trimmedData = oldDataJson.Trim();

            if (actionType == "UPDATE")
            {
                var existingEntity = dbSet.Find(targetId);
                if (existingEntity == null) return;

                // TRƯỜNG HỢP 1: Nếu là chuỗi JSON chuẩn (Bắt đầu bằng dấu {)
                if (trimmedData.StartsWith("{"))
                {
                    var oldValuesDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(trimmedData);
                    if (oldValuesDict != null)
                    {
                        foreach (var item in oldValuesDict)
                        {
                            var prop = entityType.GetProperty(item.Key);
                            if (prop != null && prop.CanWrite && item.Value != null)
                            {
                                object convertedValue = Convert.ChangeType(item.Value, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
                                prop.SetValue(existingEntity, convertedValue);
                            }
                        }
                    }
                }
                // TRƯỜNG HỢP 2: Nếu là chuỗi CŨ dùng dấu gạch đứng | (ví dụ "43|4|1")
                else if (trimmedData.Contains("|"))
                {
                    string[] parts = trimmedData.Split('|');

                    // Xử lý hoàn tác cho Đơn hàng cũ
                    if (tableName.Equals("tblHoaDon", StringComparison.OrdinalIgnoreCase) && parts.Length >= 2)
                    {
                        var donHang = existingEntity as tblHoaDon;
                        if (donHang != null)
                        {
                            donHang.TinhTrang = int.Parse(parts[1]);
                            if (parts.Length >= 3)
                            {
                                donHang.DaThanhToan = (parts[2] == "1" || parts[2].ToLower() == "true");
                            }
                        }
                    }
                    // Xử lý hoàn tác cho Danh mục cũ
                    else if (tableName.Equals("tblDanhMucHoa", StringComparison.OrdinalIgnoreCase) && parts.Length >= 2)
                    {
                        var dm = existingEntity as tblDanhMucHoa;
                        if (dm != null)
                        {
                            dm.TenDM = parts[1];
                            dm.GhiChu = parts.Length > 2 ? parts[2] : "";
                        }
                    }
                }

                db.Entry(existingEntity).State = EntityState.Modified;
                db.SaveChanges();
            }
            else if (actionType == "DELETE")
            {
                // TRƯỜNG HỢP 1: Dữ liệu log MỚI dạng JSON (Bắt đầu bằng {)
                if (trimmedData.StartsWith("{"))
                {
                    var restoredEntity = JsonConvert.DeserializeObject(trimmedData, entityType);
                    if (restoredEntity != null)
                    {
                        dbSet.Add(restoredEntity);
                        db.SaveChanges();
                    }
                }
                // TRƯỜNG HỢP 2: Dữ liệu log CŨ dạng gạch đứng | (Như trong ảnh DBeaver)
                else if (trimmedData.Contains("|"))
                {
                    string[] parts = trimmedData.Split('|');

                    if (tableName.Equals("tblHoa", StringComparison.OrdinalIgnoreCase) && parts.Length >= 5)
                    {
                        var hoaKhoiPhuc = new tblHoa
                        {
                            TenHoa = parts[0],
                            GiaBan = decimal.Parse(parts[1]),
                            AnhDaiDien = parts[2],
                            MaDM = int.Parse(parts[3]),
                            MaLoaiChinh = int.Parse(parts[4]),
                            MoTa = parts.Length > 5 ? parts[5] : ""
                        };
                        db.tblHoa.Add(hoaKhoiPhuc);
                        db.SaveChanges();
                    }
                    else if (tableName.Equals("tblDanhMucHoa", StringComparison.OrdinalIgnoreCase) && parts.Length >= 2)
                    {
                        var dmKhoiPhuc = new tblDanhMucHoa
                        {
                            TenDM = parts[1],
                            GhiChu = parts.Length > 2 ? parts[2] : ""
                        };
                        db.tblDanhMucHoa.Add(dmKhoiPhuc);
                        db.SaveChanges();
                    }
                }
            }
        }

        // 3. HÀM XỬ LÝ UNDO CREATE (Xóa bản ghi vừa tạo) - TỐI ƯU DÙNG REFLECTION
        private void UndoCreateGeneric(string tableName, int targetId)
        {
            if (targetId <= 0) return; // Nếu ID không hợp lệ thì bỏ qua

            var dbSet = GetDbSetByTableName(tableName);
            Type entityType = GetEntityTypeByTableName(tableName);

            if (dbSet != null && entityType != null)
            {
                // Cách 1: Thử Find theo Primary Key tiêu chuẩn của EF
                var entity = dbSet.Find(targetId);

                // Cách 2: Nếu .Find() không ra (do đặt tên PK khác chuẩn), dùng Property Reflection để tìm
                if (entity == null)
                {
                    foreach (var item in dbSet)
                    {
                        // Lấy property đại diện khóa chính (MaHoa, MaDM, MaHD, MaLoaiChinh...)
                        var pkProp = entityType.GetProperties()
                            .FirstOrDefault(p => p.Name.Equals("Ma" + tableName.Replace("tbl", ""), StringComparison.OrdinalIgnoreCase)
                                              || p.Name.Equals("ID", StringComparison.OrdinalIgnoreCase)
                                              || p.Name.EndsWith("ID", StringComparison.OrdinalIgnoreCase));

                        if (pkProp != null)
                        {
                            var val = Convert.ToInt32(pkProp.GetValue(item));
                            if (val == targetId)
                            {
                                entity = item;
                                break;
                            }
                        }
                    }
                }

                // Thực hiện xóa nếu tìm thấy
                if (entity != null)
                {
                    dbSet.Remove(entity);
                    db.SaveChanges();
                }
            }
        }


        // 4. HAM HELPER: Áp tên bảng chuỗi -> DbSet tương ứng trong DbContext
        private dynamic GetDbSetByTableName(string tableName)
        {
            switch (tableName.ToLower())
            {
                case "tblhoa": return db.tblHoa;
                case "tbldanhmuchoa": return db.tblDanhMucHoa;
                case "tblloaihoachinh": return db.tblLoaiHoaChinh;
                case "tblhoadon": return db.tblHoaDon;
                default: return null;
            }
        }

        // 5. HAM HELPER: Áp tên bảng chuỗi -> System.Type của Model
        private Type GetEntityTypeByTableName(string tableName)
        {
            switch (tableName.ToLower())
            {
                case "tblhoa": return typeof(tblHoa);
                case "tbldanhmuchoa": return typeof(tblDanhMucHoa);
                case "tblloaihoachinh": return typeof(tblLoaiHoaChinh);
                case "tblhoadon": return typeof(tblHoaDon);
                default: return null;
            }
        }

        // 3. Xóa lịch sử thao tác để tránh nặng dữ liệu
        [HttpPost]
        public ActionResult XoaLichSuThaoTac()
        {
            try
            {
                var cass = DoAn.MvcApplication.CassandraSession;
                if (cass != null)
                {
                    cass.Execute("TRUNCATE web_ban_hoa.admin_audit_logs");
                }
            }
            catch { }

            TempData["ThongBao"] = "Đã dọn dẹp sạch sẽ toàn bộ nhật ký thao tác!";
            return RedirectToAction("NhatKyThaoTac");
        }
    }
}