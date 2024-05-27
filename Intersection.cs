using OpenTK.Mathematics;

namespace GR2024;

public struct Intersection(Vector3 point, Vector3 normal, float distance, Primitive primitive, Vector2 uv)
{
    public Vector3 Point = point;
    public Vector3 Normal = normal;
    public float Distance = distance;
    public Primitive Primitive = primitive;
    public Vector2 UV = uv; // Texture coordinates.
}
