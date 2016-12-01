using System;
using System.IO;
using System.Text;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;

namespace ZilLion.Core.DatabaseWrapper.MongoDB
{
    public class MongoFileDb
    {
        private const string Dbname = "Z9Report";
       
        private  string _connectionString;
        public static MongoFileDb CreateMongoDb()
        {
            return new MongoFileDb();
        }

        private MongoServer _msever { get; set; }
        public  MongoFileDb()
        {
            _connectionString = string.Format("mongodb://{0}/?safe=true", MongoDbSetting.ConnectionString);
            _msever = new MongoClient(_connectionString).GetServer();
        }

       

        /// <summary>
        /// 保存报表结果
        /// </summary>
        /// <param name="savsString"></param>
        /// <param name="rootName"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public void SaveFileToMongoDb(string fileName,string savsString, string rootName)
        {
            //BsonDocument doc = new BsonDocument();
            //doc.Add("UserID", 1L);
            //option.Metadata = doc;
            MongoGridFS gf = GetGridFs(Dbname, rootName);
            using (MongoGridFSStream gfs = gf.Create(fileName, new MongoGridFSCreateOptions() { UploadDate = DateTime.Now }))
            {
                byte[] bytes = Encoding.UTF8.GetBytes(savsString);
                gfs.Write(bytes, 0, bytes.Length);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="databasename"></param>
        /// <param name="rootName"></param>
        /// <returns></returns>
        private MongoGridFS GetGridFs(string databasename,string rootName)
        {
            MongoDatabase dbName = _msever.GetDatabase(databasename);
            MongoGridFSSettings fsSetting = new MongoGridFSSettings() { Root = rootName };
            MongoGridFS gf = new MongoGridFS(dbName, fsSetting);
            return gf;
        }

        /// <summary>
        /// 删除报表结果
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="rootName"></param>
        public void DeleteFile(string fileName, string rootName)
        {
            MongoGridFS fs = GetGridFs(Dbname, rootName);
            fs.Delete(fileName);
        }

        /// <summary>
        /// 读取报表结果
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="rootName"></param>
        /// <returns></returns>
        public  string ReadFileFromMongoDb(string filename, string rootName)
        {
            string tablestr;
            MongoGridFS fs = GetGridFs(Dbname, rootName);
            MongoGridFSFileInfo gfInfo = new MongoGridFSFileInfo(fs, filename);
            using (MongoGridFSStream gfs = gfInfo.OpenRead())
            {
                using (TextReader reader = new StreamReader(gfs, Encoding.UTF8))
                {
                    tablestr = reader.ReadToEnd();
                }
            }
            return tablestr;
        }
    }

   
}
