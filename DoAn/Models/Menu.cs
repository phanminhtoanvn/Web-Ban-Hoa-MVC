using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DoAn.Models
{
    public class DanhMucMenuViewModel
    {
        public int MaDM { get; set; }
        public string TenDM { get; set; }
        public List<LoaiHoaMenuViewModel> LoaiHoaList { get; set; }
    }

    public class LoaiHoaMenuViewModel
    {
        public int MaLoaiChinh { get; set; }
        public string TenLoaiChinh { get; set; }
        public int SoLuongSanPham { get; set; }
    }
}