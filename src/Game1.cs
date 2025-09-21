using System;
using Microsoft.Xna.Framework;
using game_mono.screens;

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
        
        // Enable debug mode for better error reporting
#if DEBUG
        _graphics.SynchronizeWithVerticalRetrace = false;
        this.IsFixedTimeStep = false;
#endif
    }

    protected override void Initialize()
    {
        try
        {
            _screenManager = new ScreenManager(this, _graphics.GraphicsDevice);

            // Initialize with menu screen
            var menuScreen = new MenuScreen(_screenManager);
            _screenManager.SetMenuScreen(menuScreen);

            base.Initialize();
        }
        catch (Exception ex)
        {
            // Log detailed exception information
            Console.WriteLine($"Initialization Error: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                Console.WriteLine($"Inner Stack Trace: {ex.InnerException.StackTrace}");
            }
            throw; // Re-throw to maintain normal error handling
        }
    }

    protected override void Update(GameTime gameTime)
    {
        try
        {
            _screenManager.Update(gameTime);
            base.Update(gameTime);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Update Error: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
            }
            throw;
        }
    }

    protected override void Draw(GameTime gameTime)
    {
        try
        {
            _screenManager.Draw(gameTime);
            base.Draw(gameTime);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Draw Error: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
            }
            throw;
        }
    }
}

