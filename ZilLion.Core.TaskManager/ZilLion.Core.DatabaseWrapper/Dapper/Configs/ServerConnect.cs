using System;
using System.Text;
using System.Threading;

namespace ZilLion.Core.DatabaseWrapper.Dapper.Configs
{
    public class ServerConnect
    {
        //服务器返回的消息
        private static string _msResponse;
        //【消息请求】操作是否成功
        private static bool _mblnResponseSuccess;
        //【消息请求】操作是否已完成
        private static bool _mblnResponseDone;

        public static void Sendmsg(string sRequest, bool bNeedResponse, out string sResponse)
        {
            sResponse = "";
            if (sRequest == "") return;
            var sSends = sRequest.Split('\t');
            _mblnResponseSuccess = false;
            _mblnResponseDone = false;

            var bs = Encoding.Default.GetBytes("\0" + sRequest + "\0");
            ServerConfig.SocketVm.Send(bs, bs.Length, 0);
            if (bNeedResponse == false) return;
            ServerConfig.SocketVm.ReceiveTimeout = 5000;
            while (_mblnResponseDone == false)
            {
                Receivemsg();
                Thread.Sleep(50);
                //i++;
            }
            if (_mblnResponseSuccess)
            {
                if (_msResponse != "")
                {
                    var sRets = _msResponse.Split('\t');
                    if (sRets[0] == "RES" + sSends[0])
                    {
                        sResponse = _msResponse;
                    }
                }
            }
        }

        private static void Receivemsg()
        {
            string outmsg;
            _msResponse = "";
            var recvStr = "";
            var recvBytes = new byte[1024];
            int bytes = ServerConfig.SocketVm.Receive(recvBytes, recvBytes.Length, 0);
            recvStr += Encoding.Default.GetString(recvBytes, 0, bytes);
            ReProc:
            Getmsg(recvStr, out outmsg, out recvStr);
            if (outmsg != "")
            {
                var str = outmsg.Split('\t');
                switch (str[0])
                {
                    case "RESCONNECT":
                    case "RESLOGINEX":
                    case "RESSINGLEUSER":
                    case "RESREADSETTINGS":
                        _msResponse = outmsg;
                        _mblnResponseSuccess = true;
                        _mblnResponseDone = true;
                        break;
                    case "SERVERTEST":
                        break;
                }
            }

            if (recvStr != "")
            {
                goto ReProc;
            }
        }

        private static void Getmsg(string str, out string strmsg, out string strRemain)
        {
            strmsg = "";
            strRemain = "";
            if (str.Length == 0) return;
            var msstart = str.IndexOf("\0", 0, StringComparison.Ordinal);
            if (msstart != -1)
            {
                var mend = str.IndexOf("\0", msstart + 1, StringComparison.Ordinal);
                if (mend != 0)
                {
                    strmsg = str.Substring(msstart + 1, mend - msstart - 1);
                    strRemain = str.Substring(mend + 1);
                }
            }
        }
    }
}