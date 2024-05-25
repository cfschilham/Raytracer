namespace GR2024;

public abstract class Primitive(Material material)
{
    public Material Material = material;
    public abstract Intersection? Intersect(Ray ray);
    
}
