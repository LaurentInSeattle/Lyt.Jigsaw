using SkiaSharp;

// Consider moving this into the Avalonia area and create a new library for Avalonia images and media.
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

        public static unsafe WriteableBitmap Crop(this WriteableBitmap source, PixelRect roi)
        {
            try
            {
                var size = source.PixelSize;
                var format = source.Format ?? throw new InvalidOperationException("Source bitmap has no format");
                var alphaFormat = source.AlphaFormat ?? throw new InvalidOperationException("Source bitmap has no alpha format");
                using ILockedFramebuffer fb = source.Lock();

                int stride = fb.RowBytes;
                int minStride = (format.BitsPerPixel * size.Width + 7) / 8;
                if (minStride > stride)
                {
                    throw new Exception(nameof(stride));
                }

                byte* srcData = (byte*)fb.Address;
                int bytesPerPixel = format.BitsPerPixel / 8;
                byte[] destBytes = new byte[roi.Width * roi.Height * format.BitsPerPixel / 8];
                fixed (byte* dstData = destBytes)
                {
                    int dstRow = 0;
                    for (int y = roi.Y; y < roi.Y + roi.Height; ++y)
                    {
                        int dstCol = 0;
                        for (int x = roi.X; x < roi.X + roi.Width; ++x)
                        {
                            int dstIndex = dstRow * roi.Width * bytesPerPixel + dstCol * bytesPerPixel;
                            int srcIndex = y * size.Width * bytesPerPixel + x * bytesPerPixel;
                            for (int byteIndex = 0; byteIndex < bytesPerPixel; ++byteIndex)
                            {
                                dstData[dstIndex++] = srcData[srcIndex++];
                            }

                            ++dstCol;
                        }

                        ++dstRow;
                    }

                    var pixelSize = new PixelSize(roi.Width, roi.Height);
                    var bitmap = 
                        new WriteableBitmap(
                            format, alphaFormat, (IntPtr)dstData, pixelSize, source.Dpi, roi.Width * bytesPerPixel);
                    return bitmap;
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to crop bitmap", ex);
            }

        }

        private static readonly Dictionary<PixelFormat, SKColorType> ColorTypeMap =
            new()
            {
                [PixelFormat.Bgra8888] = SKColorType.Bgra8888
            };

        public static byte[] EncodeToJpeg(this Bitmap bitmap, int quality = 80)
        {
            if (bitmap is not WriteableBitmap writeableBitmap)
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
