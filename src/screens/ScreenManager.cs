using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace game_mono.screens;

public class ScreenManager
{
    private GameScreen _currentScreen;
    private readonly Game _game;
    private readonly GraphicsDevice _graphicsDevice;

    public ScreenManager(Game game, GraphicsDevice graphicsDevice)
    {
        _game = game;
        _graphicsDevice = graphicsDevice;
    }

    public void ChangeScreen(GameScreen newScreen)
    {
        _currentScreen?.UnloadContent();
        _currentScreen = newScreen;
        _currentScreen.Initialize(_game, _graphicsDevice);
        _currentScreen.LoadContent();
    }

    public void Update(GameTime gameTime)
    {
        _currentScreen?.Update(gameTime);
    }

    public void Draw(GameTime gameTime)
    {
        _currentScreen?.Draw(gameTime);
    }
}
