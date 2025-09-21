using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace game_mono.ui;

public class UIText : UIElement
{
    public string Text { get; set; }
    public SpriteFont Font { get; set; }
    public Color Color { get; set; }

    public UIText(string text, SpriteFont font, Vector2 position, Color color)
    {
        Text = text;
        Font = font;
        Position = position;
        Color = color;
    }

    public override void Update(GameTime gameTime)
    {
        // Text elements often don't need per-frame updates,
        // but you could add logic here for animations (e.g., fade in/out).
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        if (IsVisible && !string.IsNullOrEmpty(Text))
        {
            spriteBatch.DrawString(Font, Text, Position, Color);
        }
    }
}
