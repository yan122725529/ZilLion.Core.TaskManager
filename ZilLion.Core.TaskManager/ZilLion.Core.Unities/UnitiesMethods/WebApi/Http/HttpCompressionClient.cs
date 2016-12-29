using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Ionic.Zlib;
using ZilLion.Core.Unities.UnitiesMethods.WebApi.Entity;
using Newtonsoft.Json;

namespace ZilLion.Core.Unities.UnitiesMethods.WebApi.Http
{
    public class HttpCompressionClient : IHttpClient
    {
        public T HttpGet<T>(string url, RequestIdentity requestIdentity)
        {
            try
            {
                Func<HttpContent, string> stringFunc = hc =>
                {
                    var responsejson = hc.ReadAsByteArrayAsync().Result;
                    var str = DeflateStream.UncompressBuffer(responsejson);
                    var deresponsejson = Encoding.UTF8.GetString(str);
                    return deresponsejson;
                };
                return HttpHelper.HttpRequestGet<T>(url,
                    HttpHelper.HttpHeaderHandler(requestIdentity.ClientName, requestIdentity.ClientVersion,
                        requestIdentity.RandomCode,
                        requestIdentity.ApiVersion), stringFunc);
            }
            catch (Exception exception)
            {
                //var message = exception.Message;
                throw;
            }
            return default(T);
        }

        public T HttpPost<TP, T>(TP param, string url, RequestIdentity requestIdentity)
        {
            try
            {
                HttpContent httpContent = null;
                if (param != null)
                {
                    var requestjson = JsonConvert.SerializeObject(param);

                    httpContent = new StringContent(requestjson);
                    httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                }
                Func<HttpContent, string> stringFunc = hc =>
                {
                    var responsejson = hc.ReadAsByteArrayAsync().Result;
                    var str = DeflateStream.UncompressBuffer(responsejson);
                    var deresponsejson = Encoding.UTF8.GetString(str);
                    return deresponsejson;
                };
                return HttpHelper.HttpRequestPost<T>(url,
                    HttpHelper.HttpHeaderHandler(requestIdentity.ClientName, requestIdentity.ClientVersion,
                        requestIdentity.RandomCode,
                        requestIdentity.ApiVersion), httpContent, stringFunc);
            }
            catch (Exception exception)
            {
                //var message = exception.Message;
                throw;
            }
            return default(T);
        }
    }
}