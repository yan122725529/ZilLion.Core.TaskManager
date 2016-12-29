using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ZilLion.Core.Unities.UnitiesMethods.Wpf
{
    public enum ImageType
    {
        Bmp,
        Gif,
        Jpeg,
        Png,
        Tiff,
        Wdp
    }

    internal static class ControlToImageConvertHelper
    {
        /// <summary>
        ///     Convert any control to a PngBitmapEncoder
        /// </summary>
        /// <param name="controlToConvert">The control to convert to an ImageSource</param>
        /// <param name="imageType">The image type will indicate the type of return bitmap encoder</param>
        /// <returns>The returned ImageSource of the controlToConvert</returns>
        private static BitmapEncoder GetImageFromControl(FrameworkElement controlToConvert, ImageType imageType)
        {
            RenderTargetBitmap renderBitmap;
            var bounds = controlToConvert.GetBounds(controlToConvert.Parent as Visual);
            if (bounds.IsEmpty)
                renderBitmap = new RenderTargetBitmap((int) controlToConvert.ActualWidth,
                    (int) controlToConvert.ActualHeight, 96d,
                    96d, PixelFormats.Pbgra32);
            else
                renderBitmap = new RenderTargetBitmap((int) bounds.Width, (int) bounds.Height, 96d,
                    96d, PixelFormats.Pbgra32);
            renderBitmap.Render(controlToConvert);
            var encoder = GetBitmapEncoderByImageType(imageType);
            // puch rendered bitmap into it
            encoder.Frames.Add(BitmapFrame.Create(renderBitmap));
            return encoder;
        }

        /// <summary>
        ///     Get an encoder by a specify image type
        /// </summary>
        /// <param name="type">the image type</param>
        /// <returns>return an eccoder</returns>
        private static BitmapEncoder GetBitmapEncoderByImageType(ImageType type)
        {
            switch (type)
            {
                case ImageType.Bmp:
                    return new BmpBitmapEncoder();
                case ImageType.Gif:
                    return new GifBitmapEncoder();
                case ImageType.Jpeg:
                    return new JpegBitmapEncoder();
                case ImageType.Png:
                    return new PngBitmapEncoder();
                case ImageType.Tiff:
                    return new TiffBitmapEncoder();
                case ImageType.Wdp:
                    return new WmpBitmapEncoder();
                default:
                    return new PngBitmapEncoder();
            }
        }

        /// <summary>
        ///     Get the iamge type by image file name
        /// </summary>
        /// <param name="fileName">the file name of an image</param>
        /// <returns>the iamge type</returns>
        private static ImageType GetImageTypeByFileName(string fileName)
        {
            var returnType = ImageType.Png;

            var extension = Path.GetExtension(fileName);
            if (string.IsNullOrEmpty(extension)) return returnType;
            switch (extension.ToLower())
            {
                case ".bmp":
                    returnType = ImageType.Bmp;
                    break;
                case ".gif":
                    returnType = ImageType.Gif;
                    break;
                case ".jpeg":
                case ".jpg":
                case ".jpe":
                case "jfif":
                    returnType = ImageType.Jpeg;
                    break;
                case ".png":
                    returnType = ImageType.Png;
                    break;
                case ".tiff":
                case ".tif":
                    returnType = ImageType.Tiff;
                    break;
                case ".wdp":
                    returnType = ImageType.Wdp;
                    break;
                default:
                    returnType = ImageType.Png;
                    break;
            }

            return returnType;
        }

        /// <summary>
        ///     Get an ImageSource of a control
        /// </summary>
        /// <param name="controlToConvert">The control to convert to an ImageSource</param>
        /// <param name="imageType">the image type</param>
        /// <returns>The returned ImageSource of the controlToConvert</returns>
        public static BitmapSource GetImageOfControl(FrameworkElement controlToConvert, ImageType imageType)
        {
            // return first frame of image 
            var encoder = GetImageFromControl(controlToConvert, imageType);
            if ((encoder != null) && (encoder.Frames.Count > 0))
                return encoder.Frames[0];

            return new BitmapImage();
        }

        /// <summary>
        ///     Get an ImageSource of a control(Jpeg as default type)
        /// </summary>
        /// <param name="controlToConvert">The control to convert to an ImageSource</param>
        /// <returns>The returned ImageSource of the controlToConvert</returns>
        public static BitmapSource GetImageOfControl(FrameworkElement controlToConvert)
        {
            return GetImageOfControl(controlToConvert, ImageType.Png);
        }

        /// <summary>
        ///     Save an image of a control
        /// </summary>
        /// <param name="controlToConvert">The control to convert to an ImageSource</param>
        /// <param name="fileName">The location to save the image to</param>
        /// <returns>The returned ImageSource of the controlToConvert</returns>
        public static bool SaveImageOfControl(FrameworkElement controlToConvert, string fileName)
        {
            try
            {
                var imageType = GetImageTypeByFileName(fileName);

                using (var outStream = new FileStream(fileName, FileMode.Create))
                {
                    var encoder = GetImageFromControl(controlToConvert, imageType);
                    encoder.Save(outStream);
                }
            }
            catch (Exception e)
            {
#if DEBUG
                Console.WriteLine("Exception caught saving stream: {0}", e.Message);
#endif
                return false;
            }

            return true;
        }
    }
}