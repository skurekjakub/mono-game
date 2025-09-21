using Microsoft.Xna.Framework;
using System;

namespace game_mono;

public static class MeshFactory
{
    public static Mesh CreatePyramid(float baseSize, float height, Color color)
    {
        float halfBase = baseSize * 0.5f;

        var positions = new Vector3[]
        {
            new Vector3(-halfBase, 0, -halfBase), // 0: Back-left
            new Vector3(halfBase, 0, -halfBase),  // 1: Back-right
            new Vector3(halfBase, 0, halfBase),   // 2: Front-right
            new Vector3(-halfBase, 0, halfBase),  // 3: Front-left
            new Vector3(0, height, 0)             // 4: Apex
        };

        Vector3 backFaceNormal = CalculateFaceNormal(positions[0], positions[1], positions[4]);
        Vector3 rightFaceNormal = CalculateFaceNormal(positions[1], positions[2], positions[4]);
        Vector3 frontFaceNormal = CalculateFaceNormal(positions[2], positions[3], positions[4]);
        Vector3 leftFaceNormal = CalculateFaceNormal(positions[3], positions[0], positions[4]);

        var vertices = new VertexPositionNormalColor[]
        {
            // Base vertices
            new VertexPositionNormalColor(positions[0], Vector3.Down, color),
            new VertexPositionNormalColor(positions[1], Vector3.Down, color),
            new VertexPositionNormalColor(positions[2], Vector3.Down, color),
            new VertexPositionNormalColor(positions[3], Vector3.Down, color),

            // Side faces
            new VertexPositionNormalColor(positions[0], backFaceNormal, color),
            new VertexPositionNormalColor(positions[1], backFaceNormal, color),
            new VertexPositionNormalColor(positions[4], backFaceNormal, color),

            new VertexPositionNormalColor(positions[1], rightFaceNormal, color),
            new VertexPositionNormalColor(positions[2], rightFaceNormal, color),
            new VertexPositionNormalColor(positions[4], rightFaceNormal, color),

            new VertexPositionNormalColor(positions[2], frontFaceNormal, color),
            new VertexPositionNormalColor(positions[3], frontFaceNormal, color),
            new VertexPositionNormalColor(positions[4], frontFaceNormal, color),

            new VertexPositionNormalColor(positions[3], leftFaceNormal, color),
            new VertexPositionNormalColor(positions[0], leftFaceNormal, color),
            new VertexPositionNormalColor(positions[4], leftFaceNormal, color),
        };

        var indices = new short[]
        {
            0, 2, 1, 0, 3, 2,
            4, 5, 6,
            7, 8, 9,
            10, 11, 12,
            13, 14, 15
        };

        return new Mesh(vertices, indices);
    }

    private static Vector3 CalculateFaceNormal(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        return Vector3.Normalize(Vector3.Cross(v2 - v1, v3 - v1));
    }
}
