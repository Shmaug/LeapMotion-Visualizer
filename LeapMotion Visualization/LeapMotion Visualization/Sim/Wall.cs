using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LeapMotion_Visualization.Sim
{
    public class Wall
    {
        public Vector3 position;
        public Model model;
        public BoundingSphere sphere;

        public Wall(Vector3 pos)
        {
            this.position = pos;
            this.model = Main.main.cellModel;
            this.sphere = new BoundingSphere(pos, 10);
        }
    }
}
