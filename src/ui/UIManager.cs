using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace game_mono.ui;

public class UIManager
{
    private readonly List<UIElement> _elements = new();

    public void AddElement(UIElement element)
    {
        _elements.Add(element);
    }

    public void ClearElements()
    {
        _elements.Clear();
    }

    public void Update(GameTime gameTime)
    {
        foreach (var element in _elements)
        {
            element.Update(gameTime);
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Begin();
        foreach (var element in _elements)
        {
            element.Draw(spriteBatch);
        }
        spriteBatch.End();
    }
}
