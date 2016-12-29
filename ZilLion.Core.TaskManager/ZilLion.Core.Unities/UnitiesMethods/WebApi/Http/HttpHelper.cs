using System;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;

namespace ZilLion.Core.Unities.UnitiesMethods.WebApi.Http
{
    public class HttpHelper
    {
        /// <summary>
        ///     URL拼接
        /// </summary>
        /// <param name="randomCode"></param>
        /// <param name="version"></param>
        /// <param name="clientName"></param>
        /// <param name="clientVersion"></param>
        /// <returns></returns>
        public static Dictionary<string, string> HttpHeaderHandler(string clientName, string clientVersion,
            string randomCode, string version=null)
        {
            //version-->Action名字加Version=版本控制后的ACTION
            return new Dictionary<string, string>
            {
                {"ClientName", clientName},
                {"ClientVersion", clientVersion},
                {"pwd",  DigestHelper.Md5Converter(clientName + "@" + clientVersion + "@" + randomCode)},
                {"RandomCode", randomCode}
            };
        }

        /// <summary>
        ///     http get 请求
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="getStrFunc"></param>
        /// <param name="customeRequestHeaders"></param>
        /// <returns></returns>
        public static T HttpRequestGet<T>(string url, Dictionary<string, string> customeRequestHeaders,
            Func<HttpContent, string> getStrFunc
            ) 
        {
            using (var http = new HttpClient())
            {
                http.Timeout = new TimeSpan(0, 0, 0, 30);
                foreach (var herader in customeRequestHeaders)
                {
                    if (http.DefaultRequestHeaders.Contains(herader.Key))
                    {
                        http.DefaultRequestHeaders.Remove(herader.Key);
                    }


                    http.DefaultRequestHeaders.Add(herader.Key, herader.Value);
                }
                var result = http.GetAsync(url).Result;
                var deresponsejson = getStrFunc(result.Content);
                var lists = JsonConvert.DeserializeObject<T>(deresponsejson);
             
                return lists;
            }
        }

        /// <summary>
        ///     http Post 请求
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="httpContent"></param>
        /// <param name="getStrFunc"></param>
        /// <returns></returns>
        public static T HttpRequestPost<T>(string url, Dictionary<string, string> customeRequestHeaders,
            HttpContent httpContent,
            Func<HttpContent, string> getStrFunc)
           
        {
            using (var http = new HttpClient())
            {
                http.Timeout = new TimeSpan(0, 0, 0, 30);
                foreach (var herader in customeRequestHeaders)
                {
                    if (http.DefaultRequestHeaders.Contains(herader.Key))
                    {
                        http.DefaultRequestHeaders.Remove(herader.Key);
                    }


                    http.DefaultRequestHeaders.Add(herader.Key, herader.Value);
                }
                var result = http.PostAsync(url, httpContent).Result;
                var deresponsejson = getStrFunc(result.Content);
                var lists = JsonConvert.DeserializeObject<T>(deresponsejson);
              
                return lists;
            }
        }
    }
}