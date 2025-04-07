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
        private const float MAX_FORCE = 60f;
        private const float CHARGE_TIME = 3.0f; // Time in seconds for max charge
        private float currentChargeTime = 0f;
        private bool isCharging = false;

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
                // Handle shot charging
                MouseState mouseState = Game.MouseState;
                
                if (mouseState.LeftButton == ButtonState.Pressed)
                {
                    if (!isCharging)
                    {
                        isCharging = true;
                        currentChargeTime = 0f;
                    }
                    else
                    {
                        currentChargeTime = Math.Min(currentChargeTime + dt, CHARGE_TIME);
                    }

                    // Calculate current force for trajectory preview
                    float chargePercent = currentChargeTime / CHARGE_TIME;
                    float currentForce = MathHelper.Lerp(MIN_FORCE, MAX_FORCE, chargePercent);

                    // Update trajectory with current force
                    Vector3 direction = camera.WorldMatrix.Forward;
                    Vector3 upwardBias = new Vector3(0, 0.3f, 0);
                    Vector3 adjustedDirection = direction + upwardBias;
                    adjustedDirection.Normalize();
                    
                    trajectoryLine.UpdateTrajectory(ballEntity.Position, adjustedDirection, currentForce);
                }
                else if (mouseState.LeftButton == ButtonState.Released && isCharging)
                {
                    // Apply the charged-up force when releasing the button
                    float chargePercent = currentChargeTime / CHARGE_TIME;
                    float finalForce = MathHelper.Lerp(MIN_FORCE, MAX_FORCE, chargePercent);
                    
                    Vector3 direction = camera.WorldMatrix.Forward;
                    Vector3 upwardBias = new Vector3(0, 0.3f, 0);
                    Vector3 adjustedDirection = direction + upwardBias;
                    adjustedDirection.Normalize();
                    
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