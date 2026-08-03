using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace DoAn.Models
{
    public class HoaYeuThichMongo
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        // Khóa ngoại liên kết với MaKH trong SQL Server
        public int MaKH { get; set; }

        // Tận dụng cấu trúc mảng của MongoDB để lưu tất cả mã hoa khách hàng đã thích
        public List<int> DanhSachMaHoa { get; set; } = new List<int>();
    }
}