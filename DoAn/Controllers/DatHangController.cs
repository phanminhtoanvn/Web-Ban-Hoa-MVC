using DoAn.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using DoAn.Services;

namespace DoAn.Controllers
{
    public class DatHangController : Controller
    {
        QL_BanHoaEntities2 db = new QL_BanHoaEntities2();
        // GET: DatHang
        private List<GioHang> LayGioHang()
        {
            List<GioHang> lstGioHang = Session["GioHang"] as List<GioHang>;
            if (lstGioHang == null)
            {
                lstGioHang = new List<GioHang>();
                Session["GioHang"] = lstGioHang;
            }
            return lstGioHang;
        }

        private decimal TinhTongTien()
        {
            decimal dTongTien = 0;
            List<GioHang> lstGioHang = Session["GioHang"] as List<GioHang>;
            if (lstGioHang != null)
            {
                dTongTien = lstGioHang.Sum(n => n.dThanhTien);
            }
            return dTongTien;
        }

        public ActionResult ThanhToan()
        {
            // 1. KIỂM TRA ĐĂNG NHẬP
            if (Session["MaKH"] == null || Session["UserType"].ToString() != "Customer")
            {
                // Chưa đăng nhập => Chuyển qua trang Login
                // Kèm theo tham số 'url' là trang ThanhToan để đăng nhập xong nó tự quay lại đây
                TempData["ThongBao"] = "Vui lòng đăng nhập trước khi thanh toán!";
                return RedirectToAction("DangNhap", "TaiKhoan", new { url = Url.Action("ThanhToan", "DatHang") });
            }
            List<GioHang> lstGioHang = LayGioHang();
            //Nếu giỏ hàng trống thì không cho thanh toán
            if (lstGioHang.Count == 0) 
            {
                return RedirectToAction("XemGioHang", "GioHang");
            }
            //Kiểm tra đăng nhập
            //Nếu khách hàng đã đăng nhập thì tiến hành lấy thông tin khách hàng
            if (Session["MaKH"] != null)
            {
                int maKH = (int)Session["MaKH"];
                tblKhachHang khachHang = db.tblKhachHang.Find(maKH);
                ViewBag.KhachHang = khachHang;
            }
            //Nếu chưa đăng nhập
            else 
            {
                ViewBag.KhachHang = null;
            }

            ViewBag.TongTien = TinhTongTien();
            return View(lstGioHang);

        }

