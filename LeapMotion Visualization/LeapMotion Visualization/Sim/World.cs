using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using LeapMotion_Visualization.Graphics;

namespace LeapMotion_Visualization.Sim
{
    public class World
    {
        public List<Virus> viruses = new List<Virus>();
        public List<Cell> cells = new List<Cell>();
        public Renderer renderer;
        public Camera camera;

        public World()
        {
            renderer = new Renderer();
            camera = new Camera(new Vector2(Main.screenWidth, Main.screenHeight));
        }

        public void Update(GameTime gameTime)
        {
            camera.Update(gameTime);
        }

        public void Render(GraphicsDevice device, Effect effect)
        {
            effect.Parameters["VP"].SetValue(camera.view * camera.projection);

            foreach (Virus virus in this.viruses)
            {
                Matrix[] transforms = new Matrix[virus.model.Bones.Count];
                virus.model.CopyAbsoluteBoneTransformsTo(transforms);
                foreach (ModelMesh mesh in virus.model.Meshes)
                {
                    foreach (ModelMeshPart meshpart in mesh.MeshParts)
                    {
                        effect.Parameters["W"].SetValue(transforms[mesh.ParentBone.Index] * Matrix.CreateTranslation(virus.position));
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
