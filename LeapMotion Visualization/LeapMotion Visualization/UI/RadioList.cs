using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace LeapMotion_Visualization.UI
{
    struct RadioListElement
    {
        public float offset;
        public string name;
        public string detail;
        public bool hover;
        public bool disabled;
        public Click onSelected;
    }
    class RadioList : UIElement
    {
        public List<RadioListElement> items = new List<RadioListElement>();
        public int selected = -1;

        public RadioList(UDim2 pos, UDim2 size, string label, List<RadioListElement> items)
        {
            this.Position = pos;
            this.Size = size;
            this.text = label;
            this.items = items;
        }
    }
}
