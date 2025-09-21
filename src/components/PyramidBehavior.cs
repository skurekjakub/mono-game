using game_mono.interfaces;
using Microsoft.Xna.Framework;

namespace game_mono.components;

public class PyramidBehavior : Component, IUpdatable
{
    public Vector3 RotationSpeed { get; set; } = Vector3.Zero;
    public Vector3 Velocity { get; set; } = Vector3.Zero;

    public void Update(GameTime gameTime)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        GameObject.Transform.Position += Velocity * deltaTime;
        GameObject.Transform.Rotation += RotationSpeed * deltaTime;
    }
}
