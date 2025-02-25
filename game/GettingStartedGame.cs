using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;
using BEPUphysics.Entities.Prefabs;
using BEPUutilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using BEPUphysics.Entities;
using BEPUphysics;
using BEPUphysics.NarrowPhaseSystems.Pairs;
using Vector3 = BEPUutilities.Vector3;
using Matrix = BEPUutilities.Matrix;
using System;

/*
    todo: model, use Z forward Y up
*/

namespace game
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class GettingStartedGame : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        /// <summary>
        /// World in which the simulation runs.
        /// </summary>
        Space space;
        /// <summary>
        /// Controls the viewpoint and how the user can see the world.
        /// </summary>
        public Camera Camera;
        /// <summary>
        /// Graphical model to use for the boxes in the scene.
        /// </summary>
        public Model GolfBallModel;
        /// <summary>
        /// Graphical model to use for the environment.
        /// </summary>
        public Model PlaygroundModel;

        /// <summary>
        /// Contains the latest snapshot of the keyboard's input state.
        /// </summary>
        public KeyboardState KeyboardState;
        /// <summary>
        /// Contains the latest snapshot of the mouse's input state.
        /// </summary>
        public MouseState MouseState;

        public GettingStartedGame()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 1920;
            graphics.PreferredBackBufferHeight = 1080;
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            //Setup the camera.
            Camera = new Camera(this, new Vector3(0, 0, 10), 5);
            graphics.IsFullScreen = false;
            graphics.ApplyChanges();
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            //This 1x1x1 cube model will represent the box entities in the space.
            GolfBallModel = Content.Load<Model>("golfball");

            PlaygroundModel = Content.Load<Model>("map1");

            //Construct a new space for the physics simulation to occur within.
            space = new Space();

            //Set the gravity of the simulation by accessing the simulation settings of the space.
            //It defaults to (0,0,0); this changes it to an 'earth like' gravity.
            //Try looking around in the space's simulationSettings to familiarize yourself with the various options.
            space.ForceUpdater.Gravity = new Vector3(0, -9.81f, 0);

            //Create a physical environment from a triangle mesh.
            //First, collect the the mesh data from the model using a helper function.
            //This special kind of vertex inherits from the TriangleMeshVertex and optionally includes
            //friction/bounciness data.
            //The StaticTriangleGroup requires that this special vertex type is used in lieu of a normal TriangleMeshVertex array.
            Vector3[] vertices;
            int[] indices;
            ModelDataExtractor.GetVerticesAndIndicesFromModel(PlaygroundModel, out vertices, out indices);
            //Give the mesh information to a new StaticMesh.  
            //Give it a transformation which scoots it down below the kinematic box entity we created earlier.
            var mesh = new StaticMesh(vertices, indices, new AffineTransform(new Vector3(0, -40, 0)));

            //Add it to the space!
            space.Add(mesh);
            //Make it visible too.
            Components.Add(new StaticModel(PlaygroundModel, mesh.WorldTransform.Matrix, this));

            //Hook an event handler to an entity to handle some game logic.
            // //Refer to the Entity Events documentation for more information.
            // TODO: IMPORTANTVVVVV
            // Box deleterBox = new Box(new Vector3(5, 2, 0), 3, 3, 3);
            // space.Add(deleterBox);
            // deleterBox.CollisionInformation.Events.InitialCollisionDetected += HandleCollision;
        }

        /// <summary>
        /// Used to handle a collision event triggered by an entity specified above.
        /// </summary>
        /// <param name="sender">Entity that had an event hooked.</param>
        /// <param name="other">Entity causing the event to be triggered.</param>
        /// <param name="pair">Collision pair between the two objects in the event.</param>
        void HandleCollision(EntityCollidable sender, Collidable other, CollidablePairHandler pair)
        {
            //This type of event can occur when an entity hits any other object which can be collided with.
            //They aren't always entities; for example, hitting a StaticMesh would trigger this.
            //Entities use EntityCollidables as collision proxies; see if the thing we hit is one.
            var otherEntityInformation = other as EntityCollidable;
            if (otherEntityInformation != null)
            {
                //We hit an entity! remove it.
                space.Remove(otherEntityInformation.Entity);
                //Remove the graphics too.
                Components.Remove((EntityModel)otherEntityInformation.Entity.Tag);
            }
        }


        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            KeyboardState = Keyboard.GetState();
            MouseState = Mouse.GetState();
            // Allows the game to exit
            if (KeyboardState.IsKeyDown(Keys.Escape))
            {
                Exit();
                return;
            }
            //Update the camera.
            Camera.Update((float)gameTime.ElapsedGameTime.TotalSeconds);

            if (MouseState.LeftButton == ButtonState.Pressed)
            {
                //If the user is clicking, start firing some boxes.
                //First, create a new dynamic box at the camera's location.
                // Box toAdd = new Box(Camera.Position + new Vector3(0,-5,0), 1, 1, 1, 1);
                Sphere ball = new Sphere(Camera.Position + new Vector3(0, -5, 0), 1, 5);
                //Set the velocity of the new box to fly in the direction the camera is pointing.
                //Entities have a whole bunch of properties that can be read from and written to.
                //Try looking around in the entity's available properties to get an idea of what is available.

                float max = 0.2f;
                float min = 0;

                Random random = new Random();
                Vector3 offset = new Vector3(
                (float)(random.NextDouble() * (max - min) + min),
                (float)(random.NextDouble() * (max - min) + min),
                (float)(random.NextDouble() * (max - min) + min)
                );

                ball.LinearVelocity = (Camera.WorldMatrix.Forward + offset)* 100;
                //Add the new box to the simulation.
                space.Add(ball);

                //Add a graphical representation of the box to the drawable game components.
                EntityModel model = new EntityModel(ball, GolfBallModel, Matrix.Identity, this);
                Components.Add(model);
                ball.Tag = model;  //set the object tag of this entity to the model so that it's easy to delete the graphics component later if the entity is removed.
            }

            //Steps the simulation forward one time step.
            space.Update();

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
    }
}
