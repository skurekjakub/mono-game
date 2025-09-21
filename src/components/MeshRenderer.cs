using game_mono.interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace game_mono.components;

public class MeshRenderer : Component, interfaces.IDrawable
{
    public Mesh Mesh { get; }
    public Color Color { get; set; }

    public MeshRenderer(Mesh mesh, Color color)
    {
        Mesh = mesh;
        Color = color;
    }

    public void Draw(GraphicsDevice graphicsDevice, Matrix view, Matrix projection)
    {
        // This component uses PrimitiveRenderer, so this method is not implemented
    }

    public void Draw(PrimitiveRenderer renderer)
    {
        GameObject.Transform.UpdateMatrix();
        renderer.DrawMesh(Mesh, GameObject.Transform.WorldMatrix, Color);
    }
}
