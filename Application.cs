using System.Drawing;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace GR2024;

class Application
{
    Raytracer _raytracer;
    
    public Application(Surface screen)
    {
        // Build the scene.
        Scene scene = new();
        scene.Primitives.Add(new Sphere(new Vector3(-1.5f, 0, 5), 1, Material.Mirror));
        scene.Primitives.Add(new Sphere(new Vector3(1.5f, 0, 5), 1, Material.Earth));
        scene.Primitives.Add(new Sphere(new Vector3(0, -0.5f, 7), 0.5f, Material.Polished));
        scene.Primitives.Add(new Sphere(Vector3.Zero, 100f, Material.Sky));
        scene.Primitives.Add(new Plane(new Vector3(0, -1, 5), Vector3.UnitX, Vector3.UnitZ, 10, 10, Material.Rocks));
        scene.Lights.Add(new PointLight(new Vector3(2, 5, 1), new Color(170, 170, 170)));
        scene.Lights.Add(new PointLight(new Vector3(-2, 5, 1), new Color(170, 170, 170)));
        
        // Resolution used by the camera. If this resolution is smaller than the screen resolution, the rest of the
        // width is used for the debug view.
        Vector2i res = new(640, 480);
        Camera camera = new(res, 1f);
        camera.Move(2 * Vector3.UnitY - Vector3.UnitZ);
        camera.Rotate(camera.Right, 20);
        _raytracer = new Raytracer(screen, camera, scene, res);
    }
    public void Init()
    {
        _raytracer.Render();
    }
    /// <summary>
    /// Translates or rotates camera based on keyboard input.
    /// </summary>
    /// <param name="kbs"></param>
    public void OnKeypress(KeyboardState kbs) {
        int speed = 1;
        if (kbs.IsKeyDown(Keys.LeftShift)) speed = 3;
        if (kbs.IsKeyPressed(Keys.A)) {
            _raytracer.Camera.Move(speed * -_raytracer.Camera.Right);
            _raytracer.Render();
        }
        if (kbs.IsKeyPressed(Keys.W)) {
            _raytracer.Camera.Move(speed * _raytracer.Camera.Target);
            _raytracer.Render();
        }
        if (kbs.IsKeyPressed(Keys.S)) {
            _raytracer.Camera.Move(speed * -_raytracer.Camera.Target);
            _raytracer.Render();
        }
        if (kbs.IsKeyPressed(Keys.D)) {
            _raytracer.Camera.Move(speed * _raytracer.Camera.Right);
            _raytracer.Render();
        }
        if (kbs.IsKeyPressed(Keys.Z)) {
            _raytracer.Camera.Move(speed * _raytracer.Camera.Up);
            _raytracer.Render();
        }
        if (kbs.IsKeyPressed(Keys.X)) {
            _raytracer.Camera.Move(speed * -_raytracer.Camera.Up);
            _raytracer.Render();
        }
        if (kbs.IsKeyPressed(Keys.Q)) {
            _raytracer.Camera.Rotate(_raytracer.Camera.Up, speed * -10);
            _raytracer.Render();
        }
        if (kbs.IsKeyPressed(Keys.E)) {
            _raytracer.Camera.Rotate(_raytracer.Camera.Up, speed * 10);
            _raytracer.Render();
        }
        if (kbs.IsKeyPressed(Keys.Left)) {
            _raytracer.Camera.Rotate(_raytracer.Camera.Target, speed * 10);
            _raytracer.Render();
        }
        if (kbs.IsKeyPressed(Keys.Right)) {
            _raytracer.Camera.Rotate(_raytracer.Camera.Target, speed * -10);
            _raytracer.Render();
        }
        if (kbs.IsKeyPressed(Keys.Up)) {
            _raytracer.Camera.Rotate(_raytracer.Camera.Right, speed * -10);
            _raytracer.Render();
        }
        if (kbs.IsKeyPressed(Keys.Down)) {
            _raytracer.Camera.Rotate(_raytracer.Camera.Right, speed * 10);
            _raytracer.Render();
        }
    }

    /// <summary>
    /// Tick renders one frame.
    /// </summary>
    public void Tick()
    {
        
    }
}