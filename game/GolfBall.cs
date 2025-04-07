using BEPUutilities;
using Microsoft.Xna.Framework.Input;
using System;
using Microsoft.Xna.Framework.Graphics;
using BEPUphysics;
using BEPUphysics.Entities;
using BEPUphysics.Entities.Prefabs;
using BEPUphysics.Materials;
using BEPUphysics.CollisionRuleManagement;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;
using MathHelper = Microsoft.Xna.Framework.MathHelper;

namespace game
{
    public class GolfBall
    {
        public Matrix WorldMatrix { get; private set; }
        public Model Model { get; private set; }
        public Game Game { get; private set; }
        public Vector3 Position { get; set; }
        private Camera camera;
        private Space space;
        private Sphere ballEntity;
        private GolfCourse golfCourse;
        private TrajectoryLine trajectoryLine;
        private const float FORCE_MAGNITUDE = 40f;  // Matching the force from ApplyForce method
        private const float MIN_FORCE = 10f;
        private const float MAX_FORCE = 40f;
        private const float CHARGE_TIME = 3.0f; // Time in seconds for max charge
        private const float ANGLE_CHANGE_RATE = 45f; // Degrees per second
        private const float MIN_ANGLE = 0f;  // Parallel to ground
        private const float MAX_ANGLE = 85f; // Nearly vertical
        private const float POWER_OSCILLATION_SPEED = 2f; // Complete cycles per second
        private float currentChargeTime = 0f;
        private bool isCharging = false;
        private bool powerIncreasing = true;
        private float currentAngle = 45f; // Starting at 45 degrees

        public GolfBall(Game game, Camera camera, Space space)
        {
            Game = game;
            this.camera = camera;
            this.space = space;
            this.golfCourse = game.golfCourse;
            Model = game.GolfBallModel;
            trajectoryLine = new TrajectoryLine(game);
        }

        public bool SpawnBall()
        {
            // Create a ray from camera position in the direction of the crosshair
            Vector3 rayStart = camera.Position;
            Vector3 rayDirection = camera.WorldMatrix.Forward;
            
            // Find the first object the ray intersects with
            BEPUutilities.Ray ray = new BEPUutilities.Ray(rayStart, rayDirection);
            BEPUphysics.RayCastResult raycastResult;
            bool hit = space.RayCast(ray, (entry) => true, out raycastResult);

            if (hit && raycastResult.HitObject == golfCourse.TeeAreaMesh)
            {
                // Calculate the spawn position slightly above the hit point
                Vector3 spawnPosition = rayStart + rayDirection * raycastResult.HitData.T + new Vector3(0, 1, 0);
                
                ballEntity = new Sphere(spawnPosition, 1, 1);
                camera.SetTarget(ballEntity);

                Material mat = new Material
                {
                    Bounciness = 1.0f,
                    KineticFriction = 1f,
                    StaticFriction = 1f
                };
                ballEntity.Material = mat;

                // No initial velocity when spawning at crosshair
                ballEntity.LinearVelocity = new Vector3();
                space.Add(ballEntity);

                EntityModel model = new EntityModel(ballEntity, Model, Matrix.Identity, Game);
                Game.Components.Add(model);
                ballEntity.Tag = model;

                return true;
            }

            return false;
        }

        public void DeleteBall()
        {
            if (ballEntity != null)
            {
                space.Remove(ballEntity);
                Game.Components.Remove((EntityModel)ballEntity.Tag);
                ballEntity = null;
            }
        }

