using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using BEPUutilities;
using Vector3 = BEPUutilities.Vector3;
using ConversionHelper;

namespace game
{
    public class TrajectoryLine
    {
        private BasicEffect effect;
        private VertexPositionColor[] vertices;
        private Game game;
        private const int maxPoints = 50; 
        public TrajectoryLine(Game game)
        {
            this.game = game;
            effect = new BasicEffect(game.GraphicsDevice);
            effect.VertexColorEnabled = true;
            vertices = new VertexPositionColor[maxPoints];
        }

        public void UpdateTrajectory(Vector3 startPos, Vector3 direction, float forceMagnitude)
        {
            Vector3 velocity = direction * forceMagnitude;
            Vector3 position = startPos;
            Vector3 gravity = new Vector3(0, -30.0f, 0);  
            float timeStep = 1.0f / 60.0f;  

            for (int i = 0; i < maxPoints; i++)
            {
                vertices[i] = new VertexPositionColor(
                    new Microsoft.Xna.Framework.Vector3(position.X, position.Y, position.Z),
                    Color.Lerp(Color.White, Color.Transparent, (float)i / maxPoints)
                );

                velocity += gravity * timeStep;
                position += velocity * timeStep;
            }
        }

        public void Draw()
        {
            effect.View = MathConverter.Convert(game.Camera.ViewMatrix);
            effect.Projection = MathConverter.Convert(game.Camera.ProjectionMatrix);
            effect.World = Microsoft.Xna.Framework.Matrix.Identity;

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                game.GraphicsDevice.DrawUserPrimitives(
                    PrimitiveType.LineStrip,
                    vertices,
                    0,
                    maxPoints - 1
                );
            }
        }
    }
}