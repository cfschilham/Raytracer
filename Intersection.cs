using OpenTK.Mathematics;

namespace GR2024;

public struct Intersection(Vector3 point, Vector3 normal, float distance, Primitive primitive)
{
    public Vector3 Point = point;
    public Vector3 Normal = normal;
    public float Distance = distance;
    public Primitive Primitive = primitive;
}
