using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LeapMotion_Visualization
{
    class Camera
    {
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 offset;

        public Matrix world;

        public Matrix view
        {
            get
            {
                return Matrix.CreateTranslation(position + offset) *
                    Matrix.CreateRotationX(rotation.X) *
                    Matrix.CreateRotationY(rotation.Y) *
                    Matrix.CreateRotationZ(rotation.Z);
            }
        }
        public Matrix projection;
        public BoundingFrustum frustum
        {
            get
            {
                return new BoundingFrustum(view * projection);
            }
        }


        public void lookAt(Vector3 pos)
        {
            Vector3 s;
            Vector3 p;
            Quaternion r;
            Matrix.CreateLookAt(position, pos, Vector3.Up).Decompose(out s, out r, out p);
            Vector3 rot = Util.QuaternionToEuler(r);
            this.rotation = rot;
        }

        public void refreshProjection(Vector2 screen)
        {
            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(50f), screen.X / screen.Y, .1f, 1000f);
        }

        public Camera(Vector2 screen)
        {
            world = Matrix.Identity;
            position = Vector3.Zero;
            rotation = Vector3.Zero;
            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(50f), screen.X / screen.Y, .1f, 1000f);
        }
    }
}
