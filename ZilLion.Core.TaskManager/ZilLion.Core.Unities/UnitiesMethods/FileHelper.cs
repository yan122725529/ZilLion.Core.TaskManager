﻿using System;
using System.IO;
using System.Security.Cryptography;
using System.Web;

namespace ZilLion.Core.Unities.UnitiesMethods
{
    /// <summary>
    ///     文件相关助手类
    /// </summary>
    public class FileHelper
    {
        /// <summary>
        ///     获取文件的绝对路径,针对window程序和web程序都可使用
        /// </summary>
        /// <param name="relativePath">相对路径地址</param>
        /// <returns>绝对路径地址</returns>
        public static string GetAbsolutePath(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
                throw new ArgumentNullException(nameof(relativePath));
            relativePath = relativePath.Replace("/", "\\");
            if (relativePath[0] == '\\')
                relativePath = relativePath.Remove(0, 1);
            //判断是Web程序还是window程序
            return
                Path.Combine(
                    HttpContext.Current != null ? HttpRuntime.AppDomainAppPath : AppDomain.CurrentDomain.BaseDirectory,
                    relativePath);
        }

        /// <summary>
        ///     获取文件的绝对路径,针对window程序和web程序都可使用
        /// </summary>
        /// <returns>绝对路径地址</returns>
        public static string GetRootPath()
        {
            //判断是Web程序还是window程序
            return HttpContext.Current != null ? HttpRuntime.AppDomainAppPath : AppDomain.CurrentDomain.BaseDirectory;
        }

        /// <summary>
        ///     通过文件Hash 比较两个文件内容是否相同
        /// </summary>
        /// <param name="filePath1">文件1地址</param>
        /// <param name="filePath2">文件2地址</param>
        /// <returns></returns>
        public static bool IsValidFileContent(string filePath1, string filePath2)
        {
            //创建一个哈希算法对象
            using (var hash = HashAlgorithm.Create())
            {
                using (
                    FileStream file1 = new FileStream(filePath1, FileMode.Open),
                        file2 = new FileStream(filePath2, FileMode.Open))
                {
                    var hashByte1 = hash.ComputeHash(file1); //哈希算法根据文本得到哈希码的字节数组
                    var hashByte2 = hash.ComputeHash(file2);
                    var str1 = BitConverter.ToString(hashByte1); //将字节数组装换为字符串
                    var str2 = BitConverter.ToString(hashByte2);
                    return str1 == str2; //比较哈希码
                }
            }
        }

        /// <summary>
        ///     计算文件的hash值 用于比较两个文件是否相同
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>文件hash值</returns>
        public static string GetFileHash(string filePath)
        {
            //创建一个哈希算法对象
            using (var hash = HashAlgorithm.Create())
            {
                using (var file = new FileStream(filePath, FileMode.Open))
                {
                    //哈希算法根据文本得到哈希码的字节数组
                    var hashByte = hash.ComputeHash(file);
                    //将字节数组装换为字符串
                    return BitConverter.ToString(hashByte);
                }
            }
        }

        //读取
        public static byte[] FileContent(string fileName)
        {
            var fileinfo = new FileInfo(fileName);
            return fileinfo.ReadAllBytes();
        }
    }
}