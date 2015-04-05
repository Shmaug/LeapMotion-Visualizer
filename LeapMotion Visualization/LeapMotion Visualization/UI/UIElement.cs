using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LeapMotion_Visualization.UI
{
    class UIElement
    {
        public UDim2 Position;
        public UDim2 Size;
        public string text;

        public UIElement()
        {
            
        }

        public UIElement(UDim2 pos, UDim2 size, string text)
        {
            this.Position = pos;
            this.Size = size;
            this.text = text;
        }
    }
}
