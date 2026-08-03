using DoAn.Data;
using DoAn.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Linq;
using System.Web.Mvc;

namespace DoAn.Controllers
{
    public class TinTucController : Controller
    {
        private MongoDbContext mongoDb = new MongoDbContext();

        // -----------------------------------------------------------
        // 1. READ + LỌC + TÌM KIẾM + SẮP XẾP
        // -----------------------------------------------------------
        public ActionResult Index(string tuKhoa, string tagFilter)
        {
            var builder = Builders<CamNangHoaMongo>.Filter;
            var filter = builder.Empty; // Mặc định là lấy tất cả

            // TÌM KIẾM: Nếu có từ khóa, tìm tương đối (Regex) trong Tiêu đề
            if (!string.IsNullOrEmpty(tuKhoa))
            {
                filter &= builder.Regex(x => x.TieuDe, new BsonRegularExpression(tuKhoa, "i"));
            }

            // LỌC: Nếu có chọn Tag, lọc bài viết chứa Tag đó
            if (!string.IsNullOrEmpty(tagFilter))
            {
                filter &= builder.AnyEq(x => x.Tags, tagFilter);
            }

            // SẮP XẾP: Luôn lấy bài mới nhất lên đầu (Sort By Descending)
            var danhSachBaiViet = mongoDb.CamNangHoa_Blogs
                                         .Find(filter)
                                         .SortByDescending(x => x.NgayDang)
                                         .ToList();

            ViewBag.TuKhoa = tuKhoa;
            ViewBag.TagFilter = tagFilter;

            return View(danhSachBaiViet);
        }

        // Xem chi tiết một bài viết & Tăng lượt xem (Thống kê)
        public ActionResult ChiTiet(string id)
        {
            if (string.IsNullOrEmpty(id)) return HttpNotFound();

            // Tăng số lượt xem lên 1
            var update = Builders<CamNangHoaMongo>.Update.Inc(x => x.LuotXem, 1);
            mongoDb.CamNangHoa_Blogs.UpdateOne(x => x.Id == id, update);

            // Lấy ra bài viết để hiển thị
            var baiViet = mongoDb.CamNangHoa_Blogs.Find(x => x.Id == id).FirstOrDefault();
            if (baiViet == null) return HttpNotFound();

            return View(baiViet);
        }

        // -----------------------------------------------------------
        // 2. CREATE (THÊM MỚI)
        // -----------------------------------------------------------
        public ActionResult TaoMoi()
        {
            return View();
        }

        [HttpPost]
        [ValidateInput(false)] // Cho phép submit HTML
        public ActionResult TaoMoi(CamNangHoaMongo model)
        {
            model.NgayDang = DateTime.Now;
            model.LuotXem = 0;

            // Xử lý chuỗi TagsInput (người dùng nhập: HoaHong, CachChamSoc) thành mảng List<string>
            if (!string.IsNullOrEmpty(model.TagsInput))
            {
                model.Tags = model.TagsInput.Split(',')
                                  .Select(t => t.Trim())
                                  .Where(t => !string.IsNullOrEmpty(t))
                                  .ToList();
            }

            mongoDb.CamNangHoa_Blogs.InsertOne(model);
            return RedirectToAction("Index");
        }

        // -----------------------------------------------------------
        // 3. UPDATE (CẬP NHẬT)
        // -----------------------------------------------------------
        public ActionResult Sua(string id)
        {
            var baiViet = mongoDb.CamNangHoa_Blogs.Find(x => x.Id == id).FirstOrDefault();
            if (baiViet == null) return HttpNotFound();

            // Đưa list Tag về lại dạng chuỗi để gán vào ô Input
            if (baiViet.Tags != null && baiViet.Tags.Count > 0)
            {
                baiViet.TagsInput = string.Join(", ", baiViet.Tags);
            }

            return View(baiViet);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Sua(CamNangHoaMongo model)
        {
            var updateBuilder = Builders<CamNangHoaMongo>.Update
                .Set(x => x.TieuDe, model.TieuDe)
                .Set(x => x.NoiDung_HTML, model.NoiDung_HTML)
                .Set(x => x.TacGia, model.TacGia);

            if (!string.IsNullOrEmpty(model.TagsInput))
            {
                var tagList = model.TagsInput.Split(',')
                                   .Select(t => t.Trim())
                                   .Where(t => !string.IsNullOrEmpty(t))
                                   .ToList();
                updateBuilder = updateBuilder.Set(x => x.Tags, tagList);
            }
            else
            {
                updateBuilder = updateBuilder.Set(x => x.Tags, new System.Collections.Generic.List<string>());
            }

            mongoDb.CamNangHoa_Blogs.UpdateOne(x => x.Id == model.Id, updateBuilder);

            return RedirectToAction("Index");
        }

        // -----------------------------------------------------------
        // 4. DELETE (XÓA)
        // -----------------------------------------------------------
        public ActionResult Xoa(string id)
        {
            mongoDb.CamNangHoa_Blogs.DeleteOne(x => x.Id == id);
            return RedirectToAction("Index");
        }
    }
}