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
        Renderer titleRenderer;
        public Camera handCamera;

        Effect simpleEffect;
        Effect shadedEffect;
        Effect noiseEffect;
        BasicEffect basicEffect;
        SpriteFont debugFont;
        SpriteFont uiFont;
        public Texture2D pixelTexture;
        Model leapmodel;
        Model titletext;
        public Model icoVirusModel;
        public Model comVirusModel;
        public Model cellModel;

        System.Windows.Forms.Form form;

        public GameState gameState;

        public static Main main;

        float noiseDepth;

        public Sim.World world;

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
            if (world.camera != null)
                world.camera.refreshProjection(new Vector2(screenWidth, screenHeight));
        }

        protected override void Initialize()
        {
            leapInput = new LeapHandler();
            world = new Sim.World();
            world.camera.position = new Vector3(0, 0, 0);
            world.camera.offset = new Vector3(0, 0, 0);
            world.camera.mode = CameraMode.Orbit;
            handCamera = new Camera(new Vector2(screenWidth, screenHeight));
            handCamera.mode = CameraMode.FirstPerson;
            handCamera.position = new Vector3(0, 1, 10);
            handRenderer = new Renderer();
            titleRenderer = new Renderer();

            gameState = GameState.MainMenu;

            #region UI
            UI.Screen mainScreen = new UI.Screen(new UI.UDim2(0, 0, 0, 0), new UI.UDim2(1, 1, 0, 0));
            UI.Button start = mainScreen.addButton(new UI.UDim2(.5f, 1, -100, -300), new UI.UDim2(0, 0, 200, 60), "Start");
            UI.Button exit = mainScreen.addButton(new UI.UDim2(.5f, 1, -100, -200), new UI.UDim2(0, 0, 200, 60), "Exit");

            UI.Screen constructionScreen = new UI.Screen(new UI.UDim2(0, 0, 0, 0), new UI.UDim2(1, 1, 0, 0), false);
            UI.Button back = constructionScreen.addButton(new UI.UDim2(.5f, 1, -100, -150), new UI.UDim2(0, 0, 200, 60), "Back");
            UI.RadioList entrymethods = constructionScreen.addRadioList(new UI.UDim2(0, 0f, 20, 125), new UI.UDim2(0, 0, 200, 60), "Entry Method", new List<UI.RadioListElement>() {
                new UI.RadioListElement(){name="Trojan Horse", detail="Target cell unknowingly absorbes virus"},
                new UI.RadioListElement(){name="Ingestion", detail="Target cell ingests virus"},
                new UI.RadioListElement(){name="Injection",detail="Virus injects genetic material directly into target cell" }});
            UI.RadioList exitmethods = constructionScreen.addRadioList(new UI.UDim2(.5f, 0f, 0, 125), new UI.UDim2(0, 0, 200, 60), "Exit Method", new List<UI.RadioListElement>() {
                new UI.RadioListElement(){name="Lysis", detail="Cell bursts and releases virus"},
                new UI.RadioListElement(){name="Pinch/Bud",detail="Virus penetrates through cell membrane" }});
            UI.RadioList shapes = constructionScreen.addRadioList(new UI.UDim2(.5f, 0f, 0, 425), new UI.UDim2(0, 0, 200, 60), "Shape", new List<UI.RadioListElement>() {
                new UI.RadioListElement(){name="Spherical", detail="", onSelected = () =>
                {
                    UI.RadioListElement el = entrymethods.items[2];
                    el.disabled = true;
                    el.detail = "Not compatible with injection!";
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
            UI.Button begin = constructionScreen.addButton(new UI.UDim2(.5f, 1, -100, -250), new UI.UDim2(0, 0, 200, 60), "Start");

            constructionScreen.buttonColor = Color.CadetBlue;
            mainScreen.buttonColor = Color.CadetBlue;

            begin.onClick += () =>
            {
                if (shapes.selected + entrymethods.selected + type.selected + exitmethods.selected < 0)
                {
                    //Debug.print("hey, dumbass, you forgot one");
                }
                else
                {
                    mainScreen.visible = false;
                    constructionScreen.visible = false;
                    gameState = GameState.InGame;

                    Sim.VirusShape vshape = shapes.selected == 0 ? Sim.VirusShape.Icosahedral : Sim.VirusShape.Complex;
                    Sim.EnterType entype = (Sim.EnterType)entrymethods.selected;
                    Sim.ExitType extype = (Sim.ExitType)exitmethods.selected;
                    Sim.VirusType vtype = (Sim.VirusType)type.selected;
                    Sim.Virus virus = new Sim.Virus(vtype, vshape, entype, extype, Vector3.Zero, world);
                    virus.cameraSubject = true;

                    world.generateWorld();
                }
            };

            start.onClick += () =>
            {
                mainScreen.visible = false;
                constructionScreen.visible = true;
                titleRenderer.setModel(null, Microsoft.Xna.Framework.Matrix.Identity);
            };
            exit.onClick += () =>
            {
                this.Exit();
            };
            back.onClick += () =>
            {
                mainScreen.visible = true;
                constructionScreen.visible = false;
                titleRenderer.setModel(titletext, Microsoft.Xna.Framework.Matrix.Identity);
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
            leapmodel = Content.Load<Model>("fbx/leapmotion");
            titletext = Content.Load<Model>("fbx/protogen");
            comVirusModel = Content.Load<Model>("fbx/ComVirus");
            icoVirusModel = Content.Load<Model>("fbx/IcoVirus");
            cellModel = Content.Load<Model>("fbx/cell");

            titleRenderer.setModel(titletext, Microsoft.Xna.Framework.Matrix.Identity);
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
                        UpdateGame(frame, lastFrame, gameTime);
                        handRenderer.setLineVerticies(Util.visualizeHands(frame));
                        break;
                    }
                case (GameState.Paused):
                    {

                        break;
                    }
                case (GameState.MainMenu):
                    {
                        noiseDepth += (float)gameTime.ElapsedGameTime.TotalSeconds;
                        float t = noiseDepth / 8 - (float)Math.Floor(noiseDepth / 8);
                        t *= 2;
                        if (t < 1)
                            titleRenderer.setModelWorld(Microsoft.Xna.Framework.Matrix.CreateTranslation(0, 2f, 0) * Microsoft.Xna.Framework.Matrix.CreateRotationY(Util.cosineInterpolation(-MathHelper.PiOver4 / 4, MathHelper.PiOver4 / 4, t)));
                        else
                            titleRenderer.setModelWorld(Microsoft.Xna.Framework.Matrix.CreateTranslation(0, 2f, 0) * Microsoft.Xna.Framework.Matrix.CreateRotationY(Util.cosineInterpolation(MathHelper.PiOver4 / 4, -MathHelper.PiOver4 / 4, t - 1)));
                        
                        break;
                    }
            }
            UI.Screen.Update(gameTime, frame);

            base.Update(gameTime);
        }

        private void UpdateGame(Frame frame, Frame lastFrame, GameTime gameTime)
        {
            foreach (Hand hand in frame.Hands)
            {
                Debug.addWatch(hand.Confidence, "confidence");
                Debug.addWatch(hand.GrabStrength, "grab");
                Debug.addWatch(hand.PinchStrength, "pinch");
            }

            if (frame.Hands.Count == 1)
            {
                Hand hand = frame.Hands[0];
                Hand prevHand = lastFrame.Hands[0];
                if (hand.Confidence > .3f)
                    world.throwViruses(Util.toWorldHand(hand.PalmPosition), Util.toWorldHand(hand.PalmNormal, false), Util.toWorldHand(hand.PalmVelocity, false), hand.GrabStrength);
            }
            else if (frame.Hands.Count == 2 && lastFrame.Hands.Count == 2)
            {
                Hand lh = frame.Hands[0].IsLeft ? frame.Hands[0] : frame.Hands[1];
                Hand rh = frame.Hands[1].IsLeft ? frame.Hands[0] : frame.Hands[1];
                Hand llh = lastFrame.Hands[0].IsLeft ? lastFrame.Hands[0] : lastFrame.Hands[1];
                Hand lrh = lastFrame.Hands[1].IsLeft ? lastFrame.Hands[0] : lastFrame.Hands[1];

                if (rh.GrabStrength == 0 && lh.PinchStrength > .8f)
                {
                    // panning
                    Vector3 speed = Util.toWorldNoTransform(rh.PalmVelocity);

                    world.camera.angularVelocity.X = (speed.Y / MathHelper.Clamp(world.camera.zoom, 4, 10)) * .15f / (float)gameTime.ElapsedGameTime.TotalSeconds;
                    world.camera.angularVelocity.Y = -(speed.X / MathHelper.Clamp(world.camera.zoom, 4, 10)) * .15f / (float)gameTime.ElapsedGameTime.TotalSeconds;
                }
                // zooming
                else if (lh.GrabStrength + rh.GrabStrength + llh.GrabStrength + lrh.GrabStrength == 0 && lh.PinchStrength + rh.PinchStrength <= 0.1f)
                {
                    float curDist = Vector3.Distance(Util.toWorldNoTransform(lh.StabilizedPalmPosition), Util.toWorldNoTransform(rh.StabilizedPalmPosition));
                    float prevDist = Vector3.Distance(Util.toWorldNoTransform(llh.StabilizedPalmPosition), Util.toWorldNoTransform(lrh.StabilizedPalmPosition));

                    float delta = curDist - prevDist;
                    delta *= 10; 

                    world.camera.zoomVelocity = -delta / (float)gameTime.ElapsedGameTime.TotalSeconds;
                }
            }

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
            #endregion

            MovableObject.Update(gameTime, frame);
            world.Update(gameTime);
        }

        private void RenderGame()
        {
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            world.Render(GraphicsDevice, shadedEffect);
            handRenderer.Render(GraphicsDevice, handCamera, simpleEffect);
            MovableObject.Draw(GraphicsDevice, world.camera, shadedEffect);
        }

        protected override void Draw(GameTime gameTime)
        {
            switch (gameState)
            {
                case (GameState.InGame):
                    {
                        GraphicsDevice.Clear(Color.Black);
                        RenderGame();

                        break;
                    }
                case (GameState.Paused):
                    {
                        GraphicsDevice.Clear(Color.Black);
                        break;
                    }
                case (GameState.MainMenu):
                    {
                        GraphicsDevice.Clear(Color.DarkRed);

                        Microsoft.Xna.Framework.Matrix projection = Microsoft.Xna.Framework.Matrix.CreateOrthographicOffCenter(0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, 0, 0, 1);
                        Microsoft.Xna.Framework.Matrix halfPixelOffset = Microsoft.Xna.Framework.Matrix.CreateTranslation(-0.5f, -0.5f, 0);
                        noiseEffect.Parameters["MatrixTransform"].SetValue(halfPixelOffset * projection);
                        noiseEffect.Parameters["depth"].SetValue(noiseDepth);
                        noiseEffect.Parameters["cursorPos"].SetValue(UI.Screen.screenFingerPos);

                        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend); 
                        noiseEffect.CurrentTechnique.Passes[0].Apply();
                        spriteBatch.Draw(pixelTexture, new Rectangle(0, 0, screenWidth, screenHeight), Color.White);
                        spriteBatch.End();

                        titleRenderer.Render(GraphicsDevice, world.camera, shadedEffect);
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
