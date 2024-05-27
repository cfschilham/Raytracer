using OpenTK.Mathematics;

namespace GR2024;

public class Raytracer(Surface screen, Camera camera, Scene scene, Vector2i renderResolution)
{
    public Surface Screen = screen;
    public Camera Camera = camera;
    public Scene Scene = scene;
    public Vector2i RenderResolution = renderResolution;
    
    static ThreadLocal<Random> _rand = new (() => new Random(Environment.TickCount));
    
    const float DebugSize = 20;
    private const float DebugMargin = DebugSize * 0.002f;

    public static Vector3 RandomUnit() =>
        new Vector3(_rand.Value.NextSingle(), _rand.Value.NextSingle(), _rand.Value.NextSingle()).Normalized();
    
    public static Vector3 Reflect(Vector3 v, Vector3 n) => v - 2 * Vector3.Dot(v, n) * n;
    
    /// <summary>
    /// Render one frame
    /// </summary>
    public void Render()
    {
        if (RenderResolution.Y != Screen.Height) throw new Exception("Render resolution must match screen height");
        List<(Ray, Intersection?)> debugRays = new();
        Mutex debugRaysMutex = new();

        Parallel.For(0, RenderResolution.Y, y => {
            for (int x = 0; x < RenderResolution.X; x++) {
                Ray ray = Camera.GetRay(x, y);
                Color color;
                if (y == RenderResolution.Y / 2 && x % 50 == 0) { // Debug ray, set list to non-null
                    debugRaysMutex.WaitOne();
                    color = TraceColor(ray, 5, Array.Empty<Primitive>(), debugRays);
                    debugRaysMutex.ReleaseMutex();
                }
                else color = TraceColor(ray, 5, Array.Empty<Primitive>());
                Screen.Plot(x, y, color.ToInt());
            }
        });
        
        // Draw debug window, start by clearing.
        for (int y = 0; y < RenderResolution.Y; y++)
            for (int x = RenderResolution.X; x < Screen.Width; x++)
                Screen.Plot(x, y, Color.Black.ToInt());
        
        // Draw spheres.
        foreach (Sphere sphere in Scene.Primitives.Where(p => p is Sphere).Cast<Sphere>()) {
            PlotCircle(GetDebugPixelPoint(sphere.Center.Xz), sphere.Radius, 20);
        }

        // Draw debug rays.
        foreach ((Ray, Intersection?) debugRay in debugRays) {
            Vector2i originPixel = GetDebugPixelPoint(debugRay.Item1.Origin.Xz);
            if (!IsPixelInDebugBounds(originPixel)) continue;
            
            Vector2i hitPixel = Vector2i.Zero;
            float t = debugRay.Item2 == null ? DebugSize : 0;
            // if (debugRay.Item2 != null) continue;
            while (!IsPixelInDebugBounds(hitPixel))
            {
                if (debugRay.Item2 == null) 
                    hitPixel = GetDebugPixelPoint(debugRay.Item1.Origin.Xz + t * debugRay.Item1.Direction.Xz);
                else hitPixel = GetDebugPixelPoint(debugRay.Item2.Value.Point.Xz + t * debugRay.Item1.Direction.Xz);
                t -= 0.01f;
            }
            Screen.Line(originPixel.X, originPixel.Y, hitPixel.X, hitPixel.Y, Color.Blue.ToInt());
        }
        
        // Draw camera point.
        Vector2i cameraPoint = GetDebugPixelPoint(Camera.Position.Xz);
        Screen.Bar(cameraPoint.X - 1, cameraPoint.Y - 1, cameraPoint.X + 1, cameraPoint.Y + 1, Color.Red.ToInt());
        
        // Draw screen plane.
        Vector2i screenPlaneLeft = GetDebugPixelPoint((Camera.Position + Camera.Target * Camera.FocalLength + Camera.Right * -0.5f * Camera.ScreenPlaneWidth).Xz);
        Vector2i screenPlaneRight = GetDebugPixelPoint((Camera.Position + Camera.Target * Camera.FocalLength + Camera.Right * 0.5f * Camera.ScreenPlaneWidth).Xz);
        Screen.Line(screenPlaneLeft.X, screenPlaneLeft.Y, screenPlaneRight.X, screenPlaneRight.Y, Color.Green.ToInt());
    }

    void PlotCircle(Vector2i point, float radius, int thickness)
    {
        int radiusPixels = (int)(radius * RenderResolution.Y / DebugSize);
        
        for (int x = -radiusPixels; x <= radiusPixels; x++) {
            if (x + point.X < 0 || x + point.X >= Screen.Width) continue;
            for (int y = -radiusPixels; y <= radiusPixels; y++) {
                if (y + point.Y < 0 || y + point.Y >= Screen.Height) continue;
                
                int r2 = radiusPixels * radiusPixels;
                if (x * x + y * y < r2 + thickness && x * x + y * y > r2 - thickness) 
                    Screen.Plot(x + point.X, y + point.Y, Color.White.ToInt());
            }
        }
    }
    
