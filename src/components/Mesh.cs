using Microsoft.Xna.Framework;

namespace game_mono;

public class Mesh
{
    public VertexPositionNormalColor[] Vertices { get; }
    public short[] Indices { get; }

    public Mesh(VertexPositionNormalColor[] vertices, short[] indices)
    {
        Vertices = vertices;
        Indices = indices;
    }
}
