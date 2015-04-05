using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LeapMotion_Visualization.Graphics
{
    public class Renderer
    {
        private VertexPositionColor[] verticies;
        private short[] indicies;

        private VertexPositionColor[] lineVerticies;

        private MovableObject mobj;

        private Model model;
        private Matrix modelWorld;

        public Renderer()
        {
            verticies = new VertexPositionColor[0];
            indicies = new short[0];

            lineVerticies = new VertexPositionColor[0];
        }

        public void setVerticies(VertexPositionColor[] verts)
        {
            verticies = verts;
        }

        public void setIndicies(short[] ind)
        {
            indicies = ind;
        }

        public void setLineVerticies(VertexPositionColor[] verts)
        {
            lineVerticies = verts;
        }

        public void setObject(MovableObject m)
        {
            mobj = m;
        }

        public void setModel(Model m, Matrix w)
        {
            model = m;
            modelWorld = w;
        }
        public void setModelWorld(Matrix w)
        {
            modelWorld = w;
        }

        public void Render(GraphicsDevice device, Camera camera, Effect effect)
        {
            if (verticies.Length != 0)
            {
                effect.Parameters["VP"].SetValue(camera.view * camera.projection);
                effect.Parameters["W"].SetValue(camera.world);
                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    device.DrawUserIndexedPrimitives<VertexPositionColor>(PrimitiveType.TriangleList, verticies, 0, verticies.Length, indicies, 0, indicies.Length / 3);
                }
            }
            if (lineVerticies.Length != 0)
            {
                effect.Parameters["VP"].SetValue(camera.view * camera.projection);
                effect.Parameters["W"].SetValue(camera.world);
                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    device.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineList, lineVerticies, 0, lineVerticies.Length / 2);
                }
            }
            if (mobj != null)
            {
                Matrix world = Matrix.CreateScale(mobj.scale) * Matrix.CreateTranslation(mobj.position) * Matrix.CreateRotationX(mobj.rotation.X) * Matrix.CreateRotationY(mobj.rotation.Y) * Matrix.CreateRotationZ(mobj.rotation.Z);
                effect.Parameters["VP"].SetValue(camera.view * camera.projection);

                Matrix[] transforms = new Matrix[mobj.model.Bones.Count];
                mobj.model.CopyAbsoluteBoneTransformsTo(transforms);
                foreach (ModelMesh mesh in mobj.model.Meshes)
                {
                    foreach (ModelMeshPart meshpart in mesh.MeshParts)
                    {
                        effect.Parameters["W"].SetValue(transforms[mesh.ParentBone.Index] * world);
                        meshpart.Effect = effect;
                        foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                        {
                            pass.Apply();
                            mesh.Draw();
                        }
                    }
                }
            }
            if (model != null)
            {
                effect.Parameters["VP"].SetValue(camera.view * camera.projection);

                Matrix[] transforms = new Matrix[model.Bones.Count];
                model.CopyAbsoluteBoneTransformsTo(transforms);
                foreach (ModelMesh mesh in model.Meshes)
                {
                    foreach (ModelMeshPart meshpart in mesh.MeshParts)
                    {
                        effect.Parameters["W"].SetValue(transforms[mesh.ParentBone.Index] * modelWorld);
                        meshpart.Effect = effect;
                        foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                        {
                            pass.Apply();
                            mesh.Draw();
                        }
                    }
                }
            }
        }
    }
}