        // 1. Sửa hàm XacNhanDatHang
        [HttpPost]
        public async Task<ActionResult> XacNhanDatHang(FormCollection form)
        {
            
            List<GioHang> lstGioHang = LayGioHang();
            if (lstGioHang == null || lstGioHang.Count == 0)
            {
                return RedirectToAction("Index", "Hoa");
            }
            var neo4j = new Neo4jService();
            var client = await neo4j.GetClient();

            tblHoaDon hoaDon = new tblHoaDon();
            hoaDon.NgayLap = DateTime.Now;
            hoaDon.TinhTrang = 1;
            hoaDon.DaThanhToan = false;
            hoaDon.DiaChiGiaoHang = form["DiaChi"];
            hoaDon.TongTien = TinhTongTien();

            // --- LOGIC XỬ LÝ KHÁCH HÀNG & AUTO LOGIN ---
            if (Session["MaKH"] != null)
            {
                hoaDon.MaKH = (int)Session["MaKH"];
            }
            else
            {
                string email = form["Email"];
                string sdt = form["DienThoai"];
                var khachCu = db.tblKhachHang.FirstOrDefault(k => k.Email == email || k.DienThoai == sdt);

                if (khachCu != null)
                {
                    hoaDon.MaKH = khachCu.MaKH;
                    khachCu.TenKH = form["TenKH"];
                    khachCu.DiaChi = form["DiaChi"];
                    db.Entry(khachCu).State = EntityState.Modified;

                    // Auto Login cho khách cũ
                    Session["MaKH"] = khachCu.MaKH;
                    Session["TenKH"] = khachCu.TenKH;
                }
                else
                {
                    tblKhachHang khachMoi = new tblKhachHang();
                    khachMoi.TenKH = form["TenKH"];
                    khachMoi.DiaChi = form["DiaChi"];
                    khachMoi.DienThoai = form["DienThoai"];
                    khachMoi.Email = form["Email"];
                    khachMoi.MatKhau = "123456";
                    db.tblKhachHang.Add(khachMoi);
                    db.SaveChanges();
                    await client.Cypher
                    .Merge("(c:Customer {id: $id})")
                    .WithParam("id", khachMoi.MaKH)
                    .Set("c.name = $name")
                    .WithParam("name", khachMoi.TenKH)
                    .ExecuteWithoutResultsAsync();


                    hoaDon.MaKH = khachMoi.MaKH;

                    // Auto Login cho khách mới
                    Session["MaKH"] = khachMoi.MaKH;
                    Session["TenKH"] = khachMoi.TenKH;
                }
                Session["UserType"] = "Customer"; // Đánh dấu là đã đăng nhập
            }

            
            db.tblHoaDon.Add(hoaDon);
            db.SaveChanges();
            int maHDMoiTao = hoaDon.MaHD;

            foreach (var item in lstGioHang)
            {
                tblChiTietHoaDon chiTietHD = new tblChiTietHoaDon();
                chiTietHD.MaHD = maHDMoiTao;
                chiTietHD.MaHoa = item.iMaHoa;
                chiTietHD.SoLuong = item.iSoLuong;
                chiTietHD.GiaBan = item.dGiaBan;
                db.tblChiTietHoaDon.Add(chiTietHD);
            }
            db.SaveChanges();
            foreach (var item in lstGioHang)
            {
                await client.Cypher
                    .Match("(c:Customer)")
                    .Match("(f:Flower)")
                    .Where("c.id = $customerId")
                    .AndWhere("f.id = $flowerId")
                    .WithParam("customerId", hoaDon.MaKH)
                    .WithParam("flowerId", item.iMaHoa)
                    .Merge("(c)-[:BOUGHT]->(f)")
                    .ExecuteWithoutResultsAsync();
            }

            Session["GioHang"] = null;

            string pttt = form["PhuongThucThanhToan"];
            if (pttt == "COD")
            {
                // CHUYỀN ID ĐƠN HÀNG SANG TRANG THÀNH CÔNG
                return RedirectToAction("DatHangThanhCong", new { id = maHDMoiTao });
            }
            else
            {
                return RedirectToAction("ChonNganHang", new { maHD = maHDMoiTao });
            }
        }

        // 2. Sửa hàm XacNhanDaThanhToanQR
        public ActionResult XacNhanDaThanhToanQR(int maHD)
        {
            tblHoaDon donHang = db.tblHoaDon.Find(maHD);
            if (donHang != null)
            {
                donHang.DaThanhToan = true;
                db.SaveChanges();
            }
            // CHUYỀN ID SANG TRANG THÀNH CÔNG
            return RedirectToAction("DatHangThanhCong", new { id = maHD });
        }

        // 3. Sửa hàm DatHangThanhCong để nhận ID
        public ActionResult DatHangThanhCong(int? id)
        {
            ViewBag.MaHD = id; // Lưu ID vào ViewBag để bên View dùng
            return View();
        }

