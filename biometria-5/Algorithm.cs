using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace biometria_5
{
    public static class Algorithm
    {
        // 300x200 % 4 == 0
        // 301x200 % 4 != 0 //

        public static Bitmap Apply(Bitmap bmp, int threshold, int thresholdMin)
        {
            var bmpData = bmp.LockBits(
                new Rectangle(0, 0, bmp.Width, bmp.Height),
                ImageLockMode.ReadWrite,
                PixelFormat.Format24bppRgb
            );

            int channels = 3;
            int stride = bmpData.Stride;
            int length = bmpData.Width * bmpData.Height * 3;

            int[] offsets =
            {
            -channels,
             channels,
                        stride,
                      - stride,
            -channels + stride,
            -channels - stride,
             channels + stride,
             channels - stride
        };

            var indexToGroup = new Dictionary<int, int>();
            var groupToColor = new Dictionary<int, byte[]>();
            int groupCount = 0;
            var rand = new Random();

            var data = new byte[length];

            Marshal.Copy(bmpData.Scan0, data, 0, data.Length);

            for (int i = 0; i < length; i += channels)
            {
                if (indexToGroup.ContainsKey(i))
                    continue;

                var all = new HashSet<int>();
                var current = new List<int>() { i };
                var next = new List<int>();

                if (data[i] < threshold && data[i] > thresholdMin)
                {
                    int value = (data[i] + threshold / 2) / threshold;
                    indexToGroup[i] = groupCount;
                    byte[] rgb = new byte[3];
                    rand.NextBytes(rgb);
                    groupToColor[groupCount] = rgb;

                    while (current.Count > 0)
                    {
                        foreach (int k in current)
                            if (!all.Contains(k))
                            {
                                all.Add(k);
                                indexToGroup[k] = groupCount;

                                foreach (int offset in offsets)
                                {
                                    int o = k + offset;
                                    if (o > 0 && o < length &&
                                        value == (data[o] + threshold / 2) / threshold)
                                        next.Add(o);
                                }
                            }
                        current = next;
                        next = new();
                    }
                }
                ++groupCount;
            }

            foreach (var i in indexToGroup)
            {
                byte[] rgb = groupToColor[i.Value];
                for (int k = 0; k < channels; k++)
                    data[i.Key + k] = rgb[k];
            }

            Marshal.Copy(data, 0, bmpData.Scan0, data.Length);

            bmp.UnlockBits(bmpData);
            return bmp;
        }

        public static BitmapSource ToSource(this Bitmap bmp)
        {
            var data = bmp.LockBits(
                new Rectangle(Point.Empty, bmp.Size),
                ImageLockMode.ReadOnly,
                PixelFormat.Format24bppRgb
            );

            var src = BitmapSource.Create(
                bmp.Width,
                bmp.Height,
                96f, 96f,
                System.Windows.Media.PixelFormats.Bgr24,
                null,
                data.Scan0,
                data.Stride * data.Height,
                data.Width * 3
            );

            bmp.UnlockBits(data);
            return src;
        }
    

    public static Bitmap FloodFill(Bitmap bitmap, int x, int y, int thresholdMin, int thresholdMax, int maxPixels)
        {
            BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            byte[] vs = new byte[data.Height * data.Stride];
            Marshal.Copy(data.Scan0, vs, 0, vs.Length);

            int GetIndex(int x, int y) =>
                x * 3 + y * data.Stride;
            (int X, int Y) GetCoords(int offset) =>
                (x / 3 % 3, offset / data.Stride);

            int firstPos = x + y * bitmap.Width * 3;
            int[] directions =
            {
                -3, 3, data.Stride, -data.Stride            
            };

            Stack<int> pixels = new Stack<int>();
            byte[] color = new byte[3] { (byte)new Random().Next(0, 255), (byte)new Random().Next(0, 255), (byte)new Random().Next(0, 255) };
            pixels.Push(GetIndex(x, y));
            int pixelCount = 0;
            for (int k = 0; k < 3; k++)
                while (pixels.Count > 0 && pixelCount<maxPixels)
                {
                    var a = GetCoords(pixels.Pop() + k);
                   
                    if (a.X < data.Stride   - 3 && a.X * 3 > 3 &&
                        a.Y < bitmap.Height - 3 && a.Y     > 3)
                    {
                        int offset = a.X * 3 + a.Y * bitmap.Width * 3;

                        bool match = true;
                        if (
                           vs[offset + k] >= vs[firstPos + k] - thresholdMin &&
                           vs[offset + k] <= vs[firstPos + k] + thresholdMax)
                           match = false;

                        pixelCount++;
                        
                        if (match) 
                        {
                            vs[offset + k] = color[k];

                            foreach (var dir in directions)
                                if (vs[offset + dir + k] != color[k])
                                    pixels.Push(offset + dir);
                        }
                    }
                }
            Marshal.Copy(vs, 0, data.Scan0, vs.Length);
            bitmap.UnlockBits(data);

            return bitmap;
        }
    }

}
