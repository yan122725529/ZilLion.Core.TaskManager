namespace ZilLion.Core.DatabaseWrapper.MongoDB
{
    public class MongoDbSetting
    {
        private static string _connectionString;

        public static string ConnectionString
        {
            get { return _connectionString; }
           
        }

        public static void SetConnectionString(string connectionString)
        {
            _connectionString = connectionString;
        }
    }
}