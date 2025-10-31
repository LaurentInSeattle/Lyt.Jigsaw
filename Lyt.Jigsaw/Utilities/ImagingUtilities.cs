using SkiaSharp;

namespace Lyt.Jigsaw.Utilities
{
    public static class ImagingUtilities
    {
        public static byte[] EncodeThumbnailJpeg(Bitmap bitmap, int width, int height, int quality)
        {
            var resized = ThumbnailBitmapFrom(bitmap, width, height);
            return EncodeToJpeg(resized, quality);
        }

        public static Bitmap DecodeBitmap(IEnumerable<byte> blob)
        {
            using var stream = new MemoryStream([.. blob]);
            return new Bitmap(stream);
        }

        public static Bitmap ThumbnailBitmapFrom(Bitmap bitmap, int width, int height)
        {
            double scale = Math.Min(width / (double)bitmap.Size.Width, height / (double)bitmap.Size.Height);
            int scaledWidth = (int)(bitmap.Size.Width * scale);
            int scaledHeight = (int)(bitmap.Size.Height * scale);
            var resized = bitmap.CreateScaledBitmap(new PixelSize(scaledWidth, scaledHeight), BitmapInterpolationMode.MediumQuality);
            return resized;
        }

        public static WriteableBitmap WriteableFromBitmap(Bitmap bitmap)
        {
            var writeableBitmap = new WriteableBitmap(
                bitmap.PixelSize,
                bitmap.Dpi,
                bitmap.Format
            );
            
            using (ILockedFramebuffer fb = writeableBitmap.Lock())
            {
                bitmap.CopyPixels(fb, AlphaFormat.Opaque);
            }

            return writeableBitmap;
        }

        private static readonly Dictionary<PixelFormat, SKColorType> ColorTypeMap = 
            new()
            {
                [PixelFormat.Bgra8888] = SKColorType.Bgra8888
            };

        public static byte[] EncodeToJpeg(this Bitmap bitmap, int quality = 80)
        {
            if ( bitmap is not WriteableBitmap writeableBitmap)
            {
                writeableBitmap = WriteableFromBitmap(bitmap); 
            }

            if (writeableBitmap is null)
            {
                return []; 
            }

            try
            {
                using ILockedFramebuffer frameBuffer = writeableBitmap.Lock();
                SKColorType colorType = ColorTypeMap[bitmap.Format!.Value];
                var skImageInfo = new SKImageInfo(frameBuffer.Size.Width, frameBuffer.Size.Height, colorType);
                using var skBitmap = new SKBitmap(skImageInfo);
                skBitmap.InstallPixels(skImageInfo, frameBuffer.Address, frameBuffer.RowBytes);
                using var skImage = SKImage.FromBitmap(skBitmap);
                return skImage.Encode(SKEncodedImageFormat.Jpeg, quality).ToArray();
            }
            finally
            {
                writeableBitmap.Dispose();
            }
        }
    }
}
