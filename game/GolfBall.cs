using BEPUutilities;
using Microsoft.Xna.Framework.Input;
using System;
using Microsoft.Xna.Framework.Graphics;
using BEPUphysics;
using BEPUphysics.Entities;
using BEPUphysics.Entities.Prefabs;
using BEPUphysics.Materials;

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

        public GolfBall(Game game, Camera camera, Space space)
        {
            Game = game;
            this.camera = camera;
            this.space = space;
            Model = game.GolfBallModel;
        }

        public void SpawnBall()
        {
            ballEntity = new Sphere(camera.Position + new Vector3(0, -5, 0), 1, 1);
            camera.SetTarget(ballEntity);

            float max = 0.2f;
            float min = 0;
            Random random = new Random();
            Vector3 offset = new Vector3(
                (float)(random.NextDouble() * (max - min) + min),
                (float)(random.NextDouble() * (max - min) + min),
                (float)(random.NextDouble() * (max - min) + min)
            );

            Material mat = new Material
            {
                Bounciness = 1.0f,
                KineticFriction = 1f,
                StaticFriction = 1f
            };
            ballEntity.Material = mat;

            ballEntity.LinearVelocity = (camera.WorldMatrix.Forward + offset) * 60;
            space.Add(ballEntity);

            EntityModel model = new EntityModel(ballEntity, Model, Matrix.Identity, Game);
            Game.Components.Add(model);
            ballEntity.Tag = model;
        }

        public void Update(float dt)
        {
        }
    }
}