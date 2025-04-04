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
        private Model teeareaModel;
        public StaticMesh TeeAreaMesh { get; private set; }  // Add this line to expose the tee area mesh

        public GolfCourse(Game game, Space space)
        {
            Game = game;
            this.space = space;
            courseModel = game.Map1;
            teeareaModel = game.Map1teearea;
        }

        public void LoadCourse()
        {
            // Load the main course model
            Vector3[] courseVertices;
            int[] courseIndices;
            ModelDataExtractor.GetVerticesAndIndicesFromModel(courseModel, out courseVertices, out courseIndices);
            var courseMesh = new StaticMesh(courseVertices, courseIndices, new AffineTransform(new Vector3(0, -40, 0)));

            Material courseMaterial = new Material
            {
                Bounciness = 0.3f,
                KineticFriction = 1f,
                StaticFriction = 1f
            };

            courseMesh.Material = courseMaterial;
            space.Add(courseMesh);
            Game.Components.Add(new StaticModel(courseModel, courseMesh.WorldTransform.Matrix, Game));

            // Load the tee area model
            Vector3[] teeVertices;
            int[] teeIndices;
            ModelDataExtractor.GetVerticesAndIndicesFromModel(teeareaModel, out teeVertices, out teeIndices);
            TeeAreaMesh = new StaticMesh(teeVertices, teeIndices, new AffineTransform(new Vector3(0, -40, 0)));

            Material teeMaterial = new Material
            {
                Bounciness = 0.3f,
                KineticFriction = 1f,
                StaticFriction = 1f
            };

            TeeAreaMesh.Material = teeMaterial;
            space.Add(TeeAreaMesh);
            Game.Components.Add(new StaticModel(teeareaModel, TeeAreaMesh.WorldTransform.Matrix, Game));
        }
    }
}