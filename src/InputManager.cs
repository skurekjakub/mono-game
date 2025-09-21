using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

namespace game_mono
{
    /// <summary>
    /// Centralized input management system for clean handling of keyboard and mouse input
    /// </summary>
    public class InputManager
    {
        // Input state tracking
        private KeyboardState _currentKeyboardState;
        private KeyboardState _previousKeyboardState;
        private MouseState _currentMouseState;
        private MouseState _previousMouseState;
        
        // Mouse capture state
        public bool IsMouseCaptured { get; private set; }
        private Point _screenCenter;
        private Game _game;
        
        // Mouse movement tracking
        public Vector2 MouseDelta { get; private set; }
        public Vector2 MousePosition => new Vector2(_currentMouseState.X, _currentMouseState.Y);
        
        public InputManager(Game game)
        {
            _game = game ?? throw new ArgumentNullException(nameof(game));
            
            // Initialize input states
            _currentKeyboardState = Keyboard.GetState();
            _previousKeyboardState = _currentKeyboardState;
            _currentMouseState = Mouse.GetState();
            _previousMouseState = _currentMouseState;
            
            // Calculate screen center for mouse capture
            UpdateScreenCenter();
            
            // Start with mouse captured for FPS controls
            SetMouseCapture(true);
        }
        
        /// <summary>
        /// Updates input states and calculates mouse delta. Call once per frame.
        /// </summary>
        public void Update()
        {
            // Store previous states
            _previousKeyboardState = _currentKeyboardState;
            _previousMouseState = _currentMouseState;
            
            // Get current states
            _currentKeyboardState = Keyboard.GetState();
            _currentMouseState = Mouse.GetState();
            
            // Calculate mouse delta and handle capture
            if (IsMouseCaptured)
            {
                // Calculate delta from the center of the screen
                MouseDelta = new Vector2(
                    _currentMouseState.X - _screenCenter.X,
                    _currentMouseState.Y - _screenCenter.Y
                );
                
                // Reset mouse to the center of the screen for continuous movement
                Mouse.SetPosition(_screenCenter.X, _screenCenter.Y);
            }
            else
            {
                MouseDelta = Vector2.Zero;
            }
            
            // Toggle mouse capture with Tab key
            if (IsKeyPressed(Keys.Tab))
            {
                SetMouseCapture(!IsMouseCaptured);
            }
        }
        
        /// <summary>
        /// Checks if a key is currently pressed
        /// </summary>
        public bool IsKeyDown(Keys key)
        {
            return _currentKeyboardState.IsKeyDown(key);
        }
        
        /// <summary>
        /// Checks if a key was just pressed this frame
        /// </summary>
        public bool IsKeyPressed(Keys key)
        {
            return _currentKeyboardState.IsKeyDown(key) && !_previousKeyboardState.IsKeyDown(key);
        }
        
        /// <summary>
        /// Checks if a key was just released this frame
        /// </summary>
        public bool IsKeyReleased(Keys key)
        {
            return !_currentKeyboardState.IsKeyDown(key) && _previousKeyboardState.IsKeyDown(key);
        }
        
        /// <summary>
        /// Enables or disables mouse capture mode
        /// </summary>
        public void SetMouseCapture(bool isCaptured)
        {
            IsMouseCaptured = isCaptured;
            _game.IsMouseVisible = !isCaptured;
            
            if (IsMouseCaptured)
            {
                // When capturing, center the mouse immediately
                Mouse.SetPosition(_screenCenter.X, _screenCenter.Y);
                
                // Reset previous state to avoid a large jump on the first frame
                _previousMouseState = Mouse.GetState();
            }
        }
        
        /// <summary>
        /// Updates the screen center. Call when window size changes.
        /// </summary>
        public void UpdateScreenCenter()
        {
            _screenCenter = new Point(
                _game.GraphicsDevice.Viewport.Width / 2,
                _game.GraphicsDevice.Viewport.Height / 2
            );
        }

        // --- Mouse Button Handling ---
        
        /// <summary>
        /// Checks if a mouse button is currently pressed
        /// </summary>
        public bool IsMouseButtonDown(MouseButton button)
        {
            return button switch
            {
                MouseButton.Left => _currentMouseState.LeftButton == ButtonState.Pressed,
                MouseButton.Right => _currentMouseState.RightButton == ButtonState.Pressed,
                MouseButton.Middle => _currentMouseState.MiddleButton == ButtonState.Pressed,
                _ => false
            };
        }
        
        /// <summary>
        /// Checks if a mouse button was just pressed this frame
        /// </summary>
        public bool IsMouseButtonPressed(MouseButton button)
        {
            return button switch
            {
                MouseButton.Left => _currentMouseState.LeftButton == ButtonState.Pressed &&
                                   _previousMouseState.LeftButton == ButtonState.Released,
                MouseButton.Right => _currentMouseState.RightButton == ButtonState.Pressed &&
                                    _previousMouseState.RightButton == ButtonState.Released,
                MouseButton.Middle => _currentMouseState.MiddleButton == ButtonState.Pressed &&
                                     _previousMouseState.MiddleButton == ButtonState.Released,
                _ => false
            };
        }
        
        /// <summary>
        /// Checks if a mouse button was just released this frame
        /// </summary>
        public bool IsMouseButtonReleased(MouseButton button)
        {
            return button switch
            {
                MouseButton.Left => _currentMouseState.LeftButton == ButtonState.Released &&
                                   _previousMouseState.LeftButton == ButtonState.Pressed,
                MouseButton.Right => _currentMouseState.RightButton == ButtonState.Released &&
                                    _previousMouseState.RightButton == ButtonState.Pressed,
                MouseButton.Middle => _currentMouseState.MiddleButton == ButtonState.Released &&
                                     _previousMouseState.MiddleButton == ButtonState.Pressed,
                _ => false
            };
        }
        
        /// <summary>
        /// Gets the mouse scroll wheel delta
        /// </summary>
        public int ScrollWheelDelta => _currentMouseState.ScrollWheelValue - _previousMouseState.ScrollWheelValue;
        
        /// <summary>
        /// Gets movement input as a Vector2 (for 2D movement like strafing)
        /// </summary>
        public Vector2 GetMovementInput()
        {
            Vector2 movement = Vector2.Zero;
            
            if (IsKeyDown(Keys.W)) movement.Y += 1;
            if (IsKeyDown(Keys.S)) movement.Y -= 1;
            if (IsKeyDown(Keys.A)) movement.X -= 1;
            if (IsKeyDown(Keys.D)) movement.X += 1;
            
            // Normalize diagonal movement
            if (movement.Length() > 1)
                movement.Normalize();
                
            return movement;
        }
        
        /// <summary>
        /// Gets vertical movement input (Spacebar/Q for up, E for down)
        /// </summary>
        public float GetVerticalInput()
        {
            float vertical = 0;
            if (IsKeyDown(Keys.Space) || IsKeyDown(Keys.Q)) vertical += 1;
            if (IsKeyDown(Keys.E)) vertical -= 1;
            return vertical;
        }
    }
    
    /// <summary>
    /// Enumeration for mouse buttons
    /// </summary>
    public enum MouseButton
    {
        Left,
        Right,
        Middle
    }
}