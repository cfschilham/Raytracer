using OpenTK.Mathematics;

namespace GR2024;

public class Camera(Vector3 position, Vector3 target, Vector3 up, Vector2i resolution, float focalLength)
{
    public Vector3 Position = position;
    public Vector3 Target = target;
    public Vector3 Up = up;
    public Vector2i Resolution = resolution;
    public float FocalLength = focalLength;

    public Camera(Vector2i resolution, float focalLength) : this(Vector3.Zero, Vector3.UnitZ, Vector3.UnitY, resolution, focalLength) { }
    
    public Ray GetRay(int x, int y)
    {
        if (x >= Resolution.X || y >= Resolution.Y || x < 0 || y < 0)
            throw new ArgumentException("Pixel coordinates out of bounds");
        
        Vector3 right = Vector3.Normalize(Vector3.Cross(Target, Up));
        float dx = x / (Resolution.X - 1.0f) - 0.5f;
        dx *= Resolution.X / (float)Resolution.Y; // Adjust for aspect ratio
        float dy = y / (Resolution.Y - 1.0f) - 0.5f;
        
        // Flip y-axis, y = 0 is the pixel at the top and increasing y goes down. This means our image would otherwise
        // be flipped.
        dy = -dy;
        Vector3 direction = Vector3.Normalize(FocalLength * Target + right * dx + Up * dy);
        return new Ray(Position, direction);
    }
    
    // Set FOV in degrees
    public void SetFOV(float fov) =>
        FocalLength = 1 / MathF.Tan(fov * (float)Math.PI / 360);
}
