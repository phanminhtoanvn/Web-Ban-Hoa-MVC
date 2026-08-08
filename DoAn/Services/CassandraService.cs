using System;
using System.Collections.Generic;
using System.Linq;
using Cassandra;

namespace DoAn.Services
{
    /// <summary>
    /// Simple helper to read user events from Cassandra.
    /// Uses the existing MvcApplication.CassandraSession if no session is provided.
    /// </summary>
    public class CassandraService
    {
        private readonly ISession _session;

        public CassandraService(ISession session = null)
        {
            _session = session ?? DoAn.MvcApplication.CassandraSession;
        }

        /// <summary>
        /// Returns up to 'limit' most recent product ids for the given user from
        /// web_ban_hoa.user_events (CQL: SELECT product_id FROM web_ban_hoa.user_events WHERE user_id = ? LIMIT <limit>)
        /// </summary>
        public List<int> GetRecentlyViewedProductIds(int userId, int limit = 10)
        {
            var result = new List<int>();
            if (_session == null) return result;

            try
            {
                // Insert limit value directly because binding LIMIT may not be supported in some Cassandra versions
                var cql = $"SELECT product_id FROM web_ban_hoa.user_events WHERE user_id = ? LIMIT {limit}";
                var statement = new SimpleStatement(cql, userId);
                var rs = _session.Execute(statement);

                foreach (var row in rs)
                {
                    if (!row.IsNull("product_id"))
                    {
                        try
                        {
                            result.Add(row.GetValue<int>("product_id"));
                        }
                        catch { /* ignore conversion errors */ }
                    }
                }
            }
            catch
            {
                // Swallow exceptions to avoid breaking page rendering.
            }

            return result;
        }

        /// <summary>
        /// Đếm tổng số event của user trong bảng user_events.
        /// Trả về 0 nếu session null hoặc xảy ra lỗi.
        /// </summary>
        public int CountUserEvents(int userId)
        {
            if (_session == null) return 0;
            try
            {
                // COUNT(*) trên partition user_id (nếu schema cho phép)
                var cql = "SELECT count(*) FROM web_ban_hoa.user_events WHERE user_id = ?";
                var stmt = new SimpleStatement(cql, userId);
                var row = _session.Execute(stmt).FirstOrDefault();
                if (row != null && !row.IsNull(0))
                {
                    // count(*) trả về long
                    var cnt = row.GetValue<long>(0);
                    return (int)cnt;
                }
            }
            catch
            {
                // swallow errors and return 0 so UI will hide recommendations
            }
            return 0;
        }

        /// <summary>
        /// Lấy các product_id phổ biến nhất (theo tần suất xuất hiện) trong một số lượng mẫu recent events.
        /// Thực hiện SELECT product_id ... LIMIT maxCandidates rồi thống kê phía client.
        /// Trả về danh sách product_id đã được sắp xếp giảm dần theo tần suất, tối đa topN phần tử.
        /// </summary>
        public List<int> GetTopProductIds(int userId, int maxCandidates = 20, int topN = 4)
        {
            var result = new List<int>();
            if (_session == null) return result;
            try
            {
                var cql = $"SELECT product_id FROM web_ban_hoa.user_events WHERE user_id = ? LIMIT {maxCandidates}";
                var stmt = new SimpleStatement(cql, userId);
                var rows = _session.Execute(stmt);

                var freq = new Dictionary<int, int>();
                foreach (var r in rows)
                {
                    if (!r.IsNull("product_id"))
                    {
                        try
                        {
                            var pid = r.GetValue<int>("product_id");
                            if (freq.ContainsKey(pid)) freq[pid]++; else freq[pid] = 1;
                        }
                        catch { }
                    }
                }

                // Lấy các product_id theo tần suất giảm dần; nếu bằng tần suất, ưu tiên thứ tự xuất hiện không đảm bảo
                result = freq.OrderByDescending(kv => kv.Value).ThenByDescending(kv => kv.Key)
                             .Take(topN).Select(kv => kv.Key).ToList();
            }
            catch
            {
                // swallow errors
            }
            return result;
        }
        // Tự động khởi tạo bảng lưu Nhật ký thao tác trong Cassandra
        public void InitAuditLogTable()
        {
            if (_session == null) return;
            try
            {
                string cql = @"
            CREATE TABLE IF NOT EXISTS web_ban_hoa.admin_audit_logs (
                log_id timeuuid,
                admin_name text,
                action_type text,
                target_table text,
                target_id int,
                description text,
                old_data text,
                created_at timestamp,
                PRIMARY KEY (admin_name, log_id)
            ) WITH CLUSTERING ORDER BY (log_id DESC);";

                _session.Execute(cql);
            }
            catch { }
        }

        // Hàm ghi log thao tác ngầm
        public void LogAdminAction(string adminName, string actionType, string targetTable, int targetId, string description, object oldDataObject)
        {
            string oldDataJson = "";

            if (oldDataObject != null)
            {
                // 1. Nếu truyền vào là chuỗi string
                if (oldDataObject is string strData)
                {
                    oldDataJson = strData.Trim();

                    // FIX LỖI DƯ DẤU: Nếu lỡ bị Serialize 2 lần thành chuỗi dính ngoặc kép ngoặc ngoài dạng "\"{\\\"MaHD\\\":...}\""
                    if (oldDataJson.StartsWith("\"") && oldDataJson.EndsWith("\""))
                    {
                        try
                        {
                            // Giải mã 1 lớp ngoặc kép thừa ra
                            oldDataJson = Newtonsoft.Json.JsonConvert.DeserializeObject<string>(oldDataJson);
                        }
                        catch { }
                    }
                }
                // 2. Nếu truyền vào là Object/Model thực sự
                else
                {
                    var settings = new Newtonsoft.Json.JsonSerializerSettings
                    {
                        ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore,
                        NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore
                    };
                    oldDataJson = Newtonsoft.Json.JsonConvert.SerializeObject(oldDataObject, settings);
                }
            }

            // Lưu vào Cassandra
            var cql = "INSERT INTO web_ban_hoa.admin_audit_logs (log_id, admin_name, action_type, target_table, target_id, description, old_data, created_at) VALUES (?, ?, ?, ?, ?, ?, ?, ?)";
            var stmt = new SimpleStatement(cql, TimeUuid.NewId(), adminName, actionType, targetTable, targetId, description, oldDataJson, DateTimeOffset.Now);

            DoAn.MvcApplication.CassandraSession.Execute(stmt);
        }
    }
}
