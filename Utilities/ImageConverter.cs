/*
 * Created by Rajan Tawate.
 * User: Owner
 * Date: 9/3/2006
 * Time: 8:00 PM
 */

using System;
using System.Drawing;
using System.IO;
using System.Collections;
using System.Windows.Media.Imaging;

/// <summary>
/// Description of ImageConverter.
/// </summary>
public static class ImageConverter
{

    public static byte[] imageToByteArray(System.Drawing.Image imageIn)
    {
        MemoryStream ms = new MemoryStream();
        imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
        return ms.ToArray();
    }

    public static Image byteArrayToImage(byte[] byteArrayIn)
    {
        MemoryStream ms = new MemoryStream(byteArrayIn);
        Image returnImage = Image.FromStream(ms);
        return returnImage;
    }

    public static BitmapImage BitmapToImageSource(System.Drawing.Image bitmap)
    {
        using (MemoryStream ms = new MemoryStream())
        {
            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            ms.Position = 0;
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = ms;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();

            return bitmapImage;
        }
    }

    public static Bitmap ImageSourceToBitmap(BitmapSource bitmapSource)
    {
        using (MemoryStream ms = new MemoryStream())
        {
            BitmapEncoder enc = new PngBitmapEncoder();
            enc.Frames.Add(BitmapFrame.Create(bitmapSource));
            enc.Save(ms);

            using (Bitmap tempBitmap = new Bitmap(ms))
            {
                 return new Bitmap(tempBitmap);
            }
        }
    }
}

