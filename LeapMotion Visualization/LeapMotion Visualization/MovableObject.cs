using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Leap;

namespace LeapMotion_Visualization
{
    class MovableObject
    {
        static List<MovableObject> objects = new List<MovableObject>();

        public Vector3 position;
        public Vector3 rotation;
        public Vector3 angularVelocity;
        public Vector3 velocity;
        public float scale;
        public Model model;
        Renderer r;

        public MovableObject(Model m, float scale = 1f)
        {
            this.position = Vector3.Zero;
            this.rotation = Vector3.Zero;
            this.scale = scale;
            this.model = m;
            r = new Renderer();
            r.setObject(this);

            objects.Add(this);
        }

        public static void Update(GameTime g, Frame frame)
        {
            foreach (MovableObject o in objects)
            {
                o.position += o.velocity * (float)g.ElapsedGameTime.TotalSeconds;
                o.rotation += o.angularVelocity * (float)g.ElapsedGameTime.TotalSeconds;
                o.velocity *= .9f;
                o.angularVelocity *= .99f;
            }
        }

        public static void Draw(GraphicsDevice d, Camera c, Effect e)
        {
            foreach (MovableObject o in objects)
            {
                o.r.Render(d, c, e);
            }
        }
    }
}
