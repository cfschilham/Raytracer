using OpenTK.Mathematics;

namespace Raytracer;

public class Camera(Vector3 position, Vector3 target, Vector3 up, Vector2i resolution, float focalLength)
{
    public Vector3 Position = position;
    public Vector3 Target = target;
    public Vector3 Up = up;
    public Vector2i Resolution = resolution;
    public float FocalLength = focalLength;
    public float AspectRatio => Resolution.X / (float)Resolution.Y;
    public float ScreenPlaneWidth => AspectRatio; // Height is always 1. Width is the aspect ratio * 1 = aspect ratio.
    public Vector3 Right => Vector3.Cross(Up, Target).Normalized();

    public Camera(Vector2i resolution, float focalLength) : this(Vector3.Zero, Vector3.UnitZ, Vector3.UnitY, resolution, focalLength) { }
    
    /// <summary>
    /// Returns a ray that passes through the pixel at (x, y) in screen space.
    /// </summary>
    public Ray GetRay(int x, int y)
    {
        if (x >= Resolution.X || y >= Resolution.Y || x < 0 || y < 0)
            throw new ArgumentException("Pixel coordinates out of bounds");
        
        float dx = x / (Resolution.X - 1.0f) - 0.5f;
        dx *= Resolution.X / (float)Resolution.Y; // Adjust for aspect ratio
        float dy = y / (Resolution.Y - 1.0f) - 0.5f;
        
        // Flip y-axis, y = 0 is the pixel at the top and increasing y goes down. This means our image would otherwise
        // be flipped.
        dy = -dy;
        Vector3 direction = Vector3.Normalize(FocalLength * Target + Right * dx + Up * dy);
        return new Ray(Position, direction);
    }
    
    /// <summary>
    /// Moves the camera by delta.
    /// </summary>
    public void Move(Vector3 delta) =>
        Position += delta;
    
    /// <summary>
    /// Rotates the camera around the target by the given axis and angle in degrees.
    /// </summary>
    public void Rotate(Vector3 axis, float angle)
    {
        Quaternion quat = Quaternion.FromAxisAngle(axis, 2 * angle * (float)Math.PI / 360);
        Target = Vector3.Transform(Target, quat);
        Up = Vector3.Transform(Up, quat);
    }
    
    /// <summary>
    /// Set FOV in degrees
    /// </summary>
    public void SetFOV(float angle) =>
        FocalLength = 1 / MathF.Tan(2 * angle * (float)Math.PI / 360);
}
