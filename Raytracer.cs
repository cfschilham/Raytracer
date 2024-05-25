using OpenTK.Mathematics;

namespace GR2024;

public class Raytracer(Surface screen, Camera camera, Scene scene)
{
    public Surface Screen = screen;
    public Camera Camera = camera;
    public Scene Scene = scene;
    
    public static Vector3 Reflect(Vector3 v, Vector3 n) => v - 2 * Vector3.Dot(v, n) * n;
    
    public void Render() {
        for (int y = 0; y < Screen.Height; y++) {
            for (int x = 0; x < Screen.Width; x++) {
                Ray ray = Camera.GetRay(x, y);
                Color color = TraceColor(ray, 5);
                Screen.Plot(x, y, color.ToInt());
            }
        }
    }

    public Intersection? Trace(Ray ray) {
        Intersection? closest = null;
        foreach (Primitive primitive in Scene.Primitives) {
            Intersection? intersection = primitive.Intersect(ray);
            if (intersection != null && (closest == null || intersection.Value.Distance < closest.Value.Distance)) {
                closest = intersection;
            }
        }

        return closest;
    }
    
    public Color TraceColor(Ray ray, int depth) {
        if (depth == 0) return Color.Black;
        
        Intersection? intsect = Trace(ray);
        if (intsect == null) return Color.Black;
        Primitive prim = intsect.Value.Primitive;

        Color specular = Color.Black;
        Color diffuse = Color.Black;
        foreach (PointLight light in Scene.Lights.Where(l => l is PointLight).Cast<PointLight>())
        {
            Vector3 lightDir = Vector3.Normalize(light.Position - intsect.Value.Point);
            
            // Attenuation factor for light intensity scales with the distance to the light. Uses inverse square law.
            float attenuation = 1 / ((light.Position - intsect.Value.Point).LengthSquared * 0.03f);
            
            // If there is an object between the current intersection and the light, the current intersection is in
            // shadow. Add a small offset to the point to avoid self-intersection.
            Intersection? shadowIntsect = Trace(new Ray(intsect.Value.Point + 0.001f * lightDir, lightDir));
            if (shadowIntsect != null && shadowIntsect.Value.Distance < (light.Position - intsect.Value.Point).Length)
                continue;
            
            // Angle factor for diffuse lighting scales with the angle of the light relative to the normal. Dot product
            // 0 is perpendicular, thus no light, 1 is parallel, thus full light.
            float angleFactor = Vector3.Dot(lightDir, intsect.Value.Normal);
            diffuse += prim.Material.Diffuse * light.Color * Math.Max(angleFactor, 0) * attenuation;
            
            // Specular lighting
            Vector3 reflectDir = Reflect(-lightDir, intsect.Value.Normal);
            Vector3 viewDir = Vector3.Normalize(ray.Origin - intsect.Value.Point);
            float specAngle = Vector3.Dot(reflectDir, viewDir);
            specular += prim.Material.Specular * light.Color * MathF.Pow(Math.Max(specAngle, 0), prim.Material.Shininess) * attenuation;
        }
        
        if (prim.Material.Reflectivity > 0) {
            Vector3 reflectDir = Reflect(ray.Direction, intsect.Value.Normal);
            Ray reflectRay = new Ray(intsect.Value.Point + 0.001f * reflectDir, reflectDir);
            Color reflectColor = TraceColor(reflectRay, depth - 1);
            specular += Color.Lerp(specular, reflectColor, prim.Material.Reflectivity);
        }

        Color ambient = prim.Material.Ambient * Light.Ambient;
        return ambient + diffuse + specular;
    }
}