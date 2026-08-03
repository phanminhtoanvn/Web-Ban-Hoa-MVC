using DoAn.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace DoAn.Data
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext()
        {
            // Đọc chuỗi kết nối từ Web.config mà bạn vừa thêm trong hình
            var connectionString = ConfigurationManager.ConnectionStrings["MongoDbConn"].ConnectionString;

            // Khởi tạo Client
            var client = new MongoClient(connectionString);

            // Tạo (hoặc gọi) database có tên QL_BanHoa_NoSQL
            // Bạn có thể đổi tên DB này tùy ý, MongoDB sẽ tự động tạo nếu chưa có
            _database = client.GetDatabase("QL_BanHoa_NoSQL");
        }

        // Khai báo Collection Bình luận
        public IMongoCollection<BinhLuanMongo> BinhLuan_Reviews
        {
            get { return _database.GetCollection<BinhLuanMongo>("BinhLuan_Reviews"); }
        }
       
        public IMongoCollection<CamNangHoaMongo> CamNangHoa_Blogs
        {
            get { return _database.GetCollection<CamNangHoaMongo>("CamNangHoa_Blogs"); }
        }
        public IMongoCollection<HoaYeuThichMongo> HoaYeuThich_Wishlists
        {
            get { return _database.GetCollection<HoaYeuThichMongo>("HoaYeuThich_Wishlists"); }
        }
    }
}