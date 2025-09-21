using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace game_mono.components;

public class ModelComponent : Component, interfaces.IDrawable
{
    public Model Model { get; }
    public Color Color { get; set; }

    public ModelComponent(Model model, Color color)
    {
        Model = model;
        Color = color;
    }

    public void Draw(GraphicsDevice graphicsDevice, Matrix view, Matrix projection)
    {
        if (Model == null || GameObject == null) return;

        // Get the world matrix from the parent GameObject's transform
        Matrix world = GameObject.Transform.WorldMatrix;

        // Copy the model's absolute bone transforms to our custom array
        Matrix[] boneTransforms = new Matrix[Model.Bones.Count];
        Model.CopyAbsoluteBoneTransformsTo(boneTransforms);

        // Iterate through each mesh in the model
        foreach (var mesh in Model.Meshes)
        {
            // Iterate through each effect (material) in the mesh
            foreach (BasicEffect effect in mesh.Effects)
            {
                effect.EnableDefaultLighting();
                effect.World = boneTransforms[mesh.ParentBone.Index] * world;
                effect.View = view;
                effect.Projection = projection;

                // Set material color
                effect.DiffuseColor = Color.ToVector3();
                
                // Set up a simple lighting rig
                effect.AmbientLightColor = new Vector3(0.2f, 0.2f, 0.2f);
                effect.DirectionalLight0.Enabled = true;
                effect.DirectionalLight0.Direction = Vector3.Normalize(new Vector3(-1, -1, 0));
                effect.DirectionalLight0.DiffuseColor = new Vector3(0.8f, 0.8f, 0.8f);
            }
            // Draw the mesh
            mesh.Draw();
        }
    }
}
