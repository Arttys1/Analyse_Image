using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;
using Color = System.Drawing.Color;

namespace Analyse_Image.back
{
    public class Image
    {
        private Bitmap bitmap;
        private ImageType imageType;
        public ImageType ImageType { get => imageType; set => imageType = value; }

        public Image(BitmapImage bitMapImage)
        {
            this.bitmap = BitmapImage2Bitmap(bitMapImage);
            this.imageType = AssignTypeImage();
        }
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

        public Image(Image image)
        {
            this.bitmap = new Bitmap(image.bitmap);
            this.imageType = image.imageType;
        }

        public BitmapImage GetBitMapImage()
        {
            return Bitmap2BitmapImage(bitmap);
        }

        public Image Erosion(int size)
        {
            Bitmap newBitmap = new(bitmap.Width, bitmap.Height);
            for (int i = 0; i < bitmap.Width; i++)
            {
                for(int j = 0; j < bitmap.Height; j++)
                {
                    // Si c'est un pixel en bordure, on le met en noir
                    if (i < size || i >= bitmap.Width - size
                    || j < size || j >= bitmap.Height - size)
                    {
                        newBitmap.SetPixel(i, j, Color.FromArgb(255, 0, 0, 0));
                    }
                    else
                    {
                        bool keepIt = true;
                        for (int k = -size; k <= size; k++)
                        {
                            for (int h = -size; h <= size; h++)
                            {
                                Color color = bitmap.GetPixel(i + k, j + h);
                                keepIt &= (color.R == 255);
                                if (!keepIt) break;
                            }
                            if (!keepIt) break;
                        }
                        if (keepIt)
                        {
                            newBitmap.SetPixel(i, j, Color.FromArgb(255, 255, 255, 255));
                        }
                        else
                        {
                            newBitmap.SetPixel(i, j, Color.FromArgb(255, 0, 0, 0));
                        }
                    }
                }
            }
            return new Image(newBitmap, ImageType.BINARY);
        }

        public Image Dilatation(int size)
        {
            Bitmap newBitmap = new(bitmap.Width, bitmap.Height);
            for (int i = 0; i < bitmap.Width; i++)
            {
                for (int j = 0; j < bitmap.Height; j++)
                {
                    // Si c'est un pixel en bordure, on recopie l'ancien pixel
                    if (i < size || i >= bitmap.Width - size
                    || j < size || j >= bitmap.Height - size)
                    {
                        newBitmap.SetPixel(i, j, bitmap.GetPixel(i, j));
                    }
                    else
                    {
                        bool keepIt = false;
                        for (int k = -size; k <= size; k++)
                        {
                            for (int h = -size; h <= size; h++)
                            {
                                Color color = bitmap.GetPixel(i + k, j + h);
                                keepIt |= (color.R == 255);
                                if (keepIt) break;
                            }
                            if (keepIt) break;
                        }
                        if (keepIt)
                        {
                            newBitmap.SetPixel(i, j, Color.FromArgb(255, 255, 255, 255));
                        }
                        else
                        {
                            newBitmap.SetPixel(i, j, Color.FromArgb(255, 0, 0, 0));
                        }
                    }
                }
            }
            return new Image(newBitmap, ImageType.BINARY);
        }

        public Image Ouverture(int size)
        {
            return Erosion(size).Dilatation(size);
        }

        public Image Fermeture(int size)
        {
            return Dilatation(size).Erosion(size);
        }

        public Image Lantuejoul()
        {
            Image squelette = new Image(new Bitmap(bitmap.Width, bitmap.Height), ImageType.BINARY);
            Image lastSquelette = new(squelette);

            //n = 0, erode(X, 0) = X
            squelette = Minus(Ouverture(1));

            int n = 1;
            while (!CompareMemCmp(squelette.bitmap, lastSquelette.bitmap))
            {
                Image erode = Erosion(n);
                Image openErode = new Image(erode).Ouverture(1);
                Image res = erode.Minus(openErode);
                lastSquelette = new(squelette);
                squelette = squelette.Add(res);
                n++;
            }

            return squelette;
        }        

        public Image AminscissementHomotopique()
        {
            Image squelette = new Image(this);
            Image lastSquelette;

            do
            {
                lastSquelette = new(squelette);
                squelette = squelette.Aminscissement();
            } while (!CompareMemCmp(squelette.bitmap, lastSquelette.bitmap));

            return squelette;
        }

        public Image Aminscissement()
        {
            Image res = new Image(this);
            for (int i = 0; i < 8; i++)
            {
                int[][] voisinage = getVoisinageL(i);
                Image transformationDeVoisinage = res.TranformationDeVoisinage(voisinage);
                res = res.Minus(transformationDeVoisinage);
            }

            return res;
        }

