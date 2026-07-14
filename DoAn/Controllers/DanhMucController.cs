using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DoAn.Controllers
{
    public class DanhMucController : Controller
    {
        // GET: DanhMuc
        QL_BanHoaEntities2 db = new QL_BanHoaEntities2();
        public ActionResult Index()
        {
            return View();
        }
    
        public ActionResult _DanhMuc()
        {
            
            List<tblDanhMucHoa> lstDanhMuc = db.tblDanhMucHoa.ToList();
            ViewBag.LoaiHoaList = db.tblLoaiHoaChinh.ToList();
            return PartialView(lstDanhMuc);
        }

        public ActionResult _DanhMucTheoLoai()
        {
            
            List<tblDanhMucHoa> lstloaiHoa = db.tblDanhMucHoa.ToList();
            return PartialView(lstloaiHoa);
        }

        public ActionResult _DanhMucTheoHoa() 
        { 
            List<tblLoaiHoaChinh> lstHoa = db.tblLoaiHoaChinh.ToList();
            return PartialView(lstHoa);
        }
    }
}