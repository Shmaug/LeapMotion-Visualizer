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
using LeapMotion_Visualization.Graphics;
using Leap;

namespace LeapMotion_Visualization
{
    public enum GameState
    {
        InGame,
        Paused,
        MainMenu
    }
    public class Main : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        public static int screenWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
        public static int screenHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;

        public static int handOffset = 3;

        LeapHandler leapInput;

        Renderer handRenderer;
        Renderer gestureRenderer;
        Renderer miscRenderer;
        public Camera camera;
        public Camera handCamera;

        Effect simpleEffect;
        Effect shadedEffect;
        Effect noiseEffect;
        BasicEffect basicEffect;
        SpriteFont debugFont;
        SpriteFont uiFont;
        public Texture2D pixelTexture;
        Model cube;
        Model leapmodel;

        System.Windows.Forms.Form form;

        MouseState lastms;

        public GameState gameState;

        public static Main main;

        float noiseDepth;

        public Main()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            graphics.SynchronizeWithVerticalRetrace = true;
            graphics.PreferMultiSampling = true;

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
            handCamera = new Camera(new Vector2(screenWidth, screenHeight));
            handCamera.mode = CameraMode.FirstPerson;
            handCamera.position = new Vector3(0, 1, 10);
            handRenderer = new Renderer();
            gestureRenderer = new Renderer();
            miscRenderer = new Renderer();

            gameState = GameState.MainMenu;

            #region UI
            UI.Screen mainScreen = new UI.Screen(new UI.UDim2(0,0,0,0), new UI.UDim2(1,1,0,0));
            UI.Button start = mainScreen.addButton(new UI.UDim2(.5f, 0, -300, 25), new UI.UDim2(0, 0, 200, 60), "Start");
            UI.Button exit = mainScreen.addButton(new UI.UDim2(.5f, 0, 100, 25), new UI.UDim2(0, 0, 200, 60), "Exit");

            UI.Screen constructionScreen = new UI.Screen(new UI.UDim2(0, 0, 0, 0), new UI.UDim2(1, 1, 0, 0), false);
            UI.Button back = constructionScreen.addButton(new UI.UDim2(.5f, 1, -200, -200), new UI.UDim2(0, 0, 200, 60), "Back");
            UI.RadioList entrymethods = constructionScreen.addRadioList(new UI.UDim2(0, 0f, 20, 125), new UI.UDim2(0, 0, 200, 60), "Entry Method", new List<UI.RadioListElement>() {
                new UI.RadioListElement(){name="Trojan Horse", detail="Target cell unknowingly absorbes virus"},
                new UI.RadioListElement(){name="Injestion", detail="Target cell injests virus"},
                new UI.RadioListElement(){name="Injection",detail="Virus injects genetic material directly into target cell" }});
            UI.RadioList exitmethods = constructionScreen.addRadioList(new UI.UDim2(.5f, 0f, 0, 125), new UI.UDim2(0, 0, 200, 60), "Exit Method", new List<UI.RadioListElement>() {
                new UI.RadioListElement(){name="Lysis", detail="Cell bursts and releases virus"},
                new UI.RadioListElement(){name="Pinch/Bud",detail="Virus penetrates through cell membrane" }});
            UI.RadioList shapes = constructionScreen.addRadioList(new UI.UDim2(.5f, 0f, 0, 425), new UI.UDim2(0, 0, 200, 60), "Shape", new List<UI.RadioListElement>() {
                new UI.RadioListElement(){name="Spherical", detail="", onSelected = () =>
                {
                    UI.RadioListElement el = entrymethods.items[2];
                    el.disabled = true;
                    el.detail = "Not compatible with spherical shape!";
                    entrymethods.items[2] = el;
                } },
                new UI.RadioListElement(){name="Complex", detail="", onSelected = () => {
                    UI.RadioListElement el = entrymethods.items[2];
                    el.disabled = false;
                    el.detail = "Virus injects genetic material directly into target cell";
                    entrymethods.items[2] = el;
                } }});
            UI.RadioList type = constructionScreen.addRadioList(new UI.UDim2(0, 0f, 20, 425), new UI.UDim2(0, 0, 200, 60), "Type", new List<UI.RadioListElement>() {
                new UI.RadioListElement(){name="RNA Hijack", detail="Injection of RNA to hijack nucleus"},
                new UI.RadioListElement(){name="Lysogenic Cycle",detail="RNA fuses with DNA of cell" }});
            UI.Button begin = constructionScreen.addButton(new UI.UDim2(.5f, 1, 100, -200), new UI.UDim2(0, 0, 200, 60), "Start");
            begin.onClick += () =>
            {
                mainScreen.visible = false;
                constructionScreen.visible = false;
                gameState = GameState.InGame;
            };

