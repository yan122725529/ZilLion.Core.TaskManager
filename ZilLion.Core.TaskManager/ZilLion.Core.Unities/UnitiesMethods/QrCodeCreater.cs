using System;
using System.Drawing;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ZXing;
using ZXing.QrCode.Internal;

namespace ZilLion.Core.Unities.UnitiesMethods
{
    public static class QrCodeCreater
    {
        #region 生成二维码

        /// <summary>
        ///     从bitmap转换成ImageSource
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public static ImageSource ChangeBitmapToImageSource(Bitmap bitmap)
        {
            var hBitmap = bitmap.GetHbitmap();
            ImageSource wpfBitmap = Imaging.CreateBitmapSourceFromHBitmap(
                hBitmap,
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
            return wpfBitmap;
        }


        /// <summary>
        ///     生成二维码
        /// </summary>
        /// <param name="msg">二维码信息</param>
        /// <returns>图片</returns>
        public static Bitmap GenByZXingNet(string msg)
        {
            var writer = new BarcodeWriter {Format = BarcodeFormat.QR_CODE};
            writer.Options.Hints.Add(EncodeHintType.CHARACTER_SET, "UTF-8"); //编码问题
            writer.Options.Hints.Add(
                EncodeHintType.ERROR_CORRECTION,
                ErrorCorrectionLevel.H
            );
            const int codeSizeInPixels = 250; //设置图片长宽
            writer.Options.Height = writer.Options.Width = codeSizeInPixels;
            writer.Options.Margin = 0; //设置边框
            var bm = writer.Encode(msg);
            var img = writer.Write(bm);
            return img;
        }

        #endregion
    }
}