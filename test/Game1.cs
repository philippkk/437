using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace test;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    Vector2 ul;
    Vector2 ur;
    Vector2 ll;
    Vector2 lr;


    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        // TODO: Add your initialization logic here

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // TODO: use this.Content to load your game content here
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        // TODO: Add your update logic here

        base.Update(gameTime);
    }

    float num = 0;
    float amount = 0.01f;
    protected override void Draw(GameTime gameTime)
    {
        if (num <= 1 && num >= 0){
            num += amount;
        }else{
            if(num >= 1){
                num = 1;
            }
            if(num <= 0){
                num = 0;
            }
            amount *= -1;
        }

        GraphicsDevice.Clear(Color.Lerp(Color.DarkBlue, Color.BlueViolet, num));

        
        base.Draw(gameTime);
    }
}
