using game_mono.interfaces;
using Microsoft.Xna.Framework;

namespace game_mono;

public class MeshRenderer : Component, interfaces.IDrawable
{
    public Mesh Mesh { get; }
    public Color Color { get; set; }

    public MeshRenderer(Mesh mesh, Color color)
    {
        Mesh = mesh;
        Color = color;
    }

    public void Draw(PrimitiveRenderer renderer)
    {
        GameObject.Transform.UpdateMatrix();
        renderer.DrawMesh(Mesh, GameObject.Transform.WorldMatrix, Color);
    }
}
