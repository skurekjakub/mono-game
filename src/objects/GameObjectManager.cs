using game_mono.components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace game_mono.objects
{
    /// <summary>
    /// Manages the lifecycle of all game objects in the scene.
    /// </summary>
    public class GameObjectManager
    {
        private readonly List<GameObject> _gameObjects = new();
        private readonly List<GameObject> _gameObjectsToAdd = new();
        private bool _isUpdating;

        /// <summary>
        /// Gets a read-only view of all game objects
        /// </summary>
        public IReadOnlyList<GameObject> GameObjects => _gameObjects.AsReadOnly();

        /// <summary>
        /// Adds a game object to be managed
        /// </summary>
        public GameObject AddGameObject(GameObject gameObject)
        {
            if (_isUpdating)
            {
                _gameObjectsToAdd.Add(gameObject);
            }
            else
            {
                _gameObjects.Add(gameObject);
            }
            return gameObject;
        }

        /// <summary>
        /// Removes all game objects from management
        /// </summary>
        public void ClearAll()
        {
            foreach (var go in _gameObjects)
            {
                go.Destroy();
            }
        }

        /// <summary>
        /// Updates all managed game objects
        /// </summary>
        public void Update(GameTime gameTime)
        {
            _isUpdating = true;
            foreach (var gameObject in _gameObjects)
            {
                gameObject.Update(gameTime);
            }
            _isUpdating = false;

            // Add any new objects that were created during the update phase
            _gameObjects.AddRange(_gameObjectsToAdd);
            _gameObjectsToAdd.Clear();

            // Remove all objects that were marked for destruction
            _gameObjects.RemoveAll(go => go.IsDestroyed);
        }

        public void DrawPrimitives(PrimitiveRenderer renderer)
        {
            foreach (var gameObject in _gameObjects)
            {
                if (gameObject.IsDestroyed) continue;
                var rendererComponent = gameObject.GetComponent<MeshRenderer>();
                rendererComponent?.Draw(renderer);
            }
        }

        public void DrawModels(GraphicsDevice graphicsDevice, Matrix view, Matrix projection)
        {
            foreach (var gameObject in _gameObjects)
            {
                if (gameObject.IsDestroyed) continue;
                var modelComponent = gameObject.GetComponent<ModelComponent>();
                modelComponent?.Draw(graphicsDevice, view, projection);
            }
        }
    }
}