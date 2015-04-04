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
        Renderer miscRenderer;
        public Camera camera;

        Effect simpleEffect;
        Effect shadedEffect;
        BasicEffect basicEffect;
        SpriteFont debugFont;
        Texture2D pixelTexture;
        Model cube;
        Model ring;
        Model leapmodel;

        System.Windows.Forms.Form form;

        MouseState lastms;

        public static Main main;

        public Main()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            graphics.SynchronizeWithVerticalRetrace = true;
            graphics.PreferMultiSampling = true;

            IsMouseVisible = true;

            main = this;
            
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
            camera.position = new Vector3(0, 0, 10);
            camera.offset = new Vector3(0, 1, 0);
            camera.mode = CameraMode.Orbit;
            handRenderer = new Renderer();
            gestureRenderer = new Renderer();
            miscRenderer = new Renderer();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            basicEffect = new BasicEffect(GraphicsDevice);
            simpleEffect = Content.Load<Effect>("fx/Simple");
            shadedEffect = Content.Load<Effect>("fx/Shaded");
            debugFont = Content.Load<SpriteFont>("fonts/debug");
            pixelTexture = new Texture2D(GraphicsDevice, 1, 1);
            pixelTexture.SetData<Color>(new Color[] { Color.White });
            cube = Content.Load<Model>("fbx/Box");
            ring = Content.Load<Model>("fbx/Ring");
            leapmodel = Content.Load<Model>("fbx/leapmotion");


            MovableObject obj = new MovableObject(ring, .5f);
            obj.position = new Vector3(0, 0, 0);
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
                camera.rotation.X -= (ms.Y - lastms.Y) * (float)gameTime.ElapsedGameTime.TotalSeconds * .15f;
                camera.rotation.Y -= (ms.X - lastms.X) * (float)gameTime.ElapsedGameTime.TotalSeconds * .15f;
            }
            lastms = ms;
            #endregion

            Frame frame = leapInput.getFrame();
            Frame lastFrame = leapInput.getFrame(1);

            foreach (Hand hand in frame.Hands)
            {
                Debug.addWatch(hand.Confidence, "confidence");
                Debug.addWatch(hand.GrabStrength, "grabstrength");

                if (frame.Hands.Count == 1 && frame.Hands[0].GrabStrength == 0)
                {
                    // panning
                    Hand h = frame.Hands[0];
                    Vector3 speed = Util.toWorldNoTransform(h.PalmVelocity);

                    camera.rotation.X -= (speed.Z / camera.zoom) / camera.zoom;
                    camera.rotation.Y -= (speed.X / camera.zoom) / camera.zoom;
                }
                else if (frame.Hands.Count == 2 && lastFrame.Hands.Count == 2)
                {
                    // zooming
                    Hand lh = frame.Hands[0].IsLeft ? frame.Hands[0] : frame.Hands[1];
                    Hand rh = frame.Hands[1].IsLeft ? frame.Hands[0] : frame.Hands[1];
                    Hand llh = lastFrame.Hands[0].IsLeft ? lastFrame.Hands[0] : lastFrame.Hands[1];
                    Hand lrh = lastFrame.Hands[1].IsLeft ? lastFrame.Hands[0] : lastFrame.Hands[1];
                    if (lh.GrabStrength == 0 && rh.GrabStrength == 0 && llh.GrabStrength == 0 && lrh.GrabStrength == 0)
                    {
                        float curDist = Vector3.Distance(Util.toWorldNoTransform(lh.StabilizedPalmPosition), Util.toWorldNoTransform(rh.StabilizedPalmPosition));
                        float prevDist = Vector3.Distance(Util.toWorldNoTransform(llh.StabilizedPalmPosition), Util.toWorldNoTransform(lrh.StabilizedPalmPosition));

                        float delta = curDist - prevDist;

                        camera.zoom -= delta;
                        camera.zoom = MathHelper.Clamp(camera.zoom, 1, 30);
                    }
                }
            }

            handRenderer.setLineVerticies(Util.visualizeHands(frame));

            MovableObject.Update(gameTime, frame);

            #region gesture processing
            List<VertexPositionColor> verts = new List<VertexPositionColor>();
            foreach (Gesture g in frame.Gestures(leapInput.getFrame(10)))
            {
                switch(g.Type)
                {
                    case (Gesture.GestureType.TYPE_SWIPE):
                        {
                            break;
                        }
                    case (Gesture.GestureType.TYPE_CIRCLE):
                        {

                            break;
                        }
                    case (Gesture.GestureType.TYPE_SCREEN_TAP):
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

            shadedEffect.Parameters["lightDir"].SetValue(camera.rotation / MathHelper.TwoPi);

            handRenderer.Render(GraphicsDevice, camera, simpleEffect);
            gestureRenderer.Render(GraphicsDevice, camera, simpleEffect);
            MovableObject.Draw(GraphicsDevice, camera, shadedEffect);
            miscRenderer.Render(GraphicsDevice, camera, shadedEffect);

            spriteBatch.Begin();
            Debug.Draw(spriteBatch, debugFont);
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
