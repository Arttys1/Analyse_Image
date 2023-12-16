using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace Analyse_Image.back
{
    public class Image
    {
        private Bitmap bitmap;
        private ImageType image;

        public Image(BitmapImage bitMapImage, ImageType image)
        {
            this.bitmap = BitmapImage2Bitmap(bitMapImage);
            this.image = image;
        }

        public Image(Bitmap bitmap, ImageType image)
        {
            this.bitmap = bitmap;
            this.image = image;
        }

        public BitmapImage GetBitMapImage()
        {
            return Bitmap2BitmapImage(bitmap);
        }

        public Image ToGrayScale()
        {
            Bitmap newBitmap = new Bitmap(bitmap.Width, bitmap.Height);
            using (Graphics g = Graphics.FromImage(newBitmap))
            {
                ColorMatrix colorMatrix = new ColorMatrix(
                [
                    [.3f, .3f, .3f, 0, 0],
                    [.59f, .59f, .59f, 0, 0],
                    [.11f, .11f, .11f, 0, 0],
                    [0, 0, 0, 1, 0],
                    [0, 0, 0, 0, 1]
                ]);

                using ImageAttributes attributes = new ImageAttributes();
                attributes.SetColorMatrix(colorMatrix);
                g.DrawImage(bitmap, new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                            0, 0, bitmap.Width, bitmap.Height, GraphicsUnit.Pixel, attributes);
            }
            return new Image(newBitmap, ImageType.GRAY);    
        }

        private Bitmap BitmapImage2Bitmap(BitmapImage bitmapImage)
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(outStream);

                return new Bitmap(bitmap);
            }
        }

        private BitmapImage Bitmap2BitmapImage(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                return bitmapImage;
            }
        }
    }
}