        public void Update(float dt)
        {
            if (ballEntity != null && camera.isOrbiting)
            {
                // Handle trajectory angle adjustment
                KeyboardState keyState = Keyboard.GetState();
                if (keyState.IsKeyDown(Keys.Up))
                {
                    currentAngle = Math.Min(currentAngle + ANGLE_CHANGE_RATE * dt, MAX_ANGLE);
                }
                if (keyState.IsKeyDown(Keys.Down))
                {
                    currentAngle = Math.Max(currentAngle - ANGLE_CHANGE_RATE * dt, MIN_ANGLE);
                }

                // Handle shot charging
                MouseState mouseState = Game.MouseState;
                
                if (mouseState.LeftButton == ButtonState.Pressed)
                {
                    if (!isCharging)
                    {
                        isCharging = true;
                        currentChargeTime = 0f;
                        powerIncreasing = true;
                    }
                    else
                    {
                        // Update charge time based on oscillation direction
                        if (powerIncreasing)
                        {
                            currentChargeTime += dt * POWER_OSCILLATION_SPEED;
                            if (currentChargeTime >= CHARGE_TIME)
                            {
                                currentChargeTime = CHARGE_TIME;
                                powerIncreasing = false;
                            }
                        }
                        else
                        {
                            currentChargeTime -= dt * POWER_OSCILLATION_SPEED;
                            if (currentChargeTime <= 0f)
                            {
                                currentChargeTime = 0f;
                                powerIncreasing = true;
                            }
                        }
                    }

                    // Calculate current force for trajectory preview
                    float chargePercent = currentChargeTime / CHARGE_TIME;
                    float currentForce = MathHelper.Lerp(MIN_FORCE, MAX_FORCE, chargePercent);

                    // Get forward direction from camera but only use its horizontal component
                    Microsoft.Xna.Framework.Vector3 cameraForward = new Microsoft.Xna.Framework.Vector3(
                        camera.WorldMatrix.Forward.X,
                        0, // Zero out the vertical component
                        camera.WorldMatrix.Forward.Z
                    );
                    cameraForward.Normalize();

                    // Calculate the vertical component based on the current angle
                    float verticalComponent = (float)Math.Sin(MathHelper.ToRadians(currentAngle));
                    float horizontalScale = (float)Math.Cos(MathHelper.ToRadians(currentAngle));

                    // Combine horizontal and vertical components
                    Microsoft.Xna.Framework.Vector3 finalDirection = new Microsoft.Xna.Framework.Vector3(
                        cameraForward.X * horizontalScale,
                        verticalComponent,
                        cameraForward.Z * horizontalScale
                    );
                    finalDirection.Normalize();
                    
                    // Convert to BEPU Vector3
                    Vector3 adjustedDirection = new Vector3(
                        finalDirection.X,
                        finalDirection.Y,
                        finalDirection.Z
                    );
                    
                    trajectoryLine.UpdateTrajectory(ballEntity.Position, adjustedDirection, currentForce);
                }
                else if (mouseState.LeftButton == ButtonState.Released && isCharging)
                {
                    // Apply the charged-up force when releasing the button
                    float chargePercent = currentChargeTime / CHARGE_TIME;
                    float finalForce = MathHelper.Lerp(MIN_FORCE, MAX_FORCE, chargePercent);
                    
                    // Get forward direction from camera but only use its horizontal component
                    Microsoft.Xna.Framework.Vector3 cameraForward = new Microsoft.Xna.Framework.Vector3(
                        camera.WorldMatrix.Forward.X,
                        0, // Zero out the vertical component
                        camera.WorldMatrix.Forward.Z
                    );
                    cameraForward.Normalize();

                    // Calculate the vertical component based on the current angle
                    float verticalComponent = (float)Math.Sin(MathHelper.ToRadians(currentAngle));
                    float horizontalScale = (float)Math.Cos(MathHelper.ToRadians(currentAngle));

                    // Combine horizontal and vertical components
                    Microsoft.Xna.Framework.Vector3 finalDirection = new Microsoft.Xna.Framework.Vector3(
                        cameraForward.X * horizontalScale,
                        verticalComponent,
                        cameraForward.Z * horizontalScale
                    );
                    finalDirection.Normalize();
                    
                    // Convert to BEPU Vector3
                    Vector3 adjustedDirection = new Vector3(
                        finalDirection.X,
                        finalDirection.Y,
                        finalDirection.Z
                    );
                    
                    ballEntity.LinearVelocity = adjustedDirection * finalForce;
                    
                    // Reset charging state
                    isCharging = false;
                    currentChargeTime = 0f;
                    Game.numStrokes++;
                }
            }
        }

        public void ApplyForce(Vector3 direction)
        {
            // This method is now handled by the Update method's charge mechanic
        }

        public void Draw()
        {
            if (ballEntity != null && camera.isOrbiting)
            {
                trajectoryLine.Draw();
            }
        }
    }
}