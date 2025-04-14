using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using XNAVector2 = Microsoft.Xna.Framework.Vector2;

namespace game
{
    public class MenuScreen
    {
        private Game game;
        private SpriteBatch spriteBatch;
        private SpriteFont font;
        private SpriteFont smallFont;
        private string[] menuItems = { "Start Game", "Top Scores", "Exit" };
        private int selectedIndex = 0;
        private KeyboardState lastKeyboardState;
        private bool showingScores = false;

        public MenuScreen(Game game, SpriteBatch spriteBatch, SpriteFont font, SpriteFont smallFont)
        {
            this.game = game;
            this.spriteBatch = spriteBatch;
            this.font = font;
            this.smallFont = smallFont;
        }

        public void Update()
        {
            KeyboardState currentKeyboardState = Keyboard.GetState();

            if (showingScores)
            {
                if (currentKeyboardState.IsKeyDown(Keys.Enter) && lastKeyboardState.IsKeyUp(Keys.Enter) ||
                    currentKeyboardState.IsKeyDown(Keys.Escape) && lastKeyboardState.IsKeyUp(Keys.Escape))
                {
                    showingScores = false;
                }
                lastKeyboardState = currentKeyboardState;
                return;
            }

            if (game.isPromptingForInitials)
            {
                foreach (Keys key in currentKeyboardState.GetPressedKeys())
                {
                    if (lastKeyboardState.IsKeyUp(key))
                    {
                        game.HandleKeyPress(key);
                    }
                }
                lastKeyboardState = currentKeyboardState;
                return;
            }

            if (currentKeyboardState.IsKeyDown(Keys.Up) && lastKeyboardState.IsKeyUp(Keys.Up))
            { 
                selectedIndex--;
                if (selectedIndex < 0)
                    selectedIndex = menuItems.Length - 1;
            }
            else if (currentKeyboardState.IsKeyDown(Keys.Down) && lastKeyboardState.IsKeyUp(Keys.Down))
            {
                selectedIndex++;
                if (selectedIndex >= menuItems.Length)
                    selectedIndex = 0;
            }

            if (currentKeyboardState.IsKeyDown(Keys.Enter) && lastKeyboardState.IsKeyUp(Keys.Enter))
            {
                switch (selectedIndex)
                {
                    case 0: // Start Game
                        game.StartGame();
                        break;
                    case 1: // Top Scores
                        showingScores = true;
                        break;
                    case 2: // Exit
                        game.Exit();
                        break;
                }
            }

            lastKeyboardState = currentKeyboardState;
        }

        public void Draw()
        {
            game.GraphicsDevice.Clear(Color.CornflowerBlue);
            spriteBatch.Begin();

            if (game.isPromptingForInitials)
            {
                DrawInitialsPrompt();
            }
            else if (showingScores)
            {
                DrawHighScores();
            }
            else
            {
                string title = "Mini Golf :)";
                XNAVector2 titleSize = font.MeasureString(title);
                XNAVector2 titlePosition = new XNAVector2(
                    game.GraphicsDevice.Viewport.Width / 2 - titleSize.X / 2,
                    game.GraphicsDevice.Viewport.Height / 4
                );

                float outlineSize = 2.0f;
                Color outlineColor = Color.White;
                spriteBatch.DrawString(font, title, titlePosition + new XNAVector2(-outlineSize, -outlineSize), outlineColor);
                spriteBatch.DrawString(font, title, titlePosition + new XNAVector2(outlineSize, -outlineSize), outlineColor);
                spriteBatch.DrawString(font, title, titlePosition + new XNAVector2(-outlineSize, outlineSize), outlineColor);
                spriteBatch.DrawString(font, title, titlePosition + new XNAVector2(outlineSize, outlineSize), outlineColor);
                spriteBatch.DrawString(font, title, titlePosition, Color.Green);

                for (int i = 0; i < menuItems.Length; i++)
                {
                    XNAVector2 size = font.MeasureString(menuItems[i]);
                    XNAVector2 position = new XNAVector2(
                        game.GraphicsDevice.Viewport.Width / 2 - size.X / 2,
                        game.GraphicsDevice.Viewport.Height / 2 + i * 50
                    );

                    Color textColor = i == selectedIndex ? Color.CornflowerBlue : Color.Black;

                    spriteBatch.DrawString(font, menuItems[i], position + new XNAVector2(-outlineSize, -outlineSize), outlineColor);
                    spriteBatch.DrawString(font, menuItems[i], position + new XNAVector2(outlineSize, -outlineSize), outlineColor);
                    spriteBatch.DrawString(font, menuItems[i], position + new XNAVector2(-outlineSize, outlineSize), outlineColor);
                    spriteBatch.DrawString(font, menuItems[i], position + new XNAVector2(outlineSize, outlineSize), outlineColor);
                    spriteBatch.DrawString(font, menuItems[i], position, textColor);
                }

                DrawControls();
            }

            spriteBatch.End();
        }

