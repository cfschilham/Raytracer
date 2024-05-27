using OpenTK.Mathematics;

namespace GR2024;

public class Raytracer(Surface screen, Camera camera, Scene scene, Vector2i renderResolution)
{
    public Surface Screen = screen;
    public Camera Camera = camera;
    public Scene Scene = scene;
    public Vector2i RenderResolution = renderResolution;
    
    static ThreadLocal<Random> _rand = new (() => new Random(Environment.TickCount));
    
    const int DebugSize = 20;
    private const float DebugMargin = DebugSize * 0.002f;

    public static Vector3 RandomUnit() =>
        new Vector3(_rand.Value.NextSingle(), _rand.Value.NextSingle(), _rand.Value.NextSingle()).Normalized();
    
    public static Vector3 Reflect(Vector3 v, Vector3 n) => v - 2 * Vector3.Dot(v, n) * n;
    /// <summary>
    /// renders one frame
    /// </summary>
    /// <exception cref="Exception"></exception>
    public void Render()
    {
        if (RenderResolution.Y != Screen.Height) throw new Exception("Render resolution must match screen height");
        List<Ray> debugRays = new();
        Mutex debugRaysMutex = new();

        Parallel.For(0, RenderResolution.Y, y => {
            for (int x = 0; x < RenderResolution.X; x++) {
                Ray ray = Camera.GetRay(x, y);
                Color color = TraceColor(ray, 5, Array.Empty<Primitive>());
                Screen.Plot(x, y, color.ToInt());

                if (y != RenderResolution.Y / 2 || x % 50 != 0) continue;
                debugRaysMutex.WaitOne();
                debugRays.Add(ray);
                debugRaysMutex.ReleaseMutex();
            }
        });

        // Debug window
        Parallel.For(0, RenderResolution.Y, y => {
            for (int x = RenderResolution.X; x < Screen.Width; x++) {
                // Convert pixel coordinates to world coordinates, debug window is 10x10.
                Vector2 point = GetDebugPoint(x, y);
                Color debugColor = TraceDebugColor(point, debugRays);
                Screen.Plot(x, y, debugColor.ToInt());
            }
        });

    }
    

    Vector2 GetDebugPoint(int x, int y)
    {
        float xt = DebugSize * (float)(x - RenderResolution.X) / (Screen.Width - RenderResolution.X - 1);
        float yt = DebugSize * (float)y / (RenderResolution.Y - 1);
        yt = DebugSize - yt; // Flip y-axis
        yt -= DebugSize / 2f; // Center
        xt -= DebugSize / 2f; // Center
                
        float aspect = (Screen.Width - RenderResolution.X) / (float)RenderResolution.Y;
        xt *= aspect;
        return new Vector2(xt, yt);
    }

