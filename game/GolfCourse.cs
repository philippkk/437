using BEPUutilities;
using Microsoft.Xna.Framework.Graphics;
using BEPUphysics;
using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.Materials;

namespace game
{
    public class GolfCourse
    {
        public Game Game { get; private set; }
        private Space space;
        private Model courseModel;

        public GolfCourse(Game game, Space space)
        {
            Game = game;
            this.space = space;
            courseModel = game.PlaygroundModel;
        }

        public void LoadCourse()
        {
            Vector3[] vertices;
            int[] indices;
            ModelDataExtractor.GetVerticesAndIndicesFromModel(courseModel, out vertices, out indices);
            var mesh = new StaticMesh(vertices, indices, new AffineTransform(new Vector3(0, -40, 0)));

            Material mat = new Material
            {
                Bounciness = 0.3f,
                KineticFriction = 1f,
                StaticFriction = 1f
            };

            mesh.Material = mat;

            space.Add(mesh);
            Game.Components.Add(new StaticModel(courseModel, mesh.WorldTransform.Matrix, Game));
        }
    }
}