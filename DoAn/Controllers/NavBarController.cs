using DoAn.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DoAn.Controllers
{
    public class NavBarController : Controller
    {
        QL_BanHoaEntities2 db = new QL_BanHoaEntities2();
        // GET: NavBar
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult _NavBar()
        {
            // 1. Lấy TẤT CẢ dữ liệu cần thiết trong MỘT query duy nhất
            //    Sử dụng .Include() để Eager Loading
            var menuData = db.tblHoa
                .Where(h => h.MaDM.HasValue && h.MaLoaiChinh.HasValue)
                .GroupBy(h => new {
                    MaDM = h.MaDM.Value,
                    TenDM = h.tblDanhMucHoa.TenDM,
                    MaLoaiChinh = h.MaLoaiChinh.Value,
                    TenLoaiChinh = h.tblLoaiHoaChinh.TenLoaiChinh
                })
                .Select(g => new {
                    g.Key.MaDM,
                    g.Key.TenDM,
                    g.Key.MaLoaiChinh,
                    g.Key.TenLoaiChinh,
                    SoLuongSanPham = g.Count()
                })
                .ToList(); // Load tất cả vào memory 1 lần

            // 2. Xử lý trong C# (không query DB nữa)
            var danhMucList = menuData
                .GroupBy(x => new { x.MaDM, x.TenDM })
                .Select(g => new DanhMucMenuViewModel
                {
                    MaDM = g.Key.MaDM,
                    TenDM = g.Key.TenDM,
                    LoaiHoaList = g.Select(x => new LoaiHoaMenuViewModel
                    {
                        MaLoaiChinh = x.MaLoaiChinh,
                        TenLoaiChinh = x.TenLoaiChinh,
                        SoLuongSanPham = x.SoLuongSanPham
                    }).ToList()
                })
                .OrderBy(dm => dm.TenDM)
                .ToList();

            return PartialView(danhMucList);
        }

    }
}