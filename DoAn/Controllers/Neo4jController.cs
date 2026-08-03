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
                    .WithParam("name", c.TenKH)
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
                    .WithParam("name", f.TenHoa)
                    .ExecuteWithoutResultsAsync();
            }

            return Content("Import Flower Success");
        }
        public async Task<ActionResult> ImportOrders()
        {
            var neo4j = new Neo4jService();
            var client = await neo4j.GetClient();

            var data =
                from hd in db.tblHoaDon
                join ct in db.tblChiTietHoaDon
                on hd.MaHD equals ct.MaHD
                select new
                {
                    CustomerId = hd.MaKH,
                    FlowerId = ct.MaHoa
                };

            foreach (var item in data)
            {
                await client.Cypher
                .Match("(c:Customer)")
                .Match("(f:Flower)")
                .Where("c.id = $customerId")
                .AndWhere("f.id = $flowerId")
                .WithParam("customerId", item.CustomerId)
                .WithParam("flowerId", item.FlowerId)
                .Merge("(c)-[:BOUGHT]->(f)")
                .ExecuteWithoutResultsAsync();
            }

            return Content("Import Orders Success");
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
                    .WithParam("name", c.TenLoaiChinh)
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
                    .Merge("(c)-[r:REVIEWED]->(f)")
                    .Set("r.rating = $rating")
                    .WithParam("rating", review.SoSao)
                    .ExecuteWithoutResultsAsync();
            }

            return Content("Import Reviews Success");
        }
    }
}