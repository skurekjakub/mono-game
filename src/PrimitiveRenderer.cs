using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Runtime.InteropServices;

namespace game_mono;

/// <summary>
/// Custom vertex structure for 3D rendering with position, normal, and color
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct VertexPositionNormalColor : IVertexType
{
    public Vector3 Position;
    public Vector3 Normal;
    public Color Color;

    public VertexPositionNormalColor(Vector3 position, Vector3 normal, Color color)
    {
        Position = position;
        Normal = normal;
        Color = color;
    }

    public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration(
        new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
        new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
        new VertexElement(24, VertexElementFormat.Color, VertexElementUsage.Color, 0)
    );

    VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;
}

/// <summary>
/// Utility class for rendering 3D primitive shapes efficiently using vertex buffers and BasicEffect with lighting
/// </summary>
public class PrimitiveRenderer : IDisposable
{
    private GraphicsDevice _graphicsDevice;
    private BasicEffect _basicEffect;
    private VertexBuffer _vertexBuffer;
    private IndexBuffer _indexBuffer;
    private VertexPositionNormalColor[] _vertices;
    private short[] _indices;
    private int _vertexCount;
    private int _indexCount;
    private int _primitiveCount;
    
    // Maximum vertices and indices for batch rendering
    private const int MaxVertices = 5000;  // Increased for 3D objects
    private const int MaxIndices = 15000;
    
    private bool _disposed = false;
    
    public PrimitiveRenderer(GraphicsDevice graphicsDevice)
    {
        _graphicsDevice = graphicsDevice ?? throw new ArgumentNullException(nameof(graphicsDevice));
        
        Initialize();
    }
    
    /// <summary>
    /// Initializes the renderer with shaders and buffers for 3D rendering
    /// </summary>
    private void Initialize()
    {
        // Create BasicEffect for 3D vertex rendering with lighting
        _basicEffect = new BasicEffect(_graphicsDevice);
        _basicEffect.VertexColorEnabled = true;
        _basicEffect.LightingEnabled = true;
        _basicEffect.EnableDefaultLighting(); // Provides basic 3-point lighting
        
        // Configure lighting properties
        _basicEffect.AmbientLightColor = new Vector3(0.3f, 0.3f, 0.3f); // Soft ambient light
        _basicEffect.SpecularColor = Vector3.One * 0.2f; // Subtle specularity
        _basicEffect.SpecularPower = 16.0f;
        
        // Create vertex and index arrays
        _vertices = new VertexPositionNormalColor[MaxVertices];
        _indices = new short[MaxIndices];
        
        // Create vertex buffer for 3D vertices with normals
        _vertexBuffer = new VertexBuffer(
            _graphicsDevice,
            typeof(VertexPositionNormalColor),
            MaxVertices,
            BufferUsage.WriteOnly);
        
        // Create index buffer
        _indexBuffer = new IndexBuffer(
            _graphicsDevice,
            IndexElementSize.SixteenBits,
            MaxIndices,
            BufferUsage.WriteOnly);
        
        Reset();
    }
    
    /// <summary>
    /// Sets up the projection, view, and world matrices for 3D rendering
    /// </summary>
    public void SetMatrices(Matrix projection, Matrix view, Matrix world = default)
    {
        _basicEffect.Projection = projection;
        _basicEffect.View = view;
        _basicEffect.World = world == default ? Matrix.Identity : world;
    }
    
    /// <summary>
    /// Begins a new rendering batch
    /// </summary>
    public void Begin()
    {
        Reset();
    }
    
