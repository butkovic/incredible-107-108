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

        #endregion

        #region Initialization


        public SkinningSampleGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected bool setupKinect()
        {
           // Console.Out.WriteLine("setup kinjo");
            // Get the first Kinect on the computer
            kinect = KinectSensor.KinectSensors[0];

            // Start the Kinect running and select the depth camera
           
                kinect.SkeletonStream.Enable();
                kinect.Start();
           
           

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
                           // Console.Out.WriteLine("skeletor x" + s.Position.X + " y" + s.Position.Y + " z" + s.Position.Z);
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
             // Load the model.
            Model currentModel = Content.Load<Model>("player");
            SkinningData skinningData = currentModel.Tag as SkinningData;

            if (skinningData == null)
                throw new InvalidOperationException
                    ("This model does not contain a SkinningData tag.");

            spriteBatch = new SpriteBatch(GraphicsDevice);
            spriteFont = Content.Load<SpriteFont>("MessageFont");
            
            setupKinect();
            // Look up our custom skinning information.

            // Create an animation player, and start decoding an animation clip.
            Dictionary<JointType, int> jointMap = LoadJointMap();
            animationPlayer = new AnimationPlayer(currentModel, graphics.GraphicsDevice.RasterizerState, jointMap);
           
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
            Matrix matrix = Matrix.CreateRotationX(MathHelper.ToRadians(60)) * 
                Matrix.CreateRotationY(MathHelper.ToRadians(30)) * 
                Matrix.CreateScale(1,1,0);
            spriteBatch.Begin(SpriteSortMode.Deferred, null, null,null , null,null,matrix);

            Matrix world = Matrix.CreateWorld(new Vector3(0, 0, 0),
                                              new Vector3(15, -5, -1),
                                              Vector3.Up); //CreateRotationY(MathHelper.ToRadians(270));
           
            spriteBatch.DrawString(spriteFont,"x= " + x + " y=" + y + " z=" + z,
                           Vector2.Zero, Color.White);

            Matrix view = Matrix.CreateLookAt(new Vector3(-20, -50, -50),
                                              new Vector3(25+x, 25+y, -25+z),
                                              Vector3.Up);


            Texture2D texture2D = Content.Load<Texture2D>("skin1");
        
            var screenCenter = new Vector2(GraphicsDevice.Viewport.Bounds.Width / 2, GraphicsDevice.Viewport.Bounds.Height / 2);
            var textureCenter = new Vector2(texture2D.Width / 2, texture2D.Height / 2);
            
            spriteBatch.Draw(texture2D, screenCenter, null, Color.White, 0f, textureCenter, 1f, SpriteEffects.None, 1f);

            Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(30),
                                                                    graphics.GraphicsDevice.Viewport.AspectRatio,
                                                                    1.0f,
                                                                    10000);
            if (skeletons != null)
                foreach (Skeleton s in skeletons)
                    if (s.TrackingState == SkeletonTrackingState.Tracked)
                        animationPlayer.Draw(world, view, projection);
                         

            //if (errorMessage.Length > 0)
            //{
            //    spriteBatch.DrawString(messageFont, errorMessage, Vector2.Zero, Color.White);
            //}

            spriteBatch.End();

        }


        private Dictionary<JointType, int> LoadJointMapTube()
        {
            Dictionary<JointType, int> jointMap = new Dictionary<JointType, int>();

            jointMap.Add(JointType.Head, 0); // Head
            jointMap.Add(JointType.HipCenter, 1); // L_Ankle1 52
            jointMap.Add(JointType.AnkleRight, 1);
            jointMap.Add(JointType.AnkleLeft, 1);
            jointMap.Add(JointType.ElbowLeft, 1);
            jointMap.Add(JointType.ElbowRight, 1);
            jointMap.Add(JointType.FootLeft, 1);
            jointMap.Add(JointType.FootRight, 1);
            jointMap.Add(JointType.HandLeft, 1);
            jointMap.Add(JointType.HandRight, 1);
            jointMap.Add(JointType.HipLeft, 1);
            jointMap.Add(JointType.HipRight, 1);
            jointMap.Add(JointType.KneeLeft, 1);
            jointMap.Add(JointType.KneeRight, 1);
            jointMap.Add(JointType.ShoulderCenter, 1);
            jointMap.Add(JointType.ShoulderLeft, 1);
            jointMap.Add(JointType.ShoulderRight, 1);
            jointMap.Add(JointType.Spine, 1);
            jointMap.Add(JointType.WristLeft, 1);
            jointMap.Add(JointType.WristRight, 1);

            return jointMap;
        }

        private Dictionary<JointType, int> LoadJointMap()
        {
            Dictionary<JointType, int> jointMap = new Dictionary<JointType, int>();
            int i = 0;
            jointMap.Add(JointType.HipCenter, 2 + i); 
            jointMap.Add(JointType.Spine, 8 + i); 
            jointMap.Add(JointType.ShoulderCenter, 9 + i); 
            jointMap.Add(JointType.Head, 10 + i); 
            jointMap.Add(JointType.ShoulderLeft, 11 + i);  
            jointMap.Add(JointType.ElbowLeft, 12 + i); 
            jointMap.Add(JointType.WristLeft, 14 + i); 
            jointMap.Add(JointType.ShoulderRight, 22 + i); 
            jointMap.Add(JointType.ElbowRight, 23 + i); 
            jointMap.Add(JointType.WristRight, 25 + i); 
            jointMap.Add(JointType.HipLeft, 6 + i); //IK-tighs_L
            jointMap.Add(JointType.KneeLeft, 5 + i);
            jointMap.Add(JointType.AnkleLeft, 39 + i); 
            jointMap.Add(JointType.HipRight, 36 + i); // R_Thigh 54
            jointMap.Add(JointType.KneeRight, 35 + i); // R_Knee 55
            jointMap.Add(JointType.AnkleRight, 43 + i); // R_Ankle 56

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
