using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace game_mono.screens;

public class ScreenManager
{
    private GameScreen _menuScreen;
    private GameScreen _gameplayScreen;
    private readonly Game _game;
    private readonly GraphicsDevice _graphicsDevice;
    
    public GameState CurrentState { get; private set; } = GameState.Menu;
    public bool IsGameInitialized => _gameplayScreen != null;
    
    private KeyboardState _previousKeyboardState;

    public ScreenManager(Game game, GraphicsDevice graphicsDevice)
    {
        _game = game;
        _graphicsDevice = graphicsDevice;
    }

    public void SetMenuScreen(GameScreen menuScreen)
    {
        _menuScreen = menuScreen;
        _menuScreen.Initialize(_game, _graphicsDevice);
        _menuScreen.LoadContent();
    }

    public void SetGameplayScreen(GameScreen gameplayScreen)
    {
        _gameplayScreen = gameplayScreen;
        _gameplayScreen.Initialize(_game, _graphicsDevice);
        _gameplayScreen.LoadContent();
    }

    public void StartNewGame()
    {
        if (_gameplayScreen != null)
        {
            _gameplayScreen.UnloadContent();
            _gameplayScreen = null;
        }
        
        // Create a fresh gameplay screen
        _gameplayScreen = new GameplayScreen();
        _gameplayScreen.Initialize(_game, _graphicsDevice);
        _gameplayScreen.LoadContent();
        
        CurrentState = GameState.Playing;
    }

    public void ResumeGame()
    {
        if (_gameplayScreen != null)
        {
            CurrentState = GameState.Playing;
        }
    }

    public void PauseGame()
    {
        if (CurrentState == GameState.Playing)
        {
            CurrentState = GameState.Paused;
        }
    }

    public void ReturnToMenu()
    {
        CurrentState = GameState.Menu;
    }

    public void Update(GameTime gameTime)
    {
        var currentKeyboardState = Keyboard.GetState();
        
        // Handle ESC key for state transitions
        if (currentKeyboardState.IsKeyDown(Keys.Escape) && !_previousKeyboardState.IsKeyDown(Keys.Escape))
        {
            switch (CurrentState)
            {
                case GameState.Playing:
                    PauseGame();
                    break;
                case GameState.Paused:
                    ResumeGame();
                    break;
            }
        }

        _previousKeyboardState = currentKeyboardState;

        // Update appropriate screens based on state
        switch (CurrentState)
        {
            case GameState.Menu:
                _menuScreen?.Update(gameTime);
                break;
            case GameState.Playing:
                _gameplayScreen?.Update(gameTime);
                break;
            case GameState.Paused:
                // Update both screens when paused (gameplay might need passive updates)
                _gameplayScreen?.Update(gameTime);
                _menuScreen?.Update(gameTime);
                break;
        }
    }

    public void Draw(GameTime gameTime)
    {
        switch (CurrentState)
        {
            case GameState.Menu:
                _menuScreen?.Draw(gameTime);
                break;
            case GameState.Playing:
                _gameplayScreen?.Draw(gameTime);
                break;
            case GameState.Paused:
                // Draw gameplay first, then menu overlay
                _gameplayScreen?.Draw(gameTime);
                _menuScreen?.Draw(gameTime);
                break;
        }
    }
}
