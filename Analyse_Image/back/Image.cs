﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;
using Color = System.Drawing.Color;

namespace Analyse_Image.back
{
    public class Image
    {
        private Bitmap bitmap;
        private ImageType imageType;
        public ImageType ImageType { get => imageType; set => imageType = value; }

        public Image(BitmapImage bitMapImage, ImageType image)
        {
            this.bitmap = BitmapImage2Bitmap(bitMapImage);
            this.imageType = image;
        }

        public Image(Bitmap bitmap, ImageType image)
        {
            this.bitmap = bitmap;
            this.imageType = image;
        }

        public BitmapImage GetBitMapImage()
        {
            return Bitmap2BitmapImage(bitmap);
        }

        public Image Add(Image image)
        {
            Bitmap bitmap2 = image.bitmap;
            int width = Math.Min(bitmap.Width, bitmap2.Width);
            int height = Math.Min(bitmap.Height, bitmap2.Height);

            Bitmap newBitmap = new(width, height);

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    Color color1 = bitmap.GetPixel(i, j);
                    Color color2 = bitmap2.GetPixel(i, j);

                    int r = Math.Min(color1.R + color2.R, 255);
                    int g = Math.Min(color1.G + color2.G, 255);
                    int b = Math.Min(color1.B + color2.B, 255);

                    newBitmap.SetPixel(i, j, Color.FromArgb(255, r, g, b));
                }
            }

            return new Image(newBitmap, imageType);
        }


        internal Image Minus(Image image)
        {
            Bitmap bitmap2 = image.bitmap;
            int width = Math.Min(bitmap.Width, bitmap2.Width);
            int height = Math.Min(bitmap.Height, bitmap2.Height);

            Bitmap newBitmap = new(width, height);

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    Color color1 = bitmap.GetPixel(i, j);
                    Color color2 = bitmap2.GetPixel(i, j);

                    int r = Math.Max(color1.R - color2.R, 0);
                    int g = Math.Max(color1.G - color2.G, 0);
                    int b = Math.Max(color1.B - color2.B, 0);

                    newBitmap.SetPixel(i, j, Color.FromArgb(255, r, g, b));
                }
            }

            return new Image(newBitmap, imageType);
        }


        public List<int> ComputeGrayHistogramme()
        {
            List<int> histogramme = new List<int>(256);
            for (int i = 0; i < 256; i++)
            {
                histogramme.Add(0);
            }

            for (int i = 0; i < bitmap.Width; i++)
            {
                for (int j = 0; j < bitmap.Height; j++)
                {
                    //take red value because all components should have the same value in grayscale
                    Color color = bitmap.GetPixel(i, j);                    
                    histogramme[color.R] = histogramme[color.R] + 1;
                }
            }

            return histogramme;
        }

        /// <summary>
        /// Compute the ith statistical moment of the histogram
        /// </summary>
        /// <param name="histogramme">Histogram of the image</param>
        /// <param name="i">The ith statistical moment wanted</param>
        /// <returns>the ith statistical moment</returns>
        private long ComputeStatisticalMoment(List<int> histogramme, int i)
        {
            long moment = 0;
            for (int gray = 0; gray < histogramme.Count; gray++)
            {
                moment += histogramme[gray] * (long)Math.Pow(gray, i);
            }
            return moment / (bitmap.Height * bitmap.Width);
        } 

        /// <summary>
        /// Compute automatically the threshold using the statistical moment of the histogram.
        /// </summary>
        /// <returns>The threshold of the image</returns>
        public int ComputeThreshold()
        {
            List<int> histogramme = ComputeGrayHistogramme();
            long m1 = ComputeStatisticalMoment(histogramme, 1);
            long m2 = ComputeStatisticalMoment(histogramme, 2);
            long m3 = ComputeStatisticalMoment(histogramme, 3);

            long c1 = (m2*m1 - m3) / (m2 - m1*m1);
            long c0 = -m2 - c1 * m1;

            long delta = c1*c1 - 4 * c0;
            long sqrtDelta = (long)Math.Sqrt(delta);

            long x1 = (-c1 + sqrtDelta) / 2;
            long x2 = (-c1 - sqrtDelta) / 2;

            long threshold = (x1 + x2) / 2;
            return (int)threshold;
        }

        public Image ToBinaryImage(int threshold)
        {
            Bitmap newBitmap = new Bitmap(bitmap);

            for (int i = 0; i < bitmap.Width; i++)
            {
                for (int j = 0; j < bitmap.Height; j++)
                {
                    Color color;
                    if (bitmap.GetPixel(i, j).R < threshold)
                    {
                        color = Color.FromArgb(255, 0, 0, 0);
                    }
                    else
                    {
                        color = Color.FromArgb(255, 255, 255, 255);
                    }
                    newBitmap.SetPixel(i, j, color);
                }
            }
            
            return new Image(newBitmap, ImageType.BINARY);
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
                Bitmap bitmap = new Bitmap(outStream);

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