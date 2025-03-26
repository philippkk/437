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
        
        // Orbit-related properties
        private Func<Vector3> getTargetPosition;
        private float orbitRadius;
        private bool isOrbiting;
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

        // Set the target for orbiting
        public void SetTarget(Sphere ball)
        {
            getTargetPosition = () => ball.Position;
            
            // Calculate initial orbit offset
            Vector3 initialTarget = ball.Position;
            orbitRadius = Vector3.Distance(Position, initialTarget) + 10;
            
            // Calculate initial offset based on current camera position relative to ball
            orbitOffset = Position - initialTarget;
        }

        public void Update(float dt)
        {
            // Toggle orbit mode with F key (single press, not hold)
            if (Game.KeyboardState.IsKeyDown(Keys.F) && !lastFrameFPressed && getTargetPosition != null)
            {
                isOrbiting = !isOrbiting;
            }
            lastFrameFPressed = Game.KeyboardState.IsKeyDown(Keys.F);

            if (isOrbiting && getTargetPosition != null)
            {
                // Get current target position
                Vector3 currentTarget = getTargetPosition();

                // Calculate mouse movement for orbiting
                float mouseXDelta = 200 - Game.MouseState.X;
                float mouseYDelta = 200 - Game.MouseState.Y;

                // Update yaw and pitch based on mouse movement
                Yaw += mouseXDelta * dt * .12f;
                Pitch += mouseYDelta * dt * .12f;

                // Reset mouse position to center
                Mouse.SetPosition(200, 200);
                
                // Manually rotate the offset around the target
                Vector3 rotatedOffset = RotateOffset(orbitOffset, Pitch, Yaw);
                
                // Normalize the rotated offset to maintain constant radius
                rotatedOffset = Vector3.Normalize(rotatedOffset) * orbitRadius;
                
                // Set position relative to current target
                Position = currentTarget + rotatedOffset;

                // Update WorldMatrix to maintain camera orientation
                WorldMatrix = Matrix.CreateFromAxisAngle(Vector3.Right, Pitch) * 
                              Matrix.CreateFromAxisAngle(Vector3.Up, Yaw);
                WorldMatrix *= Matrix.CreateTranslation(Position);
            }
            else
            {
                // Existing free roam camera movement code
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

        // Existing helper methods (MoveForward, MoveRight, MoveUp, RotateOffset) remain the same
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

        private Vector3 RotateOffset(Vector3 offset, float pitch, float yaw)
        {
            // Rotate around Y (Yaw)
            Vector3 rotatedOffset = new Vector3(
                offset.X * (float)Math.Cos(yaw) - offset.Z * (float)Math.Sin(yaw),
                offset.Y,
                offset.X * (float)Math.Sin(yaw) + offset.Z * (float)Math.Cos(yaw)
            );

            // Rotate around X (Pitch)
            rotatedOffset = new Vector3(
                rotatedOffset.X,
                rotatedOffset.Y * (float)Math.Cos(pitch) - rotatedOffset.Z * (float)Math.Sin(pitch),
                rotatedOffset.Y * (float)Math.Sin(pitch) + rotatedOffset.Z * (float)Math.Cos(pitch)
            );

            return rotatedOffset;
        }
    }
}