    /// <summary>
    /// Adds a line to the current batch by creating a thin triangle
    /// </summary>
    public void AddLine(Vector3 start, Vector3 end, Color color, float thickness = 0.01f)
    {
        if (_vertexCount + 6 > MaxVertices || _indexCount + 6 > MaxIndices)
        {
            Flush();
            Reset();
        }
        
        // Calculate direction and perpendicular vectors for line thickness
        Vector3 direction = Vector3.Normalize(end - start);
        Vector3 up = Vector3.Up;
        
        // If line is vertical, use different up vector
        if (Math.Abs(Vector3.Dot(direction, up)) > 0.9f)
            up = Vector3.Right;
        
        Vector3 perpendicular = Vector3.Normalize(Vector3.Cross(direction, up)) * thickness * 0.5f;
        Vector3 normal = Vector3.Normalize(Vector3.Cross(direction, perpendicular));
        
        // Create quad vertices for the line
        Vector3 v1 = start - perpendicular;
        Vector3 v2 = start + perpendicular;
        Vector3 v3 = end + perpendicular;
        Vector3 v4 = end - perpendicular;
        
        // Add vertices
        short baseIndex = (short)_vertexCount;
        _vertices[_vertexCount] = new VertexPositionNormalColor(v1, normal, color);
        _vertices[_vertexCount + 1] = new VertexPositionNormalColor(v2, normal, color);
        _vertices[_vertexCount + 2] = new VertexPositionNormalColor(v3, normal, color);
        _vertices[_vertexCount + 3] = new VertexPositionNormalColor(v4, normal, color);
        
        // Add indices for two triangles forming a quad
        _indices[_indexCount] = baseIndex;
        _indices[_indexCount + 1] = (short)(baseIndex + 1);
        _indices[_indexCount + 2] = (short)(baseIndex + 2);
        
        _indices[_indexCount + 3] = baseIndex;
        _indices[_indexCount + 4] = (short)(baseIndex + 2);
        _indices[_indexCount + 5] = (short)(baseIndex + 3);
        
        _vertexCount += 4;
        _indexCount += 6;
        _primitiveCount += 2;
    }

    public void DrawMesh(Mesh mesh, Matrix worldMatrix, Color color)
    {
        // Flush any existing batched primitives before drawing a unique mesh.
        Flush();

        _graphicsDevice.SetVertexBuffer(null);
        _graphicsDevice.Indices = null;

        // Set the world matrix for this specific object
        _basicEffect.World = worldMatrix;
        _basicEffect.DiffuseColor = color.ToVector3();

        // Set the buffers from the mesh
        var vertexBuffer = new VertexBuffer(_graphicsDevice, typeof(VertexPositionNormalColor), mesh.Vertices.Length, BufferUsage.WriteOnly);
        vertexBuffer.SetData(mesh.Vertices);
        var indexBuffer = new IndexBuffer(_graphicsDevice, typeof(short), mesh.Indices.Length, BufferUsage.WriteOnly);
        indexBuffer.SetData(mesh.Indices);

        _graphicsDevice.SetVertexBuffer(vertexBuffer);
        _graphicsDevice.Indices = indexBuffer;

        // Apply the effect and draw the mesh
        foreach (EffectPass pass in _basicEffect.CurrentTechnique.Passes)
        {
            pass.Apply();
            _graphicsDevice.DrawIndexedPrimitives(
                PrimitiveType.TriangleList,
                0,
                0,
                mesh.Indices.Length / 3);
        }

        // Reset the world matrix to identity for subsequent batched primitives
        _basicEffect.World = Matrix.Identity;
        _basicEffect.DiffuseColor = Vector3.One; // Reset to white

        vertexBuffer.Dispose();
        indexBuffer.Dispose();
    }

    /// <summary>
    /// Ends the current batch and renders all primitives
    /// </summary>
    public void End()
    {
        if (_primitiveCount > 0)
        {
            Flush();
        }
        Reset();
    }
    
    /// <summary>
    /// Renders all primitives in the current batch with 3D rendering settings
    /// </summary>
    private void Flush()
    {
        if (_primitiveCount == 0) return;
        
        // Update buffers with current data
        _vertexBuffer.SetData(_vertices, 0, _vertexCount);
        _indexBuffer.SetData(_indices, 0, _indexCount);
        
        // Set up graphics device for 3D rendering
        _graphicsDevice.SetVertexBuffer(_vertexBuffer);
        _graphicsDevice.Indices = _indexBuffer;
        
        // Enable depth testing for 3D rendering
        _graphicsDevice.DepthStencilState = DepthStencilState.Default;
        _graphicsDevice.RasterizerState = RasterizerState.CullNone;
        _graphicsDevice.BlendState = BlendState.Opaque;
        
        // Apply effect and render
        foreach (EffectPass pass in _basicEffect.CurrentTechnique.Passes)
        {
            pass.Apply();
            _graphicsDevice.DrawIndexedPrimitives(
                PrimitiveType.TriangleList,
                0, 0, _primitiveCount);
        }
    }
    
    /// <summary>
    /// Resets the batch counters
    /// </summary>
    private void Reset()
    {
        _vertexCount = 0;
        _indexCount = 0;
        _primitiveCount = 0;
    }
    
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _basicEffect?.Dispose();
            _vertexBuffer?.Dispose();
            _indexBuffer?.Dispose();
            _disposed = true;
        }
    }
}
