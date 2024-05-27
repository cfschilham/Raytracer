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

    public bool DebugIntersect(Vector2 point, float margin)
    {
        // (x - center_x)^2 + (y - center_y)^2 = radius^2
        // (x - center_x)^2 + (y - center_y)^2 - radius^2 = 0
        // -margin < (x - center_x)^2 + (y - center_y)^2 - radius^2 < margin 
        return (point.X - Center.X) * (point.X - Center.X) + (point.Y - Center.Z) * (point.Y - Center.Z) - Radius * Radius < margin && 
               (point.X - Center.X) * (point.X - Center.X) + (point.Y - Center.Z) * (point.Y - Center.Z) - Radius * Radius > -margin;
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

        Vector3 normal = Vector3.Normalize(hit - Center);
        float theta = (float)Math.Acos(-normal.Y);
        float phi = (float)Math.Atan2(-normal.Z, normal.X) + (float)Math.PI;

        Vector2 uv = new(phi / (2 * (float)Math.PI), theta / (float)Math.PI);
        return new Intersection(hit, normal, t, this, uv);
    }
}
