using OpenTK.Mathematics;

namespace GR2024;

public class Sphere : Primitive
{
    public Vector3 Center;
    public float Radius;

    public Sphere(Vector3 center, float radius, Material material) : base(material)
    {
        Center = center;
        Radius = radius;
    }
    
    public override Intersection? Intersect(Ray ray)
    {
        Vector3 oc = ray.Origin - Center;
        float a = ray.Direction.LengthSquared;
        float b = 2 * Vector3.Dot(ray.Direction, oc);
        float c = oc.LengthSquared - Radius * Radius;
        float discriminant = b * b - 4 * a * c;
        if (discriminant < 0)
        {
            return null;
        }
        float t = (-b - MathF.Sqrt(discriminant)) / (2 * a);
        if (t < 0)
        {
            t = (-b + MathF.Sqrt(discriminant)) / (2 * a);
            if (t < 0) return null;
        }
        Vector3 hit = ray.Origin + t * ray.Direction;
        return new Intersection(hit, Vector3.Normalize(hit - Center), t, this);
    }
}
