using System.Collections.Generic;
using game_mono.ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace game_mono.screens;

public class MenuScreen : GameScreen
{
    private SpriteBatch _spriteBatch;
    private SpriteFont _titleFont;
    private SpriteFont _buttonFont;
    private UIManager _uiManager;
    private UIText _titleText;
    private List<UIButton> _menuButtons;
    private ScreenManager _screenManager;
    
    // Keyboard navigation
    private int _selectedButtonIndex = 0;
    private KeyboardState _previousKeyboardState;
    
    // Visual effects
    private Texture2D _overlayTexture;
    private bool _isOverlay = false; // True when shown over gameplay (paused state)

    public MenuScreen(ScreenManager screenManager)
    {
        _screenManager = screenManager;
        _menuButtons = new List<UIButton>();
    }

    public override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _uiManager = new UIManager();
        
        // Load fonts (using the existing InstructionsFont as a fallback)
        _titleFont = Content.Load<SpriteFont>("InstructionsFont");
        _buttonFont = Content.Load<SpriteFont>("InstructionsFont");

        // Create overlay texture for paused state
        _overlayTexture = new Texture2D(GraphicsDevice, 1, 1);
        _overlayTexture.SetData(new[] { Color.Black });

        SetupUI();
    }

    private void SetupUI()
    {
        var screenCenter = new Vector2(GraphicsDevice.Viewport.Width / 2f, GraphicsDevice.Viewport.Height / 2f);
        
        // Title
        _titleText = new UIText("MonoGame 3D Demo", _titleFont, 
            new Vector2(screenCenter.X - _titleFont.MeasureString("MonoGame 3D Demo").X / 2, 100), 
            Color.CornflowerBlue);
        _uiManager.AddElement(_titleText);

        // Clear existing buttons
        _menuButtons.Clear();
        
        // Create menu buttons based on current state
        UpdateMenuButtons();
    }

    private void UpdateMenuButtons()
    {
        // Clear existing buttons from UI manager
        _uiManager.ClearElements();
        _uiManager.AddElement(_titleText);
        
        _menuButtons.Clear();
        
        var screenCenter = new Vector2(GraphicsDevice.Viewport.Width / 2f, GraphicsDevice.Viewport.Height / 2f);
        float buttonSpacing = 60f;
        float startY = screenCenter.Y - 50f;

        // Determine button text based on game state
        string playButtonText = _screenManager.IsGameInitialized && _screenManager.CurrentState == GameState.Paused 
            ? "Resume Game" 
            : "New Game";

        // Create buttons
        var playButton = new UIButton(playButtonText, _buttonFont, 
            new Vector2(screenCenter.X - _buttonFont.MeasureString(playButtonText).X / 2, startY),
            OnPlayButtonClicked);
        
        var optionsButton = new UIButton("Options", _buttonFont,
            new Vector2(screenCenter.X - _buttonFont.MeasureString("Options").X / 2, startY + buttonSpacing),
            OnOptionsButtonClicked);
        
        var exitButton = new UIButton("Exit", _buttonFont,
            new Vector2(screenCenter.X - _buttonFont.MeasureString("Exit").X / 2, startY + buttonSpacing * 2),
            OnExitButtonClicked);

        _menuButtons.Add(playButton);
        _menuButtons.Add(optionsButton);
        _menuButtons.Add(exitButton);

        // Add buttons to UI manager
        foreach (var button in _menuButtons)
        {
            _uiManager.AddElement(button);
        }

        // Reset selection
        _selectedButtonIndex = 0;
        UpdateButtonSelection();
    }

    private void OnPlayButtonClicked()
    {
        if (_screenManager.IsGameInitialized && _screenManager.CurrentState == GameState.Paused)
        {
            _screenManager.ResumeGame();
        }
        else
        {
            // Start new game - ScreenManager will handle creating the gameplay screen
            _screenManager.StartNewGame();
        }
    }

    private void OnOptionsButtonClicked()
    {
        // Placeholder for options menu
        // Could open a sub-menu or settings screen
    }

    private void OnExitButtonClicked()
    {
        if (_screenManager.CurrentState == GameState.Paused)
        {
            // Return to main menu from paused game
            _screenManager.ReturnToMenu();
        }
        else
        {
            // Exit the application
            Game.Exit();
        }
    }

    public override void Update(GameTime gameTime)
    {
        // Check if overlay status changed
        bool wasOverlay = _isOverlay;
        _isOverlay = _screenManager.CurrentState == GameState.Paused;
        
        // Update menu buttons if overlay state changed
        if (wasOverlay != _isOverlay)
        {
            UpdateMenuButtons();
        }

        // Handle keyboard navigation
        HandleKeyboardInput();
        
        _uiManager.Update(gameTime);
    }

    private void HandleKeyboardInput()
    {
        var currentKeyboardState = Keyboard.GetState();

        // Navigation with arrow keys
        if (currentKeyboardState.IsKeyDown(Keys.Down) && !_previousKeyboardState.IsKeyDown(Keys.Down))
        {
            _selectedButtonIndex = (_selectedButtonIndex + 1) % _menuButtons.Count;
            UpdateButtonSelection();
        }
        else if (currentKeyboardState.IsKeyDown(Keys.Up) && !_previousKeyboardState.IsKeyDown(Keys.Up))
        {
            _selectedButtonIndex = (_selectedButtonIndex - 1 + _menuButtons.Count) % _menuButtons.Count;
            UpdateButtonSelection();
        }

        // Activate button with Enter or Space
        if ((currentKeyboardState.IsKeyDown(Keys.Enter) || currentKeyboardState.IsKeyDown(Keys.Space)) &&
            (!_previousKeyboardState.IsKeyDown(Keys.Enter) && !_previousKeyboardState.IsKeyDown(Keys.Space)))
        {
            if (_selectedButtonIndex >= 0 && _selectedButtonIndex < _menuButtons.Count)
            {
                _menuButtons[_selectedButtonIndex].TriggerClick();
            }
        }

        _previousKeyboardState = currentKeyboardState;
    }

    private void UpdateButtonSelection()
    {
        for (int i = 0; i < _menuButtons.Count; i++)
        {
            _menuButtons[i].IsSelected = (i == _selectedButtonIndex);
        }
    }

    public override void Draw(GameTime gameTime)
    {
        if (_isOverlay)
        {
            // Draw semi-transparent overlay over gameplay
            _spriteBatch.Begin();
            _spriteBatch.Draw(_overlayTexture, new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height), 
                Color.Black * 0.5f);
            _spriteBatch.End();
        }
        else
        {
            // Clear with menu background color
            GraphicsDevice.Clear(Color.DarkSlateGray);
        }

        // Draw UI elements
        _uiManager.Draw(_spriteBatch);
    }

    public override void UnloadContent()
    {
        _overlayTexture?.Dispose();
        Content.Unload();
    }
}