    Color TraceDebugColor(Vector2 point, List<Ray> debugRays)
    {
        // Draw spheres. Spheres are always drawn with full radius and center point vec2(p.x, p.z) where p = vec3 of
        // sphere center. This means that a sphere is always drawn regardless of height in y direction in 3D.
        foreach (Sphere sphere in Scene.Primitives.Where(p => p is Sphere).Cast<Sphere>()) {
            if (sphere.DebugIntersect(point, DebugMargin)) return Color.White;
        }
        
        // Draw the camera point. Just like spheres, camera is always drawn regardless of Y height.
        if ((point - new Vector2(Camera.Position.X, Camera.Position.Z)).Length < DebugMargin * 2) return Color.Red;
        
        // Draw the screen plane.
        Vector3 screenPlaneLeftPoint = Camera.Position + Camera.Target * Camera.FocalLength + Camera.Right * -0.5f * Camera.ScreenPlaneWidth;
        float dp = Vector2.Dot((point - screenPlaneLeftPoint.Xz).Normalized(), Camera.Right.Xz.Normalized());
        float t = Vector2.Dot(point - screenPlaneLeftPoint.Xz, Camera.Right.Xz.Normalized());
        if (dp < 1 + DebugMargin * (1/(t*40)) && dp > 1 - DebugMargin * (1/(t*40))) {
            if (t > 0 && t < Camera.ScreenPlaneWidth) return Color.Green;
        }
        
        // Draw debug rays
        foreach (Ray debugRay in debugRays) {
            Intersection? intsect = Trace(debugRay);
            
            dp = Vector2.Dot((point - debugRay.Origin.Xz).Normalized(), debugRay.Direction.Xz.Normalized());
            float rayLength = (debugRay.Origin.Xz - point).Length;
            if (dp > 1 + DebugMargin * (0.01/rayLength) || dp < 1 - DebugMargin * (0.01/rayLength)) continue;
            if (intsect == null) return Color.Blue;
            
            float intsectDistance = (intsect.Value.Point.Xz - debugRay.Origin.Xz).Length;
            t = Vector2.Dot(point - debugRay.Origin.Xz, debugRay.Direction.Xz.Normalized());
            if (t > 0 && t < intsectDistance) return Color.Blue;
        }
        
        return Color.Black;
    }
    /// <summary>
    /// calculates closest intersection between primitive and ray
    /// </summary>
    /// <param name="ray"></param>
    /// <returns></returns>
    Intersection? Trace(Ray ray) {
        Intersection? closest = null;
        foreach (Primitive primitive in Scene.Primitives) {
            Intersection? intersection = primitive.Intersect(ray);
            if (intersection != null && (closest == null || intersection.Value.Distance < closest.Value.Distance)) {
                closest = intersection;
            }
        }

        return closest;
    }
    /// <summary>
    /// calculate color based on material and all lights and (most) primitives in scene
    /// </summary>
    /// <param name="ray"></param>
    /// <param name="depth"></param>
    /// <param name="ignore"></param>
    /// <returns></returns>
    Color TraceColor(Ray ray, int depth, Primitive[] ignore) {
        if (depth == 0) return Color.Black;
        
        Intersection? intsect = Trace(ray);
        if (intsect == null) return Color.SkyBlue;
        Primitive prim = intsect.Value.Primitive;
        if (ignore.Contains(prim)) return Color.Black;

        Color specular = Color.Black;
        Color diffuse = Color.Black;
        foreach (PointLight light in Scene.Lights.Where(l => l is PointLight).Cast<PointLight>())
        {
            Vector3 lightDir = Vector3.Normalize(light.Position - intsect.Value.Point);
            
            // Attenuation factor for light intensity scales with the distance to the light. Uses inverse square law.
            float attenuation = 1 / ((light.Position - intsect.Value.Point).LengthSquared * 0.03f);
            
            // If there is an object between the current intersection and the light, the current intersection is in
            // shadow. Add a small offset to the point to avoid self-intersection.
            Intersection? shadowIntsect = Trace(new Ray(intsect.Value.Point + 0.05f * lightDir, lightDir));
            if (shadowIntsect != null && shadowIntsect.Value.Distance < (light.Position - intsect.Value.Point).Length)
                continue;
            
            // Angle factor for diffuse lighting scales with the angle of the light relative to the normal. Dot product
            // 0 is perpendicular, thus no light, 1 is parallel, thus full light.
            float angleFactor = Vector3.Dot(lightDir, intsect.Value.Normal);
            diffuse += prim.Material.Diffuse.Pixel(intsect.Value.UV.X, intsect.Value.UV.Y) * light.Color * Math.Max(angleFactor, 0) * attenuation;
            
            // Specular lighting
            Vector3 reflectDir = Reflect(-lightDir, intsect.Value.Normal);
            Vector3 viewDir = Vector3.Normalize(ray.Origin - intsect.Value.Point);
            float specAngle = Vector3.Dot(reflectDir, viewDir);
            specular += prim.Material.Specular.Pixel(intsect.Value.UV.X, intsect.Value.UV.Y) * light.Color * MathF.Pow(Math.Max(specAngle, 0), prim.Material.Shininess) * attenuation;
        }
        
        if (prim.Material.Reflectivity > 0) {
            Vector3 reflectDir = Reflect(ray.Direction, intsect.Value.Normal);
            Ray reflectRay = new Ray(intsect.Value.Point + 0.05f * reflectDir, reflectDir);

            Color reflectColor = TraceColor(reflectRay, depth - 1, ignore.Concat(new []{prim}).ToArray());
            
            if (prim.Material.Gloss > 0)
            {
                Span<Color> glossColors = stackalloc Color[200];
                for (int i = 0; i < 200; i++) {
                    Ray glossRay = reflectRay;
                    glossRay.Direction += prim.Material.Gloss * RandomUnit();
                    while (Vector3.Dot(glossRay.Direction, reflectRay.Direction) <= 0.1f) 
                        glossRay.Direction = reflectRay.Direction + prim.Material.Gloss * RandomUnit();
                    glossColors[i] = TraceColor(glossRay, depth - 1, ignore.Concat(new []{prim}).ToArray());
                }
                reflectColor = Color.Avg(glossColors);
            }
            specular += reflectColor * prim.Material.Reflectivity; 
        }

        Color ambient = prim.Material.Ambient.Pixel(intsect.Value.UV.X, intsect.Value.UV.Y) * Light.Ambient;
        return (ambient + diffuse + specular) * prim.Material.Gamma;
    }
}