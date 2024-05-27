using OpenTK.Mathematics;

namespace GR2024;

public class Plane : Primitive
{
    public Vector3 Normal;
    public Vector3 Position;
    public Vector3 X;
    public Vector3 Y;
    public float Width;
    public float Height;
    /// <summary>
    /// makes a primitive of a plane
    /// </summary>
    /// <param name="position"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="material"></param>
    /// <exception cref="ArgumentException"></exception>
    public Plane(Vector3 position, Vector3 x, Vector3 y, float width, float height, Material material) : base(material)
    {
        Position = position;
        Width = width;
        Height = height;
        X = x;
        Y = y;
        if (Math.Abs(Vector3.Dot(X, Y)) > 0.99f) throw new ArgumentException("X and Y must be linearly independent");
        Normal = Vector3.Cross(x, y).Normalized();
    }
    
    public override Intersection? Intersect(Ray ray)
    {
        float denom = Vector3.Dot(Normal, ray.Direction);
        if (Math.Abs(denom) < 1e-6) return null;
        
        float t = Vector3.Dot(Position - ray.Origin, Normal) / denom;
        if (t < 0) return null;
        Vector3 hit = ray.Origin + t * ray.Direction;
        if (Math.Abs(Vector3.Dot(hit - Position, X)) > Width / 2 || 
            Math.Abs(Vector3.Dot(hit - Position, Y)) > Height / 2) 
            return null;
        
        Vector2 uv = new(
            Vector3.Dot(hit - Position, X) / Width + 0.5f,
            Vector3.Dot(hit - Position, Y) / Height + 0.5f
        );

        Vector3 normal = denom < 0 ? Normal : -Normal;
        return new Intersection(hit, normal, t, this, uv);
    }
}
