using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace ZilLion.Core.DatabaseWrapper.Dapper.Configs
{
    public  class ServerConfig
    {
        static ServerConfig()
        {
            ComputeName = Environment.MachineName.ToLower();
            foreach (var ip in Dns.GetHostEntry(Dns.GetHostName()).AddressList.Where(ip => ip.AddressFamily.ToString() == "InterNetwork"))
            {
                ComputerIp = ip.ToString();
            }
            // ComputerIp = "192.168.5.102";
        }

        //系统编号
        public static string AppTag;                                                                                                                                         
        //端口号
        public static int McServerPort ;
        //软件名称
        public static string AppName ;
        //客户端编号
        public static string ClientType;
        //本机IP
        public static string ComputerIp;
        //本计算机名称
        public static string ComputeName;
        //选取的帐套名称
        public static string Accname;
        //服务器的IP
        public static IPAddress ServerIp;
        //登录人id
        public static int Dlrid;
        //登录人名称
        public static string Dlrmc;
        //登录名
        public static string Dlm;
        //socket连接
        public static Socket SocketVm;

    }
}
