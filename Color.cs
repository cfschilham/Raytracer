using System.Numerics;
using Vector3 = OpenTK.Mathematics.Vector3;

namespace GR2024;

public struct Color(float r, float g, float b)
{
    Vector3 _rgb = new(r, g, b);
    
    public float R => _rgb.X;
    public float G => _rgb.Y;
    public float B => _rgb.Z;
    
    public static Color White = new(255, 255, 255);
    public static Color Black = new(0, 0, 0);
    public static Color Blue = new(0, 0, 255);
    public static Color Green = new(0, 255, 0);
    public static Color Red = new(255, 0, 0);
    public static Color SkyBlue = new(135, 206, 235);
    
    public Color(int r, int g, int b) : this(r / 255f, g / 255f, b / 255f) { }

    public Color(Vector3 rgb) : this(rgb.X, rgb.Y, rgb.Z) { }
        
    /// <summary>
    /// Converts the color to an integer with the lower three bytes representing the RGB values. Blue being least
    /// significant, red being the most significant.
    /// </summary>
    public int ToInt() => 
        ((int)Math.Round(_rgb.X * 255) << 16) | ((int)Math.Round(_rgb.Y * 255) << 8) | (int)Math.Round(_rgb.Z * 255);
    
    public Vector3 ToVector3() => _rgb;
    
    public static Color operator *(Color a, Color b) =>
        new(a._rgb.X * b._rgb.X, a._rgb.Y * b._rgb.Y, a._rgb.Z * b._rgb.Z);
    
    public static Color operator *(Color a, float scalar) => 
        new(Math.Min(a._rgb.X * scalar, 1.0f), Math.Min(a._rgb.Y * scalar, 1.0f), Math.Min(a._rgb.Z * scalar, 1.0f));

    public static Color operator +(Color a, Color b) =>
        new(Math.Min(a._rgb.X + b._rgb.X, 1.0f), Math.Min(a._rgb.Y + b._rgb.Y, 1.0f), Math.Min(a._rgb.Z + b._rgb.Z, 1.0f));

    public static Color Lerp(Color a, Color b, float t) => new(Vector3.Lerp(a._rgb, b._rgb, t));
    
    public static Color Avg(params Color[] colors) => Avg(colors.AsSpan());

    public static Color Avg(Span<Color> colors) {
        Vector3 sum = Vector3.Zero;
        foreach (Color color in colors) sum += color._rgb; // Adding vector to allow overflow of 1.0f
        return new Color(sum / colors.Length);
    }
}
