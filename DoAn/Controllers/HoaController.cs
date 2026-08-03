using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using PagedList;
using PagedList.Mvc;
using System.Data.Entity;
using DoAn.Data;   // <--- THÊM DÒNG NÀY ĐỂ GỌI MONGODB
using DoAn.Models; // <--- THÊM DÒNG NÀY ĐỂ GỌI MONGODB MODEL
using MongoDB.Driver;

namespace DoAn.Controllers
{
    public class HoaController : Controller
    {
        QL_BanHoaEntities2 db = new QL_BanHoaEntities2();
        // GET: Hoa
        // (Nhớ using System.Data.Entity; và using PagedList; ở đầu file nha)
        MongoDbContext mongoDb = new MongoDbContext();

        public ActionResult Index(string q, int? maDM, int? maLoaiChinh, int[] gia, int? page)
        {
            int pageSize = 8;
            int pageNumber = (page ?? 1);

            var hoaList = db.tblHoa.Include(h => h.tblDanhMucHoa);

            // 1. Lọc Tìm kiếm (string q) - Dùng cho CẢ header với sidebar
            if (!string.IsNullOrEmpty(q))
            {
                hoaList = hoaList.Where(h => h.TenHoa.Contains(q));
            }

            // 2. Lọc Danh mục (Hoa sinh nhật...)
            if (maDM.HasValue)
            {
                hoaList = hoaList.Where(h => h.MaDM == maDM);
            }

            // 3. LỌC MỚI: Lọc Loại hoa (Hoa Hồng, Ly...)
            if (maLoaiChinh.HasValue)
            {
                hoaList = hoaList.Where(h => h.MaLoaiChinh == maLoaiChinh);
            }

            // 4. Lọc theo GIÁ (int[] gia)
            if (gia != null && gia.Length > 0)
            {
                bool check1 = gia.Contains(1);
                bool check2 = gia.Contains(2);
                bool check3 = gia.Contains(3);
                bool check4 = gia.Contains(4);
                // Đây là logic "HOẶC" (OR)
                // Ví dụ: Nếu ní check (gia.Contains(1) || gia.Contains(2))
                hoaList = hoaList.Where(h =>
                    (check1 && h.GiaBan >= 0 && h.GiaBan <= 400000) ||
                    (check2 && h.GiaBan >= 500000 && h.GiaBan <= 1000000) ||
                    (check3 && h.GiaBan > 1000000 && h.GiaBan <= 2000000) ||
                    (check4 && h.GiaBan > 2000000)
                );
            }

            // 5. GỬI DATA CHO SIDEBAR LỌC
            // Gửi list "Loại Hoa" (Hoa Hồng, Ly...) cho cái dropdown
            ViewBag.LoaiHoaList = db.tblLoaiHoaChinh.ToList();
            // thêm yêu thích
            List<int> likedFlowers = new List<int>();
            if (Session["MaKH"] != null)
            {
                int maKH = (int)Session["MaKH"];
                var mongoDb = new DoAn.Data.MongoDbContext(); // Thay thế DoAn.Data bằng namespace tương ứng chứa MongoDbContext của bạn
                var wishlist = mongoDb.HoaYeuThich_Wishlists.Find(x => x.MaKH == maKH).FirstOrDefault();

                if (wishlist != null && wishlist.DanhSachMaHoa != null)
                {
                    likedFlowers = wishlist.DanhSachMaHoa;
                }
            }
            ViewBag.LikedFlowers = likedFlowers;

            // 6. Sắp xếp (Bắt buộc)
            hoaList = hoaList.OrderBy(h => h.MaHoa);

            // 7. Phân trang và trả về View
            return View(hoaList.ToPagedList(pageNumber, pageSize));


        }

        public ActionResult ChiTietHoa(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tblHoa hoa = db.tblHoa.FirstOrDefault(n => n.MaHoa == id);
            if (hoa == null)
            {
                return HttpNotFound();
            }

            // (Code gốc của bạn) - Sản phẩm cùng loại hoa (Dùng cho "Sản phẩm tương tự")
            List<tblHoa> lstHoaCungLoai = db.tblHoa.Where(n => n.MaLoaiChinh == hoa.MaLoaiChinh && n.MaHoa != id).Take(4).ToList();
            ViewBag.HoaCungLoai = lstHoaCungLoai;

            // (Code gốc của bạn) - Sản phẩm cùng danh mục
            List<tblHoa> lstHoaCungDM = db.tblHoa.Where(n => n.MaDM == hoa.MaDM && n.MaHoa != id).Take(4).ToList();
            ViewBag.HoaCungDM = lstHoaCungDM;

            // 
            // 1. Lấy gallery ảnh (ảnh phụ)
            ViewBag.HinhAnhGallery = db.tblHinhAnh
                .Where(h => h.MaHoa == id).ToList();

            // 2. Lấy bình luận từ Mongo
            var lstBinhLuan = mongoDb.BinhLuan_Reviews
                                     .Find(b => b.MaHoa == id)
                                     .SortByDescending(b => b.Ngay)
                                     .ToList();
            ViewBag.BinhLuan = lstBinhLuan;

            // THỐNG KÊ MONGODB: Tính điểm sao trung bình bằng Aggregation Framework
            var thongKeSao = mongoDb.BinhLuan_Reviews.Aggregate()
                                    .Match(b => b.MaHoa == id)
                                    .Group(b => b.MaHoa, g => new {
                                        MaHoa = g.Key,
                                        DiemTrungBinh = g.Average(x => x.SoSao)
                                    })
                                    .FirstOrDefault();

            // Nếu chưa có bình luận nào thì mặc định hiển thị 5 sao, ngược lại thì làm tròn 1 chữ số thập phân
            ViewBag.DiemTrungBinh = thongKeSao != null ? Math.Round(thongKeSao.DiemTrungBinh, 1) : 5.0;

            return View(hoa);
        }


        

