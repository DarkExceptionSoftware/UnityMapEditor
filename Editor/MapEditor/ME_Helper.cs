using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.Runtime.InteropServices;
using UnityEngine;
using Color = System.Drawing.Color;
using System.IO;
using JetBrains.Annotations;
using static log4net.Appender.ColoredConsoleAppender;
using System.Reflection;

public static class ME_Helper
{
    /// <summary>
    /// Creates a bitmap based on data, width, height, stride and pixel format.
    /// </summary>
    /// <param name="sourceData">Byte array of raw source data</param>
    /// <param name="width">Width of the image</param>
    /// <param name="height">Height of the image</param>
    /// <param name="stride">Scanline length inside the data</param>
    /// <param name="pixelFormat">Pixel format</param>
    /// <param name="palette">Color palette</param>
    /// <param name="defaultColor">Default color to fill in on the palette if the given colors don't fully fill it.</param>
    /// <returns>The new image</returns>
    public static Bitmap BuildImage(Byte[] sourceData, Int32 width, Int32 height, Int32 stride, PixelFormat pixelFormat, Color[] palette, Color? defaultColor)
    {
        Bitmap newImage = new Bitmap(width, height, pixelFormat);
        BitmapData targetData = newImage.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, newImage.PixelFormat);
        Int32 newDataWidth = ((Image.GetPixelFormatSize(pixelFormat) * width) + 7) / 8;
        // Compensate for possible negative stride on BMP format.
        Boolean isFlipped = stride < 0;
        stride = Math.Abs(stride);
        // Cache these to avoid unnecessary getter calls.
        Int32 targetStride = targetData.Stride;
        Int64 scan0 = targetData.Scan0.ToInt64();
        for (Int32 y = 0; y < height; y++)
            Marshal.Copy(sourceData, y * stride, new IntPtr(scan0 + y * targetStride), newDataWidth);
        newImage.UnlockBits(targetData);
        // Fix negative stride on BMP format.
        if (isFlipped)
            newImage.RotateFlip(RotateFlipType.Rotate180FlipX);
        // For indexed images, set the palette.
        if ((pixelFormat & PixelFormat.Indexed) != 0 && palette != null)
        {
            ColorPalette pal = newImage.Palette;
            for (Int32 i = 0; i < pal.Entries.Length; i++)
            {
                if (i < palette.Length)
                    pal.Entries[i] = palette[i];
                else if (defaultColor.HasValue)
                    pal.Entries[i] = defaultColor.Value;
                else
                    break;
            }
            newImage.Palette = pal;
        }
        return newImage;
    }

    internal static void decimate_colors(string AssetPath, int colors)
    {

        string filename = AssetPath.Substring(0, AssetPath.LastIndexOf("."));
        string target_file = filename + "_dec" + colors + ".png";

        if (File.Exists(target_file) || AssetPath.Contains("_dec" + colors))
        {
            Debug.Log("Already converted " + AssetPath + "."); return;
        }

        Image image = Image.FromFile(AssetPath);

        bool dither = false;
        var quantizer = new PnnQuant.PnnQuantizer();
        using (var bitmap = new Bitmap(image))
        {
            try
            {
                using (var dest = quantizer.QuantizeImage(bitmap, image.PixelFormat, colors, dither))
                {
                    dest.Save(target_file, ImageFormat.Png);
                    Debug.Log("Converted image: " + Path.GetFullPath(target_file));
                }
            }
            catch (Exception q)
            {
                Debug.Log(q.StackTrace);
            }
        }

        count_colors(target_file, colors);
    }


    internal static void count_colors(String AssetPath, int colors)
    {
        string filename = AssetPath;

        if (AssetPath.Contains("_"))
            filename = AssetPath.Substring(0, AssetPath.LastIndexOf("_"));
        else filename = AssetPath.Substring(0, AssetPath.LastIndexOf("."));

        string target_file = filename + "_pal" + colors + ".png";

        if (File.Exists(target_file) || AssetPath.Contains("_pal" + colors))
        {
            Debug.Log("Already converted " + AssetPath + "."); return;
        }


        Bitmap image = (Bitmap)Image.FromFile(AssetPath);
        List<Color> palette = new List<Color>();

        for (int y = 0; y < image.Height; y++)
        {
            for (int x = 0; x < image.Width; x++)
            {
                Color result = image.GetPixel(x, y);

                if (!palette.Contains(result))
                {
                    palette.Add(result);
                }
            }
        }
        Bitmap color_palette = new Bitmap(palette.Count, 1, image.PixelFormat);

        for (int i = 0; i < palette.Count; i++)
        {
            color_palette.SetPixel(i, 0, palette[i]);
        }

        color_palette.Save(target_file);

        Debug.Log("i found " + palette.Count + " Colors!");
    }
}
