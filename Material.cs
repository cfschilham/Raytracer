using OpenTK.Mathematics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Raytracer;
using System.Drawing;

public struct Material
{
    public Texture Ambient;
    public Texture Diffuse;
    public Texture Specular;
    public Texture? NormalMap;
    public Texture? DisplacementMap;
    public float Shininess;
    public float Reflectivity; // Should be between 0.0f and 1.0f.
    public float Gloss;
    public float Refractivity;
    public float Gamma;

    public static Material Default = new()
    {
        Ambient = new SolidColor(0.7f, 0.7f, 0.7f),
        Diffuse = new SolidColor(0.7f, 0.7f, 0.7f),
        Specular = new SolidColor(0.1f, 0.1f, 0.1f),
        Shininess = 2.0f,
        Gamma = 1.0f,
    };

    public static Material Mirror = new() {
        Ambient = new SolidColor(0.1f, 0.1f, 0.1f),
        Diffuse = new SolidColor(0.1f, 0.1f, 0.1f),
        Specular = new SolidColor(1.0f, 1.0f, 1.0f),
        Shininess = 1000.0f,
        Reflectivity = 0.55f,
        Gamma = 1.0f,
    };
    
    public static Material Polished = new() {
        Ambient = new SolidColor(0.2f, 0.05f, 0.05f),
        Diffuse = new SolidColor(0.2f, 0.05f, 0.05f),
        Specular = new SolidColor(0.75f, 0.3f, 0.3f),
        Shininess = 50.0f,
        Reflectivity = 0.20f,
        Gloss = 0.5f,
        Gamma = 1.0f,
    };
    
    public static Material Checkerboard = new() {
        Ambient = new Checkerboard(new Color(0.25f, 0.4f, 0.1f), new Color(0.9f, 0.9f, 0.9f), 5),
        Diffuse = new Checkerboard(new Color(0.1f, 0.1f, 0.1f), new Color(0.9f, 0.9f, 0.9f), 5),
        Specular = new SolidColor(0.2f, 0.2f, 0.2f),
        Shininess = 20.0f,
        Gamma = 1.0f,
    };
        
    static ImageTexture _rocks = new("../../../assets/rocks_color.jpg");
    public static Material Rocks = new()
    {
        Ambient = new ImageTexture("../../../assets/rocks_ambientocclusion.jpg").Mul(0.10f).Add(new Color(0.05f, 0.05f, 0.05f)),
        Diffuse = _rocks,
        Specular = new SolidColor(0.2f, 0.2f, 0.2f),
        NormalMap = new ImageTexture("../../../assets/rocks_normal.jpg"),
        Shininess = 20.0f,
        Gamma = 1.0f,
    };
    
    static ImageTexture _test = new("../../../assets/test.jpg");
    public static Material Test = new()
    {
        Ambient = _test,
        Diffuse = _test,
        Specular = new SolidColor(0.2f, 0.2f, 0.2f),
        Shininess = 20.0f,
        Gamma = 1.0f,
    };
    
    static ImageTexture _earth = new("../../../assets/earth.jpg");
    public static Material Earth = new()
    {
        Ambient = _earth,
        Diffuse = _earth,
        Specular = new SolidColor(0.2f, 0.2f, 0.2f),
        Shininess = 20.0f,
        Gamma = 1.0f,
    };
    
    static ImageTexture _sky = new("../../../assets/skybox_sphere.jpg");
    public static Material Sky = new()
    {
        Ambient = _sky,
        Diffuse = _sky,
        Specular = new SolidColor(Color.Black),
        Gamma = 5.0f,
    };
}

public abstract class Texture
{
    public abstract Color Pixel(float x, float y);
}

/// <summary>
/// Creates a solid color texture
/// </summary>
public class SolidColor(Color color) : Texture
{
    public Color Color = color;
    
    public SolidColor(float r, float g, float b) : this(new Color(r, g, b)) { }
    
    public override Color Pixel(float x, float y) => Color;
}
/// <summary>
/// Creates a checkerboard Texture
/// </summary>
public class Checkerboard(Color a, Color b, int size) : Texture
{
    public Color A = a;
    public Color B = b;
    public int Size = size;
    
    public override Color Pixel(float x, float y) => ((int)(x / (Size * 0.01f)) + (int)(y / (Size * 0.01f))) % 2 == 0 ? A : B;
}

public class ImageTexture : Texture
{
    public Color[,] Pixels;
    
    /// <summary>
    /// Makes a pixel map of the image at the given path
    /// </summary>
    public ImageTexture(string path) {
        Image<Rgba32> img = Image.Load<Rgba32>(path);
        Pixels = new Color[img.Width, img.Height];
        for (int x = 0; x < img.Width; x++)
        for (int y = 0; y < img.Height; y++) {
            Rgba32 pixel = img[x, y];
            Pixels[x, y] = new Color(pixel.R, pixel.G, pixel.B);
        }
    }
    
    public ImageTexture Mul(float scalar) {
        for (int x = 0; x < Pixels.GetLength(0); x++)
            for (int y = 0; y < Pixels.GetLength(1); y++) 
                Pixels[x, y] *= scalar;
        return this;
    }

    public ImageTexture Add(Color color) {
        for (int x = 0; x < Pixels.GetLength(0); x++)
            for (int y = 0; y < Pixels.GetLength(1); y++) 
                Pixels[x, y] += color;
        return this;
    }
    
    /// <summary>
    /// Gets the color of the pixel based on the given coordinates. X and Y should be between 0.0f and 1.0f inclusive.
    /// </summary> 
    public override Color Pixel(float x, float y) => Pixels[(int)(x * (Pixels.GetLength(0) - 1)), (int)((1 - y) * (Pixels.GetLength(1) - 1))];
}
