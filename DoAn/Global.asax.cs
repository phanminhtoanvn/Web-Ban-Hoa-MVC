//---Đã sửa đổi---
using System;
using Cassandra;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace DoAn
{
    public class MvcApplication : System.Web.HttpApplication
    {
        // Cassandra cluster and session (optional). Initialized at application start if Cassandra is available.
        public static ICluster CassandraCluster { get; private set; }
        public static ISession CassandraSession { get; private set; }
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            GlobalConfiguration.Configure(WebApiConfig.Register);
            // Initialize Cassandra connection in background. Failures must not stop the web app.
            try
            {
                // Attempt to connect to local Cassandra (127.0.0.1:9042)
                CassandraCluster = Cluster.Builder()
                    .AddContactPoint("127.0.0.1")
                    .WithPort(9042)
                    .Build();

                // Connect without keyspace so schema can be qualified in queries.
                CassandraSession = CassandraCluster.Connect();
            }
            catch (Exception)
            {
                // Swallow all exceptions to ensure the web application remains available
                CassandraCluster = null;
                CassandraSession = null;
            }
        }
    }
}
