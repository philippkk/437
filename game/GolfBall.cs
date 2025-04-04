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

        public GolfBall(Game game, Camera camera, Space space)
        {
            Game = game;
            this.camera = camera;
            this.space = space;
            this.golfCourse = game.golfCourse;
            Model = game.GolfBallModel;
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
        }
    }
}