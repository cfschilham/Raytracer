namespace GR2024;

public abstract class Primitive(Material material)
{
    public Material Material = material;
    /// <summary>
    /// calculate closest intersection between ray and primitive
    /// </summary>
    /// <param name="ray"></param>
    /// <returns></returns>
    public abstract Intersection? Intersect(Ray ray);
    
}
