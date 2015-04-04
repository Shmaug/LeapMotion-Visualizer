using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LeapMotion_Visualization
{
    class Renderer
    {
        private VertexPositionColor[] verticies;
        private short[] indicies;

        private VertexPositionColor[] lineVerticies;

        public Renderer(GraphicsDevice device)
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

        public void Render(GraphicsDevice device, Camera camera, Effect effect)
        {
            effect.Parameters["WVP"].SetValue(camera.view * camera.projection * camera.world);
            if (verticies.Length != 0)
            {
                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    device.DrawUserIndexedPrimitives<VertexPositionColor>(PrimitiveType.TriangleList, verticies, 0, verticies.Length, indicies, 0, indicies.Length / 3);
                }
            }
            if (lineVerticies.Length != 0)
            {
                Debug.addWatch(lineVerticies.Length, "drawing line verts");
                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    device.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineList, lineVerticies, 0, lineVerticies.Length / 2);
                }
            }
        }
    }
}
