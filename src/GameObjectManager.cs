using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace game_mono
{
    /// <summary>
    /// Manages a collection of 3D game objects (pyramids) with update and drawing capabilities
    /// </summary>
    public class GameObjectManager
    {
        private readonly List<Pyramid> _pyramids;
        private readonly List<Pyramid> _pyramidsToAdd;
        private readonly List<Pyramid> _pyramidsToRemove;
        
        public GameObjectManager()
        {
            _pyramids = new List<Pyramid>();
            _pyramidsToAdd = new List<Pyramid>();
            _pyramidsToRemove = new List<Pyramid>();
        }
        
        /// <summary>
        /// Gets the current number of pyramids being managed
        /// </summary>
        public int PyramidCount => _pyramids.Count;
        
        /// <summary>
        /// Gets a read-only view of all pyramids
        /// </summary>
        public IReadOnlyList<Pyramid> Pyramids => _pyramids.AsReadOnly();
        
        /// <summary>
        /// Adds a pyramid to be managed (will be added at the start of next frame)
        /// </summary>
        public void AddPyramid(Pyramid pyramid)
        {
            if (pyramid != null)
            {
                _pyramidsToAdd.Add(pyramid);
            }
        }
        
        /// <summary>
        /// Removes a pyramid from management (will be removed at the start of next frame)
        /// </summary>
        public void RemovePyramid(Pyramid pyramid)
        {
            if (pyramid != null)
            {
                _pyramidsToRemove.Add(pyramid);
            }
        }
        
        /// <summary>
        /// Removes all pyramids from management
        /// </summary>
        public void ClearAllPyramids()
        {
            _pyramidsToRemove.AddRange(_pyramids);
        }
        
        /// <summary>
        /// Creates and adds a pyramid at the specified position
        /// </summary>
        public Pyramid CreatePyramid(Vector3 position, float baseSize = 2f, float height = 3f, Color? color = null)
        {
            var pyramid = new Pyramid(position, baseSize, height, color);
            AddPyramid(pyramid);
            return pyramid;
        }
        
        /// <summary>
        /// Creates and adds a pyramid with random properties for demo purposes
        /// </summary>
        public Pyramid CreateRandomPyramid(Vector3 worldCenter, float spawnRadius, Random random = null)
        {
            random ??= new Random();
            
            var position = worldCenter + new Vector3(
                (float)(random.NextDouble() - 0.5) * spawnRadius * 2,
                0, // Keep on ground level
                (float)(random.NextDouble() - 0.5) * spawnRadius * 2);
            
            var baseSize = (float)(random.NextDouble() * 1.5 + 0.5); // 0.5 to 2.0
            var height = (float)(random.NextDouble() * 2.0 + 1.0);   // 1.0 to 3.0
            var color = new Color(
                random.Next(50, 255),
                random.Next(50, 255),
                random.Next(50, 255));
            
            var pyramid = CreatePyramid(position, baseSize, height, color);
            
            // Add some random movement and rotation
            pyramid.SetVelocity(new Vector3(
                (float)(random.NextDouble() - 0.5) * 2f,
                0, // No Y movement for ground-based objects
                (float)(random.NextDouble() - 0.5) * 2f));
            
            pyramid.SetRotationSpeed(new Vector3(
                0, // No X rotation to keep upright
                (float)(random.NextDouble() - 0.5) * 1f, // Y rotation for spinning
                0)); // No Z rotation to keep upright
            
            return pyramid;
        }
        
        /// <summary>
        /// Updates all managed pyramids
        /// </summary>
        public void Update(GameTime gameTime)
        {
            // Process additions and removals at the start of the frame
            ProcessPendingChanges();

            // Update all pyramids
            foreach (var pyramid in _pyramids)
            {
                pyramid.Update(gameTime);
            }
            
            // Handle boundary behavior if needed
            HandleBoundaryBehavior();
        }
        
        /// <summary>
        /// Draws all managed pyramids using the provided renderer
        /// </summary>
        public void Draw(PrimitiveRenderer renderer)
        {
            if (renderer == null) return;
            
            foreach (var pyramid in _pyramids)
            {
                renderer.AddPyramid(pyramid);
            }
        }
        
        /// <summary>
        /// Processes pending additions and removals
        /// </summary>
        private void ProcessPendingChanges()
        {
            // Add new pyramids
            if (_pyramidsToAdd.Count > 0)
            {
                _pyramids.AddRange(_pyramidsToAdd);
                _pyramidsToAdd.Clear();
            }
            
            // Remove pyramids
            if (_pyramidsToRemove.Count > 0)
            {
                foreach (var pyramid in _pyramidsToRemove)
                {
                    _pyramids.Remove(pyramid);
                }
                _pyramidsToRemove.Clear();
            }
        }
        
        /// <summary>
        /// Handles boundary behavior for pyramids (world bounds, etc.)
        /// </summary>
        private void HandleBoundaryBehavior()
        {
            // For now, let pyramids move freely in 3D space
            // In a real game, you might want to implement world boundaries or physics
        }
        
        /// <summary>
        /// Sets world boundary behavior for all pyramids (simple teleport-based wrapping)
        /// </summary>
        public void SetWorldWrapping(Vector3 worldSize, bool enabled = true)
        {
            if (!enabled) return;
            
            Vector3 halfSize = worldSize * 0.5f;
            
            foreach (var pyramid in _pyramids)
            {
                var pos = pyramid.Position;
                
                if (pos.X < -halfSize.X)
                {
                    pyramid.Position = new Vector3(halfSize.X, pos.Y, pos.Z);
                }
                else if (pos.X > halfSize.X)
                {
                    pyramid.Position = new Vector3(-halfSize.X, pos.Y, pos.Z);
                }
                
                if (pos.Z < -halfSize.Z)
                {
                    pyramid.Position = new Vector3(pos.X, pos.Y, halfSize.Z);
                }
                else if (pos.Z > halfSize.Z)
                {
                    pyramid.Position = new Vector3(pos.X, pos.Y, -halfSize.Z);
                }
                
                // Keep Y position within reasonable bounds
                if (pos.Y < -10f)
                {
                    pyramid.Position = new Vector3(pos.X, 0f, pos.Z);
                }
                else if (pos.Y > 20f)
                {
                    pyramid.Position = new Vector3(pos.X, 0f, pos.Z);
                }
            }
        }
        
        /// <summary>
        /// Finds the pyramid closest to the specified position
        /// </summary>
        public Pyramid FindClosestPyramid(Vector3 position)
        {
            if (_pyramids.Count == 0) return null;
            
            return _pyramids
                .OrderBy(p => Vector3.Distance(p.Position, position))
                .First();
        }
        
        /// <summary>
        /// Gets all pyramids within a specified radius of a position
        /// </summary>
        public IEnumerable<Pyramid> GetPyramidsInRadius(Vector3 position, float radius)
        {
            float radiusSquared = radius * radius;
            return _pyramids.Where(p => 
                Vector3.DistanceSquared(p.Position, position) <= radiusSquared);
        }
    }
}