            start.onClick += () =>
            {
                mainScreen.visible = false;
                constructionScreen.visible = true;
            };
            exit.onClick += () =>
            {
                this.Exit();
            };
            back.onClick += () =>
            {
                mainScreen.visible = true;
                constructionScreen.visible = false;
            };
            #endregion
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            basicEffect = new BasicEffect(GraphicsDevice);
            simpleEffect = Content.Load<Effect>("fx/Simple");
            shadedEffect = Content.Load<Effect>("fx/Shaded");
            noiseEffect = Content.Load<Effect>("fx/noise");
            debugFont = Content.Load<SpriteFont>("fonts/debug");
            uiFont = Content.Load<SpriteFont>("fonts/uifont");
            pixelTexture = new Texture2D(GraphicsDevice, 1, 1);
            pixelTexture.SetData<Color>(new Color[] { Color.White });
            cube = Content.Load<Model>("fbx/Box");
            leapmodel = Content.Load<Model>("fbx/leapmotion");
        }

        protected override void UnloadContent()
        {

        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                this.Exit();

            Frame frame = leapInput.getFrame();
            Frame lastFrame = leapInput.getFrame(1);

            switch (gameState)
            {
                case (GameState.InGame):
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

                        foreach (Hand hand in frame.Hands)
                        {
                            Debug.addWatch(hand.Confidence, "confidence");
                            Debug.addWatch(hand.GrabStrength, "grab");
                            Debug.addWatch(hand.PinchStrength, "pinch");

                            if (hand.Confidence > .1f)
                            {
                                if (frame.Hands.Count == 1)
                                {
                                    if (hand.GrabStrength == 0)
                                    {
                                        // panning
                                        Vector3 speed = Util.toWorldNoTransform(hand.PalmVelocity);

                                        camera.angularVelocity.X = -(speed.Z / camera.zoom) * .25f / (float)gameTime.ElapsedGameTime.TotalSeconds;
                                        camera.angularVelocity.Y = -(speed.X / camera.zoom) * .25f / (float)gameTime.ElapsedGameTime.TotalSeconds;
                                    }
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
                                        delta *= 10;

                                        camera.zoomVelocity = -delta / (float)gameTime.ElapsedGameTime.TotalSeconds;
                                    }
                                }
                            }
                        }

                        handRenderer.setLineVerticies(Util.visualizeHands(frame));

                        #region gesture processing
                        List<VertexPositionColor> verts = new List<VertexPositionColor>();
                        foreach (Gesture g in frame.Gestures(leapInput.getFrame(10)))
                        {
                            switch (g.Type)
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

                        MovableObject.Update(gameTime, frame);
                        camera.Update(gameTime);
                        break;
                    }
                case (GameState.Paused):
                    {

                        break;
                    }
                case (GameState.MainMenu):
                    {
                        noiseDepth += (float)gameTime.ElapsedGameTime.TotalSeconds / 2f;
                        break;
                    }
            }
            UI.Screen.Update(gameTime, frame);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            switch (gameState)
            {
                case (GameState.InGame):
                    {
                        GraphicsDevice.Clear(Color.Black);

                        GraphicsDevice.DepthStencilState = DepthStencilState.Default;

                        shadedEffect.Parameters["lightDir"].SetValue(camera.rotation / -MathHelper.TwoPi);

                        handRenderer.Render(GraphicsDevice, handCamera, simpleEffect);
                        gestureRenderer.Render(GraphicsDevice, camera, simpleEffect);
                        MovableObject.Draw(GraphicsDevice, camera, shadedEffect);
                        miscRenderer.Render(GraphicsDevice, camera, shadedEffect);

                        break;
                    }
                case (GameState.Paused):
                    {
                        GraphicsDevice.Clear(Color.Black);
                        break;
                    }
                case (GameState.MainMenu):
                    {
                        Microsoft.Xna.Framework.Matrix projection = Microsoft.Xna.Framework.Matrix.CreateOrthographicOffCenter(0,
                            GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, 0, 0, 1);
                        Microsoft.Xna.Framework.Matrix halfPixelOffset = Microsoft.Xna.Framework.Matrix.CreateTranslation(-0.5f, -0.5f, 0);
                        noiseEffect.Parameters["MatrixTransform"].SetValue(halfPixelOffset * projection);
                        noiseEffect.Parameters["depth"].SetValue(noiseDepth);
                        noiseEffect.Parameters["cursorPos"].SetValue(UI.Screen.screenFingerPos);

                        GraphicsDevice.Clear(Color.DarkRed);
                        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend); 
                        noiseEffect.CurrentTechnique.Passes[0].Apply();
                        spriteBatch.Draw(pixelTexture, new Rectangle(0, 0, screenWidth, screenHeight), Color.White);
                        spriteBatch.End();
                        break;
                    }
            }
            
            spriteBatch.Begin();
            UI.Screen.Draw(spriteBatch, uiFont);
            Debug.Draw(spriteBatch, debugFont);
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
