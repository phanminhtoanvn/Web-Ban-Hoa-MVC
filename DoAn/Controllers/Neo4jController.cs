using DoAn.Data;
using DoAn.Services;
using MongoDB;
using Neo4jClient;
using Neo4jClient.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using MongoDB.Driver;

namespace DoAn.Controllers
{
    public class Neo4jController : Controller
    {
        // GET: Neo4j
        MongoDbContext mongoDb = new MongoDbContext();
        QL_BanHoaEntities2 db = new QL_BanHoaEntities2();
        public ActionResult Index()
        {
            return View();
        }
        public async Task<ActionResult> ImportCustomers()
        {
            var neo4j = new Neo4jService();
            var client = await neo4j.GetClient();

            var customers = db.tblKhachHang.ToList();

            foreach (var c in customers)
            {
                await client.Cypher
                    .Merge("(cus:Customer {id: $id})")
                    .WithParam("id", c.MaKH)
                    .Set("cus.name = $name")
                    .Set("cus.email = $email")
                    .Set("cus.phone = $phone")
                    .Set("cus.address = $address")
                    .Set("cus.gender = $gender")
                    .Set("cus.birthYear = $birthYear")
                    .WithParam("name", c.TenKH)
                    .WithParam("email", c.Email)
                    .WithParam("phone", c.DienThoai)
                    .WithParam("address", c.DiaChi)
                    .WithParam("gender", c.GioiTinh)
                    .WithParam("birthYear", c.NamSinh)
                    .ExecuteWithoutResultsAsync();
            }

            return Content("Import Customer Success");
        }
        public async Task<ActionResult> ImportFlowers()
        {
            var neo4j = new Neo4jService();
            var client = await neo4j.GetClient();

            var flowers = db.tblHoa.ToList();

            foreach (var f in flowers)
            {
                await client.Cypher
                    .Merge("(flower:Flower {id: $id})")
                    .WithParam("id", f.MaHoa)
                    .Set("flower.name = $name")
                    .Set("flower.price = $price")
                    .Set("flower.description = $description")
                    .Set("flower.image = $image")
                    .Set("flower.unit = $unit")
                    .Set("flower.color = $color")
                    .Set("flower.categoryId = $categoryId")
                    .Set("flower.departmentId = $departmentId")
                    .WithParam("name", f.TenHoa)
                    .WithParam("price", f.GiaBan)
                    .WithParam("description", f.MoTa)
                    .WithParam("image", f.AnhDaiDien)
                    .WithParam("unit", f.DonViTinh)
                    .WithParam("color", f.MauSacChuDao)
                    .WithParam("categoryId", f.MaLoaiChinh)
                    .WithParam("departmentId", f.MaDM)
                    .ExecuteWithoutResultsAsync();
            }

            return Content($"Import Flower Success - Tổng {flowers.Count} hoa được import");
        }
        public async Task<ActionResult> ImportOrders()
        {
            var neo4j = new Neo4jService();
            var client = await neo4j.GetClient();

            // Lấy tất cả đơn hàng TRỪ những đơn đã hủy
            var cancelledOrderIds = db.tblTinhTrang
                .Where(tt => tt.TinhTrangHoaDon.Contains("Đã hủy"))
                .Select(tt => tt.ID)
                .ToList();

            var data =
                (from hd in db.tblHoaDon
                 join ct in db.tblChiTietHoaDon on hd.MaHD equals ct.MaHD
                 where hd.MaKH != null && ct.MaHoa != null 
                    && !cancelledOrderIds.Contains(hd.TinhTrang ?? 0)
                 select new
                 {
                     CustomerId = hd.MaKH,
                     FlowerId = ct.MaHoa,
                     Quantity = ct.SoLuong,
                     UnitPrice = ct.GiaBan,
                     OrderDate = hd.NgayLap,
                     TotalAmount = hd.TongTien,
                     Status = hd.TinhTrang,
                     PaymentStatus = hd.DaThanhToan,
                     DeliveryAddress = hd.DiaChiGiaoHang
                 }).ToList();

            foreach (var item in data)
            {
                decimal totalItemAmount = (item.Quantity ?? 0) * (item.UnitPrice ?? 0);

                await client.Cypher
                .Match("(c:Customer)")
                .Match("(f:Flower)")
                .Where("c.id = $customerId")
                .AndWhere("f.id = $flowerId")
                .WithParam("customerId", item.CustomerId)
                .WithParam("flowerId", item.FlowerId)
                .WithParam("quantity", item.Quantity)
                .WithParam("unitPrice", item.UnitPrice)
                .WithParam("totalItemAmount", totalItemAmount)
                .WithParam("orderDate", item.OrderDate)
                .WithParam("totalAmount", item.TotalAmount)
                .WithParam("status", item.Status)
                .WithParam("paymentStatus", item.PaymentStatus)
                .WithParam("deliveryAddress", item.DeliveryAddress)
                .Merge("(c)-[r:BOUGHT]->(f)")
                .Set("r.quantity = COALESCE(r.quantity, 0) + $quantity")
                .Set("r.unitPrice = $unitPrice")
                .Set("r.totalAmount = COALESCE(r.totalAmount, 0) + $totalItemAmount")
                .Set("r.lastBought = $orderDate")
                .Set("r.status = $status")
                .Set("r.paymentStatus = $paymentStatus")
                .Set("r.deliveryAddress = $deliveryAddress")
                .ExecuteWithoutResultsAsync();
            }

            return Content($"Import Orders Success - Tổng {data.Count} đơn hàng (trừ đã hủy) được import");
        }
        public async Task<ActionResult> ImportCategories()
        {
            var neo4j = new Neo4jService();
            var client = await neo4j.GetClient();

            var categories = db.tblLoaiHoaChinh.ToList();

            foreach (var c in categories)
            {
                await client.Cypher
                    .Merge("(cat:Category {id:$id})")
                    .WithParam("id", c.MaLoaiChinh)
                    .Set("cat.name = $name")
                    .Set("cat.description = $description")
                    .WithParam("name", c.TenLoaiChinh)
                    .WithParam("description", c.MoTa)
                    .ExecuteWithoutResultsAsync();
            }

            return Content("Import Category Success");
        }
        public async Task<ActionResult> ImportFlowerCategory()
        {
            var neo4j = new Neo4jService();
            var client = await neo4j.GetClient();

            var data =
                from hoa in db.tblHoa
                select new
                {
                    FlowerId = hoa.MaHoa,
                    CategoryId = hoa.MaLoaiChinh
                };

            foreach (var item in data)
            {
                await client.Cypher
                    .Match("(f:Flower)")
                    .Match("(c:Category)")
                    .Where("f.id = $flowerId")
                    .AndWhere("c.id = $categoryId")
                    .WithParam("flowerId", item.FlowerId)
                    .WithParam("categoryId", item.CategoryId)
                    .Merge("(f)-[:BELONGS_TO]->(c)")
                    .ExecuteWithoutResultsAsync();
            }

            return Content("Import Flower Category Success");
        }
        public async Task<ActionResult> ImportReviews()
        {
            var neo4j = new Neo4jService();
            var client = await neo4j.GetClient();

            var reviews = mongoDb.BinhLuan_Reviews.Find(_ => true).ToList();

            foreach (var review in reviews)
            {
                await client.Cypher
                    .Match("(c:Customer)")
                    .Match("(f:Flower)")
                    .Where("c.name = $customerName")
                    .AndWhere("f.id = $flowerId")
                    .WithParam("customerName", review.HoTen)
                    .WithParam("flowerId", review.MaHoa)
                    .WithParam("rating", review.SoSao)
                    .WithParam("comment", review.NoiDung)
                    .WithParam("reviewDate", review.Ngay)
                    .Merge("(c)-[r:REVIEWED]->(f)")
                    .Set("r.rating = $rating")
                    .Set("r.comment = $comment")
                    .Set("r.reviewDate = $reviewDate")
                    .ExecuteWithoutResultsAsync();
            }

            return Content("Import Reviews Success");
        }
    }
}