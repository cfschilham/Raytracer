namespace GR2024;

public struct Material(Color ambient, Color diffuse, Color specular, float shininess, float reflectivity = 0.0f)
{
    public Color Ambient = ambient;
    public Color Diffuse= diffuse;
    public Color Specular = specular;
    public float Shininess = shininess;
    public float Reflectivity = Math.Min(reflectivity, 1.0f);
    
    public static Material Default = new(new Color(0.7f, 0.7f, 0.7f),
        new Color(0.7f, 0.7f, 0.7f),
        new Color(0.1f, 0.1f, 0.1f), 2.0f);
    
    public static Material Mirror = new(new Color(0.1f, 0.1f, 0.1f),
        new Color(0.1f, 0.1f, 0.1f),
        new Color(1.0f, 1.0f, 1.0f), 100.0f, 0.65f);
}
