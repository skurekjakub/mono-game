using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace game_mono
{
    /// <summary>
    /// Represents a 3D pyramid entity with transform properties and rendering capabilities
    /// </summary>
    public class Pyramid
    {
        // Transform properties
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; } // Euler angles in radians
        public Vector3 Scale { get; set; }
        public Color Color { get; set; }
        
        // Animation properties
        public Vector3 RotationSpeed { get; set; } // Radians per second for each axis
        public Vector3 Velocity { get; set; }
        
        // Pyramid geometry (relative to center, base on XZ plane)
        public VertexPositionNormalColor[] LocalVertices { get; private set; }
        public short[] Indices { get; private set; }
        
        // World space vertices (calculated each frame)
        public VertexPositionNormalColor[] WorldVertices { get; private set; }
        
        // Transform matrix for efficient calculations
        public Matrix WorldMatrix { get; private set; }
        
        // Pyramid dimensions
        private float _baseSize;
        private float _height;
        
        public Pyramid(Vector3 position, float baseSize = 2f, float height = 3f, Color? color = null)
        {
            Position = position;
            Rotation = Vector3.Zero;
            Scale = Vector3.One;
            Color = color ?? Color.White;
            RotationSpeed = Vector3.Zero;
            Velocity = Vector3.Zero;
            
            _baseSize = baseSize;
            _height = height;
            
            CreatePyramidGeometry();
            UpdateTransform();
        }
        
        /// <summary>
        /// Creates the pyramid geometry with vertices, normals, and indices
        /// </summary>
        private void CreatePyramidGeometry()
        {
            float halfBase = _baseSize * 0.5f;
            
            // Define pyramid vertices in local space
            // Base vertices (on XZ plane, Y = 0)
            Vector3[] positions = new Vector3[]
            {
                // Base vertices (clockwise when viewed from above)
                new Vector3(-halfBase, 0, -halfBase), // 0: Back-left
                new Vector3( halfBase, 0, -halfBase), // 1: Back-right
                new Vector3( halfBase, 0,  halfBase), // 2: Front-right
                new Vector3(-halfBase, 0,  halfBase), // 3: Front-left
                new Vector3(0, _height, 0)            // 4: Apex
            };
            
            // Calculate normals for each face
            Vector3[] normals = new Vector3[5];
            
            // Base normal (pointing down)
            normals[0] = normals[1] = normals[2] = normals[3] = Vector3.Down;
            
            // Calculate side face normals
            // For a pyramid, we'll use face normals rather than vertex normals for flat shading
            Vector3 backFaceNormal = CalculateFaceNormal(positions[0], positions[1], positions[4]);
            Vector3 rightFaceNormal = CalculateFaceNormal(positions[1], positions[2], positions[4]);
            Vector3 frontFaceNormal = CalculateFaceNormal(positions[2], positions[3], positions[4]);
            Vector3 leftFaceNormal = CalculateFaceNormal(positions[3], positions[0], positions[4]);
            
            // Create vertices with positions, normals, and colors
            LocalVertices = new VertexPositionNormalColor[]
            {
                // Base vertices (using base normal)
                new VertexPositionNormalColor(positions[0], Vector3.Down, Color),
                new VertexPositionNormalColor(positions[1], Vector3.Down, Color),
                new VertexPositionNormalColor(positions[2], Vector3.Down, Color),
                new VertexPositionNormalColor(positions[3], Vector3.Down, Color),
                
                // Side face vertices (duplicated for proper normals)
                // Back face
                new VertexPositionNormalColor(positions[0], backFaceNormal, Color),  // 4
                new VertexPositionNormalColor(positions[1], backFaceNormal, Color),  // 5
                new VertexPositionNormalColor(positions[4], backFaceNormal, Color),  // 6
                
                // Right face
                new VertexPositionNormalColor(positions[1], rightFaceNormal, Color), // 7
                new VertexPositionNormalColor(positions[2], rightFaceNormal, Color), // 8
                new VertexPositionNormalColor(positions[4], rightFaceNormal, Color), // 9
                
                // Front face
                new VertexPositionNormalColor(positions[2], frontFaceNormal, Color), // 10
                new VertexPositionNormalColor(positions[3], frontFaceNormal, Color), // 11
                new VertexPositionNormalColor(positions[4], frontFaceNormal, Color), // 12
                
                // Left face
                new VertexPositionNormalColor(positions[3], leftFaceNormal, Color),  // 13
                new VertexPositionNormalColor(positions[0], leftFaceNormal, Color),  // 14
                new VertexPositionNormalColor(positions[4], leftFaceNormal, Color),  // 15
            };
            
            // Define triangle indices (clockwise winding for front faces)
            Indices = new short[]
            {
                // Base (two triangles, clockwise from below)
                0, 2, 1,  // Triangle 1
                0, 3, 2,  // Triangle 2
                
                // Side faces (clockwise from outside)
                4, 5, 6,   // Back face
                7, 8, 9,   // Right face
                10, 11, 12, // Front face
                13, 14, 15  // Left face
            };
            
            WorldVertices = new VertexPositionNormalColor[LocalVertices.Length];
        }
        
        /// <summary>
        /// Calculates the normal vector for a triangle face
        /// </summary>
        private Vector3 CalculateFaceNormal(Vector3 v1, Vector3 v2, Vector3 v3)
        {
            Vector3 edge1 = v2 - v1;
            Vector3 edge2 = v3 - v1;
            return Vector3.Normalize(Vector3.Cross(edge1, edge2));
        }
        
        /// <summary>
        /// Updates the pyramid's position, rotation, and calculates world vertices
        /// </summary>
        public void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            // Update position based on velocity
            Position += Velocity * deltaTime;
            
            // Update rotation based on rotation speed
            Rotation += RotationSpeed * deltaTime;
            
            // Keep rotation in reasonable bounds
            Rotation = new Vector3(
                NormalizeAngle(Rotation.X),
                NormalizeAngle(Rotation.Y),
                NormalizeAngle(Rotation.Z)
            );
            
            // Recalculate transform matrix and world vertices
            UpdateTransform();
        }
        
        /// <summary>
        /// Normalizes an angle to the range [-π, π]
        /// </summary>
        private float NormalizeAngle(float angle)
        {
            while (angle > MathHelper.Pi)
                angle -= MathHelper.TwoPi;
            while (angle < -MathHelper.Pi)
                angle += MathHelper.TwoPi;
            return angle;
        }
        
        /// <summary>
        /// Calculates the transform matrix and updates world vertices
        /// </summary>
        private void UpdateTransform()
        {
            // Create transform matrix: Scale * Rotation * Translation
            WorldMatrix = Matrix.CreateScale(Scale) *
                         Matrix.CreateFromYawPitchRoll(Rotation.Y, Rotation.X, Rotation.Z) *
                         Matrix.CreateTranslation(Position);
            
            // Transform local vertices to world space
            for (int i = 0; i < LocalVertices.Length; i++)
            {
                Vector3 worldPosition = Vector3.Transform(LocalVertices[i].Position, WorldMatrix);
                Vector3 worldNormal = Vector3.TransformNormal(LocalVertices[i].Normal, WorldMatrix);
                
                WorldVertices[i] = new VertexPositionNormalColor(
                    worldPosition,
                    Vector3.Normalize(worldNormal),
                    LocalVertices[i].Color
                );
            }
        }
        
        /// <summary>
        /// Sets the pyramid's velocity for movement
        /// </summary>
        public void SetVelocity(Vector3 velocity)
        {
            Velocity = velocity;
        }
        
        /// <summary>
        /// Sets the pyramid's rotation speed in radians per second for each axis
        /// </summary>
        public void SetRotationSpeed(Vector3 radiansPerSecond)
        {
            RotationSpeed = radiansPerSecond;
        }
        
        /// <summary>
        /// Sets the pyramid's scale uniformly
        /// </summary>
        public void SetScale(float scale)
        {
            Scale = new Vector3(scale);
        }
        
        /// <summary>
        /// Sets the pyramid's scale with different X, Y, Z values
        /// </summary>
        public void SetScale(float scaleX, float scaleY, float scaleZ)
        {
            Scale = new Vector3(scaleX, scaleY, scaleZ);
        }
        
        /// <summary>
        /// Updates the color of all vertices
        /// </summary>
        public void SetColor(Color color)
        {
            Color = color;
            
            // Update all local vertices with new color
            for (int i = 0; i < LocalVertices.Length; i++)
            {
                LocalVertices[i].Color = color;
            }
            
            // Trigger transform update to propagate to world vertices
            UpdateTransform();
        }
        
        /// <summary>
        /// Gets the number of triangles in this pyramid
        /// </summary>
        public int TriangleCount => Indices.Length / 3;
        
        /// <summary>
        /// Gets the bounding sphere radius for collision detection
        /// </summary>
        public float BoundingRadius
        {
            get
            {
                float maxDimension = Math.Max(_baseSize, _height);
                return maxDimension * Scale.Length() * 0.5f;
            }
        }
    }
}