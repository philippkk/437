using BEPUutilities;
using Microsoft.Xna.Framework.Input;
using System;
using BEPUphysics.Entities;
using BEPUphysics.Entities.Prefabs;

namespace game
{
    public class Camera
    {
        public Vector3 Position { get; set; }
        float yaw;
        float pitch;

        private Func<Vector3> getTargetPosition;
        private float orbitRadius;
        public bool isOrbiting { get; set; }
        private bool lastFrameFPressed;
        private Vector3 orbitOffset;

        public float Yaw
        {
            get { return yaw; }
            set { yaw = MathHelper.WrapAngle(value); }
        }

        public float Pitch
        {
            get { return pitch; }
            set { pitch = MathHelper.Clamp(value, -MathHelper.PiOver2, MathHelper.PiOver2); }
        }

        public float Speed { get; set; }
        public Matrix ViewMatrix { get; private set; }
        public Matrix ProjectionMatrix { get; set; }
        public Matrix WorldMatrix { get; private set; }
        public Game Game { get; private set; }

        public Camera(Game game, Vector3 position, float speed)
        {
            Game = game;
            Position = position;
            Speed = speed;
            pitch = -0.86f;
            yaw = -2.30f;
            ProjectionMatrix = Matrix.CreatePerspectiveFieldOfViewRH(MathHelper.PiOver4, 4f / 3f, .1f, 10000.0f);
            Mouse.SetPosition(200, 200);
        }

        public void SetTarget(Sphere ball)
        {
            getTargetPosition = () => ball.Position;

            Vector3 initialTarget = ball.Position;
            orbitRadius = Vector3.Distance(Position, initialTarget) + 10;

            orbitOffset = Position - initialTarget;
        }

        public void Update(float dt)
        {
            // Don't update camera if game is in menu
            if (!Game.isInGame)
                return;

            if (Game.KeyboardState.IsKeyDown(Keys.F) && !lastFrameFPressed && getTargetPosition != null)
            {
                isOrbiting = !isOrbiting;

                if (isOrbiting)
                {
                    Vector3 targetPosition = getTargetPosition();
                    orbitOffset = Position - targetPosition;
                    orbitRadius = orbitOffset.Length();
                }
            }

            lastFrameFPressed = Game.KeyboardState.IsKeyDown(Keys.F);

            if (isOrbiting && getTargetPosition != null)
            {
                Vector3 targetPosition = getTargetPosition();

                Yaw += (200 - Game.MouseState.X) * dt * .12f;
                Pitch += (200 - Game.MouseState.Y) * dt * .12f;
                Mouse.SetPosition(200, 200);

                float x = orbitRadius * (float)Math.Sin(MathHelper.PiOver2 + Pitch) * (float)Math.Sin(Yaw);
                float y = orbitRadius * (float)Math.Cos(MathHelper.PiOver2 + Pitch);
                float z = orbitRadius * (float)Math.Sin(MathHelper.PiOver2 + Pitch) * (float)Math.Cos(Yaw);

                Position = targetPosition + new Vector3(x, y, z);

                WorldMatrix = Matrix.CreateLookAtRH(Position, targetPosition, Vector3.Up);
                WorldMatrix = Matrix.Invert(WorldMatrix);
            }
            else
            {
                Yaw += (200 - Game.MouseState.X) * dt * .12f;
                Pitch += (200 - Game.MouseState.Y) * dt * .12f;
                Mouse.SetPosition(200, 200);

                WorldMatrix = Matrix.CreateFromAxisAngle(Vector3.Right, Pitch) *
                              Matrix.CreateFromAxisAngle(Vector3.Up, Yaw);
                              

                float distance = Speed * dt;
                if (Game.KeyboardState.IsKeyDown(Keys.W))
                    MoveForward(distance);
                if (Game.KeyboardState.IsKeyDown(Keys.S))
                    MoveForward(-distance);
                if (Game.KeyboardState.IsKeyDown(Keys.A))
                    MoveRight(-distance);
                if (Game.KeyboardState.IsKeyDown(Keys.D))
                    MoveRight(distance);
                if (Game.KeyboardState.IsKeyDown(Keys.E))
                    MoveUp(distance);
                if (Game.KeyboardState.IsKeyDown(Keys.Q))
                    MoveUp(-distance);

                WorldMatrix *= Matrix.CreateTranslation(Position);
            }

            ViewMatrix = Matrix.Invert(WorldMatrix);
        }

        public void MoveForward(float dt)
        {
            Position += WorldMatrix.Forward * (dt * Speed);
        }

        public void MoveRight(float dt)
        {
            Position += WorldMatrix.Right * (dt * Speed);
        }

        public void MoveUp(float dt)
        {
            Position += new Vector3(0, (dt * Speed), 0);
        }
    }
}