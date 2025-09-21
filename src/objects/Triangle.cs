using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace game_mono
{
    /// <summary>
    /// Represents a triangle entity with transform properties and rendering capabilities
    /// </summary>
    public class Triangle
    {
        // Transform properties
        public Vector2 Position { get; set; }
        public float Rotation { get; set; }
        public Vector2 Scale { get; set; }
        public Color Color { get; set; }
        
        // Animation properties
        public float RotationSpeed { get; set; }
        public Vector2 Velocity { get; set; }
        
        // Triangle geometry (relative to center)
        public Vector2[] LocalVertices { get; private set; }
        
        // World space vertices (calculated each frame)
        public Vector2[] WorldVertices { get; private set; }
        
        // Transform matrix for efficient calculations
        public Matrix Transform { get; private set; }
        
        public Triangle(Vector2 position, float size = 50f, Color? color = null)
        {
            Position = position;
            Rotation = 0f;
            Scale = Vector2.One;
            Color = color ?? Color.White;
            RotationSpeed = 0f;
            Velocity = Vector2.Zero;
            
            // Define triangle vertices in local space (equilateral triangle centered at origin)
            LocalVertices = new Vector2[]
            {
                new Vector2(0, -size),           // Top vertex
                new Vector2(-size * 0.866f, size * 0.5f),  // Bottom left (cos(60°) = 0.5, sin(60°) = 0.866)
                new Vector2(size * 0.866f, size * 0.5f)    // Bottom right
            };
            
            WorldVertices = new Vector2[3];
            UpdateTransform();
        }
        
        /// <summary>
        /// Updates the triangle's position, rotation, and calculates world vertices
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values</param>
        public void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            // Update position based on velocity
            Position += Velocity * deltaTime;
            
            // Update rotation based on rotation speed
            Rotation += RotationSpeed * deltaTime;
            
            // Keep rotation in reasonable bounds
            if (Rotation > MathHelper.TwoPi)
                Rotation -= MathHelper.TwoPi;
            else if (Rotation < -MathHelper.TwoPi)
                Rotation += MathHelper.TwoPi;
            
            // Recalculate transform matrix and world vertices
            UpdateTransform();
        }
        
        /// <summary>
        /// Calculates the transform matrix and updates world vertices
        /// </summary>
        private void UpdateTransform()
        {
            // Create transform matrix: Scale * Rotation * Translation
            Transform = Matrix.CreateScale(Scale.X, Scale.Y, 1f) *
                       Matrix.CreateRotationZ(Rotation) *
                       Matrix.CreateTranslation(Position.X, Position.Y, 0f);
            
            // Transform local vertices to world space
            for (int i = 0; i < LocalVertices.Length; i++)
            {
                Vector3 localVertex = new Vector3(LocalVertices[i], 0f);
                Vector3 worldVertex = Vector3.Transform(localVertex, Transform);
                WorldVertices[i] = new Vector2(worldVertex.X, worldVertex.Y);
            }
        }
        
        /// <summary>
        /// Sets the triangle's velocity for movement
        /// </summary>
        public void SetVelocity(Vector2 velocity)
        {
            Velocity = velocity;
        }
        
        /// <summary>
        /// Sets the triangle's rotation speed in radians per second
        /// </summary>
        public void SetRotationSpeed(float radiansPerSecond)
        {
            RotationSpeed = radiansPerSecond;
        }
        
        /// <summary>
        /// Sets the triangle's scale uniformly
        /// </summary>
        public void SetScale(float scale)
        {
            Scale = new Vector2(scale, scale);
        }
        
        /// <summary>
        /// Sets the triangle's scale with different X and Y values
        /// </summary>
        public void SetScale(float scaleX, float scaleY)
        {
            Scale = new Vector2(scaleX, scaleY);
        }
    }
}