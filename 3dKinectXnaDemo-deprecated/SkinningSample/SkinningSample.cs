#region File Description
//-----------------------------------------------------------------------------
// SkinningSample.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using SkinnedModel;
using Microsoft.Kinect;

#endregion

namespace SkinningSample
{
    /// <summary>
    /// Sample game showing how to display skinned character animation.
    /// </summary>
    public class SkinningSampleGame : Microsoft.Xna.Framework.Game
    {
        #region Fields

        GraphicsDeviceManager graphics;

        KinectSensor kinect;

        KeyboardState currentKeyboardState = new KeyboardState();
        GamePadState currentGamePadState = new GamePadState();

        AnimationPlayer animationPlayer;
        SpriteFont spriteFont;

        float cameraArc = 0;
        float cameraRotation = 0;
        float cameraDistance = 100;
        String errorMessage = null;


        Model currentModel;
        #endregion

        #region Initialization


        public SkinningSampleGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected bool setupKinect()
        {
            // Check to see if a Kinect is available
            if (KinectSensor.KinectSensors.Count == 0)
            {
                errorMessage = "No Kinects detected";
                return false;
            }

            // Get the first Kinect on the computer
            kinect = KinectSensor.KinectSensors[0];

            // Start the Kinect running and select the depth camera
            try
            {
                kinect.SkeletonStream.Enable();
                kinect.Start();
            }
            catch
            {
                errorMessage = "Kinect initialise failed";
                return false;
            }

            // connect a handler to the event that fires when new frames are available

            kinect.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(myKinect_SkeletonFrameReady);

            return true;
        }

        void myKinect_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            using (SkeletonFrame frame = e.OpenSkeletonFrame())
            {
                if (frame != null)
                {
                    skeletons = new Skeleton[frame.SkeletonArrayLength];
                    frame.CopySkeletonDataTo(skeletons);

                    foreach (Skeleton s in skeletons)
                    {
                        if (s.TrackingState == SkeletonTrackingState.Tracked){
                            Console.Out.WriteLine("skeletor x" + s.Position.X + " y" + s.Position.Y + " z" + s.Position.Z);
                            animationPlayer.Update(s);
                        }
                    }
                   
                }
            }
        }

        /// <summary>
        /// Load your graphics content.
        /// </summary>
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            spriteFont = Content.Load<SpriteFont>("MessageFont");
             // Load the model.
            currentModel = Content.Load<Model>("dude");

            SkinningData skinningData = currentModel.Tag as SkinningData;

            if (skinningData == null)
                throw new InvalidOperationException
                    ("This model does not contain a SkinningData tag.");
            
            setupKinect();
            // Look up our custom skinning information.

            // Create an animation player, and start decoding an animation clip.
           

            //v1
            //Dictionary<JointType, int> jointMap = LoadJointMapDude();
            //animationPlayer = new AnimationPlayer(currentModel, graphics.GraphicsDevice.RasterizerState, jointMap);

            //v2
            animationPlayer = new AnimationPlayer(skinningData, graphics.GraphicsDevice.RasterizerState);


