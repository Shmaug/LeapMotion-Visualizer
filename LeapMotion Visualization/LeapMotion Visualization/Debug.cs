using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LeapMotion_Visualization
{
    static class Debug
    {
        private static object[] log = new object[10];
        private static List<object> watch = new List<object>();

        public static void addWatch(object o, string name = "")
        {
            watch.Add(name + (name != "" ? ": " : "") + o);
        }

        public static void print(object o)
        {
            for (int i = 9; i >= 0; i--)
            {
                if (i == 0)
                    log[i] = o;
                else
                    log[i] = log[i - 1];
            }
        }

        public static void Draw(SpriteBatch batch, SpriteFont font)
        {
            string final = "";
            for (int i = 0; i < 9; i++)
            {
                final += log[i] + "\n";
            }
            batch.DrawString(font, final, Vector2.One * 10, Color.White);

            string final2 = "";
            foreach (object item in watch)
            {
                final2 += item + "\n";
            }
            batch.DrawString(font, final2, new Vector2(10, font.MeasureString(final).Y + 20), Color.Gray);

            watch.Clear();
        }
    }
}
