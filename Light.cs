using OpenTK.Mathematics;

namespace GR2024;
/// <summary>
/// makes a lightsource
/// </summary>
/// <param name="color">change the color and intensity of the light</param>
public abstract class Light(Color color)
{
    public Color Color = color;
    
    public static Color Ambient = new(0.2f, 0.2f, 0.2f);
}
/// <summary>
/// make a lightsource at a given position
/// </summary>
/// <param name="position">coordinates of lightsource</param>
/// <param name="color">change the color and intensity of the light</param>
public class PointLight(Vector3 position, Color color) : Light(color)
{
    public Vector3 Position = position;
}

public class DirectionalLight(Vector3 direction, Color color) : Light(color)
{
    public Vector3 Direction = direction;
}

public class Spotlight(Vector3 position, Vector3 direction, float angle, Color color) : Light(color)
{
    public Vector3 Position = position;
    public Vector3 Direction = direction;
    public float Angle = angle;
}
