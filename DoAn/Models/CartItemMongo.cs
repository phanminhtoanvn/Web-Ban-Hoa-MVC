using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace DoAn.Models
{
    public class CartItemMongo
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        // Dùng để định danh giỏ hàng của từng khách hàng (nếu họ chưa đăng nhập)
        public string SessionId { get; set; }

        public int MaHoa { get; set; }

        public int SoLuong { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime NgayCapNhat { get; set; }
    }
}