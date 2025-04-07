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
        private Model holeModel;
        public StaticMesh TeeAreaMesh { get; private set; }
        public StaticMesh HoleMesh { get; private set; }

        public GolfCourse(Game game, Space space)
        {
            Game = game;
            this.space = space;
            courseModel = game.Map1;
            teeareaModel = game.Map1teearea;
            holeModel = game.Map1hole;
        }

        public void LoadCourse()
        {
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

            Vector3[] holeVertices;
            int[] holeIndices;
            ModelDataExtractor.GetVerticesAndIndicesFromModel(holeModel, out holeVertices, out holeIndices);
            HoleMesh = new StaticMesh(holeVertices, holeIndices, new AffineTransform(new Vector3(0, -40, 0)));

            Material holeMaterial = new Material
            {
                Bounciness = 0.3f,
                KineticFriction = 1f,
                StaticFriction = 1f
            };

            HoleMesh.Material = holeMaterial;
            space.Add(HoleMesh);
            Game.Components.Add(new StaticModel(holeModel, HoleMesh.WorldTransform.Matrix, Game));
        }
    }
}