using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;
using BEPUphysics.Entities.Prefabs;
using BEPUutilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using BEPUphysics.Entities;
using BEPUphysics;
using BEPUphysics.NarrowPhaseSystems.Pairs;
using Vector3 = BEPUutilities.Vector3;
using Matrix = BEPUutilities.Matrix;
using XNAVector2 = Microsoft.Xna.Framework.Vector2;
using System;
using BEPUphysics.Materials;
using System.Runtime.InteropServices;

/*
    todo: model, use Z forward Y up
*/

namespace game
{
    public class Game : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        Space space;
        public Camera Camera;
        public Model GolfBallModel;
        public Model Map1;
        public Model Map1teearea;
        public KeyboardState KeyboardState;
        public MouseState MouseState;
        private GolfBall golfBall;
        private GolfCourse golfCourse;
        private SpriteBatch spriteBatch;
        private SpriteFont font;
        private Texture2D crosshairTexture;

        public int numBalls = 0;
        public int numStrokes = 0;

        public Game()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = (int)1920 / 2;
            graphics.PreferredBackBufferHeight = (int)1080 / 2;
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            Camera = new Camera(this, new Vector3(-17, 11, -18), 5);
            graphics.IsFullScreen = false;
            graphics.ApplyChanges();
            base.Initialize();
        }
        protected override void LoadContent()
        {
            GolfBallModel = Content.Load<Model>("golfball3");
            Map1 = Content.Load<Model>("map1");
            Map1teearea = Content.Load<Model>("map1teearea");

            // Create a 1x1 white texture for the crosshair
            crosshairTexture = new Texture2D(GraphicsDevice, 1, 1);
            crosshairTexture.SetData(new[] { Color.Black });

            space = new Space();
            space.ForceUpdater.Gravity = new Vector3(0, -15.00f, 0);

            TimeStepSettings timeStep = new TimeStepSettings();
            timeStep.TimeStepDuration = 1f / 60f;
            space.TimeStepSettings = timeStep;

            golfCourse = new GolfCourse(this, space);
            golfCourse.LoadCourse();

            golfBall = new GolfBall(this, Camera, space);

            spriteBatch = new SpriteBatch(GraphicsDevice);
            font = Content.Load<SpriteFont>("Arial");
        }

        void HandleCollision(EntityCollidable sender, Collidable other, CollidablePairHandler pair)
        {
            var otherEntityInformation = other as EntityCollidable;
            if (otherEntityInformation != null)
            {
                space.Remove(otherEntityInformation.Entity);
                Components.Remove((EntityModel)otherEntityInformation.Entity.Tag);
            }
        }


        protected override void UnloadContent()
        {

        }


        bool clicking = false;
        protected override void Update(GameTime gameTime)
        {
            KeyboardState = Keyboard.GetState();
            MouseState = Mouse.GetState();

            if (KeyboardState.IsKeyDown(Keys.Escape))
            {
                Exit();
                return;
            }

            Camera.Update((float)gameTime.ElapsedGameTime.TotalSeconds);

            if (MouseState.LeftButton == ButtonState.Released)
            {
                clicking = false;
            }

            if (numBalls < 1 && MouseState.LeftButton == ButtonState.Pressed && (!clicking || KeyboardState.IsKeyDown(Keys.LeftShift)))
            {
                clicking = true;
                numStrokes++;
                golfBall.SpawnBall();
                numBalls++;
            }

            space.Update();
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

            base.Draw(gameTime);

           
            drawCroshair();
            drawStrokeText();
        }

        void drawCroshair(){
            if(!Camera.isOrbiting)
            {
            spriteBatch.Begin();
            spriteBatch.Draw(crosshairTexture, 
                new XNAVector2(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2), 
                null, 
                new Color(128, 128, 128, 128), 
                0f, 
                new XNAVector2(0.5f, 0.5f), 
                4f, 
                SpriteEffects.None, 
                0f);
            spriteBatch.End();
            }
        }

        void drawStrokeText()
        {
            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            GraphicsDevice.DepthStencilState = DepthStencilState.None;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                            SamplerState.PointClamp, null, null, null, null);
            string strokeText = $"Strokes: {numStrokes}";
            XNAVector2 textSize = font.MeasureString(strokeText);
            XNAVector2 position = new XNAVector2(GraphicsDevice.Viewport.Width / 2 - textSize.X / 2, 10);
            spriteBatch.DrawString(font, strokeText, position, Color.Black);
            spriteBatch.End();
        }
    }

}
