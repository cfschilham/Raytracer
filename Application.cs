using System.Drawing;
using OpenTK.Mathematics;

namespace GR2024;

class Application
{
    Raytracer _raytracer;

    public Application(Surface screen)
    {
        Scene scene = new();
        scene.Primitives.Add(new Sphere(new Vector3(0, 0, 5), 1, Material.Mirror));
        scene.Primitives.Add(new Sphere(new Vector3(2.5f, 0, 5), 1, Material.Default));
        scene.Primitives.Add(new Plane(Vector3.UnitY, new Vector3(0, -1, 5), 10, 10, Material.Default));
        scene.Lights.Add(new PointLight(new Vector3(2, 5, 1), new Color(170, 170, 170)));
        // scene.Lights.Add(new PointLight(new Vector3(-2, 5, 1), new Color(170, 170, 170)));
        Camera camera = new(new Vector2i(640, 480), 1f);
        _raytracer = new Raytracer(screen, camera, scene);
    }
    public void Init()
    {
        _raytracer.Render();
    }

    // Tick renders one frame.
    public void Tick()
    {
        // _raytracer.Camera.Position += new Vector3(0.01f, 0f, 0f);
        // _raytracer.Render();
    }
}