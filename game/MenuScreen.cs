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
        private string[] menuItems = { "Start Game", "Exit" };
        private int selectedIndex = 0;
        private KeyboardState lastKeyboardState;

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

            // Handle menu navigation
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

            // Handle menu selection
            if (currentKeyboardState.IsKeyDown(Keys.Enter) && lastKeyboardState.IsKeyUp(Keys.Enter))
            {
                switch (selectedIndex)
                {
                    case 0: // Start Game
                        game.StartGame();
                        break;
                    case 1: // Exit
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

            // Draw title
            string title = "Mini Golf";
            XNAVector2 titleSize = font.MeasureString(title);
            XNAVector2 titlePosition = new XNAVector2(
                game.GraphicsDevice.Viewport.Width / 2 - titleSize.X / 2,
                game.GraphicsDevice.Viewport.Height / 4
            );

            // Draw title with dark gray outline
            float outlineSize = 2.0f;
            Color outlineColor = Color.White;
            spriteBatch.DrawString(font, title, titlePosition + new XNAVector2(-outlineSize, -outlineSize), outlineColor);
            spriteBatch.DrawString(font, title, titlePosition + new XNAVector2(outlineSize, -outlineSize), outlineColor);
            spriteBatch.DrawString(font, title, titlePosition + new XNAVector2(-outlineSize, outlineSize), outlineColor);
            spriteBatch.DrawString(font, title, titlePosition + new XNAVector2(outlineSize, outlineSize), outlineColor);
            spriteBatch.DrawString(font, title, titlePosition, Color.Green);

            // Draw menu items
            for (int i = 0; i < menuItems.Length; i++)
            {
                XNAVector2 size = font.MeasureString(menuItems[i]);
                XNAVector2 position = new XNAVector2(
                    game.GraphicsDevice.Viewport.Width / 2 - size.X / 2,
                    game.GraphicsDevice.Viewport.Height / 2 + i * 50
                );

                Color textColor = i == selectedIndex ? Color.CornflowerBlue : Color.Black;

                // Draw outline
                spriteBatch.DrawString(font, menuItems[i], position + new XNAVector2(-outlineSize, -outlineSize), outlineColor);
                spriteBatch.DrawString(font, menuItems[i], position + new XNAVector2(outlineSize, -outlineSize), outlineColor);
                spriteBatch.DrawString(font, menuItems[i], position + new XNAVector2(-outlineSize, outlineSize), outlineColor);
                spriteBatch.DrawString(font, menuItems[i], position + new XNAVector2(outlineSize, outlineSize), outlineColor);

                // Draw main text
                spriteBatch.DrawString(font, menuItems[i], position, textColor);
            }

            // Draw controls in the same SpriteBatch.Begin/End block
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

            spriteBatch.End();
        }
    }
}