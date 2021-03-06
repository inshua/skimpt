﻿#region "License Agreement"
/* Skimpt, an open source screenshot utility.
      Copyright (C) <year>  <name of author>

      This program is free software: you can redistribute it and/or modify
      it under the terms of the GNU General Public License as published by
      the Free Software Foundation, either version 3 of the License, or
      (at your option) any later version.

      this program is distributed in the hope that it will be useful,
      but WITHOUT ANY WARRANTY; without even the implied warranty of
      MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
      GNU General Public License for more details.

      You should have received a copy of the GNU General Public License
      along with this program.  If not, see <http://www.gnu.org/licenses/>. */
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
public class BitmapFilter
{
    public static bool Invert(Bitmap b)
    {
        // GDI+ still lies to us - the return format is BGR, NOT RGB.
        BitmapData bmData = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

        int stride = bmData.Stride;
        System.IntPtr Scan0 = bmData.Scan0;

        unsafe
        {
            byte* p = (byte*)(void*)Scan0;

            int nOffset = stride - b.Width * 3;
            int nWidth = b.Width * 3;

            for (int y = 0; y < b.Height; ++y)
            {
                for (int x = 0; x < nWidth; ++x)
                {
                    p[0] = (byte)(255 - p[0]);
                    ++p;
                }
                p += nOffset;
            }
        }

        b.UnlockBits(bmData);

        return true;
    }

    public static bool GrayScale(Bitmap b)
    {
        // GDI+ still lies to us - the return format is BGR, NOT RGB.
        BitmapData bmData = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

        int stride = bmData.Stride;
        System.IntPtr Scan0 = bmData.Scan0;

        unsafe
        {
            byte* p = (byte*)(void*)Scan0;

            int nOffset = stride - b.Width * 3;

            byte red, green, blue;

            for (int y = 0; y < b.Height; ++y)
            {
                for (int x = 0; x < b.Width; ++x)
                {
                    blue = p[0];
                    green = p[1];
                    red = p[2];

                    p[0] = p[1] = p[2] = (byte)(.299 * red + .587 * green + .114 * blue);

                    p += 3;
                }
                p += nOffset;
            }
        }

        b.UnlockBits(bmData);

        return true;
    }

    public static bool Brightness(Bitmap b, int nBrightness)
    {
        if (nBrightness < -255 || nBrightness > 255)
            return false;

        // GDI+ still lies to us - the return format is BGR, NOT RGB.
        BitmapData bmData = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

        int stride = bmData.Stride;
        System.IntPtr Scan0 = bmData.Scan0;

        int nVal = 0;

        unsafe
        {
            byte* p = (byte*)(void*)Scan0;

            int nOffset = stride - b.Width * 3;
            int nWidth = b.Width * 3;

            for (int y = 0; y < b.Height; ++y)
            {
                for (int x = 0; x < nWidth; ++x)
                {
                    nVal = (int)(p[0] + nBrightness);

                    if (nVal < 0) nVal = 0;
                    if (nVal > 255) nVal = 255;

                    p[0] = (byte)nVal;

                    ++p;
                }
                p += nOffset;
            }
        }

        b.UnlockBits(bmData);

        return true;
    }

    public static bool Contrast(Bitmap b, sbyte nContrast)
    {
        if (nContrast < -100) return false;
        if (nContrast > 100) return false;

        double pixel = 0, contrast = (100.0 + nContrast) / 100.0;

        contrast *= contrast;

        int red, green, blue;

        // GDI+ still lies to us - the return format is BGR, NOT RGB.
        BitmapData bmData = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

        int stride = bmData.Stride;
        System.IntPtr Scan0 = bmData.Scan0;

        unsafe
        {
            byte* p = (byte*)(void*)Scan0;

            int nOffset = stride - b.Width * 3;

            for (int y = 0; y < b.Height; ++y)
            {
                for (int x = 0; x < b.Width; ++x)
                {
                    blue = p[0];
                    green = p[1];
                    red = p[2];

                    pixel = red / 255.0;
                    pixel -= 0.5;
                    pixel *= contrast;
                    pixel += 0.5;
                    pixel *= 255;
                    if (pixel < 0) pixel = 0;
                    if (pixel > 255) pixel = 255;
                    p[2] = (byte)pixel;

                    pixel = green / 255.0;
                    pixel -= 0.5;
                    pixel *= contrast;
                    pixel += 0.5;
                    pixel *= 255;
                    if (pixel < 0) pixel = 0;
                    if (pixel > 255) pixel = 255;
                    p[1] = (byte)pixel;

                    pixel = blue / 255.0;
                    pixel -= 0.5;
                    pixel *= contrast;
                    pixel += 0.5;
                    pixel *= 255;
                    if (pixel < 0) pixel = 0;
                    if (pixel > 255) pixel = 255;
                    p[0] = (byte)pixel;

                    p += 3;
                }
                p += nOffset;
            }
        }

        b.UnlockBits(bmData);

        return true;
    }

