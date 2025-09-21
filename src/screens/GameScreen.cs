using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace game_mono.screens;

public abstract class GameScreen
{
    protected ContentManager Content;
    protected GraphicsDevice GraphicsDevice;
    protected Game Game;

    public virtual void Initialize(Game game, GraphicsDevice graphicsDevice)
    {
        Game = game;
        GraphicsDevice = graphicsDevice;
        Content = new ContentManager(game.Services, "Content");
    }

    public abstract void LoadContent();
    public abstract void UnloadContent();
    public abstract void Update(GameTime gameTime);
    public abstract void Draw(GameTime gameTime);
}
