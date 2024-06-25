using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using OpenTK.Graphics.OpenGL;

namespace Raytracer;

public class Surface
{
    public int Width, Height;
    public int[] Pixels;
    static Surface? _font;
    static int[]? _fontRedir;

    public Surface(int w, int h)
    {
        Width = w;
        Height = h;
        Pixels = new int[w * h];
    }

    public Surface(string fileName)
    {
        Image<Bgra32> bmp = Image.Load<Bgra32>(fileName);
        Width = bmp.Width;
        Height = bmp.Height;
        Pixels = new int[Width * Height];
        for (int y = 0; y < Height; y++)
        for (int x = 0; x < Width; x++)
            Pixels[y * Width + x] = (int)bmp[x, y].Bgra;
    }
    // create an OpenGL texture
    public int GenTexture()
    {
        int id = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, id);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Width, Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, Pixels);
        return id;
    }
    // clear the surface
    public void Clear(int c)
    {
        for (int s = Width * Height, p = 0; p < s; p++) Pixels[p] = c;
    }
    // copy the surface to another surface
    public void CopyTo(Surface target, int x = 0, int y = 0)
    {
        int src = 0;
        int dst = 0;
        int srcwidth = Width;
        int srcheight = Height;
        int dstwidth = target.Width;
        int dstheight = target.Height;
        if ((srcwidth + x) > dstwidth) srcwidth = dstwidth - x;
        if ((srcheight + y) > dstheight) srcheight = dstheight - y;
        if (x < 0)
        {
            src -= x;
            srcwidth += x;
            x = 0;
        }
        if (y < 0)
        {
            src -= y * Width;
            srcheight += y;
            y = 0;
        }
        if ((srcwidth > 0) && (srcheight > 0))
        {
            dst += x + dstwidth * y;
            for (int v = 0; v < srcheight; v++)
            {
                for (int u = 0; u < srcwidth; u++) target.Pixels[dst + u] = Pixels[src + u];
                dst += dstwidth;
                src += Width;
            }
        }
    }
    // draw a rectangle
    public void Box(int x1, int y1, int x2, int y2, int c)
    {
        int dest = y1 * Width;
        for (int y = y1; y <= y2; y++, dest += Width)
        {
            Pixels[dest + x1] = c;
            Pixels[dest + x2] = c;
        }
        int dest1 = y1 * Width;
        int dest2 = y2 * Width;
        for (int x = x1; x <= x2; x++)
        {
            Pixels[dest1 + x] = c;
            Pixels[dest2 + x] = c;
        }
    }
    // draw a solid bar
    public void Bar(int x1, int y1, int x2, int y2, int c)
    {
        int dest = y1 * Width;
        for (int y = y1; y <= y2; y++, dest += Width) for (int x = x1; x <= x2; x++)
        {
            Pixels[dest + x] = c;
        }
    }
    // helper function for line clipping
    int OUTCODE(int x, int y)
    {
        int xmin = 0, ymin = 0, xmax = Width - 1, ymax = Height - 1;
        return ((x < xmin) ? 1 : ((x > xmax) ? 2 : 0)) + ((y < ymin) ? 4 : ((y > ymax) ? 8 : 0));
    }
    // draw a line, clipped to the window
    public void Line(int x1, int y1, int x2, int y2, int c)
    {
        int xmin = 0, ymin = 0, xmax = Width - 1, ymax = Height - 1;
        int c0 = OUTCODE(x1, y1), c1 = OUTCODE(x2, y2);
        bool accept = false;
        while (true)
        {
            if (c0 == 0 && c1 == 0) { accept = true; break; }
            else if ((c0 & c1) > 0) break;
            else
            {
                int x = 0, y = 0;
                int co = (c0 > 0) ? c0 : c1;
                if ((co & 8) > 0) { x = x1 + (x2 - x1) * (ymax - y1) / (y2 - y1); y = ymax; }
                else if ((co & 4) > 0) { x = x1 + (x2 - x1) * (ymin - y1) / (y2 - y1); y = ymin; }
                else if ((co & 2) > 0) { y = y1 + (y2 - y1) * (xmax - x1) / (x2 - x1); x = xmax; }
                else if ((co & 1) > 0) { y = y1 + (y2 - y1) * (xmin - x1) / (x2 - x1); x = xmin; }
                if (co == c0) { x1 = x; y1 = y; c0 = OUTCODE(x1, y1); }
                else { x2 = x; y2 = y; c1 = OUTCODE(x2, y2); }
            }
        }
        if (!accept) return;
        if (Math.Abs(x2 - x1) >= Math.Abs(y2 - y1))
        {
            if (x2 < x1) { (x2, x1) = (x1, x2); (y2, y1) = (y1, y2); }
            int l = x2 - x1;
            if (l == 0) return;
            int dy = ((y2 - y1) * 8192) / l;
            y1 *= 8192;
            for (int i = 0; i < l; i++)
            {
                Pixels[x1++ + (y1 / 8192) * Width] = c;
                y1 += dy;
            }
        }
        else
        {
            if (y2 < y1) { (x2, x1) = (x1, x2); (y2, y1) = (y1, y2); }
            int l = y2 - y1;
            if (l == 0) return;
            int dx = ((x2 - x1) * 8192) / l;
            x1 *= 8192;
            for (int i = 0; i < l; i++)
            {
                Pixels[x1 / 8192 + y1++ * Width] = c;
                x1 += dx;
            }
        }
    }
    // plot a single pixel
    public void Plot(int x, int y, int c)
    {
        if ((x >= 0) && (y >= 0) && (x < Width) && (y < Height))
        {
            Pixels[x + y * Width] = c;
        }
    }
    // print a string
    public void Print(string t, int x, int y, int c)
    {
        if (_font == null || _fontRedir == null)
        {
            _font = new Surface("../../../assets/font.png");
            string ch = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()_-+={}[];:<>,.?/\\ ";
            _fontRedir = new int[256];
            for (int i = 0; i < 256; i++) _fontRedir[i] = 0;
            for (int i = 0; i < ch.Length; i++)
            {
                int l = (int)ch[i];
                _fontRedir[l & 255] = i;
            }
        }
        for (int i = 0; i < t.Length; i++)
        {
            int f = _fontRedir[(int)t[i] & 255];
            int dest = x + i * 12 + y * Width;
            int src = f * 12;
            for (int v = 0; v < _font.Height; v++, src += _font.Width, dest += Width) for (int u = 0; u < 12; u++)
            {
                if ((_font.Pixels[src + u] & 0xffffff) != 0) Pixels[dest + u] = c;
            }
        }
    }
}