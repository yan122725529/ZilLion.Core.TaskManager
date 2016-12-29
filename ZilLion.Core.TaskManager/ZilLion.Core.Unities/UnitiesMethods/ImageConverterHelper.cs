using System;
using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;

namespace ZilLion.Core.Unities.UnitiesMethods
{
    public static class ImageConverterHelper
    {
        /// <summary>
        /// 读取二级制流并转为BitmapImage
        /// </summary>
        /// <param name="byteArray"></param>
        /// <returns></returns>

        public static BitmapImage ByteArrayToBitmapImage(byte[] byteArray)
        {
            BitmapImage bmp = null;
            try
            {
                bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.StreamSource = new MemoryStream(byteArray);
                bmp.EndInit();
            }
            catch
            {
                bmp = null;
            }
            return bmp;
        }

        /// <summary>
        /// 转换BitmapImage为二进制流
        /// </summary>
        /// <param name="bmp"></param>
        /// <returns></returns>
        public static byte[] BitmapImageToByteArray(BitmapImage bmp)
        {
            byte[] byteArray = null;
            try
            {
                var sMarket = bmp.StreamSource;
                if (sMarket != null && sMarket.Length > 0)
                {
                    //很重要，因为Position经常位于Stream的末尾，导致下面读取到的长度为0。
                    sMarket.Position = 0;
                    using (var br = new BinaryReader(sMarket))
                    {
                        byteArray = br.ReadBytes((int) sMarket.Length);
                    }
                }
            }
            catch
            {
                //other exception handling
            }
            return byteArray;
        }

        /// <summary>
        /// BitmapSourceToBitmap
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public static Bitmap BitmapSourceToBitmap(BitmapSource image)
        {
            var encoder = new BmpBitmapEncoder();

            using (var memoryStream = new MemoryStream())
            {
                encoder.Frames.Add(BitmapFrame.Create(image));
                encoder.Save(memoryStream);
                memoryStream.Position = 0;
                var bImg = new Bitmap(memoryStream);
                memoryStream.Close();
                return bImg;
            }
        }


        /// <summary>
        /// BitmapToIcon
        /// </summary>
        /// <param name="bmp"></param>
        /// <returns></returns>
        public static Icon BitmapToIcon(Bitmap bmp)
        {
            // Create a Bitmap object from an image file.
            // Get an Hicon for myBitmap. 
            var hicon = bmp.GetHicon();
            // Create a new icon from the handle. 
            var newIcon = Icon.FromHandle(hicon);
            return newIcon;
        }
    }
}