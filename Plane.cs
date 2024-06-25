using OpenTK.Mathematics;

namespace Raytracer;

public class Plane : Primitive
{
    public Vector3 Normal;
    public Vector3 Position;
    public Vector3 X;
    public Vector3 Y;
    public float Width;
    public float Height;
    
    /// <summary>
    /// Construct a plane primitive. The plane is defined by a position, two vectors X and Y, and a material. X defines
    /// the width direction and Y defines the height direction. The normal is calculated as the cross product of X and Y.
    /// </summary>
    public Plane(Vector3 position, Vector3 x, Vector3 y, float width, float height, Material material) : base(material)
    {
        Position = position;
        Width = width;
        Height = height;
        X = x;
        Y = y;
        if (Math.Abs(Vector3.Dot(X, Y)) > 0.99f) throw new ArgumentException("X and Y must be linearly independent");
        Normal = Vector3.Cross(y, x).Normalized();
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

        Vector3 normal = Normal;
        if (Material.NormalMap != null)
            normal = Material.NormalMap.Pixel(uv.X, uv.Y).ToVector3() * 2 - Vector3.One;
        
        // Always make sure the normal points towards the ray origin.
        normal = Vector3.Dot(normal, ray.Direction) < 0 ? normal : -normal; 
        return new Intersection(hit, normal, t, this, uv);
    }
}
