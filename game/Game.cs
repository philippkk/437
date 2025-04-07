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

    whats left:
    X proper ball hit logic, done
    - cup logic
    x gui, done
    - main menu
    - other maps
    - keeping top three scores
    - two player mode
    - main menu
    - sounds
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
        public Model Map1hole; 
        public KeyboardState KeyboardState;
        public MouseState MouseState;
        private GolfBall golfBall;
        public GolfCourse golfCourse { get; private set; }
        private SpriteBatch spriteBatch;
        private SpriteFont font;
        private Texture2D crosshairTexture;
        private Texture2D powerBarTexture; 

        public int numBalls = 0;
        public int numStrokes = 0;

        private MenuScreen menuScreen;
        public bool isInGame { get; private set; } = false;

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
            spriteBatch = new SpriteBatch(GraphicsDevice);
            font = Content.Load<SpriteFont>("Arial");
            menuScreen = new MenuScreen(this, spriteBatch, font);

            GolfBallModel = Content.Load<Model>("golfball3");
            Map1 = Content.Load<Model>("map1");
            Map1teearea = Content.Load<Model>("map1teearea");
            Map1hole = Content.Load<Model>("map1hole");  

            crosshairTexture = new Texture2D(GraphicsDevice, 1, 1);
            crosshairTexture.SetData(new[] { Color.Black });

            powerBarTexture = new Texture2D(GraphicsDevice, 1, 1);
            powerBarTexture.SetData(new[] { Color.White });
        }

        public void StartGame()
        {
            if (!isInGame)
            {
                space = new Space();
                space.ForceUpdater.Gravity = new Vector3(0, -30.00f, 0);

                TimeStepSettings timeStep = new TimeStepSettings();
                timeStep.TimeStepDuration = 1f / 60f;
                space.TimeStepSettings = timeStep;

                golfCourse = new GolfCourse(this, space);
                golfCourse.LoadCourse();

                golfBall = new GolfBall(this, Camera, space);
                
                isInGame = true;
            }
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
            // lol
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

            if (!isInGame)
            {
                menuScreen.Update();
            }
            else
            {
                if (KeyboardState.IsKeyDown(Keys.R))
                {
                    if (numBalls > 0)
                    {
                        Camera.isOrbiting = false;
                        golfBall.DeleteBall();
                        numBalls = 0;
                    }
                }

                Camera.Update((float)gameTime.ElapsedGameTime.TotalSeconds);

                if (Camera.isOrbiting)
                {
                    golfBall.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
                }
                else if (numBalls < 1 && MouseState.LeftButton == ButtonState.Pressed && (!clicking || KeyboardState.IsKeyDown(Keys.LeftShift)))
                {
                    clicking = true;
                    golfBall.SpawnBall();
                    numBalls++;
                }

                if (MouseState.LeftButton == ButtonState.Released)
                {
                    clicking = false;
                }

                space.Update();
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DarkGray);

            if (!isInGame)
            {
                menuScreen.Draw();
            }
            else
            {
                GraphicsDevice.BlendState = BlendState.Opaque;
                GraphicsDevice.DepthStencilState = DepthStencilState.Default;
                GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

                base.Draw(gameTime);
                
                golfBall.Draw();
               
                drawCroshair();
                drawPowerBar();
                drawStrokeText();
                drawControls();
                drawWinText(); 
            }
        }

        private void DrawTextWithOutline(string text, XNAVector2 position, Color textColor)
        {
            float outlineSize = 2.0f;
            
            spriteBatch.DrawString(font, text, position + new XNAVector2(-outlineSize, -outlineSize), Color.White);
            spriteBatch.DrawString(font, text, position + new XNAVector2(outlineSize, -outlineSize), Color.White);
            spriteBatch.DrawString(font, text, position + new XNAVector2(-outlineSize, outlineSize), Color.White);
            spriteBatch.DrawString(font, text, position + new XNAVector2(outlineSize, outlineSize), Color.White);
            
            spriteBatch.DrawString(font, text, position, textColor);
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

        void drawPowerBar()
        {
            if (golfBall != null && Camera.isOrbiting && golfBall.IsCharging)
            {
                GraphicsDevice.BlendState = BlendState.AlphaBlend;
                GraphicsDevice.DepthStencilState = DepthStencilState.None;
                
                spriteBatch.Begin();
                
                int barWidth = 200;
                int barHeight = 20;
                int barX = GraphicsDevice.Viewport.Width / 2 - barWidth / 2;
                int barY = GraphicsDevice.Viewport.Height - 50;

                string angleText = $"{golfBall.CurrentAngle:F0}°";
                XNAVector2 textSize = font.MeasureString(angleText);
                XNAVector2 anglePosition = new XNAVector2(
                    barX + (barWidth / 2) - (textSize.X / 2), 
                    barY - textSize.Y - 5  
                );
                DrawTextWithOutline(angleText, anglePosition, Color.Black);
                
                spriteBatch.Draw(powerBarTexture,
                    new Rectangle(barX, barY, barWidth, barHeight),
                    Color.DarkGray);

                float powerPercent = golfBall.CurrentPowerPercent;
                int fillWidth = (int)(barWidth * powerPercent);
                
                Color powerColor = Color.Lerp(Color.Green, Color.Red, powerPercent);
                spriteBatch.Draw(powerBarTexture,
                    new Rectangle(barX, barY, fillWidth, barHeight),
                    powerColor);
                
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
            DrawTextWithOutline(strokeText, position, Color.Black);
            spriteBatch.End();
        }

        void drawControls()
        {
            if (KeyboardState.IsKeyDown(Keys.Tab))
            {
                spriteBatch.Begin();
                string[] controls = {
                    "Controls:",
                    "F - Toggle Orbit Mode",
                    "Left Click - Place/Shoot Ball",
                    "Q/E - Adjust Shot Angle",
                    "Mouse - Aim Direction",
                    "R - Reset Ball",
                    "ESC - Exit",
                };

                int padding = 10;
                int lineSpacing = 5;
                float maxWidth = 0;
                
                foreach (string line in controls)
                {
                    XNAVector2 size = font.MeasureString(line);
                    maxWidth = Math.Max(maxWidth, size.X);
                }

                float currentY = GraphicsDevice.Viewport.Height - padding - (controls.Length * (font.LineSpacing + lineSpacing));
                foreach (string line in controls)
                {
                    XNAVector2 size = font.MeasureString(line);
                    float x = GraphicsDevice.Viewport.Width - padding - maxWidth;
                    DrawTextWithOutline(line, new XNAVector2(x, currentY), Color.Black);
                    currentY += font.LineSpacing + lineSpacing;
                }
                
                spriteBatch.End();
            }
        }

        void drawWinText()
        {
            if (golfBall != null && golfBall.HasWon)
            {
                GraphicsDevice.BlendState = BlendState.AlphaBlend;
                GraphicsDevice.DepthStencilState = DepthStencilState.None;

                spriteBatch.Begin();
                
                string winText = "U WIN";
                XNAVector2 textSize = font.MeasureString(winText);
                XNAVector2 position = new XNAVector2(
                    GraphicsDevice.Viewport.Width / 2 - textSize.X / 2,
                    GraphicsDevice.Viewport.Height / 2 - textSize.Y / 2
                );
                
                DrawTextWithOutline(winText, position, Color.Green);
                spriteBatch.End();
            }
        }
    }

}
