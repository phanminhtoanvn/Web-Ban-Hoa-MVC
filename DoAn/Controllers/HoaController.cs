using DoAn.Data;   // <--- THÊM DÒNG NÀY ĐỂ GỌI MONGODB
using DoAn.Models; // <--- THÊM DÒNG NÀY ĐỂ GỌI MONGODB MODEL
using DoAn.Services;
using MongoDB.Driver;
using PagedList;
using PagedList.Mvc;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using StackExchange.Redis;
using System.Web.Mvc;

namespace DoAn.Controllers
{
    public class HoaController : Controller
    {
        QL_BanHoaEntities2 db = new QL_BanHoaEntities2();
        // GET: Hoa
        // (Nhớ using System.Data.Entity; và using PagedList; ở đầu file nha)
        MongoDbContext mongoDb = new MongoDbContext();

        public ActionResult Index(string q, int? maDM, int? maLoaiChinh, int[] gia, bool sortByViews = false, int? page = null)
        {
            var cache = RedisService.GetDatabase();

            string priceKey = gia != null && gia.Length > 0 ? string.Join(",", gia.OrderBy(x => x)) : "";
            string cacheKey = $"flowers:{q}:{maDM}:{maLoaiChinh}:{priceKey}:{sortByViews}:{page}";

            var cachedData = cache.StringGet(cacheKey);

            if (!sortByViews && !cachedData.IsNullOrEmpty)
            {
                var hoaCache = JsonConvert.DeserializeObject<List<tblHoa>>(cachedData);

                ViewBag.LoaiHoaList = db.tblLoaiHoaChinh.ToList();

                return View(hoaCache.ToPagedList(page ?? 1, 8));
            }

            int pageSize = 8;
            int pageNumber = (page ?? 1);

            var hoaQuery = db.tblHoa.Include(h => h.tblDanhMucHoa);

            // 1. Lọc Tìm kiếm (string q) - Dùng cho CẢ header với sidebar
            if (!string.IsNullOrEmpty(q))
            {
                hoaQuery = hoaQuery.Where(h => h.TenHoa.Contains(q));
            }

            // 2. Lọc Danh mục (Hoa sinh nhật...)
            if (maDM.HasValue)
            {
                hoaQuery = hoaQuery.Where(h => h.MaDM == maDM);
            }

            // 3. LỌC MỚI: Lọc Loại hoa (Hoa Hồng, Ly...)
            if (maLoaiChinh.HasValue)
            {
                hoaQuery = hoaQuery.Where(h => h.MaLoaiChinh == maLoaiChinh);
            }

            // 4. Lọc theo GIÁ (int[] gia)
            if (gia != null && gia.Length > 0)
            {
                bool check1 = gia.Contains(1);
                bool check2 = gia.Contains(2);
                bool check3 = gia.Contains(3);
                bool check4 = gia.Contains(4);

                hoaQuery = hoaQuery.Where(h =>
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

            // Chuyển sang danh sách thực thi để lọc / sắp xếp lượt xem
            var hoaList = hoaQuery.ToList();

            Dictionary<int, int> viewCounts = null;

            if (sortByViews)
            {
                viewCounts = hoaList.ToDictionary(
                    h => h.MaHoa,
                    h => {
                        var value = cache.StringGet($"view:flower:{h.MaHoa}");
                        return value.HasValue && int.TryParse(value, out int parsed) ? parsed : 0;
                    }
                );

                hoaList = hoaList.OrderByDescending(h => viewCounts[h.MaHoa])
                                   .ThenBy(h => h.MaHoa)
                                   .ToList();

                ViewBag.ViewCounts = viewCounts;
            }
            else
            {
                hoaList = hoaList.OrderBy(h => h.MaHoa).ToList();
            }

            var result = hoaList.Select(h => new tblHoa
            {
                MaHoa = h.MaHoa,
                TenHoa = h.TenHoa,
                GiaBan = h.GiaBan,
                AnhDaiDien = h.AnhDaiDien,
                MaDM = h.MaDM,
                MaLoaiChinh = h.MaLoaiChinh
            }).ToList();

            cache.StringSet(
                cacheKey,
                JsonConvert.SerializeObject(result),
                TimeSpan.FromMinutes(5)
            );

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
            var cache = RedisService.GetDatabase();

            cache.StringIncrement($"view:flower:{id}");

            ViewBag.LuotXem = cache.StringGet($"view:flower:{id}");

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

            // 2. Lấy bình luận từ Mongo (nếu MongoDB đang chạy)
            try
            {
                var lstBinhLuan = mongoDb.BinhLuan_Reviews
                                         .Find(b => b.MaHoa == id)
                                         .SortByDescending(b => b.Ngay)
                                         .ToList();
                ViewBag.BinhLuan = lstBinhLuan;

                var thongKeSao = mongoDb.BinhLuan_Reviews.Aggregate()
                                        .Match(b => b.MaHoa == id)
                                        .Group(b => b.MaHoa, g => new {
                                            MaHoa = g.Key,
                                            DiemTrungBinh = g.Average(x => x.SoSao)
                                        })
                                        .FirstOrDefault();

                ViewBag.DiemTrungBinh = thongKeSao != null ? Math.Round(thongKeSao.DiemTrungBinh, 1) : 5.0;
            }
            catch (Exception)
            {
                ViewBag.BinhLuan = new List<DoAn.Models.BinhLuanMongo>();
                ViewBag.DiemTrungBinh = 5.0;
            }

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

        // Tạo QR payment token và lưu vào Redis với TTL 1 phút
        [HttpPost]
        public ActionResult CreateQrPayment(int orderId)
        {
            var cache = RedisService.GetDatabase();

            var order = db.tblHoaDon.Find(orderId);
            if (order == null)
            {
                return Json(new { success = false, message = "Đơn hàng không tồn tại" }, JsonRequestBehavior.AllowGet);
            }

            var orderItems = db.tblChiTietHoaDon
                .Where(x => x.MaHD == orderId)
                .Select(x => new
                {
                    x.MaHoa,
                    x.SoLuong,
                    x.GiaBan
                })
                .ToList();

            var customer = order.MaKH.HasValue ? db.tblKhachHang.Find(order.MaKH.Value) : null;

            string token = Guid.NewGuid().ToString("N");
            var paymentData = new
            {
                OrderId = orderId,
                CreatedUtc = DateTime.UtcNow,
                OrderCode = order.MaHD,
                TotalAmount = order.TongTien,
                Status = order.DaThanhToan == true ? "Paid" : "Pending",
                CustomerName = customer?.TenKH,
                CustomerPhone = customer?.DienThoai,
                CustomerEmail = customer?.Email,
                ShippingAddress = order.DiaChiGiaoHang,
                Items = orderItems
            };

            // Lưu key với TTL 5 phút (300 giây)
            cache.StringSet($"qr_pay:{token}", JsonConvert.SerializeObject(paymentData), TimeSpan.FromMinutes(5));

            return Json(new { success = true, token = token, expiresInSeconds = 300, data = paymentData });
        }

        // Kiểm tra token QR còn hiệu lực hay không
        [HttpGet]
        public ActionResult CheckQrPayment(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return Json(new { status = "invalid" }, JsonRequestBehavior.AllowGet);
            }

            var cache = RedisService.GetDatabase();

            var val = cache.StringGet($"qr_pay:{token}");
            if (val.IsNullOrEmpty)
            {
                // Key không tồn tại => đã hết hạn hoặc chưa được tạo
                return Json(new { status = "expired" }, JsonRequestBehavior.AllowGet);
            }

            dynamic data = JsonConvert.DeserializeObject<dynamic>(val);
            int orderId = data != null && data.OrderId != null ? (int)data.OrderId : 0;

            if (orderId > 0)
            {
                var order = db.tblHoaDon.Find(orderId);
                if (order != null && order.DaThanhToan == true)
                {
                    return Json(new { status = "paid", data = data }, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(new { status = "active", data = data }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ThemBinhLuan(int MaHoa, string NoiDung, int SoSao)
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
                var neo4j = new Neo4jService();
                var client = await neo4j.GetClient();

                await client.Cypher
                    .Match("(c:Customer)")
                    .Match("(f:Flower)")
                    .Where("c.id = $customerId")
                    .AndWhere("f.id = $flowerId")
                    .WithParam("customerId", maKH)
                    .WithParam("flowerId", MaHoa)
                    .WithParam("rating", SoSao)
                    .WithParam("comment", NoiDung)
                    .WithParam("reviewDate", DateTime.Now)
                    .Merge("(c)-[r:REVIEWED]->(f)")
                    .Set("r.rating = $rating")
                    .Set("r.comment = $comment")
                    .Set("r.reviewDate = $reviewDate")
                    .ExecuteWithoutResultsAsync();
            }

            return RedirectToAction("ChiTietHoa", new { id = MaHoa });
        }

    }
}