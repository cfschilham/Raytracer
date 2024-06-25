namespace Raytracer;

public abstract class Primitive(Material material)
{
    public Material Material = material;
    
    /// <summary>
    /// Calculate the closest intersection between ray and this specific primitive
    /// </summary>
    public abstract Intersection? Intersect(Ray ray);
    
}