        public Image Epaississement()
        {
            Image res = new Image(this);
            for (int i = 0; i < 8; i++)
            {
                int[][] voisinage = getVoisinageL(i);
                voisinage[1][1] = 0;
                Image transformationDeVoisinage = res.TranformationDeVoisinage(voisinage);
                res = res.Add(transformationDeVoisinage);
            }

            return res;
        }

        private Image TranformationDeVoisinage(int[][] voisinage)
        {
            Bitmap newBitmap = new Bitmap(bitmap.Width, bitmap.Height);

            for (int i = 0; i < bitmap.Width; i++)
            {
                for (int j = 0; j < bitmap.Height; j++)
                {
                    bool keepIt = true;
                    for (int z = -1; z <= 1; z++)
                    {
                        for (int w = -1; w <= 1; w++)
                        {
                            int pixelColor;

                            if (i == 0 || i == bitmap.Width - 1 || j == 0 || j == bitmap.Height - 1)
                            {
                                pixelColor = 0;
                            }
                            else
                            {
                                pixelColor = bitmap.GetPixel(i + z, j + w).R;
                            }


                            int value = voisinage[z + 1][w + 1];
                            if (value != -1)
                            {
                                keepIt &= (value * 255 == pixelColor);
                                if (!keepIt) break;
                            }
                        }
                        if (!keepIt) break;

                    }

                    if (keepIt)
                    {
                        newBitmap.SetPixel(i, j, Color.FromArgb(255, 255, 255, 255));
                    }
                    else
                    {
                        newBitmap.SetPixel(i, j, Color.FromArgb(255, 0, 0, 0));

                    }
                }

            }


            return new Image(newBitmap, ImageType.BINARY);
        }

        private int[][] getVoisinageL(int configuration) {
            int[][] voisinage = new int[3][];
            const int n = -1; //unknown point

            switch (configuration)
            {
                case 0:
                    voisinage = [
                        [1, 1, 1],
                        [n, 1, n],
                        [0, 0, 0]];
                    break;
                case 1:
                    voisinage = [
                        [n, 1, 1],
                        [0, 1, 1],
                        [0, 0, n]];
                    break;
                case 2:
                    voisinage = [
                        [0, n, 1],
                        [0, 1, 1],
                        [0, n, 1]];
                    break;
                case 3:
                    voisinage = [
                        [0, 0, n],
                        [0, 1, 1],
                        [n, 1, 1]];
                    break;
                case 4:
                    voisinage = [
                        [0, 0, 0],
                        [n, 1, n],
                        [1, 1, 1]];
                    break;
                case 5:
                    voisinage = [
                        [n, 0, 0],
                        [1, 1, 0],
                        [1, 1, n]];
                    break;
                case 6:
                    voisinage = [
                        [1, n, 0],
                        [1, 1, 0],
                        [1, n, 0]];
                    break;
                case 7:
                    voisinage = [
                        [1, 1, n],
                        [1, 1, 0],
                        [n, 0, 0]];
                    break;
                default:
                    throw new Exception("Configuration should be between 0 and 7");
            }

            return voisinage;
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

        public Image Minus(Image image)
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

        public void Save(string filename)
        {
            bitmap.Save(filename, ImageFormat.Png);
        }

        private ImageType AssignTypeImage()
        {
            bool isGrayScale = true;
            bool isBinary = true;

            for (int i = 0; i < bitmap.Width; i++)
            {
                for (int j = 0; j < bitmap.Height; j++)
                {
                    Color color = bitmap.GetPixel(i, j);
                    byte r = color.R;
                    byte g = color.G;
                    byte b = color.B;

                    isGrayScale &= (r == b && r == g);
                    isBinary &= isGrayScale && (r == 255  || r == 0);
                    if (!isGrayScale && !isBinary) break;
                }
                if (!isGrayScale && !isBinary) break;
            }

            if(isBinary)
            {
                return ImageType.BINARY;
            }
            if (isGrayScale)
            {
                return ImageType.GRAY;
            }
            return ImageType.RGB;
        }

        // Méthode pour rapidement comparer deux bitmaps, utilise la fonction memcmp
        [DllImport("msvcrt.dll")]
        private static extern int memcmp(IntPtr b1, IntPtr b2, long count);
        public static bool CompareMemCmp(Bitmap b1, Bitmap b2)
        {
            if ((b1 == null) || (b2 == null)) return false;
            if (b1.Size != b2.Size) return false;

            var bd1 = b1.LockBits(new Rectangle(new Point(0, 0), b1.Size), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            var bd2 = b2.LockBits(new Rectangle(new Point(0, 0), b2.Size), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            try
            {
                IntPtr bd1scan0 = bd1.Scan0;
                IntPtr bd2scan0 = bd2.Scan0;

                int stride = bd1.Stride;
                int len = stride * b1.Height;

                return memcmp(bd1scan0, bd2scan0, len) == 0;
            }
            finally
            {
                b1.UnlockBits(bd1);
                b2.UnlockBits(bd2);
            }
        }
    }
}
