using Microsoft.Xna.Framework;

namespace game_mono.components;

public class Transform : Component
{
    public Vector3 Position { get; set; } = Vector3.Zero;
    public Vector3 Rotation { get; set; } = Vector3.Zero; // Euler angles in radians
    public Vector3 Scale { get; set; } = Vector3.One;

    public Matrix WorldMatrix { get; private set; } = Matrix.Identity;

    public void UpdateMatrix()
    {
        WorldMatrix = Matrix.CreateScale(Scale) *
                      Matrix.CreateFromYawPitchRoll(Rotation.Y, Rotation.X, Rotation.Z) *
                      Matrix.CreateTranslation(Position);
    }
}
