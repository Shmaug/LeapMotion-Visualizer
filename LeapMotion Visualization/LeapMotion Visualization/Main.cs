using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Leap;

namespace LeapMotion_Visualization
{
    public class Main : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        int screenWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
        int screenHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;

        LeapHandler leapInput;

        Renderer handRenderer;
        Renderer gestureRenderer;
        Renderer sceneRenderer;
        Camera camera;

        Effect simpleEffect;
        BasicEffect basicEffect;
        SpriteFont debugFont;
        Texture2D pixelTexture;
        Model cube;

        System.Windows.Forms.Form form;

        MouseState lastms;

        public Main()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            graphics.SynchronizeWithVerticalRetrace = true;
            graphics.PreferMultiSampling = true;

            IsMouseVisible = true;
            
            try
            {
                IntPtr hWnd = this.Window.Handle;
                var control = System.Windows.Forms.Control.FromHandle(hWnd);
                form = control.FindForm();
                form.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
                form.WindowState = System.Windows.Forms.FormWindowState.Maximized;
                
                form.Resize += form_Resize;
            }
            catch { }
        }

        private void form_Resize(object sender, EventArgs e)
        {
            screenWidth = form.Width;
            screenHeight = form.Height;
            graphics.PreferredBackBufferWidth = screenWidth;
            graphics.PreferredBackBufferHeight = screenHeight;
            if (camera != null)
                camera.refreshProjection(new Vector2(screenWidth, screenHeight));
        }

        protected override void Initialize()
        {
            leapInput = new LeapHandler();
            camera = new Camera(new Vector2(screenWidth, screenHeight));
            camera.position = new Vector3(0, 1, -10);
            handRenderer = new Renderer(GraphicsDevice);
            gestureRenderer = new Renderer(GraphicsDevice);
            sceneRenderer = new Renderer(GraphicsDevice);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            basicEffect = new BasicEffect(GraphicsDevice);
            simpleEffect = Content.Load<Effect>("fx/Simple");
            debugFont = Content.Load<SpriteFont>("fonts/debug");
            pixelTexture = new Texture2D(GraphicsDevice, 1, 1);
            pixelTexture.SetData<Color>(new Color[] { Color.White });
            cube = Content.Load<Model>("fbx/Box");
        }

        protected override void UnloadContent()
        {

        }

        protected override void Update(GameTime gameTime)
        {
            #region mouselook
            MouseState ms = Mouse.GetState();
            if (ms.LeftButton == ButtonState.Pressed)
            {
                camera.rotation.X += (ms.Y - lastms.Y) * (float)gameTime.ElapsedGameTime.TotalSeconds * .25f;
                camera.rotation.Y += (ms.X - lastms.X) * (float)gameTime.ElapsedGameTime.TotalSeconds * .25f;
            }
            lastms = ms;
            #endregion

            sceneRenderer.setModel(cube, Microsoft.Xna.Framework.Matrix.CreateTranslation(new Vector3(0,1,0)));

            Frame frame = leapInput.getFrame();

            handRenderer.setLineVerticies(Util.visualizeHands(frame));

            #region hand input
            if (frame.Hands.Count > 0)
            {
                camera.offset = Util.toV3(frame.Hands[0].StabilizedPalmPosition);
            }
            #endregion

            #region gesture processing
            List<VertexPositionColor> verts = new List<VertexPositionColor>();
            foreach (Gesture g in frame.Gestures(leapInput.getFrame(10)))
            {
                Debug.print(g.Type);
                switch(g.Type)
                {
                    case (Gesture.GestureType.TYPE_SWIPE):
                        {
                            foreach (Pointable p in g.Pointables)
                            {
                                Vector3 pos = Util.toV3(p.StabilizedTipPosition);
                                Vector3 to = pos + Util.toV3(p.Direction);

                                verts.Add(new VertexPositionColor(pos, Color.Blue));
                                verts.Add(new VertexPositionColor(to, Color.Red));
                            }
                            break;
                        }
                    case (Gesture.GestureType.TYPE_CIRCLE):
                        {

                            break;
                        }
                }
            }
            gestureRenderer.setLineVerticies(verts.ToArray());
            #endregion
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            handRenderer.Render(GraphicsDevice, camera, simpleEffect);
            gestureRenderer.Render(GraphicsDevice, camera, simpleEffect);
            sceneRenderer.Render(GraphicsDevice, camera, basicEffect);

            spriteBatch.Begin();
            Debug.Draw(spriteBatch, debugFont);
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
