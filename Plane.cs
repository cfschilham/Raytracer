using OpenTK.Mathematics;

namespace GR2024;

public class Plane : Primitive
{
    public Vector3 Normal;
    public Vector3 Position;
    public float Width;
    public float Height;
    
    public Plane(Vector3 normal, Vector3 position, float width, float height, Material material) : base(material)
    {
        Normal = normal;
        Position = position;
        Width = width;
        Height = height;
    }

    public override Intersection? Intersect(Ray ray)
    {
        float denom = Vector3.Dot(Normal, ray.Direction);
        if (Math.Abs(denom) < 1e-6) return null;
        
        float t = Vector3.Dot(Position - ray.Origin, Normal) / denom;
        if (t < 0) return null;
        Vector3 hit = ray.Origin + t * ray.Direction;
        if (hit.X < Position.X - Width / 2 || hit.X > Position.X + Width / 2 ||
            hit.Z < Position.Z - Height / 2 || hit.Z > Position.Z + Height / 2)
            return null;
        
        return new Intersection(hit, denom < 0 ? Normal : -Normal, t, this);
    }
}
