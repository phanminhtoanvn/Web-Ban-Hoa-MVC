using DoAn.Data;
using DoAn.Models;
using DoAn.Services;
using MongoDB.Driver;
using Neo4jClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace DoAn.Controllers
{
    public class YeuThichController : Controller
    {
        private MongoDbContext mongoDb = new MongoDbContext();
        private QL_BanHoaEntities2 db = new QL_BanHoaEntities2();

        [HttpPost]
        public async Task<ActionResult> ToggleYeuThich(int maHoa)
        {
            // Kiểm tra đăng nhập
            if (Session["MaKH"] == null)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập" });
            }

            int maKH = (int)Session["MaKH"];
            var wishlist = mongoDb.HoaYeuThich_Wishlists.Find(x => x.MaKH == maKH).FirstOrDefault();
            var neo4j = new Neo4jService();
            var client = await neo4j.GetClient();

            if (wishlist == null)
            {
                // Nếu khách hàng chưa có wishlist, tạo mới
                wishlist = new HoaYeuThichMongo { MaKH = maKH };
                wishlist.DanhSachMaHoa.Add(maHoa);
                mongoDb.HoaYeuThich_Wishlists.InsertOne(wishlist);

                await client.Cypher
                    .Match("(c:Customer)")
                    .Match("(f:Flower)")
                    .Where("c.id = $customerId")
                    .AndWhere("f.id = $flowerId")
                    .WithParam("customerId", maKH)
                    .WithParam("flowerId", maHoa)
                    .Merge("(c)-[r:LIKED]->(f)")
                    .Set("r.liked = true")
                    .Set("r.addedAt = datetime()")
                    .ExecuteWithoutResultsAsync();

                return Json(new { success = true, liked = true });
            }
            else
            {
                // Nếu đã có wishlist, kiểm tra xem hoa đã nằm trong mảng chưa
                if (wishlist.DanhSachMaHoa.Contains(maHoa))
                {
                    // Nếu có rồi -> Bỏ thích (Xóa khỏi mảng)
                    var update = Builders<HoaYeuThichMongo>.Update.Pull(x => x.DanhSachMaHoa, maHoa);
                    mongoDb.HoaYeuThich_Wishlists.UpdateOne(x => x.Id == wishlist.Id, update);

                    await client.Cypher
                        .Match("(c:Customer)-[r:LIKED]->(f:Flower)")
                        .Where("c.id = $customerId")
                        .AndWhere("f.id = $flowerId")
                        .WithParam("customerId", maKH)
                        .WithParam("flowerId", maHoa)
                        .Delete("r")
                        .ExecuteWithoutResultsAsync();

                    return Json(new { success = true, liked = false });
                }
                else
                {
                    // Nếu chưa có -> Thêm vào yêu thích (Push vào mảng)
                    var update = Builders<HoaYeuThichMongo>.Update.Push(x => x.DanhSachMaHoa, maHoa);
                    mongoDb.HoaYeuThich_Wishlists.UpdateOne(x => x.Id == wishlist.Id, update);

                    await client.Cypher
                        .Match("(c:Customer)")
                        .Match("(f:Flower)")
                        .Where("c.id = $customerId")
                        .AndWhere("f.id = $flowerId")
                        .WithParam("customerId", maKH)
                        .WithParam("flowerId", maHoa)
                        .Merge("(c)-[r:LIKED]->(f)")
                        .Set("r.liked = true")
                        .Set("r.addedAt = datetime()")
                        .ExecuteWithoutResultsAsync();

                    return Json(new { success = true, liked = true });
                }
            }
        }
    }
}