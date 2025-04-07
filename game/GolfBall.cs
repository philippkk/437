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
        public Model Model { get; private set; }
        public Game Game { get; private set; }
        private Camera camera;
        private Space space;
        private Sphere ballEntity;
        private GolfCourse golfCourse;
        private TrajectoryLine trajectoryLine;
        private const float minForce = 10f;
        private const float maxForce = 40f;
        private const float chargeTime = 3.0f; 
        private const float angleChangeRate = 45f; 
        private const float minAngle = 0f;  
        private const float maxAngle = 85f; 
        private const float powerOscillationSpeed = 2f; 
        private float currentChargeTime = 0f;
        private bool isCharging = false;
        private bool powerIncreasing = true;
        private float currentAngle = 45f; 

        public bool IsCharging => isCharging;
        public float CurrentPowerPercent => currentChargeTime / chargeTime;
        public float CurrentAngle => currentAngle;

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
            Vector3 rayStart = camera.Position;
            Vector3 rayDirection = camera.WorldMatrix.Forward;
            
            BEPUutilities.Ray ray = new BEPUutilities.Ray(rayStart, rayDirection);
            BEPUphysics.RayCastResult raycastResult;
            bool hit = space.RayCast(ray, (entry) => true, out raycastResult);

            if (hit && raycastResult.HitObject == golfCourse.TeeAreaMesh)
            {
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
                KeyboardState keyState = Keyboard.GetState();
                if (keyState.IsKeyDown(Keys.E))
                {
                    currentAngle = Math.Min(currentAngle + angleChangeRate * dt, maxAngle);
                }
                if (keyState.IsKeyDown(Keys.Q))
                {
                    currentAngle = Math.Max(currentAngle - angleChangeRate * dt, minAngle);
                }

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
                        if (powerIncreasing)
                        {
                            currentChargeTime += dt * powerOscillationSpeed;
                            if (currentChargeTime >= chargeTime)
                            {
                                currentChargeTime = chargeTime;
                                powerIncreasing = false;
                            }
                        }
                        else
                        {
                            currentChargeTime -= dt * powerOscillationSpeed;
                            if (currentChargeTime <= 0f)
                            {
                                currentChargeTime = 0f;
                                powerIncreasing = true;
                            }
                        }
                    }

                    float chargePercent = currentChargeTime / chargeTime;
                    float currentForce = MathHelper.Lerp(minForce, maxForce, chargePercent);

                    Microsoft.Xna.Framework.Vector3 cameraForward = new Microsoft.Xna.Framework.Vector3(
                        camera.WorldMatrix.Forward.X,
                        0,
                        camera.WorldMatrix.Forward.Z
                    );
                    cameraForward.Normalize();

                    float verticalComponent = (float)Math.Sin(MathHelper.ToRadians(currentAngle));
                    float horizontalScale = (float)Math.Cos(MathHelper.ToRadians(currentAngle));

                    Microsoft.Xna.Framework.Vector3 finalDirection = new Microsoft.Xna.Framework.Vector3(
                        cameraForward.X * horizontalScale,
                        verticalComponent,
                        cameraForward.Z * horizontalScale
                    );
                    finalDirection.Normalize();
                    
                    Vector3 adjustedDirection = new Vector3(
                        finalDirection.X,
                        finalDirection.Y,
                        finalDirection.Z
                    );
                    
                    trajectoryLine.UpdateTrajectory(ballEntity.Position, adjustedDirection, currentForce);
                }
                else if (mouseState.LeftButton == ButtonState.Released && isCharging)
                {
                    float chargePercent = currentChargeTime / chargeTime;
                    float finalForce = MathHelper.Lerp(minForce, maxForce, chargePercent);
                    
                    Microsoft.Xna.Framework.Vector3 cameraForward = new Microsoft.Xna.Framework.Vector3(
                        camera.WorldMatrix.Forward.X,
                        0,
                        camera.WorldMatrix.Forward.Z
                    );
                    cameraForward.Normalize();

                    float verticalComponent = (float)Math.Sin(MathHelper.ToRadians(currentAngle));
                    float horizontalScale = (float)Math.Cos(MathHelper.ToRadians(currentAngle));

                    Microsoft.Xna.Framework.Vector3 finalDirection = new Microsoft.Xna.Framework.Vector3(
                        cameraForward.X * horizontalScale,
                        verticalComponent,
                        cameraForward.Z * horizontalScale
                    );
                    finalDirection.Normalize();
                    
                    Vector3 adjustedDirection = new Vector3(
                        finalDirection.X,
                        finalDirection.Y,
                        finalDirection.Z
                    );
                    
                    ballEntity.LinearVelocity = adjustedDirection * finalForce;
                    
                    isCharging = false;
                    currentChargeTime = 0f;
                    Game.numStrokes++;
                }
            }
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