using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Web.Mvc; // Cần dùng cho AllowHtml

namespace DoAn.Models
{
    public class CamNangHoaMongo
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string TieuDe { get; set; }

        [AllowHtml] // Cho phép submit thẻ HTML từ trình soạn thảo (Rich Text Editor)
        public string NoiDung_HTML { get; set; }

        public string TacGia { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime NgayDang { get; set; }

        public int LuotXem { get; set; }

        // Khởi tạo List rỗng để tránh lỗi null khi thêm mới
        public List<string> Tags { get; set; } = new List<string>();

        // Thuộc tính phụ trợ để xử lý việc nhập Tags từ Input Form (cách nhau bằng dấu phẩy)
        [BsonIgnore] // Không lưu trường này vào DB
        public string TagsInput { get; set; }
    }
}