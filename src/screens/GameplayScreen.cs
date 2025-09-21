using System;
using game_mono.components;
using game_mono.input;
using game_mono.objects;
using game_mono.ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace game_mono.screens;

public class GameplayScreen : GameScreen
{
    private SpriteBatch _spriteBatch;
    private PrimitiveRenderer _primitiveRenderer;
    private GameObjectManager _gameObjectManager;
    private Camera3D _camera;
    private InputManager _inputManager;
    private GroundPlane _groundPlane;
    private UIManager _uiManager;
    private UIText _instructionsText;
    private Random _random = new();

    public override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _primitiveRenderer = new PrimitiveRenderer(GraphicsDevice);
        _gameObjectManager = new GameObjectManager();
        _groundPlane = new GroundPlane(40, 2f);
        _uiManager = new UIManager();

        var instructionsFont = Content.Load<SpriteFont>("InstructionsFont");
        string instructions = "WASD/Arrows: Move | Mouse: Look | Space/Q: Up/Down\n" +
                              "Tab: Toggle Mouse Capture | R: Add Random | C: Clear All";
        _instructionsText = new UIText(instructions, instructionsFont, new Vector2(10, 10), Color.White);
        _uiManager.AddElement(_instructionsText);

        // Load the pyramid model using this screen's custom ContentManager
        var pyramidModel = Content.Load<Model>("models/pyramid");
        CreatePyramidFromModel(pyramidModel, new Vector3(0, 0, 0), Color.White);
    }

    public override void UnloadContent()
    {
        _primitiveRenderer?.Dispose();
        Content.Unload();
    }

    public override void Update(GameTime gameTime)
    {
        if (_inputManager == null)
        {
            _inputManager = new InputManager(Game);
            int screenWidth = GraphicsDevice.Viewport.Width;
            int screenHeight = GraphicsDevice.Viewport.Height;
            _camera = new Camera3D(new Vector3(5, 3, 15), screenWidth, screenHeight);
        }

        _inputManager.Update();
        Game.IsMouseVisible = !_inputManager.IsMouseCaptured || !_inputManager.IsWindowFocused;

        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || _inputManager.IsKeyDown(Keys.Escape))
            Game.Exit();

        _camera.Update(gameTime, _inputManager);

        if (_inputManager.IsKeyPressed(Keys.R))
        {
            CreateRandomPyramidGameObject();
        }
        
        _gameObjectManager.Update(gameTime);

        _instructionsText.Text = $"WASD/Arrows: Move | Mouse: Look | Space/Q: Up/Down\n" +
                                 $"Tab: Toggle Mouse Capture | R: Add Random | C: Clear All\n" +
                                 $"Objects: {_gameObjectManager.GameObjects.Count} | Camera Pos: {_camera.Position:F1}";
        _uiManager.Update(gameTime);
    }

    public override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        var projection = _camera.ProjectionMatrix;
        var view = _camera.ViewMatrix;

        _primitiveRenderer.SetMatrices(projection, view);
        _primitiveRenderer.Begin();
        _groundPlane.Draw(_primitiveRenderer);
        _gameObjectManager.DrawPrimitives(_primitiveRenderer);
        _primitiveRenderer.End();

        _gameObjectManager.DrawModels(GraphicsDevice, view, projection);

        _uiManager.Draw(_spriteBatch);
    }

    private GameObject CreatePyramidFromModel(Model model, Vector3 position, Color color)
    {
        var gameObject = new GameObject();
        gameObject.Transform.Position = position;
        gameObject.AddComponent(new ModelComponent(model, color));
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
}
