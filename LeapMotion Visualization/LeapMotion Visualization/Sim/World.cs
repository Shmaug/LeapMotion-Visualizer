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
        public static int tunnelLength = 100;
        public static int tunnelDensity = 36;
        public List<Virus> viruses = new List<Virus>();
        public Wall[,] walls;
        public List<Cell> cells = new List<Cell>();
        public Renderer renderer;
        public Camera camera;
        public float worldFade = 1f;
        public bool flipCellFaces = false;

        float cellSpawnTimer;

        public World()
        {
            renderer = new Renderer();
            camera = new Camera(new Vector2(Main.screenWidth, Main.screenHeight));
        }

        public void resetVirus()
        {
            for (int i = 0; i < viruses.Count; i++)
            {
                viruses[i].velocity = Vector3.Zero;
                viruses[i].position = Vector3.Zero;
            }
        }

        public void throwViruses(Vector3 palm, Vector3 norm, Vector3 palmVel, float grab)
        {
            for (int i = 0; i < viruses.Count; i++)
            {
                Virus v = viruses[i];

                if (((palm - v.position).LengthSquared() < 4 && grab < .4f))
                {
                    if (palmVel.Z > 1)
                        v.awake = true;
                }
            }
        }

        public void Update(GameTime gameTime)
        {
            // spawn cells
            cellSpawnTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (cellSpawnTimer <= 0)
            {
                cellSpawnTimer = 2f;
                Random r = new Random();
                Cell c = new Cell(new Vector3(-r.Next(-50,50), r.Next(-50, 50), -10));
                c.velocity = new Vector3(r.Next(-2, 2), r.Next(-2, 2), r.Next(15, 25));
                cells.Add(c);
            }

            // update cells
            List<Cell> cellremovequeue = new List<Cell>();
            for (int i = 0; i < cells.Count; i++)
            {
                Cell c = cells[i];
                c.position += c.velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
                c.sphere.Center = c.position;

                if (c.infected)
                {
                    c.virusTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                    c.update();
                }
            }
            foreach (Cell c in cellremovequeue)
                cells.Remove(c);

            if (viruses.Count > 0)
            {   
                // update viruses, make camera center on avgs
                Vector3 camPos = Vector3.Zero;
                foreach (Virus v in viruses)
                {
                    if (v.awake)
                    {
                        if (v.target != null)
                        {
                            if (v.attached)
                            {
                                if (v.cameraSubject)
                                {
                                    worldFade -= (float)gameTime.ElapsedGameTime.TotalSeconds / 2f;
                                    if (worldFade < 0)
                                        worldFade = 0;
                                    else
                                        camera.zoom += (float)gameTime.ElapsedGameTime.TotalSeconds * 8f;
                                }
                                v.velocity = v.target.velocity;
                                v.position = v.target.position + v.offset;
                                switch (v.enterType)
                                {
                                    case (EnterType.Trojan):
                                        {
                                            // wait for a second, enter
                                            v.timer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                                            if (v.injecting)
                                                v.position = Vector3.Lerp(v.target.position, v.position, v.timer / 10f);
                                            if (v.timer <= 0)
                                            {
                                                if (!v.injecting)
                                                {
                                                    v.injecting = true;
                                                    v.timer = 10f;
                                                    flipCellFaces = true;
                                                }
                                                else
                                                {
                                                    v.injecting = false;
                                                    v.awake = false;
                                                    v.reproduce();
                                                }
                                            }
                                            break;
                                        }
                                    case (EnterType.Injection):
                                        {
                                            // wait on surface, inject
                                            if (!v.injecting)
                                                v.timer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                                            if (v.timer <= 0)
                                            {
                                                v.injecting = true;
                                                flipCellFaces = true;
                                                MovableObject obj = new MovableObject(Main.main.cellModel, .025f);
                                                obj.color = Color.Red;
                                                v.data = obj;
                                            }
                                            if (v.injecting)
                                            {
                                                v.timer += (float)gameTime.ElapsedGameTime.TotalSeconds / 6f;
                                                MovableObject obj = v.data as MovableObject;
                                                obj.position = Vector3.Lerp(v.position, v.target.position, v.timer);
                                                if (v.timer > 1f)
                                                {
                                                    v.injecting = false;
                                                    v.awake = false;
                                                    MovableObject.removeObject(obj);
                                                    v.reproduce();
                                                }
                                            }
                                            break;
                                        }
                                    case (EnterType.Ingestion):
                                        {
                                            // get absorbed
                                            MovableObject obj = v.data as MovableObject;
                                            v.timer += (float)gameTime.ElapsedGameTime.TotalSeconds / 10f;
                                            if (!v.injecting)
                                            {
                                                obj.position = Vector3.Lerp(v.target.position, v.position, v.timer);
                                                if (v.timer > 1f)
                                                {
                                                    v.timer = 0f;
                                                    v.injecting = true;
                                                    if (v.cameraSubject)
                                                        flipCellFaces = true;
                                                }
                                            }
                                            else
                                            {
                                                obj.position = Vector3.Lerp(v.position, v.target.position, v.timer);
                                                v.position = obj.position;
                                                if (v.timer > 1f)
                                                {
                                                    v.injecting = false;
                                                    v.awake = false;
                                                    MovableObject.removeObject(obj);
                                                    v.reproduce();
                                                }
                                            }
                                            break;
                                        }
                                }
                            }
                            else
                            {
                                Vector3 desired = v.rotation;

                                if (Vector3.DistanceSquared(v.position, v.target.position) < 200)
                                {
                                    desired.X = (float)Math.Atan2(v.position.Y - v.target.position.Y, v.position.X - v.target.position.X) - MathHelper.PiOver2;
                                    desired.Y = (float)Math.Atan2(v.position.Z - v.target.position.Z, v.position.X - v.target.position.X);
                                    v.rotation = desired;
                                }
                                v.velocity = Vector3.Normalize(v.target.position - v.position) * 30f;
                            
                                // on collision
                                if (Vector3.DistanceSquared(v.position, v.target.position) <= 102)
                                {
                                    desired.X = (float)Math.Atan2(v.position.Y - v.target.position.Y, v.position.X - v.target.position.X) - MathHelper.PiOver2;
                                    desired.Y = (float)Math.Atan2(v.position.Z - v.target.position.Z, v.position.X - v.target.position.X);
                                    v.rotation = desired;

                                    v.attached = true;
                                    if (v.cameraSubject)
                                    {
                                        v.target.velocity = Vector3.Zero;
                                        v.velocity = Vector3.Zero;
                                    }
                                    else
                                    {
                                        v.target.velocity /= 10;
                                        v.velocity = v.target.velocity;
                                    }
                                    v.offset = v.position - v.target.position;
                                    switch (v.enterType)
                                    {
                                        case (EnterType.Trojan):
                                            {
                                                // wait for a second, enter
                                                v.timer = 2f;
                                                break;
                                            }
                                        case (EnterType.Injection):
                                            {
                                                // wait on surface, inject
                                                v.timer = 1f;
                                                break;
                                            }
                                        case (EnterType.Ingestion):
                                            {
                                                // get absorbed
                                                v.timer = 1f;
                                                MovableObject obj = new MovableObject(Main.main.cellModel, .2f);
                                                obj.color = Color.CornflowerBlue;
                                                obj.position = v.target.position;
                                                v.data = obj;
                                                break;
                                            }
                                    }
                                }
                            }
                        }
                        else
                        {
                            // find nearest target
                            Cell c = null;
                            float d = 10000;
                            foreach (Cell cell in cells)
                            {
                                float dist = Vector3.DistanceSquared(cell.position, v.position);
                                if (dist < d)
                                {
                                    d = dist;
                                    c = cell;
                                }
                            }
                            v.target = c;
                        }

                        v.position += v.velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
                        v.rotation += v.angularVelocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    }
                    camPos += v.position;
                }
                camPos /= viruses.Count;
                camera.position = Vector3.Lerp(camera.position, camPos, .2f);
            }
            camera.Update(gameTime);
        }

        public void generateWorld()
        {
            this.walls = new Wall[tunnelLength, tunnelDensity];
            for (int x = 0; x < tunnelLength; x++)
            {
                for (int a = 0; a < tunnelDensity; a++)
                {
                    float ang = MathHelper.ToRadians(a * 10);
                    Wall c = new Wall(new Vector3((float)Math.Cos(ang) * 100, (float)Math.Sin(ang) * 100, x * 10f));
                    this.walls[x, a] = c;
                }
            }
            this.camera.position = Vector3.Up;
            this.camera.rotation = new Vector3(0, MathHelper.Pi, 0);
        }

        public void Render(GraphicsDevice device, Effect effect)
        {
            device.BlendState = BlendState.AlphaBlend;
            effect.Parameters["VP"].SetValue(camera.view * camera.projection);

            effect.Parameters["alpha"].SetValue(1);
            effect.Parameters["ambient"].SetValue(Color.LawnGreen.ToVector4());
            List<Cell> cellsToKeep = new List<Cell>();

            foreach (Virus virus in this.viruses)
            {
                if (virus.target != null)
                    cellsToKeep.Add(virus.target);
                Matrix[] transforms = new Matrix[virus.model.Bones.Count];
                virus.model.CopyAbsoluteBoneTransformsTo(transforms);
                foreach (ModelMesh mesh in virus.model.Meshes)
                {
                    foreach (ModelMeshPart meshpart in mesh.MeshParts)
                    {
                        effect.Parameters["W"].SetValue(transforms[mesh.ParentBone.Index] * Matrix.CreateRotationX(virus.rotation.X) * Matrix.CreateRotationY(virus.rotation.Y) * Matrix.CreateRotationZ(virus.rotation.Z) * Matrix.CreateTranslation(virus.position));
                        meshpart.Effect = effect;
                        foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                        {
                            pass.Apply();
                            mesh.Draw();
                        }
                    }
                }
            }

            BoundingFrustum f = camera.frustum;
            if (worldFade > 0)
            {
                effect.Parameters["alpha"].SetValue(worldFade);
                effect.Parameters["ambient"].SetValue(Color.IndianRed.ToVector4());
                for (int x = 0; x < tunnelLength; x++)
                {
                    for (int a = 0; a < tunnelDensity; a++)
                    {
                        if (f.Intersects(walls[x, a].sphere))
                        {
                            Matrix[] transforms = new Matrix[walls[x, a].model.Bones.Count];
                            walls[x, a].model.CopyAbsoluteBoneTransformsTo(transforms);
                            foreach (ModelMesh mesh in walls[x, a].model.Meshes)
                            {
                                foreach (ModelMeshPart meshpart in mesh.MeshParts)
                                {
                                    effect.Parameters["W"].SetValue(transforms[mesh.ParentBone.Index] * Matrix.CreateTranslation(walls[x, a].position));
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

            if (flipCellFaces)
                device.RasterizerState = RasterizerState.CullClockwise;
            else
                device.RasterizerState = RasterizerState.CullCounterClockwise;
            foreach (Cell cell in this.cells)
            {
                if (f.Intersects(cell.sphere))
                {
                    bool c=false;
                    foreach (Cell cl in cellsToKeep)
                    {
                        if (cl == cell)
                        {
                            c = true;
                            break;
                        }
                    }
                    if (c)
                        effect.Parameters["alpha"].SetValue(cell.alpha);
                    else
                        effect.Parameters["alpha"].SetValue(worldFade);
                    // cell body
                    effect.Parameters["ambient"].SetValue(Color.CornflowerBlue.ToVector4());
                    cell.drawSphere(1, Vector3.Zero, effect);
                    // nucleus
                    effect.Parameters["ambient"].SetValue(Color.Black.ToVector4());
                    cell.drawSphere(.4f, Vector3.Zero, effect);
                    // ribosomes
                    effect.Parameters["ambient"].SetValue(Color.Orange.ToVector4());
                    foreach (Vector3 v in cell.ribosomes)
                    {
                        cell.drawSphere(.025f, v, effect);
                    }
                }
            }
            device.RasterizerState = RasterizerState.CullCounterClockwise;
        }
    }
}
