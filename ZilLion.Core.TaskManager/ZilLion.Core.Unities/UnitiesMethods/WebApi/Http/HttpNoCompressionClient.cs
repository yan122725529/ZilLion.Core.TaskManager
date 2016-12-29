using System;
using System.Net.Http;
using System.Net.Http.Headers;
using ZilLion.Core.Unities.UnitiesMethods.WebApi.Entity;
using Newtonsoft.Json;

namespace ZilLion.Core.Unities.UnitiesMethods.WebApi.Http
{
    public class HttpNoCompressionClient : IHttpClient
    {
        public T HttpGet<T>(string url, RequestIdentity requestIdentity)  
        {
            try
            {
                Func<HttpContent, string> stringFunc = hc =>
                {
                    var responsejson = hc.ReadAsStringAsync().Result;
                    return responsejson;
                };
                return HttpHelper.HttpRequestGet<T>(url,  HttpHelper.HttpHeaderHandler(requestIdentity.ClientName, requestIdentity.ClientVersion, requestIdentity.RandomCode,
                        requestIdentity.ApiVersion), stringFunc);
            }
            catch (Exception exception)
            {
                throw;
            }
            return default(T);
        }

        public T HttpPost<P, T>(P param, string url, RequestIdentity requestIdentity)  
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
                    var responsejson = hc.ReadAsStringAsync().Result;
                    return responsejson;
                };
                return HttpHelper.HttpRequestPost<T>(url,  HttpHelper.HttpHeaderHandler(requestIdentity.ClientName, requestIdentity.ClientVersion, requestIdentity.RandomCode,
                        requestIdentity.ApiVersion), httpContent, stringFunc);
            }
            catch (Exception exception)
            {
                //var Message = exception.Message;
                throw;
            }
            return default(T);
        }
    }
}