    public static bool Gamma(Bitmap b, double red, double green, double blue)
    {
        if (red < .2 || red > 5) return false;
        if (green < .2 || green > 5) return false;
        if (blue < .2 || blue > 5) return false;

        byte[] redGamma = new byte[256];
        byte[] greenGamma = new byte[256];
        byte[] blueGamma = new byte[256];

        for (int i = 0; i < 256; ++i)
        {
            redGamma[i] = (byte)Math.Min(255, (int)((255.0 * Math.Pow(i / 255.0, 1.0 / red)) + 0.5));
            greenGamma[i] = (byte)Math.Min(255, (int)((255.0 * Math.Pow(i / 255.0, 1.0 / green)) + 0.5));
            blueGamma[i] = (byte)Math.Min(255, (int)((255.0 * Math.Pow(i / 255.0, 1.0 / blue)) + 0.5));
        }

        // GDI+ still lies to us - the return format is BGR, NOT RGB.
        BitmapData bmData = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

        int stride = bmData.Stride;
        System.IntPtr Scan0 = bmData.Scan0;

        unsafe
        {
            byte* p = (byte*)(void*)Scan0;

            int nOffset = stride - b.Width * 3;

            for (int y = 0; y < b.Height; ++y)
            {
                for (int x = 0; x < b.Width; ++x)
                {
                    p[2] = redGamma[p[2]];
                    p[1] = greenGamma[p[1]];
                    p[0] = blueGamma[p[0]];

                    p += 3;
                }
                p += nOffset;
            }
        }

        b.UnlockBits(bmData);

        return true;
    }

    public static bool Color(Bitmap b, int red, int green, int blue)
    {
        if (red < -255 || red > 255) return false;
        if (green < -255 || green > 255) return false;
        if (blue < -255 || blue > 255) return false;

        // GDI+ still lies to us - the return format is BGR, NOT RGB.
        BitmapData bmData = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

        int stride = bmData.Stride;
        System.IntPtr Scan0 = bmData.Scan0;

        unsafe
        {
            byte* p = (byte*)(void*)Scan0;

            int nOffset = stride - b.Width * 3;
            int nPixel;

            for (int y = 0; y < b.Height; ++y)
            {
                for (int x = 0; x < b.Width; ++x)
                {
                    nPixel = p[2] + red;
                    nPixel = Math.Max(nPixel, 0);
                    p[2] = (byte)Math.Min(255, nPixel);

                    nPixel = p[1] + green;
                    nPixel = Math.Max(nPixel, 0);
                    p[1] = (byte)Math.Min(255, nPixel);

                    nPixel = p[0] + blue;
                    nPixel = Math.Max(nPixel, 0);
                    p[0] = (byte)Math.Min(255, nPixel);

                    p += 3;
                }
                p += nOffset;
            }
        }

        b.UnlockBits(bmData);

        return true;
    }

    public static bool HighlightWithOutColor(Bitmap b, Rectangle r)
    {
        int nContrast = -30;
        
        double pixel = 0, contrast = (100.0 + nContrast) / 100.0; //70 / 100 = 0.7

        contrast *= contrast;

        int red, green, blue;

        // GDI+ still lies to us - the return format is BGR, NOT RGB.
        //lock the bits and get the data. 
        BitmapData bmData = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
        
        int stride = bmData.Stride;
        System.IntPtr Scan0 = bmData.Scan0;

        unsafe
        {
            //first cast it to generic pointer then casst it to byte pointer
            byte* p = (byte*)(void*)Scan0;

            /* The Stride property, as shown in figure 1, holds the width of one row in bytes.
             * The size of a row however may not be an exact multiple of the pixel size because for efficiency,
             * the system ensures that the data is packed into rows that begin on a four byte boundary 
             * and are padded out to a multiple of four bytes. This means for example that a 24 bit per 
             * pixel image 17 pixels wide would have a stride of 52. 
             * The used data in each row would take up 3*17 = 51 bytes and the 
             * padding of 1 byte would expand each row to 52 bytes or 13*4 bytes.
             * A 4BppIndexed image of 17 pixels wide would have a stride of 12. 
             * Nine of the bytes, or more properly eight and a half,  would contain data and the 
             * row would be padded out with a further 3 bytes to a 4 byte boundary.*/


            //the offset is the padding.
            int nOffset = stride - b.Width * 3;

            for(int y = 0; y < b.Height; ++y)
            {              
                for(int x = 0; x < b.Width; ++x)
                {
                    
                        if((x < r.Left || x > r.Right) || (y < r.Top || y >r.Bottom))
                        {
                            blue = p[0];
                            green = p[1];
                            red = p[2];

                            pixel = red / 255.0;
                            pixel -= 0.5;
                            pixel *= contrast;
                            pixel += 0.5;
                            pixel *= 255;
                            if(pixel < 0) pixel = 0;
                            if(pixel > 255) pixel = 255;
                            p[2] = (byte)pixel;

                            pixel = green / 255.0;
                            pixel -= 0.5;
                            pixel *= contrast;
                            pixel += 0.5;
                            pixel *= 255;
                            if(pixel < 0) pixel = 0;
                            if(pixel > 255) pixel = 255;
                            p[1] = (byte)pixel;

                            pixel = blue / 255.0;
                            pixel -= 0.5;
                            pixel *= contrast;
                            pixel += 0.5;
                            pixel *= 255;
                            if(pixel < 0) pixel = 0;
                            if(pixel > 255) pixel = 255;
                            p[0] = (byte)pixel;

                        }

                    
                
                    p += 3;
                }
                p += nOffset;
            }
        }

        b.UnlockBits(bmData);
        b = null;
        return true;        
    }
}