        private void DrawInitialsPrompt()
        {
            string prompt = "Enter your initials:";
            string currentInitials = game.GetCurrentInitials();
            string instructions = "Press ENTER when done";

            XNAVector2 promptSize = font.MeasureString(prompt);
            XNAVector2 initialsSize = font.MeasureString(currentInitials);
            XNAVector2 instructionsSize = smallFont.MeasureString(instructions);

            float centerX = game.GraphicsDevice.Viewport.Width / 2;
            float centerY = game.GraphicsDevice.Viewport.Height / 2;

            spriteBatch.DrawString(font, prompt, 
                new XNAVector2(centerX - promptSize.X / 2, centerY - 50), Color.Black);
            spriteBatch.DrawString(font, currentInitials, 
                new XNAVector2(centerX - initialsSize.X / 2, centerY), Color.Black);
            spriteBatch.DrawString(smallFont, instructions, 
                new XNAVector2(centerX - instructionsSize.X / 2, centerY + 50), Color.Black);
        }

        private void DrawHighScores()
        {
            string title = "Top Scores";
            XNAVector2 titleSize = font.MeasureString(title);
            float centerX = game.GraphicsDevice.Viewport.Width / 2;
            
            spriteBatch.DrawString(font, title, 
                new XNAVector2(centerX - titleSize.X / 2, 50), Color.White);

            var scores = game.GetHighScores();
            float yPos = 150;

            if (scores.Count == 0)
            {
                string noScores = "No scores yet!";
                XNAVector2 textSize = font.MeasureString(noScores);
                spriteBatch.DrawString(font, noScores, 
                    new XNAVector2(centerX - textSize.X / 2, yPos), Color.Black);
            }
            else
            {
                foreach (string score in scores)
                {
                    XNAVector2 scoreSize = font.MeasureString(score);
                    spriteBatch.DrawString(font, score, 
                        new XNAVector2(centerX - scoreSize.X / 2, yPos), Color.Black);
                    yPos += 40;
                }
            }

            string backMsg = "Press ENTER to go back";
            XNAVector2 backSize = smallFont.MeasureString(backMsg);
            spriteBatch.DrawString(smallFont, backMsg, 
                new XNAVector2(centerX - backSize.X / 2, 
                game.GraphicsDevice.Viewport.Height - 50), Color.White);
        }

        private void DrawControls()
        {
            string[] controls = {
                "Controls:",
                "Up/Down - Move selection",
                "Enter - Select",
                "ESC - Exit",
            };

            int padding = 10;
            int lineSpacing = 3;
            float maxWidth = 0;

            foreach (string line in controls)
            {
                XNAVector2 size = smallFont.MeasureString(line);
                maxWidth = Math.Max(maxWidth, size.X);
            }

            float currentY = game.GraphicsDevice.Viewport.Height - padding - (controls.Length * (smallFont.LineSpacing + lineSpacing));
            foreach (string line in controls)
            {
                XNAVector2 size = smallFont.MeasureString(line);
                float x = game.GraphicsDevice.Viewport.Width - padding - maxWidth;

                float smallOutlineSize = 2.0f;
                Color smallOutlineColor = Color.White;
                spriteBatch.DrawString(smallFont, line, new XNAVector2(x - smallOutlineSize, currentY - smallOutlineSize), smallOutlineColor);
                spriteBatch.DrawString(smallFont, line, new XNAVector2(x + smallOutlineSize, currentY - smallOutlineSize), smallOutlineColor);
                spriteBatch.DrawString(smallFont, line, new XNAVector2(x - smallOutlineSize, currentY + smallOutlineSize), smallOutlineColor);
                spriteBatch.DrawString(smallFont, line, new XNAVector2(x + smallOutlineSize, currentY + smallOutlineSize), smallOutlineColor);
                spriteBatch.DrawString(smallFont, line, new XNAVector2(x, currentY), Color.Black);

                currentY += smallFont.LineSpacing + lineSpacing;
            }
        }
    }
}