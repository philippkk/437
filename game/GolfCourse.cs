using BEPUutilities;
using Microsoft.Xna.Framework.Graphics;
using BEPUphysics;
using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.Materials;
using System.Linq;

namespace game
{
    public class GolfCourse
    {
        public Game Game { get; private set; }
        private Space space;
        private Model[] courseMaps;
        private Model[] teeAreaMaps;
        private Model[] holeMaps;
        private Vector3[] cameraPositions;
        public StaticMesh TeeAreaMesh { get; private set; }
        public StaticMesh HoleMesh { get; private set; }
        public StaticMesh CourseMesh { get; private set; }
        private int currentMap = 0;

        public GolfCourse(Game game, Space space)
        {
            Game = game;
            this.space = space;
            courseMaps = new Model[] { game.Map1, game.Map2 };
            teeAreaMaps = new Model[] { game.Map1teearea, game.Map2teearea };
            holeMaps = new Model[] { game.Map1hole, game.Map2hole };
            
            cameraPositions = new Vector3[]
            {
                new Vector3(-17, 11, -18),  
                new Vector3(-17, 11, -18)   
            };
        }

        public void LoadCourse()
        {
            LoadMap(currentMap);
        }

        public void LoadNextMap(Camera camera)
        {
            currentMap++;
            if (currentMap >= courseMaps.Length)
            {
                return;
            }

            UnloadCurrentMap();
            LoadMap(currentMap);
            camera.Position = cameraPositions[currentMap];
        }

        private void UnloadCurrentMap()
        {
            if (CourseMesh != null)
            {
                space.Remove(CourseMesh);
                var courseModel = Game.Components.First(c => c is StaticModel) as StaticModel;
                Game.Components.Remove(courseModel);
            }
            
            if (TeeAreaMesh != null)
            {
                space.Remove(TeeAreaMesh);
                var teeModel = Game.Components.First(c => c is StaticModel) as StaticModel;
                Game.Components.Remove(teeModel);
            }
            
            if (HoleMesh != null)
            {
                space.Remove(HoleMesh);
                var holeModel = Game.Components.First(c => c is StaticModel) as StaticModel;
                Game.Components.Remove(holeModel);
            }
        }

        public bool IsLastMap()
        {
            return currentMap == courseMaps.Length - 1;
        }

        private void LoadMap(int mapIndex)
        {
            Vector3[] courseVertices;
            int[] courseIndices;
            ModelDataExtractor.GetVerticesAndIndicesFromModel(courseMaps[mapIndex], out courseVertices, out courseIndices);
            CourseMesh = new StaticMesh(courseVertices, courseIndices, new AffineTransform(new Vector3(0, -40, 0)));

            Material courseMaterial = new Material
            {
                Bounciness = 0.3f,
                KineticFriction = 1f,
                StaticFriction = 1f
            };

            CourseMesh.Material = courseMaterial;
            space.Add(CourseMesh);
            Game.Components.Add(new StaticModel(courseMaps[mapIndex], CourseMesh.WorldTransform.Matrix, Game));

            Vector3[] teeVertices;
            int[] teeIndices;
            ModelDataExtractor.GetVerticesAndIndicesFromModel(teeAreaMaps[mapIndex], out teeVertices, out teeIndices);
            TeeAreaMesh = new StaticMesh(teeVertices, teeIndices, new AffineTransform(new Vector3(0, -40, 0)));

            Material teeMaterial = new Material
            {
                Bounciness = 0.3f,
                KineticFriction = 1f,
                StaticFriction = 1f
            };

            TeeAreaMesh.Material = teeMaterial;
            space.Add(TeeAreaMesh);
            Game.Components.Add(new StaticModel(teeAreaMaps[mapIndex], TeeAreaMesh.WorldTransform.Matrix, Game));

            Vector3[] holeVertices;
            int[] holeIndices;
            ModelDataExtractor.GetVerticesAndIndicesFromModel(holeMaps[mapIndex], out holeVertices, out holeIndices);
            HoleMesh = new StaticMesh(holeVertices, holeIndices, new AffineTransform(new Vector3(0, -40, 0)));

            Material holeMaterial = new Material
            {
                Bounciness = 0.3f,
                KineticFriction = 1f,
                StaticFriction = 1f
            };

            HoleMesh.Material = holeMaterial;
            space.Add(HoleMesh);
            Game.Components.Add(new StaticModel(holeMaps[mapIndex], HoleMesh.WorldTransform.Matrix, Game));
        }
    }
}