using OpenTK.Mathematics;

namespace GR2024;

public struct Scene(List<Primitive> primitives, List<Light> lights)
{
    public List<Primitive> Primitives = primitives;
    public List<Light> Lights = lights;
    public Scene() : this([], []) { }
}