        // GET: /DatHang/ThanhToanQR
        public ActionResult ThanhToanQR(int maHD, int bankId,string bankName)
        {
            // Tìm đơn hàng vừa tạo
            tblHoaDon donHang = db.tblHoaDon.Find(maHD);
            if (donHang == null)
            {
                return HttpNotFound();
            }

            // --- Thông tin tài khoản của chủ tiệm ---
            // 
 
            string accountNo = "0123456789"; // Ví dụ: STK
            string accountName = "NGUYEN HOAI PHONG"; // Tên chủ TK
                                                 // ------------------------------------

            string amount = donHang.TongTien.Value.ToString("0");
            string memo = "HD" + donHang.MaHD; // Nội dung chuyển khoản, VD: "HD1"

            // Tạo link QR động bằng API của VietQR lấy trên gg xuống á
            // Link này sẽ tự tạo ảnh QR chứa đủ: STK, Bank, Số tiền, Nội dung
            string qrImageUrl = $"https://img.vietqr.io/image/{bankId}-{accountNo}-compact.png?amount={amount}&addInfo={memo}";

            ViewBag.QRImageUrl = qrImageUrl;
            ViewBag.Amount = donHang.TongTien;
            ViewBag.Memo = memo;
            ViewBag.AccountName = accountName;
            ViewBag.AccountNo = accountNo;
            ViewBag.BankName = bankName;
            ViewBag.MaHD = donHang.MaHD;
            return View();
        }


        
        public ActionResult ChonNganHang(int maHD)
        {
            tblHoaDon donHang = db.tblHoaDon.Find(maHD);
            if (donHang == null)
            {
                return HttpNotFound();
            }

            // Tạo danh sách các ngân hàng
           
            // Link logo lấy từ API của VietQR
            var bankList = new List<ThongTinNH>
    {
        new ThongTinNH { Id = "970422", Name = "MB Bank", LogoUrl = Url.Content("~/Content/HinhAnh/MB.png") },
        new ThongTinNH { Id = "970415", Name = "ViettinBank", LogoUrl =  Url.Content("~/Content/HinhAnh/logo-vietinbank.png") },
        new ThongTinNH { Id = "970436", Name = "Vietcombank", LogoUrl = Url.Content("~/Content/HinhAnh/VCB.png") },
        new ThongTinNH { Id = "970405", Name = "Agribank", LogoUrl =  Url.Content("~/Content/HinhAnh/AGRIBANK.png") },
        
        new ThongTinNH { Id = "970407", Name = "Techcombank", LogoUrl = Url.Content("~/Content/HinhAnh/TCB.png") },
        new ThongTinNH { Id = "970416", Name = "ACB", LogoUrl = Url.Content("~/Content/HinhAnh/ACB.png") },
        new ThongTinNH { Id = "970423", Name = "TienPhongBank", LogoUrl = Url.Content("~/Content/HinhAnh/TPB.png") }
    };

            ViewBag.BankList = bankList;
            ViewBag.MaHD = maHD;
            ViewBag.TongTien = donHang.TongTien;

            return View();
        }

        public ActionResult HuyThanhToan(int maHD)
        {
            // 1. Tìm đơn hàng
            tblHoaDon donHang = db.tblHoaDon.Find(maHD);

            // 2. Kiểm tra (chỉ hủy đơn nào chưa thanh toán)
            if (donHang != null && donHang.DaThanhToan == false)
            {
                // 3. Lấy lại chi tiết đơn hàng
                var chiTietDonHang = db.tblChiTietHoaDon.Where(ct => ct.MaHD == maHD).ToList();

                // 4. KHÔI PHỤC GIỎ HÀNG (QUAN TRỌNG)
                List<GioHang> gioHangMoi = new List<GioHang>();
                foreach (var item in chiTietDonHang)
                {
                    // (Giả sử class giỏ hàng của bạn tên là 'GioHang')
                    // Dùng constructor của GioHang(MaHoa) để lấy lại thông tin
                    GioHang cartItem = new GioHang(item.MaHoa);

                    // Cập nhật lại ĐÚNG số lượng đã lưu
                    cartItem.iSoLuong = item.SoLuong.HasValue ? item.SoLuong.Value : 1;

                    gioHangMoi.Add(cartItem);
                }

                // Lưu giỏ hàng vừa khôi phục vào Session
                Session["GioHang"] = gioHangMoi;

                // 5. XÓA ĐƠN HÀNG TẠM
                // Xóa chi tiết trước
                foreach (var chiTiet in chiTietDonHang)
                {
                    db.tblChiTietHoaDon.Remove(chiTiet);
                }
                // Xóa đơn hàng sau
                db.tblHoaDon.Remove(donHang);

                db.SaveChanges();
            }

            // 6. Chuyển người dùng về trang giỏ hàng
            return RedirectToAction("XemGioHang", "GioHang");
        }
    }
}