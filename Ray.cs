using OpenTK.Mathematics;

namespace GR2024;

public struct Ray(Vector3 origin, Vector3 direction)
{
    public Vector3 Origin = origin;
    public Vector3 Direction = direction;
}
