using OpenTK.Mathematics;

namespace Raytracer;

public struct Scene(List<Primitive> primitives, List<Light> lights)
{
    public List<Primitive> Primitives = primitives;
    public List<Light> Lights = lights;
    public Scene() : this([], []) { }
}
