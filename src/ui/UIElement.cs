using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace game_mono.ui;

public abstract class UIElement
{
    public Vector2 Position { get; set; }
    public bool IsVisible { get; set; } = true;

    public abstract void Update(GameTime gameTime);
    public abstract void Draw(SpriteBatch spriteBatch);
}
