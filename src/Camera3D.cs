using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace game_mono
{
    /// <summary>
    /// Represents a 3D camera with position, rotation, and standard FPS-style controls
    /// </summary>
    public class Camera3D
    {
        // Camera transform properties
        public Vector3 Position { get; set; }
        public Vector3 Target { get; private set; }
        public Vector3 Up { get; private set; }
        
        // Rotation in radians
        public float Yaw { get; set; }   // Left/Right rotation (around Y-axis)
        public float Pitch { get; set; } // Up/Down rotation (around X-axis)
        
        // Camera parameters
        public float MovementSpeed { get; set; } = 5.0f;
        public float MouseSensitivity { get; set; } = 0.003f; // Adjusted for delta-based rotation
        public float FieldOfView { get; set; } = MathHelper.PiOver4; // 45 degrees
        public float NearPlane { get; set; } = 0.1f;
        public float FarPlane { get; set; } = 1000.0f;
        
        // Calculated matrices
        public Matrix ViewMatrix { get; private set; }
        public Matrix ProjectionMatrix { get; private set; }
        
        // Screen dimensions for projection
        private int _screenWidth;
        private int _screenHeight;
        
        public Camera3D(Vector3 position, int screenWidth, int screenHeight)
        {
            Position = position;
            _screenWidth = screenWidth;
            _screenHeight = screenHeight;
            
            // Initialize rotation
            Yaw = 0.0f;
            Pitch = 0.0f;
            
            // Initialize up vector
            Up = Vector3.Up;
            
            UpdateCameraVectors();
            UpdateMatrices();
        }
        
        /// <summary>
        /// Updates camera based on input and elapsed time
        /// </summary>
        public void Update(GameTime gameTime, InputManager inputManager)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            // Adjust mouse sensitivity with scroll wheel
            int scrollDelta = inputManager.ScrollWheelDelta;
            if (scrollDelta != 0)
            {
                MouseSensitivity = MathHelper.Clamp(MouseSensitivity + (scrollDelta * 0.0005f), 0.001f, 0.01f);
            }
            
            // Handle mouse look
            HandleMouseLook(inputManager);
            
            // Handle movement
            HandleMovement(inputManager, deltaTime);
            
            // Update camera vectors and matrices
            UpdateCameraVectors();
            UpdateMatrices();
        }
        
        /// <summary>
        /// Handles mouse look rotation based on mouse position relative to screen center
        /// </summary>
        private void HandleMouseLook(InputManager inputManager)
        {
            var mouseDelta = inputManager.MouseDelta;
            
            // Convert mouse offset to rotation changes (continuous, unlimited rotation)
            // Scale mouse movement to rotation speed
            float rotationScale = MouseSensitivity;
            
            // Apply continuous rotation based on mouse offset from center
            // This allows unlimited rotation in any direction
            Yaw += mouseDelta.X * rotationScale;
            Pitch -= mouseDelta.Y * rotationScale; // Invert Y for natural feel
            
            // Only constrain pitch to prevent camera flipping upside down
            // Yaw can rotate freely 360 degrees
            Pitch = MathHelper.Clamp(Pitch, -MathHelper.PiOver2 + 0.1f, MathHelper.PiOver2 - 0.1f);
            
            // Normalize yaw to keep it in reasonable bounds (optional, for cleaner values)
            while (Yaw > MathHelper.TwoPi)
                Yaw -= MathHelper.TwoPi;
            while (Yaw < -MathHelper.TwoPi)
                Yaw += MathHelper.TwoPi;
        }
        
        /// <summary>
        /// Handles WASD movement
        /// </summary>
        private void HandleMovement(InputManager inputManager, float deltaTime)
        {
            float moveSpeed = MovementSpeed * deltaTime;
            Vector3 forward = Vector3.Normalize(Target - Position);
            Vector3 right = Vector3.Normalize(Vector3.Cross(forward, Up));
            
            // WASD movement
            if (inputManager.IsKeyDown(Keys.W))
                Position += forward * moveSpeed;
            if (inputManager.IsKeyDown(Keys.S))
                Position -= forward * moveSpeed;
            if (inputManager.IsKeyDown(Keys.A))
                Position -= right * moveSpeed;
            if (inputManager.IsKeyDown(Keys.D))
                Position += right * moveSpeed;
            
            // Vertical movement: Spacebar for up, E for down, Q for up (alternative)
            if (inputManager.IsKeyDown(Keys.Space))
                Position += Up * moveSpeed;
            if (inputManager.IsKeyDown(Keys.Q))
                Position += Up * moveSpeed;
            if (inputManager.IsKeyDown(Keys.E))
                Position -= Up * moveSpeed;
        }
        
        /// <summary>
        /// Updates the camera's forward, right, and up vectors based on yaw and pitch
        /// </summary>
        private void UpdateCameraVectors()
        {
            // Calculate forward vector from yaw and pitch
            var forward = new Vector3(
                (float)(Math.Cos(Yaw) * Math.Cos(Pitch)),
                (float)Math.Sin(Pitch),
                (float)(Math.Sin(Yaw) * Math.Cos(Pitch))
            );
            
            Target = Position + Vector3.Normalize(forward);
            
            // Right vector is perpendicular to forward and world up
            Vector3 right = Vector3.Normalize(Vector3.Cross(forward, Vector3.Up));
            
            // Up vector is perpendicular to right and forward
            Up = Vector3.Normalize(Vector3.Cross(right, forward));
        }
        
        /// <summary>
        /// Updates view and projection matrices
        /// </summary>
        private void UpdateMatrices()
        {
            // Create view matrix
            ViewMatrix = Matrix.CreateLookAt(Position, Target, Up);
            
            // Create projection matrix
            float aspectRatio = (float)_screenWidth / _screenHeight;
            ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                FieldOfView, aspectRatio, NearPlane, FarPlane);
        }
        
        /// <summary>
        /// Updates screen dimensions and recalculates projection matrix
        /// </summary>
        public void UpdateScreenSize(int width, int height)
        {
            _screenWidth = width;
            _screenHeight = height;
            UpdateMatrices();
        }
        
        /// <summary>
        /// Gets the camera's forward direction vector
        /// </summary>
        public Vector3 Forward => Vector3.Normalize(Target - Position);
        
        /// <summary>
        /// Gets the camera's right direction vector
        /// </summary>
        public Vector3 Right => Vector3.Normalize(Vector3.Cross(Forward, Up));
        
        /// <summary>
        /// Converts a world position to screen coordinates
        /// </summary>
        public Vector2 WorldToScreen(Vector3 worldPosition, GraphicsDevice graphicsDevice)
        {
            Vector4 clipSpace = Vector4.Transform(worldPosition, ViewMatrix * ProjectionMatrix);
            
            if (clipSpace.W == 0) return Vector2.Zero;
            
            Vector3 ndcSpace = new Vector3(clipSpace.X, clipSpace.Y, clipSpace.Z) / clipSpace.W;
            
            return new Vector2(
                (ndcSpace.X + 1.0f) * 0.5f * _screenWidth,
                (1.0f - ndcSpace.Y) * 0.5f * _screenHeight
            );
        }
    }
}