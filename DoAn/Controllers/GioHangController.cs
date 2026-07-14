using DoAn.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DoAn.Controllers
{
    public class GioHangController : Controller
    {
        QL_BanHoaEntities2 db = new QL_BanHoaEntities2();
        // GET: GioHang
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

        private int TongSoLuong()
        {
            int iTongSoLuong = 0;
            List<GioHang> lstGioHang = Session["GioHang"] as List<GioHang>;
            if (lstGioHang != null)
            {
                iTongSoLuong = lstGioHang.Sum(n => n.iSoLuong);
            }
            return iTongSoLuong;
        }
        private decimal TongThanhTien()
        {
            decimal dTongThanhTien = 0;
            List<GioHang> lstGioHang = Session["GioHang"] as List<GioHang>;
            if (lstGioHang != null)
            {
                dTongThanhTien = lstGioHang.Sum(n => n.dThanhTien);
            }
            return dTongThanhTien;
        }
        public ActionResult XemGioHang()
        {
            List<GioHang> lstGioHang = LayGioHang();
            ViewBag.TongSoLuong = TongSoLuong();
            ViewBag.TongThanhTien = TongThanhTien();
            return View(lstGioHang);
        }
        // Thêm vào giỏ hàng
        public RedirectResult ThemGioHang(int MaHoa, string strURL)
        {
            List<GioHang> lstGioHang = LayGioHang();
            GioHang sanPham = lstGioHang.Find(n => n.iMaHoa == MaHoa);
            if (sanPham == null)
            {
                sanPham = new GioHang(MaHoa);
                lstGioHang.Add(sanPham);

            }
            else
            {
                sanPham.iSoLuong++;

            }

            //Lưu lại Session
            Session["GioHang"] = lstGioHang;

            //Chuyển về trang hiện tại
            return Redirect(strURL);
        }

        [HttpPost]
        public ActionResult CapNhatGioHang(int MaHoa, FormCollection form)
        {
            List<GioHang> lstGioHang = LayGioHang();
            GioHang sanPham = lstGioHang.Find(n => n.iMaHoa == MaHoa);
            if (sanPham != null)
            {
                sanPham.iSoLuong = int.Parse(form["txtSoLuong"].ToString());
            }
            //Lưu lại Session
            Session["GioHang"] = lstGioHang;
            return RedirectToAction("XemGioHang");
        }

        public ActionResult XoaGioHang(int MaHoa)
        {
            List<GioHang> lstGioHang = LayGioHang();
            GioHang sanPham = lstGioHang.FirstOrDefault(n => n.iMaHoa == MaHoa);

            if (sanPham != null)
            {
                lstGioHang.RemoveAll(n => n.iMaHoa == MaHoa);
            }

            Session["GioHang"] = lstGioHang;
            return RedirectToAction("XemGioHang");
        }

        public ActionResult XoaTatCaGioHang()
        {
            Session["GioHang"] = null; // Xóa Session
            return RedirectToAction("XemGioHang");
        }

        // Action để render cái Icon giỏ hàng trên Header (PartialView)
        [ChildActionOnly] // Chỉ cho phép gọi từ trong View
        public ActionResult _GioHangPartial()
        {
            ViewBag.TongSoLuong = TongSoLuong();
            return PartialView();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // Action này nhận cả MaHoa và SoLuong từ Form
        public ActionResult ThemGioHangChiTiet(int MaHoa, int SoLuong)
        {
            List<GioHang> lstGioHang = LayGioHang(); // (Giả sử class là ItemGioHang)

            GioHang sanPham = lstGioHang.FirstOrDefault(n => n.iMaHoa == MaHoa);

            if (sanPham == null)
                
        {
                // Nếu chưa có, tạo mới và gán đúng số lượng
                sanPham = new GioHang(MaHoa);
                sanPham.iSoLuong = SoLuong; // Gán số lượng người dùng nhập
                lstGioHang.Add(sanPham);
            }
        else
            {
                // Nếu đã có, CỘNG THÊM số lượng
                // Ví dụ: Giỏ có 6, người dùng thêm 4 => 6 + 4 = 10
                sanPham.iSoLuong += SoLuong;
            }

            // Lưu lại giỏ hàng vào Session
            Session["GioHang"] = lstGioHang;

            // Thêm xong thì chuyển người dùng đến trang Giỏ Hàng
            return RedirectToAction("XemGioHang");
        }
    }
}