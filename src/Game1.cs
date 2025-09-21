using game_mono.screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace game_mono;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private ScreenManager _screenManager;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        _graphics.PreferredBackBufferWidth = 1024;
        _graphics.PreferredBackBufferHeight = 768;
    }

    protected override void Initialize()
    {
        _screenManager = new ScreenManager(this, _graphics.GraphicsDevice);
        _screenManager.ChangeScreen(new GameplayScreen());
        base.Initialize();
    }

    protected override void Update(GameTime gameTime)
    {
        _screenManager.Update(gameTime);
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        _screenManager.Draw(gameTime);
        base.Draw(gameTime);
    }
}

