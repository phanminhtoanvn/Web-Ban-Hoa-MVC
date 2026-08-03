using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace DoAn.Models
{
    public class BinhLuanMongo
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        // Khớp với kiểu int của MaHoa trong SQL Server
        public int MaHoa { get; set; }

        public string HoTen { get; set; }
        public string NoiDung { get; set; }
        public int SoSao { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime Ngay { get; set; }

        // Tận dụng thế mạnh của Document DB: mảng dữ liệu
        public List<string> HinhAnh { get; set; }
        public int Thich { get; set; }
    }
}