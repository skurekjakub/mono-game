using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace game_mono;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private SpriteFont _instructionsFont;
    
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
        _instructionsFont = Content.Load<SpriteFont>("InstructionsFont");

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

        // Control mouse visibility based on capture and focus state
        IsMouseVisible = !_inputManager.IsMouseCaptured || !_inputManager.IsWindowFocused;
        
        // Exit game
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || 
            _inputManager.IsKeyDown(Keys.Escape))
            Exit();
        
        // Update camera with input
        _camera.Update(gameTime, _inputManager);
        
        // Add pyramid with mouse click
        if (_inputManager.IsMouseButtonPressed(MouseButton.Left))
        {
            Vector3 spawnPosition = _camera.Position + _camera.Forward * 10f;
            spawnPosition.Y = 0;
            var pyramid = CreatePyramidGameObject(spawnPosition, 1f, 2f, Color.Red);
            var behavior = pyramid.GetComponent<PyramidBehavior>();
            if (behavior != null)
            {
                behavior.RotationSpeed = new Vector3(0, 2f, 0);
            }
        }
        
        // Clear all pyramids with C key
        if (_inputManager.IsKeyPressed(Keys.C))
        {
            _gameObjectManager.ClearAll();
            CreateDemoPyramids(); // Recreate demo pyramids
        }
        
        // Spawn random pyramid with R key
        if (_inputManager.IsKeyPressed(Keys.R))
        {
            CreateRandomPyramidGameObject();
        }
        
        // Update all game objects
        _gameObjectManager.Update(gameTime);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue); // Darker blue

        // Set up 3D rendering matrices
        var projection = _camera.ProjectionMatrix;
        _primitiveRenderer.SetMatrices(projection, _camera.ViewMatrix);

        // Begin primitive rendering
        _primitiveRenderer.Begin();
        
        // Draw ground plane first
        _groundPlane.Draw(_primitiveRenderer);
        
        // Draw all game objects
        _gameObjectManager.Draw(_primitiveRenderer);
        
        // End primitive rendering
        _primitiveRenderer.End();

        // Draw UI overlay with instructions
        _spriteBatch.Begin();
        
        string instructions = $"WASD/Arrows: Move | Mouse: Look | Space/Q: Up/Down\n" +
                             $"Tab: Toggle Mouse Capture | R: Add Random | C: Clear All\n" +
                             $"Objects: {_gameObjectManager.GameObjects.Count} | Camera Pos: {_camera.Position:F1}";
        
        _spriteBatch.DrawString(_instructionsFont, instructions, new Vector2(10, 10), Color.White);
        
        _spriteBatch.End();

        base.Draw(gameTime);
    }
    
    private GameObject CreatePyramidGameObject(Vector3 position, float baseSize, float height, Color color)
    {
        var gameObject = new GameObject();
        gameObject.Transform.Position = position;

        var mesh = MeshFactory.CreatePyramid(baseSize, height, color);
        gameObject.AddComponent(new MeshRenderer(mesh, color));
        gameObject.AddComponent(new PyramidBehavior());

        _gameObjectManager.AddGameObject(gameObject);
        return gameObject;
    }

    private void CreateRandomPyramidGameObject()
    {
        var position = new Vector3(
            (float)(_random.NextDouble() - 0.5) * 40,
            0,
            (float)(_random.NextDouble() - 0.5) * 40);
        
        var baseSize = (float)(_random.NextDouble() * 1.5 + 0.5);
        var height = (float)(_random.NextDouble() * 2.0 + 1.0);
        var color = new Color(
            _random.Next(50, 255),
            _random.Next(50, 255),
            _random.Next(50, 255));

        var pyramid = CreatePyramidGameObject(position, baseSize, height, color);
        var behavior = pyramid.GetComponent<PyramidBehavior>();
        if (behavior != null)
        {
            behavior.Velocity = new Vector3(
                (float)(_random.NextDouble() - 0.5) * 2f, 0, (float)(_random.NextDouble() - 0.5) * 2f);
            behavior.RotationSpeed = new Vector3(
                0, (float)(_random.NextDouble() - 0.5) * 1f, 0);
        }
    }

    private void CreateDemoPyramids()
    {
        // Large white pyramid at origin
        CreatePyramidGameObject(new Vector3(0, 0, 0), 2f, 3f, Color.White);

        // Red spinning pyramid
        var redPyramid = CreatePyramidGameObject(new Vector3(-5, 0, -3), 1.5f, 2.5f, Color.Red);
        redPyramid.GetComponent<PyramidBehavior>().RotationSpeed = new Vector3(0, 1f, 0);

        // Green spinning pyramid
        var greenPyramid = CreatePyramidGameObject(new Vector3(4, 0, -2), 1.2f, 2f, Color.Lime);
        greenPyramid.GetComponent<PyramidBehavior>().RotationSpeed = new Vector3(0, -0.5f, 0);

        // Blue moving pyramid
        var bluePyramid = CreatePyramidGameObject(new Vector3(0, 0, -8), 1f, 1.5f, Color.Blue);
        bluePyramid.GetComponent<PyramidBehavior>().Velocity = new Vector3(1f, 0, 0);
    }
    
    protected override void UnloadContent()
    {
        // Dispose of our resources
        _primitiveRenderer?.Dispose();
        base.UnloadContent();
    }
}
