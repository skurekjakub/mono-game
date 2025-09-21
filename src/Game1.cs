using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using game_mono.ui;

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
    private UIManager _uiManager;
    private UIText _instructionsText;
    
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
        
        // Initialize UI Manager
        _uiManager = new UIManager();

        // Initialize 3D camera at a good starting position
        Vector3 startPosition = new Vector3(5, 3, 15); // Better overview position
        _camera = new Camera3D(startPosition, _screenWidth, _screenHeight);
        
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        var instructionsFont = Content.Load<SpriteFont>("InstructionsFont");

        // Set up UI
        string instructions = "WASD/Arrows: Move | Mouse: Look | Space/Q: Up/Down\n" +
                              "Tab: Toggle Mouse Capture | R: Add Random | C: Clear All";
        _instructionsText = new UIText(instructions, instructionsFont, new Vector2(10, 10), Color.White);
        _uiManager.AddElement(_instructionsText);

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

        // Update all game objects
        _gameObjectManager.Update(gameTime);

        // Update UI
        _instructionsText.Text = $"WASD/Arrows: Move | Mouse: Look | Space/Q: Up/Down\n" +
                                 $"Tab: Toggle Mouse Capture | R: Add Random | C: Clear All\n" +
                                 $"Objects: {_gameObjectManager.GameObjects.Count} | Camera Pos: {_camera.Position:F1}";
        _uiManager.Update(gameTime);

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

        // Draw UI overlay
        _uiManager.Draw(_spriteBatch);

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
