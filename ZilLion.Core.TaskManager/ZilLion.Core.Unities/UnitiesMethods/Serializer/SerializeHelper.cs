using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace ZilLion.Core.Unities.UnitiesMethods.Serializer
{
    public class SerializeHelper
    {
        /// <summary>
        ///     序列化对象 并返回xml字符串
        /// </summary>
        /// <returns></returns>
        public static string SerializerToXml<T>(T serializerObject)
        {
            var xmlSerializer = new XmlSerializer(serializerObject.GetType());

            using (var stringWriter = new StringWriter())
            {
                xmlSerializer.Serialize(stringWriter, serializerObject);
                using (var streamReader = new StringReader(stringWriter.GetStringBuilder().ToString()))
                {
                    return streamReader.ReadToEnd();
                }
            }
        }

        /// <summary>
        ///     把对象序列化成json字符串
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serializerObject"></param>
        /// <returns></returns>
        public static string SerializerToJson<T>(T serializerObject)
        {
            return JsonConvert.SerializeObject(serializerObject);
        }

        /// <summary>
        ///     通过json反序列化成对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jsonString"></param>
        /// <returns></returns>
        public static T DeserializeWithJson<T>(string jsonString)
        {
            return JsonConvert.DeserializeObject<T>(jsonString);
        }

        /// <summary>
        ///     把xml字符反序列化成对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="xmlString"></param>
        /// <returns></returns>
        public static T DeserializeWithXml<T>(string xmlString) where T : new()
        {
            if (string.IsNullOrEmpty(xmlString))
                return new T();
            try
            {
                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(xmlString)))
                {
                    var xmlSerializer = new XmlSerializer(typeof (T));
                    return (T) xmlSerializer.Deserialize(stream);
                }
            }
            catch (Exception ex)
            {
                return new T();
            }
        }

        /// <summary>
        ///     二进制字符串反序列化成对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="deserializestring"></param>
        /// <returns></returns>
        public static T DeserializeBinary<T>(string deserializestring)
        {
            using (var stream = new MemoryStream(Encoding.Default.GetBytes(deserializestring)))
            {
                var binaryRead = new BinaryFormatter();
                return (T) binaryRead.Deserialize(stream);
            }
        }
        /// <summary>
        /// 将对象序列化成二进制字符串
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="deserializeObject"></param>
        /// <returns></returns>
        public static string SerializeBinary<T>(T deserializeObject)
        {
            var binaryWrite = new BinaryFormatter();

            using (var memoryStream = new MemoryStream())
            {
                binaryWrite.Serialize(memoryStream, deserializeObject);
                return Encoding.Default.GetString(memoryStream.ToArray());
            }
        }

        /// <summary>
        /// Json 转Json
        /// </summary>
        /// <param name="strIJson"></param>
        /// <param name="deserializeRootElementName"></param>
        /// <returns></returns>
        public static string JsonToXml(string strIJson,string deserializeRootElementName="Root")
        {
            if (string.IsNullOrEmpty(strIJson))
                return string.Empty;

            XmlDocument pXml = Newtonsoft.Json.JsonConvert.DeserializeXmlNode(strIJson, deserializeRootElementName);
            return pXml.OuterXml;
        }

     
    }
}