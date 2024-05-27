using OpenTK.Mathematics;

namespace GR2024;

public abstract class Light(Color color)
{
    public Color Color = color;
    
    public static Color Ambient = new(0.2f, 0.2f, 0.2f);
}
/// <summary>
/// Represents a point light source at the given position with the given color. 
/// </summary>
/// <param name="position">Point of light source</param>
/// <param name="color">Color/intensity of the light source</param>
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
