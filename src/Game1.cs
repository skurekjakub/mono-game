using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace game_mono;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    
    // Our 3D systems
    private PrimitiveRenderer _primitiveRenderer;
    private GameObjectManager _gameObjectManager;
    private Camera3D _camera;
    private InputManager _inputManager;
    private GroundPlane _groundPlane;
    
    // Random number generator for demo purposes
    private Random _random;
    
    // Screen dimensions
    private int _screenWidth;
    private int _screenHeight;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = false; // Hidden for FPS camera controls
        
        // Set a reasonable window size
        _graphics.PreferredBackBufferWidth = 1024;
        _graphics.PreferredBackBufferHeight = 768;
        
        _random = new Random();
    }

    protected override void Initialize()
    {
        // Store screen dimensions
        _screenWidth = _graphics.PreferredBackBufferWidth;
        _screenHeight = _graphics.PreferredBackBufferHeight;
        
        // Initialize input manager
        _inputManager = new InputManager(this);
        
        // Initialize 3D camera at a good starting position
        Vector3 startPosition = new Vector3(5, 3, 15); // Better overview position
        _camera = new Camera3D(startPosition, _screenWidth, _screenHeight);
        
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // Initialize our 3D systems
        _primitiveRenderer = new PrimitiveRenderer(GraphicsDevice);
        _gameObjectManager = new GameObjectManager();
        _groundPlane = new GroundPlane(40, 2f); // 40x40 grid with 2 unit spacing
        
        // Create some demo pyramids
        CreateDemoPyramids();
    }

    protected override void Update(GameTime gameTime)
    {
        // Update input manager
        _inputManager.Update();
        
        // Exit game
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || 
            _inputManager.IsKeyDown(Keys.Escape))
            Exit();
        
        // Update camera with input
        _camera.Update(gameTime, _inputManager);
        
        // Add pyramid with mouse click (world raycast from camera)
        if (_inputManager.IsMouseButtonPressed(MouseButton.Left))
        {
            // Simple method: place pyramid in front of camera
            Vector3 spawnPosition = _camera.Position + _camera.Forward * 5f;
            spawnPosition.Y = 0; // Keep on ground level
            
            var newPyramid = _gameObjectManager.CreatePyramid(spawnPosition, 1f, 2f, Color.Red);
            newPyramid.SetRotationSpeed(new Vector3(0, 2f, 0)); // Spin around Y-axis
        }
        
        // Clear all pyramids with C key
        if (_inputManager.IsKeyPressed(Keys.C))
        {
            _gameObjectManager.ClearAllPyramids();
            CreateDemoPyramids(); // Recreate demo pyramids
        }
        
        // Spawn random pyramid with R key
        if (_inputManager.IsKeyPressed(Keys.R))
        {
            var randomPyramid = _gameObjectManager.CreateRandomPyramid(
                Vector3.Zero, 20f, _random);
        }
        
        // Update all game objects
        _gameObjectManager.Update(gameTime);
        
        // Enable world wrapping
        _gameObjectManager.SetWorldWrapping(new Vector3(50, 20, 50), true);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        // Render 3D scene
        if (_gameObjectManager.PyramidCount > 0 || _groundPlane != null)
        {
            // Set up 3D rendering matrices
            var projection = PrimitiveRenderer.CreatePerspectiveProjection(
                _camera.FieldOfView, 
                (float)_screenWidth / _screenHeight, 
                _camera.NearPlane, 
                _camera.FarPlane);
            
            _primitiveRenderer.SetMatrices(projection, _camera.ViewMatrix);

            // Begin primitive rendering
            _primitiveRenderer.Begin();
            
            // Draw ground plane first for depth ordering
            _groundPlane.Draw(_primitiveRenderer);
            
            // Draw all pyramids
            _gameObjectManager.Draw(_primitiveRenderer);
            
            // End primitive rendering (this actually draws everything)
            _primitiveRenderer.End();
        }

        // Draw UI overlay with instructions
        _spriteBatch.Begin();
        
        string instructions = $"WASD: Move | Mouse: Free Look (Unlimited) | Space/Q: Up | E: Down | Tab: Toggle Mouse\n" +
                             $"Left Click: Add Pyramid | R: Random Pyramid | C: Clear\n" +
                             $"Scroll: Adjust Mouse Sensitivity ({_camera.MouseSensitivity:F4})\n" +
                             $"Pyramids: {_gameObjectManager.PyramidCount} | Pos: {_camera.Position:F1}";
        
        // Simple text rendering without needing fonts (using default system font if available)
        // Note: This is a basic approach - in a real game you'd load a SpriteFont
        
        _spriteBatch.End();

        base.Draw(gameTime);
    }
    
    /// <summary>
    /// Creates some demo pyramids for initial display
    /// </summary>
    private void CreateDemoPyramids()
    {
        // Create a large white pyramid at the origin
        _gameObjectManager.CreatePyramid(
            new Vector3(0, 0, 0), 
            2f, 3f, 
            Color.White);
        
        // Add a bright red pyramid for contrast
        var redPyramid = _gameObjectManager.CreatePyramid(
            new Vector3(-5, 0, -3), 
            1.5f, 2.5f, 
            Color.Red);
        redPyramid.SetRotationSpeed(new Vector3(0, 1f, 0)); // Slow rotation
        
        // Add a bright green pyramid
        var greenPyramid = _gameObjectManager.CreatePyramid(
            new Vector3(4, 0, -2), 
            1.2f, 2f, 
            Color.Lime);
        greenPyramid.SetRotationSpeed(new Vector3(0, -0.5f, 0)); // Opposite rotation
        
        // Add a blue pyramid higher up
        var bluePyramid = _gameObjectManager.CreatePyramid(
            new Vector3(0, 3, -8), 
            1f, 1.5f, 
            Color.Blue);
        bluePyramid.SetVelocity(new Vector3(1f, 0, 0)); // Moving
    }
    
    protected override void UnloadContent()
    {
        // Dispose of our resources
        _primitiveRenderer?.Dispose();
        base.UnloadContent();
    }
}