            AnimationClip clip = skinningData.AnimationClips["Take 001"];
            animationPlayer.StartClip(clip);

        }


        #endregion

        #region Update and Draw


        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            // TODO: Add your update logic here
            HandleInput();
            UpdateCamera(gameTime);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides v1 snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
           
            GraphicsDevice.Clear(Color.CornflowerBlue);
           
            spriteBatch.Begin();

            Matrix world =
                Matrix.CreateWorld(new Vector3(0, 0, 0),
                                              Vector3.Forward,
                                              Vector3.Up); //CreateRotationY(MathHelper.ToRadians(270));
           
            spriteBatch.DrawString(spriteFont, errorMessage + ";   x= " + x + " y=" + y + " z=" + z,
                           Vector2.Zero, Color.White);

            Matrix view; 
                //= Matrix.CreateLookAt(new Vector3(-110, -90, 10),
                //                              new Vector3(-50, 50, 10),
                //                              Vector3.Up);
            view = Matrix.CreateLookAt(new Vector3(5+x, 5+y, 5+z), new Vector3(0, 0, 0), Vector3.Up);

            Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver2,
                                                                    graphics.GraphicsDevice.Viewport.AspectRatio,
                                                                    1.0f,
                                                                    10000.0f);
            if (skeletons != null)
                foreach (Skeleton s in skeletons)
                    if (s.TrackingState == SkeletonTrackingState.Tracked)
                        animationPlayer.Draw(world, view, projection);

            spriteBatch.End();

        }

        private Dictionary<JointType, int> LoadJointMapDude()
        {
            Dictionary<JointType, int> jointMap = new Dictionary<JointType, int>();
 
            // Root 0
            jointMap.Add(JointType.HipCenter, 1); // Pelvis 1
            // Spine 2
            jointMap.Add(JointType.Spine, 3); // Spine1 3
            // Spine2 4
            // Spine3 5
            jointMap.Add(JointType.ShoulderCenter, 6); // Neck
            jointMap.Add(JointType.Head, 7); // Head
 
            // L_eye_joint1 8
            // R_eye_joint 9
            // L_eyeBall_joint2 10
            // R_eyeBall_joint 11
 
            // L_Clavicle 12
            jointMap.Add(JointType.ShoulderLeft, 13); // L_UpperArm 13  
            jointMap.Add(JointType.ElbowLeft, 14); // L_Forearm 14
            jointMap.Add(JointType.WristLeft, 15); // L_Hand 15
 
            //...
            // Left Hand fingers
            //...
 
            // R_Clavicle 31
            jointMap.Add(JointType.ShoulderRight, 32); // R_UpperArm 32
            jointMap.Add(JointType.ElbowRight, 33); // R_Forearm 33
            jointMap.Add(JointType.WristRight, 34); // R_Hand 34
 
            //...
            // Right Hand Fingers
            //...

            jointMap.Add(JointType.HipLeft, 50); // L_Thigh1 50
            jointMap.Add(JointType.KneeLeft, 51); // L_Knee2 51
            jointMap.Add(JointType.AnkleLeft, 52); // L_Ankle1 52
            // L_Ball 53

            jointMap.Add(JointType.HipRight, 54); // R_Thigh 54
            jointMap.Add(JointType.KneeRight, 55); // R_Knee 55
            jointMap.Add(JointType.AnkleRight, 56); // R_Ankle 56
            // R_Ball 57
 
            return jointMap;
        }

  
        
        #endregion

        #region Handle Input


        /// <summary>
        /// Handles input for quitting the game.
        /// </summary>
        private void HandleInput()
        {
            currentKeyboardState = Keyboard.GetState();
            currentGamePadState = GamePad.GetState(PlayerIndex.One);

            // Check for exit.
            if (currentKeyboardState.IsKeyDown(Keys.Escape) ||
                currentGamePadState.Buttons.Back == ButtonState.Pressed)
            {
                Exit();
            }
        }

        float x = 1.0f;
        float y = 1.0f;
        float z = 1.0f;
        /// <summary>
        /// Handles camera input.
        /// </summary>
        private void UpdateCamera(GameTime gameTime)
        {
            
            float time = (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            // Check for input to rotate the camera up and down around the model.
            if (currentKeyboardState.IsKeyDown(Keys.Up) ||
                currentKeyboardState.IsKeyDown(Keys.W))
            {
                x += time * 0.1f;
            }
            
            if (currentKeyboardState.IsKeyDown(Keys.Down) ||
                currentKeyboardState.IsKeyDown(Keys.S))
            {
                x -= time * 0.1f;
            }

            x += currentGamePadState.ThumbSticks.Right.Y * time * 0.25f;

            if (currentKeyboardState.IsKeyDown(Keys.Z))
            {
                z += time * 0.1f;
            }

            if (
                currentKeyboardState.IsKeyDown(Keys.U))
            {
                z -= time * 0.1f;
            }

            z += currentGamePadState.ThumbSticks.Right.Y * time * 0.25f;

            // Limit the arc movement.
            if (cameraArc > 90.0f)
                cameraArc = 90.0f;
            else if (cameraArc < -90.0f)
                cameraArc = -90.0f;

            // Check for input to rotate the camera around the model.
            if (currentKeyboardState.IsKeyDown(Keys.Right) ||
                currentKeyboardState.IsKeyDown(Keys.D))
            {
                //Console.Out.WriteLine("*********" + y);
                y += time * 0.1f;
            }

            if (currentKeyboardState.IsKeyDown(Keys.Left) ||
                currentKeyboardState.IsKeyDown(Keys.A))
            {
                y -= time * 0.1f;
            }

            y += currentGamePadState.ThumbSticks.Right.X * time * 0.25f;

            //// Check for input to zoom camera in and out.
            //if (currentKeyboardState.IsKeyDown(Keys.Z))
            //    cameraDistance += time * 0.25f;

            //if (currentKeyboardState.IsKeyDown(Keys.X))
            //    cameraDistance -= time * 0.25f;

            //cameraDistance += currentGamePadState.Triggers.Left * time * 0.5f;
            //cameraDistance -= currentGamePadState.Triggers.Right * time * 0.5f;

            //// Limit the camera distance.
            //if (cameraDistance > 500.0f)
            //    cameraDistance = 500.0f;
            //else if (cameraDistance < 10.0f)
            //    cameraDistance = 10.0f;

            if (currentGamePadState.Buttons.RightStick == ButtonState.Pressed ||
                currentKeyboardState.IsKeyDown(Keys.R))
            {
                x = 1.0f;
                 y = 1.0f;
                 z = 1.0f;
            }
        }


        #endregion

        public SpriteBatch spriteBatch { get; set; }

        public Skeleton[] skeletons { get; set; }
    }


    #region Entry Point

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    static class Program
    {
        static void Main()
        {
            using (SkinningSampleGame game = new SkinningSampleGame())
            {
                game.Run();
            }
        }
    }

    #endregion
}
