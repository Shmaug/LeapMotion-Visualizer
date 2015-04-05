using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LeapMotion_Visualization.UI
{
    class UDim2
    {
        public Vector2 Scale;
        public Vector2 Offset;
        public UDim2()
        {
        }
        public UDim2(float x, float y, float offX, float offY)
        {
            Scale = new Vector2(x, y);
            Offset = new Vector2(offX, offY);
        }
        public Vector2 calc(float w, float h)
        {
            return new Vector2(w * this.Scale.X, h * this.Scale.Y) + this.Offset;
        }
    }

    class Screen
    {
        public static List<Screen> screens = new List<Screen>();
        public static Vector2 screenFingerPos;

        List<UIElement> elements = new List<UIElement>();

        public bool visible;
        public UDim2 Position;
        public UDim2 Size;
        public Color backgroundColor;
        public Color buttonColor;

        public Screen(UDim2 pos, UDim2 size, bool vis = true)
        {
            this.Position = pos;
            this.Size = size;
            this.visible = vis;
            backgroundColor = Color.Transparent;
            buttonColor = Color.CornflowerBlue;

            screens.Add(this);
        }

        public Button addButton(UDim2 pos, UDim2 size, string txt)
        {
            Button b = new Button(pos, size, txt);
            this.elements.Add(b);
            return b;
        }

        public RadioList addRadioList(UDim2 pos, UDim2 size, string txt, List<RadioListElement> items)
        {
            RadioList rl = new RadioList(pos, size, txt, items);
            this.elements.Add(rl);
            return rl;
        }

        public static void Update(GameTime gameTime, Leap.Frame frame)
        {
            Leap.Pointable currentPointer = null;
            foreach (Leap.Hand hand in frame.Hands)
            {
                Vector3 furthest = new Vector3(0,0,1000);
                foreach (Leap.Pointable p in hand.Pointables)
                {
                    if (p.IsExtended)
                    {
                        Vector3 worldPos = Util.toWorldNoTransform(p.StabilizedTipPosition) - new Vector3(0, Main.handOffset, 0);
                        if (worldPos.Z < furthest.Z)
                        {
                            furthest = worldPos;
                            currentPointer = p;
                        }
                    }
                }
                Vector3 screenPos = Main.main.GraphicsDevice.Viewport.Project(furthest, Main.main.handCamera.projection, Main.main.handCamera.view, Matrix.Identity);
                screenFingerPos = new Vector2(screenPos.X, screenPos.Y);
            }

            int w = Main.screenWidth;
            int h = Main.screenHeight;
            Point finger = new Point((int)screenFingerPos.X, (int)screenFingerPos.Y);
            foreach (Screen screen in screens)
            {
                if (screen.visible)
                {
                    float sw = screen.Size.calc(w,h).X;
                    float sh = screen.Size.calc(w,h).Y;
                    foreach (UIElement e in screen.elements)
                    {
                        if (e is Button)
                        {
                            Button btn = e as Button;
                            Rectangle r = new Rectangle((int)btn.Position.calc(sw, sh).X, (int)btn.Position.calc(sw, sh).Y, (int)btn.Size.calc(sw, sh).X + (int)btn.selOffset.X, (int)btn.Size.calc(sw, sh).Y + (int)btn.selOffset.Y);
                            if (r.Contains(finger))
                            {
                                btn.selOffset.Y = MathHelper.Lerp(btn.selOffset.Y, 50, .2f);
                                if (currentPointer != null)
                                    if (currentPointer.TipVelocity.y < -200 && finger.Y > r.Bottom - 40)
                                        if (btn.onClick != null)
                                            btn.onClick.Invoke();
                            }
                            else
                            {
                                btn.selOffset.Y = MathHelper.Lerp(btn.selOffset.Y, 0, .2f);
                            }
                        }
                        else if (e is RadioList)
                        {
                            RadioList rl = e as RadioList;
                            for (int i = 0; i < rl.items.Count; i++)
                            {
                                RadioListElement item = rl.items[i];

                                int yoffset = (int)(i * rl.Size.calc(sw, sh).Y + i * 5);
                                Rectangle r = new Rectangle((int)rl.Position.calc(sw, sh).X, (int)rl.Position.calc(sw, sh).Y + yoffset, (int)rl.Size.calc(sw, sh).X, (int)rl.Size.calc(sw, sh).Y);
                                if (r.Contains(finger) && !item.disabled)
                                {
                                    item.hover = true;
                                    item.offset = MathHelper.Lerp(item.offset, i==rl.selected ? 50 : 25, .2f);
                                    if (currentPointer != null)
                                        if (currentPointer.TipVelocity.x > 150 && finger.X > r.Center.X)
                                        {
                                            rl.selected = i;
                                            if (item.onSelected != null)
                                                item.onSelected.Invoke();
                                        }
                                }
                                else
                                {
                                    item.hover = false;
                                    if (i != rl.selected)
                                        item.offset = MathHelper.Lerp(item.offset, 0, .2f);
                                    else
                                    {
                                        item.offset = MathHelper.Lerp(item.offset, 50, .2f);
                                        if (item.disabled)
                                            rl.selected = -1;
                                    }
                                }
                                rl.items[i] = item;
                            }
                        }
                    }
                }
            }
        }

        public static void Draw(SpriteBatch batch, SpriteFont uiFont)
        {
            int w = Main.screenWidth;
            int h = Main.screenHeight;
            foreach (Screen screen in screens)
            {
                if (screen.visible)
                {
                    if (screen.backgroundColor != Color.Transparent)
                        batch.Draw(Main.main.pixelTexture, new Rectangle((int)screen.Position.calc(w, h).X, (int)screen.Position.calc(w, h).Y, (int)screen.Size.calc(w, h).X, (int)screen.Size.calc(w, h).Y), screen.backgroundColor);
                    
                    float sw = screen.Size.calc(w,h).X;
                    float sh = screen.Size.calc(w,h).Y;
                    foreach (UIElement e in screen.elements)
                    {
                        Vector2 ePos = e.Position.calc(sw, sh);
                        Vector2 eSize = e.Size.calc(sw, sh);
                        if (e is Button)
                        {
                            Button btn = e as Button;
                            ePos +=  btn.selOffset;
                            batch.Draw(Main.main.pixelTexture, new Rectangle((int)ePos.X, (int)ePos.Y, (int)eSize.X, (int)eSize.Y), screen.buttonColor);
                            Vector2 s = uiFont.MeasureString(e.text);
                            batch.DrawString(uiFont, e.text, ePos + eSize / 2, Color.White, 0f, s / 2, 1f, SpriteEffects.None, 0f);
                        }
                        if (e is RadioList)
                        {
                            RadioList rl = e as RadioList;

                            Vector2 lSize = uiFont.MeasureString(e.text);
                            batch.DrawString(uiFont, e.text, ePos + eSize / 2 + new Vector2(0, eSize.Y * -1), Color.White, 0f, lSize / 2, 1f, SpriteEffects.None, 0f);

                            int i = 0;
                            foreach (RadioListElement item in rl.items)
                            {
                                batch.Draw(Main.main.pixelTexture, new Rectangle((int)ePos.X + (int)item.offset, (int)ePos.Y + (int)eSize.Y * i + (i * 5), (int)eSize.X, (int)eSize.Y), !item.disabled ? screen.buttonColor : Color.Gray);
                                Vector2 s = uiFont.MeasureString(item.name);
                                batch.DrawString(uiFont, item.name, ePos + eSize / 2 + new Vector2((int)item.offset, eSize.Y * i + (i * 5)), Color.White, 0f, s / 2, 1f, SpriteEffects.None, 0f);
                                if (i == rl.selected || item.hover)
                                {
                                    Vector2 ds = uiFont.MeasureString(item.detail);
                                    batch.DrawString(uiFont, item.detail, ePos + new Vector2(item.offset + eSize.X, eSize.Y * i + (i * 5) + eSize.Y / 2), Color.White, 0f, new Vector2(-5, ds.Y / 2), 1f, SpriteEffects.None, 0f);
                                }
                                i++;
                            }
                        }
                    }
                }
            }
            if (Main.main.gameState != GameState.InGame)
                batch.Draw(Main.main.pixelTexture, new Rectangle((int)screenFingerPos.X - 5, (int)screenFingerPos.Y - 5, 10, 10), Color.White * .5f);
        }
    }
}
