using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;
using BEPUphysics.Entities.Prefabs;
using BEPUutilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using BEPUphysics.Entities;
using BEPUphysics;
using BEPUphysics.NarrowPhaseSystems.Pairs;
using Vector3 = BEPUutilities.Vector3;
using Matrix = BEPUutilities.Matrix;
using XNAVector2 = Microsoft.Xna.Framework.Vector2;
using System;
using System.Collections.Generic;
using BEPUphysics.Materials;
using System.Runtime.InteropServices;
using System.IO;
using System.Linq;

/*
    todo: model, use Z forward Y up

    whats left:
    X proper ball hit logic, done
    x cup logic
    x gui, done
    x main menu
    x add help on bottom right (tab and menu controlls)
    x other maps
    x keeping top three scores
    x keep for each map and have initials too
    x two player mode
        x logic for stopping a ball when too low speed to swap players
        steps:
            add main menu button,
            add second player stats and ball
            add logic to swap players between shots 
    x sounds
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
        public Model Map2;
        public Model Map2teearea;
        public Model Map2hole;
        public KeyboardState KeyboardState;
        public MouseState MouseState;
        private GolfBall golfBall;
        private GolfBall player1Ball;
        private GolfBall player2Ball;
        private GolfBall activeBall;
        public GolfCourse golfCourse { get; private set; }
        private SpriteBatch spriteBatch;
        private SpriteFont font;
        private SpriteFont smallFont;  
        private Texture2D crosshairTexture;
        private Texture2D powerBarTexture;
        public SoundEffect ShootSound { get; private set; }
        public SoundEffect HoleSound { get; private set; }

        public int numBalls = 0;
        public int numStrokes = 0;
        public int totalStrokes = 0;

        public bool isTwoPlayerMode { get; private set; } = false;
        public int currentPlayer { get; private set; } = 1; 
        public int player1Strokes = 0;
        public int player1TotalStrokes = 0;
        public int player2Strokes = 0;
        public int player2TotalStrokes = 0;
        public bool player1FinishedHole = false;
        public bool player2FinishedHole = false;
        private string player1Initials = "";
        private string player2Initials = "";
        public bool player1PlacedBall = false;
        public bool player2PlacedBall = false;
        private bool canSwitchPlayer = false;
        private KeyboardState lastKeyboardState;

        private MenuScreen menuScreen;
        public bool isInGame { get; private set; } = false;
        public bool isPromptingForInitials { get; private set; } = false;
        private string currentInitials = "";
        private const string SCORES_FILE = "highscores.txt";

        private bool isPlayer1InitialsCollected = false;
        private bool isPlayer2InitialsCollected = false;
        private int initialsCollectionPlayer = 1; 
        private void SaveHighScore()
        {
            try
            {
                string directory = Path.GetDirectoryName(SCORES_FILE);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                List<(string initials, int strokes)> scores = new List<(string, int)>();
                if (File.Exists(SCORES_FILE))
                {
                    foreach (string line in File.ReadAllLines(SCORES_FILE))
                    {
                        string[] parts = line.Split(':');
                        if (parts.Length == 2 && int.TryParse(parts[1], out int strokes))
                        {
                            scores.Add((parts[0], strokes));
                        }
                    }
                }

                if (isTwoPlayerMode && golfCourse.IsLastMap())
                {
                    scores.Add((player1Initials, player1TotalStrokes));
                    scores.Add((player2Initials, player2TotalStrokes));
                }
                else
                {
                    scores.Add((currentInitials, totalStrokes));
                }

                scores = scores.OrderBy(s => s.strokes).ToList();
                scores = scores.Take(3).ToList();

                File.WriteAllLines(SCORES_FILE, 
                    scores.Select(s => $"{s.initials}:{s.strokes}"));
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving high score: {e.Message}");
            }
        }

        public void HandleKeyPress(Keys key)
        {
            if (!isPromptingForInitials) return;
            
            if (key >= Keys.A && key <= Keys.Z && currentInitials.Length < 3)
            {
                currentInitials += key.ToString();
            }
            else if (key == Keys.Back && currentInitials.Length > 0)
            {
                currentInitials = currentInitials.Substring(0, currentInitials.Length - 1);
            }
            else if (key == Keys.Enter && currentInitials.Length > 0)
            {
                if (isTwoPlayerMode && golfCourse.IsLastMap())
                {
                    if (initialsCollectionPlayer == 1)
                    {
                        player1Initials = currentInitials;
                        isPlayer1InitialsCollected = true;
                        isPromptingForInitials = false;
                        currentInitials = "";
                        
                        if (player2FinishedHole && isPlayer2InitialsCollected)
                        {
                            SaveHighScore();
                            isInGame = false;
                        }
                        else if (player2FinishedHole)
                        {
                            initialsCollectionPlayer = 2;
                            PromptForInitials();
                        }
                        else
                        {
                            SwitchToPlayer(2);
                        }
                    }
                    else if (initialsCollectionPlayer == 2)
                    {
                        player2Initials = currentInitials;
                        isPlayer2InitialsCollected = true;
                        isPromptingForInitials = false;
                        currentInitials = "";
                        
                        if (player1FinishedHole && isPlayer1InitialsCollected)
                        {
                            SaveHighScore();
                            isInGame = false;
                        }
                        else if (player1FinishedHole)
                        {
                            initialsCollectionPlayer = 1;
                            PromptForInitials();
                        }
                        else
                        {
                            SwitchToPlayer(1);
                        }
                    }
                }
                else
                {
                    SaveHighScore();
                    isPromptingForInitials = false;
                    currentInitials = "";
                    isInGame = false;
                }
            }
        }

        public string GetCurrentInitials() => currentInitials;
        public void PromptForInitials() => isPromptingForInitials = true;

        public List<string> GetHighScores()
        {
            List<string> scores = new List<string>();
            try
            {
                if (File.Exists(SCORES_FILE))
                {
                    scores = File.ReadAllLines(SCORES_FILE).ToList();
                }
            }
            catch (Exception) { }
            return scores;
        }

        public Game()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = (int)1280;
            graphics.PreferredBackBufferHeight = (int)720;
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
            smallFont = Content.Load<SpriteFont>("ArialSmall");
            menuScreen = new MenuScreen(this, spriteBatch, font, smallFont);

            GolfBallModel = Content.Load<Model>("golfball3");
            Map1 = Content.Load<Model>("map1");
            Map1teearea = Content.Load<Model>("map1teearea");
            Map1hole = Content.Load<Model>("map1hole");
            Map2 = Content.Load<Model>("map2");
            Map2teearea = Content.Load<Model>("map2teearea");
            Map2hole = Content.Load<Model>("map2hole");
            ShootSound = Content.Load<SoundEffect>("click");
            HoleSound = Content.Load<SoundEffect>("powerUp");

            crosshairTexture = new Texture2D(GraphicsDevice, 1, 1);
            crosshairTexture.SetData(new[] { Color.Black });

            powerBarTexture = new Texture2D(GraphicsDevice, 1, 1);
            powerBarTexture.SetData(new[] { Color.White });
        }

        public void StartGame(bool twoPlayerMode = false)
        {
            if (!isInGame)
            {
                isTwoPlayerMode = twoPlayerMode;
                currentPlayer = 1;
                
                player1Strokes = 0;
                player1TotalStrokes = 0;
                player2Strokes = 0;
                player2TotalStrokes = 0;
                player1FinishedHole = false;
                player2FinishedHole = false;
                player1PlacedBall = false;
                player2PlacedBall = false;
                player1Initials = "";
                player2Initials = "";
                
                isPlayer1InitialsCollected = false;
                isPlayer2InitialsCollected = false;
                initialsCollectionPlayer = 1;
                
                space = new Space();
                space.ForceUpdater.Gravity = new Vector3(0, -30.00f, 0);

                TimeStepSettings timeStep = new TimeStepSettings();
                timeStep.TimeStepDuration = 1f / 60f;
                space.TimeStepSettings = timeStep;

                golfCourse = new GolfCourse(this, space);
                golfCourse.LoadCourse();

                player1Ball = new GolfBall(this, Camera, space);
                
                if (isTwoPlayerMode) {
                    player2Ball = new GolfBall(this, Camera, space);
                }
                
                activeBall = player1Ball;
                golfBall = activeBall; 

                isInGame = true;
                totalStrokes = 0; 
                numStrokes = 0;
                numBalls = 0;
            }
        }

        public void OnHoleComplete()
        {
            if (isTwoPlayerMode)
            {
                if (currentPlayer == 1)
                {
                    player1FinishedHole = true;
                    player1TotalStrokes += player1Strokes;
                    
                    if (golfCourse.IsLastMap())
                    {
                        initialsCollectionPlayer = 1;
                        isPlayer1InitialsCollected = false; 
                        PromptForInitials();
                    }
                    else if (!player2FinishedHole)
                    {
                        SwitchToPlayer(2);
                    }
                    else
                    {
                        player2TotalStrokes += player2Strokes;
                        player1Strokes = 0;
                        player2Strokes = 0;
                        totalStrokes = player1TotalStrokes + player2TotalStrokes;
                    }
                }
                else 
                {
                    player2FinishedHole = true;
                    player2TotalStrokes += player2Strokes;
                    
                    if (golfCourse.IsLastMap()) 
                    {
                        initialsCollectionPlayer = 2;
                        isPlayer2InitialsCollected = false;
                        PromptForInitials();
                    }
                    else if (!player1FinishedHole)
                    {
                        SwitchToPlayer(1);
                    }
                    else
                    {
                        player1TotalStrokes += player1Strokes;
                        player1Strokes = 0;
                        player2Strokes = 0;
                        totalStrokes = player1TotalStrokes + player2TotalStrokes;
                    }
                }
            }
            else
            {
                totalStrokes += numStrokes;
                numStrokes = 0;
            }
        }

        private void SwitchToPlayer(int playerNumber)
        {
            if (!isTwoPlayerMode || (playerNumber != 1 && playerNumber != 2))
                return;

            currentPlayer = playerNumber;

            if (currentPlayer == 1)
            {
                activeBall = player1Ball;
                golfBall = player1Ball; 
            }
            else
            {
                activeBall = player2Ball;
                golfBall = player2Ball;  
            }

            bool hasPlacedBall = (currentPlayer == 1) ? player1PlacedBall : player2PlacedBall;
            
            if (hasPlacedBall && activeBall.BallEntity != null)
            {
                Camera.isOrbiting = false;
                Camera.Position = new Vector3(
                    activeBall.BallEntity.Position.X - 2,
                    activeBall.BallEntity.Position.Y + 1.5f,
                    activeBall.BallEntity.Position.Z - 2
                );
                Camera.SetTarget(activeBall.BallEntity);
            }
            else
            {
                Camera.isOrbiting = false;
                Camera.Position = new Vector3(-17, 11, -18);
                
                numBalls = 0;
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
        private float winTextTimer = 0f;
        private const float WIN_TEXT_DURATION = 4.0f; 
        protected override void Update(GameTime gameTime)
        {
            KeyboardState currentKeyboardState = Keyboard.GetState();
            KeyboardState = currentKeyboardState;  
            MouseState = Mouse.GetState();

            if (currentKeyboardState.IsKeyDown(Keys.Escape))
            {
                Exit();
                return;
            }

            if (isPromptingForInitials)
            {
                menuScreen.Update();
                lastKeyboardState = currentKeyboardState;
                return;
            }

            if (!isInGame)
            {
                menuScreen.Update();
            }
            else
            {
                if (golfBall != null && golfBall.HasWon && golfCourse.IsLastMap())
                {
                    winTextTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    if (winTextTimer >= WIN_TEXT_DURATION)
                    {
                        if (isTwoPlayerMode)
                        {
                            if (currentPlayer == 1 && !isPlayer1InitialsCollected)
                            {
                                initialsCollectionPlayer = 1;
                                PromptForInitials();
                            }
                            else if (currentPlayer == 2 && !isPlayer2InitialsCollected)
                            {
                                initialsCollectionPlayer = 2;
                                PromptForInitials();
                            }
                        }
                        else
                        {
                            PromptForInitials();
                        }
                        winTextTimer = 0f;
                    }
                }

                if (currentKeyboardState.IsKeyDown(Keys.R))
                {
                    if (numBalls > 0)
                    {
                        Camera.isOrbiting = false;
                        Camera.Position = new Vector3(-17, 11, -18);
                        golfBall.DeleteBall();
                        numBalls = 0;
                    }
                }

                Camera.Update((float)gameTime.ElapsedGameTime.TotalSeconds);

                if (Camera.isOrbiting)
                {
                    golfBall.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
                    
                    if (isTwoPlayerMode && !golfBall.IsCharging && !golfBall.HasWon)
                    {
                        bool playerHasTakenShot = (currentPlayer == 1 && player1PlacedBall && player1Strokes > 0) ||
                                               (currentPlayer == 2 && player2PlacedBall && player2Strokes > 0);
                        
                        canSwitchPlayer = playerHasTakenShot;
                        
                        if (currentKeyboardState.IsKeyDown(Keys.Enter) && lastKeyboardState.IsKeyUp(Keys.Enter) && canSwitchPlayer)
                        {
                            if (currentPlayer == 1 && !player1FinishedHole)
                            {
                                SwitchToPlayer(2);
                            }
                            else if (currentPlayer == 2 && !player2FinishedHole)
                            {
                                SwitchToPlayer(1);
                            }
                        }
                    }
                }
                else if (numBalls < 1 && MouseState.LeftButton == ButtonState.Pressed && (!clicking || KeyboardState.IsKeyDown(Keys.LeftShift)))
                {
                    clicking = true;
                    if (golfBall.SpawnBall()){
                        numBalls++;
                        
                        if (isTwoPlayerMode) {
                            if (currentPlayer == 1) {
                                player1PlacedBall = true;
                            } else {
                                player2PlacedBall = true;
                            }
                        }
                    }
                }

                if (MouseState.LeftButton == ButtonState.Released)
                {
                    clicking = false;
                }

                space.Update();
                
                if (golfCourse != null)
                {
                    golfCourse.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
                }
            }

            lastKeyboardState = currentKeyboardState;

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

        void drawCroshair()
        {
            if (!Camera.isOrbiting)
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

            if (isTwoPlayerMode)
            {
                string player1Text = $"Player 1: {player1Strokes}";
                string player2Text = $"Player 2: {player2Strokes}";
                string currentPlayerText = $"Player {currentPlayer}'s Turn";
                
                XNAVector2 player1Size = font.MeasureString(player1Text);
                XNAVector2 player1Pos = new XNAVector2(20, 10);
                Color player1Color = currentPlayer == 1 ? Color.Green : Color.Black;
                DrawTextWithOutline(player1Text, player1Pos, player1Color);
                
                XNAVector2 player2Size = font.MeasureString(player2Text);
                XNAVector2 player2Pos = new XNAVector2(GraphicsDevice.Viewport.Width - player2Size.X - 20, 10);
                Color player2Color = currentPlayer == 2 ? Color.Green : Color.Black;
                DrawTextWithOutline(player2Text, player2Pos, player2Color);
                
                XNAVector2 turnSize = font.MeasureString(currentPlayerText);
                XNAVector2 turnPos = new XNAVector2(GraphicsDevice.Viewport.Width / 2 - turnSize.X / 2, 10);
                DrawTextWithOutline(currentPlayerText, turnPos, Color.Black);
                
                bool needsToPlaceBall = (currentPlayer == 1 && !player1PlacedBall) || 
                                      (currentPlayer == 2 && !player2PlacedBall);
                
                if (needsToPlaceBall && !Camera.isOrbiting) {
                    string placementText = "Click on tee area to place ball";
                    XNAVector2 placementSize = font.MeasureString(placementText);
                    XNAVector2 placementPos = new XNAVector2(
                        GraphicsDevice.Viewport.Width / 2 - placementSize.X / 2,
                        GraphicsDevice.Viewport.Height - 50);
                    DrawTextWithOutline(placementText, placementPos, Color.Black);
                }
                else if (canSwitchPlayer && Camera.isOrbiting) {
                    string switchText = "Press ENTER to end your turn";
                    XNAVector2 switchSize = font.MeasureString(switchText);
                    XNAVector2 switchPos = new XNAVector2(
                        GraphicsDevice.Viewport.Width / 2 - switchSize.X / 2,
                        GraphicsDevice.Viewport.Height - 50);
                    DrawTextWithOutline(switchText, switchPos, Color.Black);
                }
            }
            else
            {
                string strokeText = $"Strokes: {numStrokes}";
                XNAVector2 textSize = font.MeasureString(strokeText);
                XNAVector2 position = new XNAVector2(GraphicsDevice.Viewport.Width / 2 - textSize.X / 2, 10);
                DrawTextWithOutline(strokeText, position, Color.Black);
            }
            
            spriteBatch.End();
        }
        void drawControls()
        {
            spriteBatch.Begin();

            string[] controls = {"press tab for controls"};
            if (KeyboardState.IsKeyDown(Keys.Tab))
            {
                controls = new string[]
                {
                    "Controls:",
                    "F - Toggle Orbit Mode",
                    "Left Click - Place/Shoot Ball",
                    "Q/E - Adjust Shot Angle",
                    "Mouse - Aim Direction",
                    "R - Reset Ball",
                    "ESC - Exit",
                };
            }

            int padding = 10;
            int lineSpacing = 3;
            float maxWidth = 0;

            foreach (string line in controls)
            {
                XNAVector2 size = smallFont.MeasureString(line);
                maxWidth = Math.Max(maxWidth, size.X);
            }

            float currentY = GraphicsDevice.Viewport.Height - padding - (controls.Length * (smallFont.LineSpacing + lineSpacing));
            foreach (string line in controls)
            {
                XNAVector2 size = smallFont.MeasureString(line);
                float x = GraphicsDevice.Viewport.Width - padding - maxWidth;

                float outlineSize = 2.0f;
                Color outlineColor = Color.White;
                spriteBatch.DrawString(smallFont, line, new XNAVector2(x - outlineSize, currentY - outlineSize), outlineColor);
                spriteBatch.DrawString(smallFont, line, new XNAVector2(x + outlineSize, currentY - outlineSize), outlineColor);
                spriteBatch.DrawString(smallFont, line, new XNAVector2(x - outlineSize, currentY + outlineSize), outlineColor);
                spriteBatch.DrawString(smallFont, line, new XNAVector2(x + outlineSize, currentY + outlineSize), outlineColor);

                spriteBatch.DrawString(smallFont, line, new XNAVector2(x, currentY), Color.Black);

                currentY += smallFont.LineSpacing + lineSpacing;
            }

            spriteBatch.End();
        }
        void drawWinText()
        {
            if (golfBall != null && golfBall.HasWon)
            {
                GraphicsDevice.BlendState = BlendState.AlphaBlend;
                GraphicsDevice.DepthStencilState = DepthStencilState.None;

                spriteBatch.Begin();

                if (isPromptingForInitials)
                {
                    string playerIndicator = isTwoPlayerMode ? $"Player {initialsCollectionPlayer} - " : "";
                    string prompt = $"{playerIndicator}Enter your initials:";
                    string currentInput = currentInitials;
                    string instructions = "Press ENTER when done";
                    
                    XNAVector2 promptSize = font.MeasureString(prompt);
                    XNAVector2 inputSize = font.MeasureString(currentInput);
                    XNAVector2 instructionsSize = smallFont.MeasureString(instructions);
                    
                    float centerX = GraphicsDevice.Viewport.Width / 2;
                    float startY = GraphicsDevice.Viewport.Height / 2 - 50;

                    DrawTextWithOutline(prompt, 
                        new XNAVector2(centerX - promptSize.X / 2, startY), 
                        Color.Black);
                        
                    DrawTextWithOutline(currentInput + "_", 
                        new XNAVector2(centerX - inputSize.X / 2, startY + 50), 
                        Color.Black);
                        
                    spriteBatch.DrawString(smallFont, instructions,
                        new XNAVector2(centerX - instructionsSize.X / 2, startY + 100),
                        Color.Black);

                    if (isTwoPlayerMode && golfCourse.IsLastMap())
                    {
                        string scoreText = $"Final Score: {(initialsCollectionPlayer == 1 ? player1TotalStrokes : player2TotalStrokes)} strokes";
                        XNAVector2 scoreSize = smallFont.MeasureString(scoreText);
                        spriteBatch.DrawString(smallFont, scoreText,
                            new XNAVector2(centerX - scoreSize.X / 2, startY + 130),
                            Color.Black);
                    }
                }
                else
                {
                    string winText;
                    if (golfCourse.IsLastMap()) 
                    {
                        if (isTwoPlayerMode)
                        {
                            if (player1FinishedHole && player2FinishedHole)
                            {
                                winText = $"Congratulations! You've completed all maps!\nPlayer 1: {player1TotalStrokes} strokes\nPlayer 2: {player2TotalStrokes} strokes\nTotal: {totalStrokes} strokes";
                            }
                            else
                            {
                                string playerName = currentPlayer == 1 ? "Player 1" : "Player 2";
                                winText = $"{playerName} completed the course!\n{(currentPlayer == 1 ? "Player 2" : "Player 1")} still needs to finish.";
                            }
                        }
                        else
                        {
                            winText = $"Congratulations! You've completed all maps!\nTotal Strokes: {totalStrokes}";
                        }
                    }
                    else 
                    {
                        winText = "Hole Complete!";
                    }
                    
                    XNAVector2 textSize = font.MeasureString(winText);
                    XNAVector2 position = new XNAVector2(
                        GraphicsDevice.Viewport.Width / 2 - textSize.X / 2,
                        GraphicsDevice.Viewport.Height / 2 - textSize.Y / 2
                    );

                    DrawTextWithOutline(winText, position, Color.Green);
                }
                spriteBatch.End();
            }
        }

        public void ResetPlayersForNewHole()
        {
            player1FinishedHole = false;
            player2FinishedHole = false;
            player1PlacedBall = false;
            player2PlacedBall = false;
            
            if (player1Ball != null)
            {
                player1Ball.ResetWonState();
            }
            
            if (player2Ball != null)
            {
                player2Ball.ResetWonState();
            }
            
            if (golfBall != null)
            {
                golfBall.ResetWonState();
            }
            
            if (isTwoPlayerMode)
            {
                SwitchToPlayer(1);
            }
        }
    }
}