    bool IsPixelInDebugBounds(Vector2i pixel) =>
        pixel.X >= RenderResolution.X && pixel.X < Screen.Width && pixel.Y >= 0 && pixel.Y < RenderResolution.Y;
    
    Vector2i GetDebugPixelPoint(Vector2 point) => GetDebugPixelPoint(point.X, point.Y);
    
    Vector2i GetDebugPixelPoint(float x, float y)
    {
        float aspect = (Screen.Width - RenderResolution.X) / (float)RenderResolution.Y;
        x += DebugSize * aspect / 2;
        y += DebugSize / 2;
        x *= (Screen.Width - RenderResolution.X) / (DebugSize * aspect);
        y *= RenderResolution.Y / DebugSize;
        y = RenderResolution.Y - y; // Flip y-axis
        // x *= (Screen.Width - RenderResolution.X) / (float)Screen.Width; // Adjust for aspect ratio
        return new Vector2i((int)x + RenderResolution.X, (int)y);
    }
    
    /// <summary>
    /// Calculates closest intersection between primitive and ray
    /// </summary>
    Intersection? Trace(Ray ray) {
        Intersection? closest = null;
        foreach (Primitive primitive in Scene.Primitives) {
            Intersection? hit = primitive.Intersect(ray);
            if (hit != null && (closest == null || hit.Value.Distance < closest.Value.Distance)) {
                closest = hit;
            }
        }

        return closest;
    }
    
    Color TraceColor(Ray ray, int depth, Primitive[] ignore, List<(Ray, Intersection?)>? debugRays = null) {
        if (depth == 0) return Color.Black;
        
        Intersection? hit = Trace(ray);
        if (debugRays != null) debugRays.Add((ray, hit));
        if (hit == null) return Color.SkyBlue;
        Primitive prim = hit.Value.Primitive;
        if (ignore.Contains(prim)) return Color.Black;

        Color specular = Color.Black;
        Color diffuse = Color.Black;
        foreach (PointLight light in Scene.Lights.Where(l => l is PointLight).Cast<PointLight>())
        {
            Vector3 lightDir = Vector3.Normalize(light.Position - hit.Value.Point);
            
            // Attenuation factor for light intensity scales with the distance to the light. Uses inverse square law.
            float attenuation = 1 / ((light.Position - hit.Value.Point).LengthSquared * 0.03f);
            
            // If there is an object between the current intersection and the light, the current intersection is in
            // shadow. Add a small offset to the point to avoid self-intersection.
            Intersection? shadowHit = Trace(new Ray(hit.Value.Point + 0.05f * lightDir, lightDir));
            if (shadowHit != null && shadowHit.Value.Distance < (light.Position - hit.Value.Point).Length)
                continue;
            
            // Angle factor for diffuse lighting scales with the angle of the light relative to the normal. Dot product
            // 0 is perpendicular, thus no light, 1 is parallel, thus full light.
            float angleFactor = Vector3.Dot(lightDir, hit.Value.Normal);
            diffuse += prim.Material.Diffuse.Pixel(hit.Value.UV.X, hit.Value.UV.Y) * light.Color * Math.Max(angleFactor, 0) * attenuation;
            
            // Specular lighting
            Vector3 reflectDir = Reflect(-lightDir, hit.Value.Normal);
            Vector3 viewDir = Vector3.Normalize(ray.Origin - hit.Value.Point);
            float specAngle = Vector3.Dot(reflectDir, viewDir);
            specular += prim.Material.Specular.Pixel(hit.Value.UV.X, hit.Value.UV.Y) * light.Color * MathF.Pow(Math.Max(specAngle, 0), prim.Material.Shininess) * attenuation;
        }
        
        if (prim.Material.Reflectivity > 0) {
            Vector3 reflectDir = Reflect(ray.Direction, hit.Value.Normal);
            Ray reflectRay = new Ray(hit.Value.Point + 0.05f * reflectDir, reflectDir);

            Color reflectColor = TraceColor(reflectRay, depth - 1, ignore.Concat(new []{prim}).ToArray(), debugRays);
            
            if (prim.Material.Gloss > 0)
            {
                Span<Color> glossColors = stackalloc Color[200];
                for (int i = 0; i < 200; i++) {
                    Ray glossRay = reflectRay;
                    glossRay.Direction += prim.Material.Gloss * RandomUnit();
                    while (Vector3.Dot(glossRay.Direction, reflectRay.Direction) <= 0.1f) 
                        glossRay.Direction = reflectRay.Direction + prim.Material.Gloss * RandomUnit();
                    if (i%50 == 0) 
                        glossColors[i] = TraceColor(glossRay, depth - 1, ignore.Concat(new []{prim}).ToArray(), debugRays);
                    else glossColors[i] = TraceColor(glossRay, depth - 1, ignore.Concat(new []{prim}).ToArray());
                }
                reflectColor = Color.Avg(glossColors);
            }
            specular += reflectColor * prim.Material.Reflectivity; 
        }

        Color ambient = prim.Material.Ambient.Pixel(hit.Value.UV.X, hit.Value.UV.Y) * Light.Ambient;
        return (ambient + diffuse + specular) * prim.Material.Gamma;
    }
}