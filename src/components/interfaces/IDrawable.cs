using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace game_mono.interfaces;

public interface IDrawable
{
    void Draw(GraphicsDevice graphicsDevice, Matrix view, Matrix projection);
}
