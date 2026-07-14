using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DoAn.Models
{
    public class GioHang
    {
        QL_BanHoaEntities2 db = new QL_BanHoaEntities2();
        public int iMaHoa { get; set; }
        public string sTenHoa { get; set; }
        public string sAnhDaiDien { get; set; }
        public decimal dGiaBan { get; set; }
        public int iSoLuong { get; set; }

        public decimal dThanhTien
        {
            //Chỉ cần đọc, không cần gán giá trị
            get { return iSoLuong * dGiaBan; }
        }

        public GioHang(int MaHoa)
        {
            iMaHoa = MaHoa;

            tblHoa hoa = db.tblHoa.SingleOrDefault(n => n.MaHoa == iMaHoa);

            if (hoa != null)
            {
                sTenHoa = hoa.TenHoa;
                sAnhDaiDien = hoa.AnhDaiDien;
                dGiaBan = (decimal)hoa.GiaBan; // Ép kiểu vì GiaBan có thể là nullable
                iSoLuong = 1; // Mặc định khi thêm vào giỏ là 1
            }
        }
    }
}