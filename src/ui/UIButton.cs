using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace game_mono.ui;

public class UIButton : UIElement
{
    public string Text { get; set; }
    public SpriteFont Font { get; set; }
    public Color NormalColor { get; set; } = Color.White;
    public Color HoverColor { get; set; } = Color.Yellow;
    public Color PressedColor { get; set; } = Color.Orange;
    public Action OnClick { get; set; }
    
    // Button state management
    public bool IsHovered { get; private set; }
    public bool IsPressed { get; private set; }
    public bool IsSelected { get; set; } // For keyboard navigation
    
    private Rectangle _bounds;
    private MouseState _previousMouseState;
    private MouseState _currentMouseState;

    public UIButton(string text, SpriteFont font, Vector2 position, Action onClick = null)
    {
        Text = text;
        Font = font;
        Position = position;
        OnClick = onClick;
        UpdateBounds();
    }

    private void UpdateBounds()
    {
        if (Font != null && !string.IsNullOrEmpty(Text))
        {
            var textSize = Font.MeasureString(Text);
            _bounds = new Rectangle((int)Position.X, (int)Position.Y, (int)textSize.X, (int)textSize.Y);
        }
    }

    public override void Update(GameTime gameTime)
    {
        if (!IsVisible) return;

        UpdateBounds();
        
        _previousMouseState = _currentMouseState;
        _currentMouseState = Mouse.GetState();

        // Mouse interaction
        var mousePoint = new Point(_currentMouseState.X, _currentMouseState.Y);
        bool wasHovered = IsHovered;
        IsHovered = _bounds.Contains(mousePoint);

        // Handle mouse clicks
        if (IsHovered)
        {
            IsPressed = _currentMouseState.LeftButton == ButtonState.Pressed;
            
            // Click detection (button was pressed and now released)
            if (_previousMouseState.LeftButton == ButtonState.Pressed && 
                _currentMouseState.LeftButton == ButtonState.Released)
            {
                OnClick?.Invoke();
            }
        }
        else
        {
            IsPressed = false;
        }

        // Update selection state for hover (can be overridden by keyboard navigation)
        if (IsHovered && !wasHovered)
        {
            IsSelected = true;
        }
        else if (!IsHovered && wasHovered)
        {
            IsSelected = false;
        }
    }

    public void TriggerClick()
    {
        OnClick?.Invoke();
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        if (!IsVisible || string.IsNullOrEmpty(Text)) return;

        Color textColor = NormalColor;
        
        if (IsPressed)
            textColor = PressedColor;
        else if (IsHovered || IsSelected)
            textColor = HoverColor;

        // Add slight scaling effect for selected/hovered buttons
        float scale = (IsHovered || IsSelected) ? 1.1f : 1.0f;
        
        Vector2 origin = Font.MeasureString(Text) * 0.5f;
        Vector2 drawPosition = Position + origin;

        spriteBatch.DrawString(Font, Text, drawPosition, textColor, 0f, origin, scale, SpriteEffects.None, 0f);
    }
}