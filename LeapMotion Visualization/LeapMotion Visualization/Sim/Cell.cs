using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LeapMotion_Visualization.Sim
{
    public class Cell
    {
        public Vector3 position;
        public Vector3 velocity;
        public Vector3 rotation;
        public Vector3 angularVelocity;
        public Vector3 ribosomeOffset;
        public List<Vector3> ribosomes;
        public Model model;
        public BoundingSphere sphere;
        public float virusTimer;
        public float scale = 1f;
        public float alpha = 1f;
        public bool infected;
        public Virus host;
        private bool exploding;
        private bool reproduced;
        private bool dying;
        private List<Virus> viruses = new List<Virus>();

        public Cell(Vector3 pos)
        {
            this.position = pos;
            this.model = Main.main.cellModel;
            this.sphere = new BoundingSphere(pos, 10);
            this.ribosomes = new List<Vector3>();
            Random r = new Random();
            int r1 = r.Next(4,6) * (r.Next(1,10)>5?-1:1);
            int r2 = r.Next(4,6) * (r.Next(1,10)>5?-1:1);
            int r3 = r.Next(4,6) * (r.Next(1,10)>5?-1:1);
            ribosomeOffset = new Vector3(r1, r2, r3);
            for (int i = 0; i < r.Next(7,10); i++)
            {
                ribosomes.Add(ribosomeOffset + new Vector3(r.Next(-2, 2), r.Next(-2, 2), r.Next(-2, 2)));
            }
        }

        public void drawSphere(float scale, Vector3 offset, Effect effect)
        {
            Matrix[] transforms = new Matrix[this.model.Bones.Count];
            this.model.CopyAbsoluteBoneTransformsTo(transforms);
            foreach (ModelMesh mesh in this.model.Meshes)
            {
                foreach (ModelMeshPart meshpart in mesh.MeshParts)
                {
                    effect.Parameters["W"].SetValue(Matrix.CreateScale(this.scale * scale) * transforms[mesh.ParentBone.Index] * Matrix.CreateTranslation(this.position + offset));
                    meshpart.Effect = effect;
                    foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                    {
                        pass.Apply();
                        mesh.Draw();
                    }
                }
            }
        }

        public void update()
        {
            if (this.host.data is LeapMotion_Visualization.Graphics.MovableObject)
            {
                LeapMotion_Visualization.Graphics.MovableObject obj = this.host.data as LeapMotion_Visualization.Graphics.MovableObject;
                obj.position = Vector3.SmoothStep(this.position, this.ribosomeOffset + this.position, 1 - this.virusTimer / 5f);
                if (this.virusTimer <= 0)
                {
                    LeapMotion_Visualization.Graphics.MovableObject.removeObject(obj);
                    this.host.data = null;

                    this.virusTimer = 3f;
                }
            }
            if (this.dying)
            {
                foreach (Virus v in this.viruses)
                    v.awake = true;

                if (this.virusTimer <= 0.5f)
                {
                    if (this.host.cameraSubject)
                        Main.main.world.worldFade = 1f - this.virusTimer / .5f;
                }
                if (this.virusTimer <= 0)
                {
                    if (this.host.cameraSubject)
                        Main.main.world.worldFade = 1f;
                    Main.main.world.cells.Remove(this);
                    Main.main.world.flipCellFaces = false;
                }
            }
            if (this.exploding)
            {
                this.scale += this.virusTimer / 500f;

                generateVirus(this.host, 10);

                if (this.virusTimer <= 0.5f)
                {
                    if (this.host.cameraSubject)
                        Main.main.world.worldFade = 1f - this.virusTimer / .5f;
                    this.alpha = this.virusTimer / .5f;
                }
                if (this.virusTimer <= 0f)
                {
                    if (this.host.cameraSubject)
                        Main.main.world.worldFade = 1f;
                    Main.main.world.cells.Remove(this);
                    Main.main.world.flipCellFaces = false;

                    foreach (Virus v in this.viruses)
                        v.awake = true;
                }
            }
            if (this.virusTimer <= 0)
            {
                generateVirus(this.host);
                this.virusTimer = 3f;
            }
        }

        public void generateVirus(Virus reference, float off = 3f)
        {
            Random r = new Random(Environment.TickCount);
            Virus v = new Virus(reference.type, reference.shape, reference.enterType, reference.exitType, this.position + Vector3.Normalize(new Vector3((float)r.NextDouble() - (float)r.NextDouble(), (float)r.NextDouble() - (float)r.NextDouble(), (float)r.NextDouble() - (float)r.NextDouble())) * off, Main.main.world);
            v.velocity = Vector3.Normalize(v.position - this.position) * 10 + this.velocity;
            v.awake = false;
            v.angularVelocity = new Vector3((float)r.NextDouble() * MathHelper.PiOver2) * .5f * this.angularVelocity;
            this.viruses.Add(v);
            if (this.viruses.Count > new Random().Next(5, 7))
            {
                if (reference.exitType == ExitType.Lysis)
                    this.exploding = true;
                else
                    this.dying = true;
            }
        }
    }
}
