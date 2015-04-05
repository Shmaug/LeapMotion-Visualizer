using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace LeapMotion_Visualization.UI
{
    public delegate void Click();
    class Button : UIElement
    {
        public Vector2 selOffset;
        public Click onClick;

        public Button(UDim2 pos, UDim2 size, string txt)
        {
            this.Position = pos;
            this.Size = size;
            this.text = txt;
        }
    }
}
