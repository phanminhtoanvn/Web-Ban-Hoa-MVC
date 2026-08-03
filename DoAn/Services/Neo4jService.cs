using Neo4jClient;
using System;
using System.Threading.Tasks;

namespace DoAn.Services
{
    public class Neo4jService
    {
        private readonly BoltGraphClient _client;

        public Neo4jService()
        {
            _client = new BoltGraphClient(
                new Uri("bolt://127.0.0.1:7687"),
                "neo4j",
                "leduy205");
        }

        public async Task<BoltGraphClient> GetClient()
        {
            if (!_client.IsConnected)
            {
                await _client.ConnectAsync();
            }

            return _client;
        }
    }
}