        // Lọc theo Danh Mục (Hoa sinh nhật, khai trương...)
        //public ActionResult LocTheoDM(int id)
        //{
        //    List<tblHoa> listHoa = db.tblHoa.Where(h => h.MaDM == id).ToList();
        //    // Dùng lại View "Index" để hiển thị kết quả lọc
        //    return View("Index", listHoa);
        //}

        //// Lọc theo Loại Hoa Chính (Hồng, Ly...)
        //public ActionResult LocTheoLoaiHoa(int id)
        //{
        //    List<tblHoa> listHoa = db.tblHoa.Where(h => h.MaLoaiChinh == id).ToList();
        //    return View("Index", listHoa);
        //}

        // Thanh tìm kiếm nhanh (trên header)
        //public ActionResult ThanhTimKiem(string q)
        //{
        //    List<tblHoa> listHoa = db.tblHoa
        //        .Where(h => h.TenHoa.Contains(q.ToLower())).ToList();
        //    return View("Index", listHoa);
        //}

        
        public ActionResult TimKiem(string kw, int? danhmuc, string[] gia)
        {
            
            IQueryable<tblHoa> query = db.tblHoa;

            // 2. Lọc theo từ khóa (nếu có)
            if (!string.IsNullOrEmpty(kw))
            {
                string kwLower = kw.ToLower();
                query = query.Where(h => h.TenHoa.ToLower().Contains(kwLower));
            }

            // 3. Lọc theo danh mục (nếu có)
            
            if (danhmuc.HasValue)
            {
                query = query.Where(h => h.MaDM == danhmuc.Value);
            }

            // 4. Chạy ToList() MỘT LẦN DUY NHẤT
            List<tblHoa> listHoa = query.ToList();

            // 5. Lọc theo giá (trên danh sách listHoa đã lọc)
            if (gia != null && gia.Length > 0)
            {
                var listGia = new List<tblHoa>();
                foreach (var g in gia)
                {
                    if (g.Contains('-'))
                    {
                        var arr = g.Split('-');
                        int min = int.Parse(arr[0]);
                        int max = int.Parse(arr[1]);
                        listGia.AddRange(listHoa.Where(h => h.GiaBan >= min && h.GiaBan <= max));
                    }
                    else if (g.Contains('>'))
                    {
                        var arr = g.Replace(">", "");
                        int min = int.Parse(arr);
                        listGia.AddRange(listHoa.Where(h => h.GiaBan >= min));
                    }
                }
                listHoa = listGia.Distinct().ToList();
            }

            
            return View("Index", listHoa);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ThemBinhLuan(int MaHoa, string NoiDung, int SoSao)
        {
            // 1. Kiểm tra đăng nhập (để lấy tên người bình luận)
            if (Session["MaKH"] == null)
            {
                return RedirectToAction("DangNhap", "TaiKhoan", new { url = Url.Action("ChiTietHoa", "Hoa", new { id = MaHoa }) });
            }

            // 2. Lấy tên khách hàng từ CSDL
            int maKH = (int)Session["MaKH"];
            var khachHang = db.tblKhachHang.Find(maKH);

            if (khachHang != null)
            {
                // 3. Tạo bình luận (Khớp với cột trong db_moi.sql) đã sửa khớp với mongo
                BinhLuanMongo bl = new BinhLuanMongo();
                bl.MaHoa = MaHoa;
                bl.HoTen = khachHang.TenKH; // Lưu tên khách vào cột HoTen
                bl.NoiDung = NoiDung;
                bl.SoSao = SoSao;
                bl.Ngay = DateTime.Now;

                // Lưu ý: DB của ní không có cột MaKH nên mình không gán bl.MaKH

                mongoDb.BinhLuan_Reviews.InsertOne(bl);
            }

            return RedirectToAction("ChiTietHoa", new { id = MaHoa });
        }

    }
}