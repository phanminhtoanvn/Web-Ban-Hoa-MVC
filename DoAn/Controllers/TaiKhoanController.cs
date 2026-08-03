using DoAn.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace DoAn.Controllers
{
    public class TaiKhoanController : Controller
    {
        QL_BanHoaEntities2 db = new QL_BanHoaEntities2();
        // GET: TaiKhoan
        public ActionResult DangNhap()
        {
            ViewBag.URL = Request.UrlReferrer.ToString()??"/";
            if (TempData["ThongBao"] != null)
            {
                ViewBag.ThongBao = TempData["ThongBao"];
            }
            return View();
        }
        public ActionResult DangKy()
        {
            return View();
        }

        

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DangKy(FormCollection collection)
        {
            // 1. Lấy dữ liệu từ form
            string hoTen = collection["FullName"];
            string email = collection["Email"];
            string matKhau = collection["Password"];
            string xacNhanMatKhau = collection["ConfirmPassword"];

            // 2. Kiểm tra dữ liệu
            if (String.IsNullOrEmpty(hoTen) || String.IsNullOrEmpty(email) || String.IsNullOrEmpty(matKhau))
            {
                ViewBag.Loi = "Vui lòng nhập đầy đủ thông tin!";
                return View();
            }

            if (matKhau != xacNhanMatKhau)
            {
                ViewBag.Loi = "Mật khẩu xác nhận không khớp!";
                return View();
            }

            // 3. Kiểm tra Email đã tồn tại chưa
            var checkEmail = db.tblKhachHang.FirstOrDefault(x => x.Email == email);
            if (checkEmail != null)
            {
                ViewBag.Loi = "Email này đã được đăng ký! Vui lòng chọn email khác.";
                return View();
            }

            // 4. Lưu vào CSDL
            tblKhachHang khMoi = new tblKhachHang();
            khMoi.TenKH = hoTen;
            khMoi.Email = email;
            khMoi.MatKhau = matKhau; // Lưu ý: Đang lưu pass thường (theo code cũ của ní)
            khMoi.DienThoai = ""; // Để trống hoặc xử lý sau
            khMoi.DiaChi = "";

            // Mặc định avatar (nếu cần)
            // khMoi.Avarta = "default.jpg"; 

            db.tblKhachHang.Add(khMoi);
            db.SaveChanges();

            var neo4j = new Neo4jService();
            var client = await neo4j.GetClient();

            await client.Cypher
                .Merge("(c:Customer {id: $id})")
                .WithParam("id", khMoi.MaKH)
                .Set("c.name = $name")
                .WithParam("name", khMoi.TenKH)
                .ExecuteWithoutResultsAsync();

            // 1. TỰ ĐỘNG ĐĂNG NHẬP LUÔN (Không bắt khách nhập lại nữa)
            Session["TenKH"] = khMoi.TenKH;
            Session["MaKH"] = khMoi.MaKH;
            Session["UserType"] = "Customer";

            // 2. Thông báo và chuyển hướng sang trang THÔNG TIN
            TempData["ThongBao"] = "Đăng ký thành công! Vui lòng cập nhật thêm thông tin giao hàng.";

            // Chuyển sang trang hồ sơ để điền nốt Địa chỉ, SĐT...
            return RedirectToAction("ThongTin");
        }

        [HttpPost]
        public ActionResult XuLyDangNhap(FormCollection form, string url)
        {
            string taiKhoan = form["Email"];
            string matKhau = form["Password"];

            // 1. KIỂM TRA KHÁCH HÀNG TRƯỚC
            var khachHang = db.tblKhachHang.FirstOrDefault(n => n.Email == taiKhoan && n.MatKhau == matKhau);

            if (khachHang != null)
            {
                // Lưu Session cho Khách
                Session["TenKH"] = khachHang.TenKH;
                Session["MaKH"] = khachHang.MaKH;
                Session["UserType"] = "Customer";

                // --- LOGIC CHUYỂN HƯỚNG KHÁCH HÀNG ---
                // Nếu có link cũ (url) và link đó không phải là trang đăng nhập -> Quay lại đó
                if (!string.IsNullOrEmpty(url) &&!url.Contains("DangNhap") &&!url.Contains("DangKy") &&!url.Contains("QuenMatKhau")) 
                {
                    return Redirect(url); // Quay lại trang sản phẩm đang xem dở
                }
                else
                {
                    return RedirectToAction("Index", "Hoa"); // Mặc định về Trang chủ
                }
            }

            // 2. NẾU KHÔNG PHẢI KHÁCH, KIỂM TRA NHÂN VIÊN / ADMIN
            var nhanVien = db.tblNhanVien.FirstOrDefault(n => n.TaiKhoan == taiKhoan && n.MatKhau == matKhau);

            if (nhanVien != null)
            {
                // Lưu Session cho Admin/Nhân viên
                Session["TenNV"] = nhanVien.TenNV;
                Session["MaNV"] = nhanVien.MaNV;
                Session["VaiTro"] = nhanVien.VaiTro; // 1: Admin, 2: Nhân viên
                Session["UserType"] = "Admin"; // Đánh dấu để qua mặt bộ lọc AdminController

                // --- LOGIC CHUYỂN HƯỚNG ADMIN ---
                // Bắt buộc vào trang quản trị
                return RedirectToAction("Index", "Admin");
            }

            // 3. ĐĂNG NHẬP THẤT BẠI
            ViewBag.Loi = "Sai tên đăng nhập hoặc mật khẩu!";
            // Giữ lại url cũ để nếu nhập lại đúng thì vẫn redirect đúng
            ViewBag.URL = url;
            return View("DangNhap");
        }

        // 1. Sửa hàm ThongTin để lấy thêm danh sách đơn hàng
        public ActionResult ThongTin()
        {
            if (Session["MaKH"] == null)
            {
                return RedirectToAction("DangNhap");
            }
            if (TempData["ThongBao"] != null)
            {
                ViewBag.ThongBao = TempData["ThongBao"];
            }

            ViewBag.Title = "Hồ sơ của tôi";
            int maKH = (int)Session["MaKH"];

            // Lấy thông tin khách hàng
            tblKhachHang khachHang = db.tblKhachHang.Find(maKH);

            // --- LẤY LỊCH SỬ ĐƠN HÀNG (Mới nhất lên đầu) ---
            var lichSuDonHang = db.tblHoaDon
                                  .Where(d => d.MaKH == maKH)
                                  .OrderByDescending(d => d.NgayLap)
                                  .ToList();

            ViewBag.LichSuDonHang = lichSuDonHang; // Gửi sang View
                                                   // -----------------------------------------------

            return View(khachHang);
        }

        // 2. Thêm hàm xem chi tiết đơn hàng cụ thể
        public ActionResult ChiTietDonHang(int id)
        {
            if (Session["MaKH"] == null)
            {
                return RedirectToAction("DangNhap");
            }

            // Tìm đơn hàng theo ID
            var donHang = db.tblHoaDon.Find(id);

            // Kiểm tra bảo mật: Đơn hàng phải tồn tại VÀ phải đúng của khách hàng này
            // (Tránh trường hợp khách A nhập lụi ID để xem đơn của khách B)
            int maKH = (int)Session["MaKH"];
            if (donHang == null || donHang.MaKH != maKH)
            {
                return RedirectToAction("ThongTin"); // Đá về nếu không hợp lệ
            }

            return View(donHang);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CapNhatThongTin(tblKhachHang khachHangForm)
        {
            // 1. Kiểm tra đăng nhập
            if (Session["MaKH"] == null)
            {
                return RedirectToAction("DangNhap");
            }

            // 2. Kiểm tra dữ liệu (Nếu dùng Model Validation)
            if (ModelState.IsValid)
            {
                // 3. Tìm khách hàng trong CSDL
                tblKhachHang khachHangDB = db.tblKhachHang.Find(khachHangForm.MaKH);

                if (khachHangDB != null && khachHangDB.MaKH == (int)Session["MaKH"])
                {
                    // 4. Cập nhật các trường cho phép sửa
                    khachHangDB.TenKH = khachHangForm.TenKH;
                    khachHangDB.Email = khachHangForm.Email;
                    khachHangDB.DienThoai = khachHangForm.DienThoai;
                    khachHangDB.DiaChi = khachHangForm.DiaChi;
                    khachHangDB.NamSinh = khachHangForm.NamSinh;
                    khachHangDB.GioiTinh = khachHangForm.GioiTinh;

                    // 5. Lưu CSDL
                    db.Entry(khachHangDB).State = EntityState.Modified;
                    db.SaveChanges();

                    // Cập nhật lại Session tên khách hàng (nếu tên bị đổi)
                    Session["TenKH"] = khachHangDB.TenKH;

                    // Sau khi cập nhật, quay lại trang thông tin
                    TempData["ThongBao"] = "Cập nhật thông tin thành công!";

                    var gioHang = Session["GioHang"] as List<DoAn.Models.GioHang>;

                    if (gioHang != null && gioHang.Count > 0)
                    {
                        // Nếu đang có hàng chờ mua -> Đẩy thẳng qua thanh toán
                        TempData["ThongBao"] = "Cập nhật xong! Mời bạn tiếp tục thanh toán.";
                        return RedirectToAction("ThanhToan", "DatHang");
                    }
                    else
                    {
                        // Nếu không mua gì thì cứ ở lại trang thông tin
                        ViewBag.ThongBao = "Cập nhật thông tin thành công!";
                        return RedirectToAction("ThongTin");
                    }
                }
            }

            // Nếu có lỗi, quay lại form cũ
            ViewBag.Title = "Thông tin cá nhân";
            return View("ThongTin", khachHangForm);
        }
        public ActionResult DangXuat()
        {
            Session.Clear(); // Xóa tất cả các Session
            return RedirectToAction("Index", "Hoa"); // Về trang chủ
        }

        public ActionResult QuenMatKhau()
        {
            return View();
        }

        // 2. Xử lý logic gửi mail
        [HttpPost]
        public ActionResult QuenMatKhau(string email)
        {
            // Kiểm tra email có trong DB không
            var kh = db.tblKhachHang.FirstOrDefault(k => k.Email == email);

            if (kh != null)
            {
                // Tạo mật khẩu mới ngẫu nhiên (8 ký tự)
                string matKhauMoi = Guid.NewGuid().ToString().Substring(0, 8);

                // Lưu mật khẩu mới vào DB
                kh.MatKhau = matKhauMoi;
                db.SaveChanges();

                // Gửi Email
                string subject = "Cấp lại mật khẩu - FlowerShop";
                string body = "Chào " + kh.TenKH + ",\n\nMật khẩu mới của bạn là: " + matKhauMoi + "\nVui lòng đăng nhập và đổi lại mật khẩu ngay.";

                bool guithanhcong = GuiEmail(email, subject, body);

                if (guithanhcong)
                {
                    ViewBag.ThongBao = "Mật khẩu mới đã được gửi về Email của bạn!";
                }
                else
                {
                    ViewBag.Loi = "Lỗi gửi mail! Vui lòng thử lại sau.";
                }
            }
            else
            {
                ViewBag.Loi = "Email này chưa đăng ký tài khoản!";
            }

            return View();
        }

        // 3. Hàm phụ trợ gửi Email (Dùng Gmail)
        public bool GuiEmail(string toEmail, string subject, string body)
        {
            try
            {
                var senderEmail = new MailAddress("nguyenlevithan1906@gmail.com", "FlowerShop Admin");
                var receiverEmail = new MailAddress(toEmail, "Receiver");
                var password = "lgjftzfocqqscldx"; // KHÔNG PHẢI MẬT KHẨU GMAIL THƯỜNG

                var smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(senderEmail.Address, password)
                };

                using (var mess = new MailMessage(senderEmail, receiverEmail)
                {
                    Subject = subject,
                    Body = body
                })
                {
                    smtp.Send(mess);
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        

        // 1. Hiển thị form đổi mật khẩu
        public ActionResult DoiMatKhau()
        {
            // Phải đăng nhập mới được đổi
            if (Session["MaKH"] == null)
            {
                return RedirectToAction("DangNhap");
            }
            return View();
        }

        // 2. Xử lý logic đổi mật khẩu
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DoiMatKhau(string MatKhauCu, string MatKhauMoi, string XacNhanMatKhau)
        {
            if (Session["MaKH"] == null)
            {
                return RedirectToAction("DangNhap");
            }

            // Kiểm tra dữ liệu nhập vào
            if (string.IsNullOrEmpty(MatKhauCu) || string.IsNullOrEmpty(MatKhauMoi) || string.IsNullOrEmpty(XacNhanMatKhau))
            {
                ViewBag.Loi = "Vui lòng nhập đầy đủ thông tin!";
                return View();
            }

            if (MatKhauMoi != XacNhanMatKhau)
            {
                ViewBag.Loi = "Mật khẩu xác nhận không khớp!";
                return View();
            }

            // Lấy thông tin khách hàng từ DB
            int maKH = (int)Session["MaKH"];
            var kh = db.tblKhachHang.Find(maKH);

            if (kh != null)
            {
                // Kiểm tra mật khẩu cũ (So sánh text thường vì hệ thống ní chưa mã hóa)
                if (kh.MatKhau != MatKhauCu)
                {
                    ViewBag.Loi = "Mật khẩu cũ không đúng!";
                    return View();
                }

                // Cập nhật mật khẩu mới
                kh.MatKhau = MatKhauMoi;

                // Lưu vào DB
                db.Entry(kh).State = EntityState.Modified;
                db.SaveChanges();

                ViewBag.ThongBao = "Đổi mật khẩu thành công!";
            }

            return View();
        }
    }
}