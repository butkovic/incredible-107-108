#region File Description
//-----------------------------------------------------------------------------
// AnimationPlayer.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Kinect;
using SkinnedModel;

#endregion

namespace SkinnedModel
{

    /// <summary>
    /// The animation player is in charge of decoding bone position
    /// matrices from an animation clip.
    /// </summary>
    public class AnimationPlayer
    {
        #region Fields

        const float scale = 1.0f;

        // Information about the currently playing animation clip.
        AnimationClip currentClipValue;
        TimeSpan currentTimeValue;
       // int currentKeyframe;

        RasterizerState originalRasterizerState;
        RasterizerState WIREFRAME_RASTERIZER_STATE = new RasterizerState() { CullMode = CullMode.None, FillMode = FillMode.WireFrame };

        // Current animation transform matrices.
        Matrix[] boneTransforms;
        Matrix[] worldTransforms;
        Matrix[] skinTransforms;

        Dictionary<JointType, int> jointMap;

        // Backlink to the bind pose and skeleton hierarchy data.
        SkinningData skinningDataValue;


        #endregion

        public AnimationPlayer() { }

        /// <summary>
        /// Constructs a new animation player.
        /// </summary>
        public AnimationPlayer(SkinningData skinningDataValue, RasterizerState rasterizerState)
        {
            this.skinningDataValue = skinningDataValue;
            this.originalRasterizerState = rasterizerState;
            //this.model = model;
            //if (model == null)
            //    throw new ArgumentNullException("model");

            //skinningDataValue = model.Tag as SkinningData;

            boneTransforms = new Matrix[skinningDataValue.BindPose.Count];
            worldTransforms = new Matrix[skinningDataValue.BindPose.Count];
            skinTransforms = new Matrix[skinningDataValue.BindPose.Count];

            this.jointMap = LoadJointMapDude();
        }

        public AnimationPlayer(Model model, RasterizerState rasterizerState, Dictionary<JointType, int> jointMap)
        {
            this.originalRasterizerState = rasterizerState;
            this.model = model;
            if (model == null)
                throw new ArgumentNullException("model");

            skinningDataValue = model.Tag as SkinningData;

            boneTransforms = new Matrix[skinningDataValue.BindPose.Count];
            worldTransforms = new Matrix[skinningDataValue.BindPose.Count];
            skinTransforms = new Matrix[skinningDataValue.BindPose.Count];

            this.jointMap = jointMap;
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


        /// <summary>
        /// Starts decoding the specified animation clip.
        /// </summary>

        public void StartClip(AnimationClip clip)
        {
            if (clip == null)
                throw new ArgumentNullException("clip");

            currentClipValue = clip;
            currentTimeValue = TimeSpan.Zero;
           // currentKeyframe = 0;

            // Initialize bone transforms to the bind pose.
            skinningDataValue.BindPose.CopyTo(boneTransforms, 0);
        }


        /// <summary>
        /// Advances the current animation position.
        /// </summary>
        public void Update(Skeleton skeleton)
        {
            this.skeleton = skeleton;
            Matrix rootTransform = Matrix.Identity;
            UpdateBoneTransforms();
            UpdateWorldTransforms(rootTransform);
            UpdateSkinTransforms();
            Console.Out.WriteLine("head " + Position(JointType.Head));
            
        }

        public void UpdateBoneTransforms()
        {
            skinningDataValue.BindPose.CopyTo(boneTransforms, 0);

            if (skeleton != null)
            {
                Vector3 head = Position(JointType.Head);
               
                Vector3 shoulderCenter = Position(JointType.ShoulderCenter);
                Vector3 spine = Position(JointType.Spine);
                Vector3 hipCenter = Position(JointType.HipCenter);

                Vector3 shoulderLeft = Position(JointType.ShoulderLeft);
                Vector3 elbowLeft = Position(JointType.ElbowLeft);
                Vector3 wristLeft = Position(JointType.WristLeft);
                Vector3 handLeft = Position(JointType.HandLeft);

                Vector3 shoulderRight = Position(JointType.ShoulderRight);
                Vector3 elbowRight = Position(JointType.ElbowRight);
                Vector3 wristRight = Position(JointType.WristRight);
                Vector3 handRight = Position(JointType.HandRight);

                Vector3 hipLeft = Position(JointType.HipLeft);
                Vector3 kneeLeft = Position(JointType.KneeLeft);
                Vector3 ankleLeft = Position(JointType.AnkleLeft);
                Vector3 footLeft = Position(JointType.FootLeft);

                Vector3 hipRight = Position(JointType.HipRight);
                Vector3 kneeRight = Position(JointType.KneeRight);
                Vector3 ankleRight = Position(JointType.AnkleRight);
                Vector3 footRight = Position(JointType.FootRight);


                //// Left arm
                //{
                //    //Vector3 p1;
                //    //Vector3 p2;
                //    //Vector3 p3;

                //    //p1 = shoulderCenter_Pos;
                //    //p2 = shoulderLeft_Pos;
                //    //p3 = elbowLeft_Pos;

                //    //Vector3 a = Vector3.Normalize(p1 - p2);
                //    //Vector3 b = Vector3.Normalize(p3 - p2);

                //    //double angle = Math.Acos(Convert.ToDouble(Vector3.Dot(a, b)));

                //    //Vector3 cross = Vector3.Cross(a, b);

                //    //bones[14] = LookAt(shoulderLeft_Pos, elbowLeft_Pos);
                //    //bones[15] = LookAt(elbowLeft_Pos, wristLeft_Pos);
                //}

                //boneTransforms[13] = Matrix.CreateTranslation(elbowLeft - shoulderLeft);
                //boneTransforms[13] *= LookAt(elbowLeft, shoulderLeft);
                //boneTransforms[13] *= LookAt(shoulderLeft, elbowLeft);
                //boneTransforms[13] = LookAt(shoulderLeft, elbowLeft) * boneTransforms[13];


                // Left Arm v1
                {
                    //float shoulderToElbowLeftAngle = AngleBetweenTwoVectors(Vector3.Down, new Vector3(elbowLeft.X - shoulderLeft.X, elbowLeft.Y - shoulderLeft.Y, 0));
                    //boneTransforms[jointMap[JointType.ShoulderLeft]] = Matrix.CreateRotationY(MathHelper.ToRadians(45) - shoulderToElbowLeftAngle) * boneTransforms[jointMap[JointType.ShoulderLeft]];
                    float shoulderToElbowLeftAngle = AngleBetweenTwoVectors(new Vector3(shoulderCenter.X - shoulderLeft.X, shoulderCenter.Y - shoulderLeft.Y, 0), new Vector3(shoulderLeft.X - elbowLeft.X, shoulderLeft.Y - elbowLeft.Y, 0));
                    //boneTransforms[jointMap[JointType.ShoulderLeft]] = Matrix.CreateRotationY(shoulderToElbowLeftAngle) * boneTransforms[jointMap[JointType.ShoulderLeft]];

                    float secondShoulderToElbowLeftAngle = AngleBetweenTwoVectors(new Vector3(0, shoulderCenter.Y - shoulderLeft.Y, shoulderCenter.Z - shoulderLeft.Z), new Vector3(0, shoulderLeft.Y - elbowLeft.Y, shoulderLeft.Z - elbowLeft.Z));
                    //boneTransforms[jointMap[JointType.ShoulderLeft]] = Matrix.CreateRotationZ(secondShoulderToElbowLeftAngle) * boneTransforms[jointMap[JointType.ShoulderLeft]];

                    //tempAngle = String.Format("Up elbowAngle Y = {0}", MathHelper.ToDegrees(shoulderToElbowLeftAngle));
                }

                // Left Elbow v1
                {
                    //float elbowToWristLeftAngle = AngleBetweenTwoVectors(Vector3.Down, new Vector3(wristLeft.X - elbowLeft.X, wristLeft.Y - elbowLeft.Y, 0));
                    //boneTransforms[jointMap[JointType.ElbowLeft]] = Matrix.CreateRotationY(MathHelper.ToRadians(45) - elbowToWristLeftAngle) * boneTransforms[jointMap[JointType.ElbowLeft]];

                    //float wristToHandLeftAngle = AngleBetweenTwoVectors(Vector3.Down, new Vector3(handLeft.X - wristLeft.X, handLeft.Y - wristLeft.Y, 0));
                    //boneTransforms[jointMap[JointType.WristLeft]] = Matrix.CreateRotationY(MathHelper.ToRadians(45) - wristToHandLeftAngle) * boneTransforms[jointMap[JointType.WristLeft]];
                }

                // Left Wrist v1
                {
                    //float shoulderToElbowRightAngle = AngleBetweenTwoVectors(Vector3.Down, new Vector3(elbowRight.X - shoulderRight.X, elbowRight.Y - shoulderRight.Y, 0));
                    //boneTransforms[jointMap[JointType.ShoulderRight]] = Matrix.CreateRotationY(MathHelper.ToRadians(-45) + shoulderToElbowRightAngle) * boneTransforms[jointMap[JointType.ShoulderRight]];

                    //float elbowToWristRightAngle = AngleBetweenTwoVectors(Vector3.Down, new Vector3(wristRight.X - elbowRight.X, wristRight.Y - elbowRight.Y, 0)); ;
                    //boneTransforms[jointMap[JointType.ElbowRight]] = Matrix.CreateRotationY(MathHelper.ToRadians(-45) + elbowToWristRightAngle) * boneTransforms[jointMap[JointType.ElbowRight]];

                    //float wristToHandRightAngle = AngleBetweenTwoVectors(Vector3.Down, new Vector3(handRight.X - wristRight.X, handRight.Y - wristRight.Y, 0));
                    //boneTransforms[jointMap[JointType.WristRight]] = Matrix.CreateRotationY(MathHelper.ToRadians(-45) + wristToHandRightAngle) * boneTransforms[jointMap[JointType.WristRight]];
                }

                // Left Arm v2
                {
                    Vector3 shoulderToElbowSideVector = new Vector3(elbowLeft.X - shoulderLeft.X, elbowLeft.Y - shoulderLeft.Y, 0);
                    float leftArmSideAngle = AngleBetweenTwoVectors(Vector3.Down + Vector3.Left, shoulderToElbowSideVector);
                    //float leftArmSideAngle = AngleBetweenTwoVectors(new Vector3(spine.X - shoulderCenter.X, spine.Y - shoulderCenter.Y, 0), new Vector3(elbowLeft.X - shoulderLeft.X, elbowLeft.Y - shoulderLeft.Y, 0));

                    if (AngleBetweenTwoVectors(Vector3.Left, shoulderToElbowSideVector) < AngleBetweenTwoVectors(Vector3.Down, shoulderToElbowSideVector))
                        leftArmSideAngle *= -1;

                    AddBoneMatrix(JointType.ShoulderLeft, Matrix.CreateRotationY(leftArmSideAngle));


                    float leftArmFrontAngle = AngleBetweenTwoVectors(Vector3.Down, new Vector3(0, elbowLeft.Y - shoulderLeft.Y, elbowLeft.Z - shoulderLeft.Z));
                    //float leftArmFrontAngle = AngleBetweenTwoVectors(new Vector3(0, shoulderCenter.Y - shoulderLeft.Y, shoulderCenter.Z - shoulderLeft.Z), new Vector3(0, elbowLeft.Y - shoulderLeft.Y, elbowLeft.Z - shoulderLeft.Z));

                    if (elbowLeft.Z < shoulderLeft.Z)
                        leftArmFrontAngle *= -1;

                    AddBoneMatrix(JointType.ShoulderLeft, Matrix.CreateRotationZ(leftArmFrontAngle));
                }

                // Left Elbow v2
                {
                    float leftElbowSideAngle = AngleBetweenTwoVectors(new Vector3(elbowLeft.X - shoulderLeft.X, elbowLeft.Y - shoulderLeft.Y, 0), new Vector3(wristLeft.X - elbowLeft.X, wristLeft.Y - elbowLeft.Y, 0));

                    //if (wristLeft.X > elbowLeft.X)
                    //    leftElbowSideAngle *= -1;

                    AddBoneMatrix(JointType.ElbowLeft, Matrix.CreateRotationY(leftElbowSideAngle));


                    float leftElbowFrontAngle = AngleBetweenTwoVectors(new Vector3(0, elbowLeft.Y - shoulderLeft.Y, elbowLeft.Z - shoulderLeft.Z), new Vector3(0, wristLeft.Y - elbowLeft.Y, wristLeft.Z - elbowLeft.Z));

                    //if (wristLeft.Z < elbowLeft.Z)
                    leftElbowFrontAngle *= -1;

                    AddBoneMatrix(JointType.ElbowLeft, Matrix.CreateRotationZ(leftElbowFrontAngle));
                }

                // Left Wrist v2
                {
                    float leftWristSideAngle = AngleBetweenTwoVectors(new Vector3(wristLeft.X - elbowLeft.X, wristLeft.Y - elbowLeft.Y, 0), new Vector3(handLeft.X - wristLeft.X, handLeft.Y - wristLeft.Y, 0));

                    //if (handLeft.X > wristLeft.X)
                    //    leftWristSideAngle *= -1;

                    AddBoneMatrix(JointType.WristLeft, Matrix.CreateRotationY(leftWristSideAngle));


                    float leftWristFrontAngle = AngleBetweenTwoVectors(new Vector3(0, wristLeft.Y - elbowLeft.Y, wristLeft.Z - elbowLeft.Z), new Vector3(0, handLeft.Y - wristLeft.Y, handLeft.Z - wristLeft.Z));

                    //if (handLeft.Z > wristLeft.Z)
                    leftWristFrontAngle *= -1;

                    AddBoneMatrix(JointType.WristLeft, Matrix.CreateRotationZ(leftWristFrontAngle));
                }

                // Right Arm v2
                {
                    Vector3 shoulderToElbowSideVector = new Vector3(elbowRight.X - shoulderRight.X, elbowRight.Y - shoulderRight.Y, 0);
                    float RightArmSideAngle = AngleBetweenTwoVectors(Vector3.Down + Vector3.Right, shoulderToElbowSideVector);
                    //float RightArmSideAngle = AngleBetweenTwoVectors(new Vector3(spine.X - shoulderCenter.X, spine.Y - shoulderCenter.Y, 0), new Vector3(elbowRight.X - shoulderRight.X, elbowRight.Y - shoulderRight.Y, 0));

                    if (AngleBetweenTwoVectors(Vector3.Right, shoulderToElbowSideVector) > AngleBetweenTwoVectors(Vector3.Down, shoulderToElbowSideVector))
                        RightArmSideAngle *= -1;

                    AddBoneMatrix(JointType.ShoulderRight, Matrix.CreateRotationY(RightArmSideAngle));


                    float RightArmFrontAngle = AngleBetweenTwoVectors(Vector3.Down, new Vector3(0, elbowRight.Y - shoulderRight.Y, elbowRight.Z - shoulderRight.Z));
                    //float RightArmFrontAngle = AngleBetweenTwoVectors(new Vector3(0, shoulderCenter.Y - shoulderRight.Y, shoulderCenter.Z - shoulderRight.Z), new Vector3(0, elbowRight.Y - shoulderRight.Y, elbowRight.Z - shoulderRight.Z));

                    if (elbowRight.Z < shoulderRight.Z)
                        RightArmFrontAngle *= -1;

                    AddBoneMatrix(JointType.ShoulderRight, Matrix.CreateRotationZ(RightArmFrontAngle));
                }

                // Right Elbow v2
                {
                    float RightElbowSideAngle = AngleBetweenTwoVectors(new Vector3(elbowRight.X - shoulderRight.X, elbowRight.Y - shoulderRight.Y, 0), new Vector3(wristRight.X - elbowRight.X, wristRight.Y - elbowRight.Y, 0));

                    //if (wristRight.X > elbowRight.X)
                    RightElbowSideAngle *= -1;

                    AddBoneMatrix(JointType.ElbowRight, Matrix.CreateRotationY(RightElbowSideAngle));


                    float RightElbowFrontAngle = AngleBetweenTwoVectors(new Vector3(0, elbowRight.Y - shoulderRight.Y, elbowRight.Z - shoulderRight.Z), new Vector3(0, wristRight.Y - elbowRight.Y, wristRight.Z - elbowRight.Z));

                    //if (wristRight.Z < elbowRight.Z)
                    RightElbowFrontAngle *= -1;

                    AddBoneMatrix(JointType.ElbowRight, Matrix.CreateRotationZ(RightElbowFrontAngle));
                }

                // Right Wrist v2
                {
                    float RightWristSideAngle = AngleBetweenTwoVectors(new Vector3(wristRight.X - elbowRight.X, wristRight.Y - elbowRight.Y, 0), new Vector3(handRight.X - wristRight.X, handRight.Y - wristRight.Y, 0));

                    //if (handRight.X > wristRight.X)
                    //    RightWristSideAngle *= -1;

                    AddBoneMatrix(JointType.WristRight, Matrix.CreateRotationY(RightWristSideAngle));


                    float RightWristFrontAngle = AngleBetweenTwoVectors(new Vector3(0, wristRight.Y - elbowRight.Y, wristRight.Z - elbowRight.Z), new Vector3(0, handRight.Y - wristRight.Y, handRight.Z - wristRight.Z));

                    //if (handRight.Z > wristRight.Z)
                    RightWristFrontAngle *= -1;

                    AddBoneMatrix(JointType.WristRight, Matrix.CreateRotationZ(RightWristFrontAngle));
                }

                // Left Hip
                {
                    float leftLegSideAngle = AngleBetweenTwoVectors(Vector3.Down, new Vector3(kneeLeft.X - hipLeft.X, kneeLeft.Y - hipLeft.Y, 0));
                    //float leftLegSideAngle = AngleBetweenTwoVectors(new Vector3(hipCenter.X - spine.X, hipLeft.Y - spine.Y, 0), new Vector3(kneeLeft.X - hipLeft.X, kneeLeft.Y - hipLeft.Y, 0));

                    if (kneeLeft.X > hipLeft.X)
                        leftLegSideAngle *= -1;

                    AddBoneMatrix(JointType.HipLeft, Matrix.CreateRotationY(leftLegSideAngle));


                    float leftLegFrontAngle = AngleBetweenTwoVectors(Vector3.Down, new Vector3(0, kneeLeft.Y - hipLeft.Y, kneeLeft.Z - hipLeft.Z));
                    //float leftLegFrontAngle = AngleBetweenTwoVectors(new Vector3(0, hipCenter.Y - spine.Y, hipLeft.Z - spine.Z), new Vector3(0, kneeLeft.Y - hipLeft.Y, kneeLeft.Z - hipLeft.Z));

                    if (kneeLeft.Z > hipLeft.Z)
                        leftLegFrontAngle *= -1;

                    AddBoneMatrix(JointType.HipLeft, Matrix.CreateRotationZ(leftLegFrontAngle));
                }

                // Left Knee
                {
                    float leftKneeSideAngle = AngleBetweenTwoVectors(new Vector3(kneeLeft.X - hipLeft.X, kneeLeft.Y - hipLeft.Y, 0), new Vector3(ankleLeft.X - kneeLeft.X, ankleLeft.Y - kneeLeft.Y, 0));

                    if (ankleLeft.X > kneeLeft.X)
                        leftKneeSideAngle *= -1;

                    AddBoneMatrix(JointType.KneeLeft, Matrix.CreateRotationY(leftKneeSideAngle));


                    float leftKneeFrontAngle = AngleBetweenTwoVectors(new Vector3(0, kneeLeft.Y - hipLeft.Y, kneeLeft.Z - hipLeft.Z), new Vector3(0, ankleLeft.Y - kneeLeft.Y, ankleLeft.Z - kneeLeft.Z));

                    if (ankleLeft.Z > kneeLeft.Z)
                        leftKneeFrontAngle *= -1;

                    AddBoneMatrix(JointType.KneeLeft, Matrix.CreateRotationZ(leftKneeFrontAngle));

                    //UpdateSkinnedModelJoint(JointType.KneeLeft, JointType.HipLeft, JointType.AnkleLeft, skeleton);
                }

                // Left Foot
                {
                    float leftFootSideAngle = AngleBetweenTwoVectors(new Vector3(ankleLeft.X - kneeLeft.X, ankleLeft.Y - kneeLeft.Y, 0), new Vector3(footLeft.X - ankleLeft.X, footLeft.Y - ankleLeft.Y, 0));

                    if (footLeft.X > ankleLeft.X)
                        leftFootSideAngle *= -1;

                    //AddBoneMatrix(JointType.AnkleLeft, Matrix.CreateRotationY(leftFootSideAngle));


                    float leftFootFrontAngle = AngleBetweenTwoVectors(new Vector3(0, ankleLeft.Y - kneeLeft.Y, ankleLeft.Z - kneeLeft.Z), new Vector3(0, footLeft.Y - ankleLeft.Y, footLeft.Z - ankleLeft.Z));

                    if (footLeft.Z > ankleLeft.Z)
                        leftFootFrontAngle *= -1;

                    //AddBoneMatrix(JointType.AnkleLeft, Matrix.CreateRotationZ(leftFootFrontAngle));
                }

                // Right Hip
                {
                    float RightLegSideAngle = AngleBetweenTwoVectors(Vector3.Down, new Vector3(kneeRight.X - hipRight.X, kneeRight.Y - hipRight.Y, 0));
                    //float RightLegSideAngle = AngleBetweenTwoVectors(new Vector3(hipCenter.X - spine.X, hipRight.Y - spine.Y, 0), new Vector3(kneeRight.X - hipRight.X, kneeRight.Y - hipRight.Y, 0));

                    if (kneeRight.X > hipRight.X)
                        RightLegSideAngle *= -1;

                    AddBoneMatrix(JointType.HipRight, Matrix.CreateRotationY(RightLegSideAngle));


                    float RightLegFrontAngle = AngleBetweenTwoVectors(Vector3.Down, new Vector3(0, kneeRight.Y - hipRight.Y, kneeRight.Z - hipRight.Z));
                    //float RightLegFrontAngle = AngleBetweenTwoVectors(new Vector3(0, hipCenter.Y - spine.Y, hipRight.Z - spine.Z), new Vector3(0, kneeRight.Y - hipRight.Y, kneeRight.Z - hipRight.Z));

                    if (kneeRight.Z > hipRight.Z)
                        RightLegFrontAngle *= -1;

                    AddBoneMatrix(JointType.HipRight, Matrix.CreateRotationZ(RightLegFrontAngle));
                }

                // Right Knee
                {
                    float RightKneeSideAngle = AngleBetweenTwoVectors(new Vector3(kneeRight.X - hipRight.X, kneeRight.Y - hipRight.Y, 0), new Vector3(ankleRight.X - kneeRight.X, ankleRight.Y - kneeRight.Y, 0));

                    if (ankleRight.X > kneeRight.X)
                        RightKneeSideAngle *= -1;

                    AddBoneMatrix(JointType.KneeRight, Matrix.CreateRotationY(RightKneeSideAngle));


                    float RightKneeFrontAngle = AngleBetweenTwoVectors(new Vector3(0, kneeRight.Y - hipRight.Y, kneeRight.Z - hipRight.Z), new Vector3(0, ankleRight.Y - kneeRight.Y, ankleRight.Z - kneeRight.Z));

                    if (ankleRight.Z > kneeRight.Z)
                        RightKneeFrontAngle *= -1;

                    AddBoneMatrix(JointType.KneeRight, Matrix.CreateRotationZ(RightKneeFrontAngle));
                }

                // Right Ankle
                {

                }

                // Spine
                {
                    float spineSideAngle = AngleBetweenTwoVectors(Vector3.Up, new Vector3(shoulderCenter.X - spine.X, shoulderCenter.Y - spine.Y, 0));

                    if (spine.X > shoulderCenter.X)
                        spineSideAngle *= -1;

                    AddBoneMatrix(JointType.Spine, Matrix.CreateRotationY(spineSideAngle));


                    float spineFrontAngle = AngleBetweenTwoVectors(Vector3.Up, new Vector3(0, shoulderCenter.Y - spine.Y, shoulderCenter.Z - spine.Z));

                    if (shoulderCenter.Z > spine.Z)
                        spineFrontAngle *= -1;

                    AddBoneMatrix(JointType.Spine, Matrix.CreateRotationZ(spineFrontAngle));
                }

                // Head
                {
                    float headSideAngle = AngleBetweenTwoVectors(Vector3.Up, new Vector3(head.X - shoulderCenter.X, head.Y - shoulderCenter.Y, 0));
                    //float headSideAngle = AngleBetweenTwoVectors(new Vector3(shoulderCenter.X - spine.X, shoulderCenter.Y - spine.Y, 0), new Vector3(head.X - shoulderCenter.X, head.Y - shoulderCenter.Y, 0));

                    if (shoulderCenter.X > head.X)
                        headSideAngle *= -1;

                    AddBoneMatrix(JointType.Head, Matrix.CreateRotationY(headSideAngle));


                    float headFrontAngle = AngleBetweenTwoVectors(Vector3.Up, new Vector3(0, head.Y - shoulderCenter.Y, head.Z - shoulderCenter.Z));
                    //float headFrontAngle = AngleBetweenTwoVectors(new Vector3(0, shoulderCenter.Y - spine.Y, shoulderCenter.Z - spine.Z), new Vector3(0, head.Y - shoulderCenter.Y, head.Z - shoulderCenter.Z));

                    if (head.Z > shoulderCenter.Z)
                        headFrontAngle *= -1;

                    AddBoneMatrix(JointType.Head, Matrix.CreateRotationZ(headFrontAngle));
                }
            }

            //boneTransforms[13] *= Matrix.CreateRotationX(MathHelper.ToRadians(ang));
            //boneTransforms[34] *= Matrix.CreateRotationX((float)Math.Cos(MathHelper.ToRadians(ang)));
            //boneTransforms[3] *= Matrix.CreateRotationZ((float)Math.Cos(MathHelper.ToRadians(ang)));

            //AddBoneMatrix(JointType.ShoulderLeft, Matrix.CreateRotationY((float)Math.Cos(MathHelper.ToRadians(ang))));
            //AddBoneMatrix(JointType.ElbowRight, Matrix.CreateRotationZ((float)Math.Cos(MathHelper.ToRadians(ang))));

            ang++;
        }

        float ang = 0;

        //public void UpdateSkinnedModelJoint(JointType joint, JointType parentJoint, JointType childJoint, SkeletonData skeleton)
        //{
        //    Vector3 jointPosition = Position(joint);
        //    Vector3 parentJointPosition = Position(parentJoint);
        //    Vector3 childJointPosition = Position(childJoint);

        //    float sideAngle = AngleBetweenTwoVectors(new Vector3(jointPosition.X - parentJointPosition.X, jointPosition.Y - parentJointPosition.Y, 0), new Vector3(childJointPosition.X - jointPosition.X, childJointPosition.Y - jointPosition.Y, 0));

        //    if (childJointPosition.X > jointPosition.X)
        //        sideAngle *= -1;

        //    AddBoneMatrix(joint, Matrix.CreateRotationY(sideAngle));


        //    float frontAngle = AngleBetweenTwoVectors(new Vector3(0, jointPosition.Y - parentJointPosition.Y, jointPosition.Z - parentJointPosition.Z), new Vector3(0, childJointPosition.Y - jointPosition.Y, childJointPosition.Z - jointPosition.Z));

        //    if (childJointPosition.Z > jointPosition.Z)
        //        frontAngle *= -1;

        //    AddBoneMatrix(joint, Matrix.CreateRotationZ(frontAngle));
        //}

        public Vector3 Position(JointType JointType)
        {
            return ConvertRealWorldPoint(skeleton.Joints[JointType].Position);
        }

        public void Draw(Matrix world, Matrix view, Matrix projection)
        {
            if (this.model != null)
            {
                if (skeleton != null)
                {
                    world *= Matrix.CreateTranslation(GetModelPosition());
                    //stylinPurpleHat.Draw(Matrix.CreateScale(10) * skinTransforms[jointMap[JointID.Head]], view, projection);
                }

                // Render the skinned mesh.
                foreach (ModelMesh mesh in model.Meshes)
                {
                    foreach (SkinnedEffect skinnedEffect in mesh.Effects)
                    {
                        skinnedEffect.SetBoneTransforms(skinTransforms);

                        skinnedEffect.World = world;
                        skinnedEffect.View = view;
                        skinnedEffect.Projection = projection;

                        skinnedEffect.EnableDefaultLighting();

                        skinnedEffect.SpecularColor = new Vector3(0.25f);
                        skinnedEffect.SpecularPower = 16;

                        //if (originalRasterizerState == null)
                        //    originalRasterizerState = device.RasterizerState;

                        //if (config.DrawWireframe)
                        //    skinnedEffect.GraphicsDevice.RasterizerState = WIREFRAME_RASTERIZER_STATE;
                        //else
                        skinnedEffect.GraphicsDevice.RasterizerState = originalRasterizerState;

                        skinnedEffect.CurrentTechnique.Passes[0].Apply();
                    }
                    //Console.Out.WriteLine("Drawing mesh");
                    mesh.Draw();
                }
            }
        }

        private Vector3 GetModelPosition()
        {
            Vector3 playerPosition = Position(JointType.HipCenter);
            Vector3 modelPosition = new Vector3(-1 * playerPosition.X * 40, playerPosition.Y, -1 * playerPosition.Z * 40);

            return modelPosition;
        }

        public void AddBoneMatrix(JointType JointType, Matrix boneMatrix)
        {
            boneTransforms[jointMap[JointType]] = boneMatrix * boneTransforms[jointMap[JointType]];
        }

        //public float AngleBetweenTwoJointsAndVector(SkeletonData skeleton, JointType mainJoint, JointType otherJoint, Vector3 referenceVector)
        //{
        //    Vector3 mainJointPosition = ConvertRealWorldPoint(skeleton.Joints[mainJoint].Position);
        //    Vector3 otherJointPosition = ConvertRealWorldPoint(skeleton.Joints[otherJoint].Position);

        //    float angle = AngleBetweenTwoVectors(Vector3.Down, new Vector3(otherJointPosition.X - mainJointPosition.X, otherJointPosition.Y - mainJointPosition.Y, 0));

        //    return angle;
        //}

        //public void UpdateLeftBoneTransform(SkeletonData skeleton, JointType mainJoint, JointType otherJoint)
        //{
        //    float angle = AngleBetweenTwoJointsAndVector(skeleton, mainJoint, otherJoint, Vector3.Down);

        //    boneTransforms[jointMap[mainJoint]] = Matrix.CreateRotationY(MathHelper.ToRadians(45) - angle) * boneTransforms[jointMap[mainJoint]];
        //    //boneTransforms[jointMap[mainJoint]] = Matrix.CreateRotationX(MathHelper.ToRadians(45) - angle) * boneTransforms[jointMap[mainJoint]];
        //}

        /// <summary>
        /// Calculate the rotation for one vector to face another vector
        /// http://www.xnawiki.com/index.php?title=Vector_Math
        /// </summary>
        /// <param name="position"></param>
        /// <param name="lookAt"></param>
        /// <returns>Rotation matrix</returns>
        public Matrix LookAt(Vector3 position, Vector3 lookAt)
        {
            Matrix rotation = new Matrix();

            //TODO sredit
            rotation.Forward = /*Vector3.Normalize*/(lookAt - position);
            rotation.Right = /*Vector3.Normalize*/(Vector3.Cross(rotation.Forward, Vector3.Up));
            rotation.Up = /*Vector3.Normalize*/(Vector3.Cross(rotation.Right, rotation.Forward));

            return rotation;
        }

        /// <summary>
        /// Angle Between Two Vectors
        /// http://social.msdn.microsoft.com/Forums/en-AU/kinectsdknuiapi/thread/8516bab7-c28b-4834-82c9-b3ef911cd1f7
        /// </summary>
        /// <param name="a">first vector</param>
        /// <param name="b">second vector</param>
        /// <returns>Angle Between the Two Vectors in Radians</returns>
        public float AngleBetweenTwoVectors(Vector3 a, Vector3 b)
        {
            float dotProduct = Vector3.Dot(a,b);//Vector3.Normalize(a), Vector3.Normalize(b));

            float angle = Convert.ToSingle(Math.Acos(dotProduct));

            // http://stackoverflow.com/questions/1524748/how-to-calculate-obtuse-angle-between-two-vectors
            //if (dotProduct < 0)
            //    angle *= -1;

            return angle;
        }

        /// <summary>
        /// Convert the position of a joint (by kinect) to XNA friendly coordinate values.
        /// </summary>
        /// <param name="position">Position of the joint (by kinect)</param>
        /// <returns>XNA friendly coordinate</returns>
        /// 


        public Vector3 ConvertRealWorldPoint(SkeletonPoint position)
        {
            Vector3 res = new Vector3();
            //res.X = position.X * 10;
            //res.Y = position.Y * 10;
            //res.Z = -position.Z;
            res.X = position.X * scale;
            res.Y = (position.Y + 1.0f) * scale;
            res.Z = (position.Z - 1.0f) * scale;
            return res;
        }

        public void UpdateWorldTransforms(Matrix rootTransform)
        {
            // Root bone.
            worldTransforms[0] = boneTransforms[0] * rootTransform;

            // Child bones.
            for (int bone = 1; bone < worldTransforms.Length; bone++)
            {
                int parentBone = skinningDataValue.SkeletonHierarchy[bone];

                worldTransforms[bone] = boneTransforms[bone] * worldTransforms[parentBone];
            }
        }

        public void UpdateSkinTransforms()
        {
            for (int bone = 0; bone < skinTransforms.Length; bone++)
            {
                skinTransforms[bone] = skinningDataValue.InverseBindPose[bone] * worldTransforms[bone];
            }
        }

        

        ///// <summary>
        ///// Helper used by the Update method to refresh the BoneTransforms data.
        ///// </summary>
        //public void UpdateBoneTransforms(TimeSpan time, bool relativeToCurrentTime)
        //{
        //    if (currentClipValue == null)
        //        throw new InvalidOperationException(
        //                    "AnimationPlayer.Update was called before StartClip");

        //    // Update the animation position.
        //    if (relativeToCurrentTime)
        //    {
        //        time += currentTimeValue;

        //        // If we reached the end, loop back to the start.
        //        while (time >= currentClipValue.Duration)
        //            time -= currentClipValue.Duration;
        //    }

        //    if ((time < TimeSpan.Zero) || (time >= currentClipValue.Duration))
        //        throw new ArgumentOutOfRangeException("time");

        //    // If the position moved backwards, reset the keyframe index.
        //    if (time < currentTimeValue)
        //    {
        //        currentKeyframe = 0;
        //        skinningDataValue.BindPose.CopyTo(boneTransforms, 0);
        //    }

        //    currentTimeValue = time;

        //    // Read keyframe matrices.
        //    IList<Keyframe> keyframes = currentClipValue.Keyframes;

        //    while (currentKeyframe < keyframes.Count)
        //    {
        //        Keyframe keyframe = keyframes[currentKeyframe];

        //        // Stop when we've read up to the current time position.
        //        if (keyframe.Time > currentTimeValue)
        //            break;

        //        // Use this keyframe.
        //        boneTransforms[keyframe.Bone] = keyframe.Transform;

        //        currentKeyframe++;
        //    }
        //}


        ///// <summary>
        ///// Helper used by the Update method to refresh the WorldTransforms data.
        ///// </summary>
        //public void UpdateWorldTransforms(Matrix rootTransform)
        //{
        //    // Root bone.
        //    worldTransforms[0] = boneTransforms[0] * rootTransform;

        //    // Child bones.
        //    for (int bone = 1; bone < worldTransforms.Length; bone++)
        //    {
        //        int parentBone = skinningDataValue.SkeletonHierarchy[bone];

        //        worldTransforms[bone] = boneTransforms[bone] *
        //                                     worldTransforms[parentBone];
        //    }
        //}


        ///// <summary>
        ///// Helper used by the Update method to refresh the SkinTransforms data.
        ///// </summary>
        //public void UpdateSkinTransforms()
        //{
        //    for (int bone = 0; bone < skinTransforms.Length; bone++)
        //    {
        //        skinTransforms[bone] = skinningDataValue.InverseBindPose[bone] *
        //                                    worldTransforms[bone];
        //    }
        //}


        ///// <summary>
        ///// Gets the current bone transform matrices, relative to their parent bones.
        ///// </summary>
        //public Matrix[] GetBoneTransforms()
        //{
        //    return boneTransforms;
        //}


        ///// <summary>
        ///// Gets the current bone transform matrices, in absolute format.
        ///// </summary>
        //public Matrix[] GetWorldTransforms()
        //{
        //    return worldTransforms;
        //}


        /// <summary>
        /// Gets the current bone transform matrices,
        /// relative to the skinning bind pose.
        /// </summary>
        public Matrix[] GetSkinTransforms()
        {
            return skinTransforms;
        }


        /// <summary>
        /// Gets the clip currently being decoded.
        /// </summary>
        public AnimationClip CurrentClip
        {
            get { return currentClipValue; }
        }


        /// <summary>
        /// Gets the current play position.
        /// </summary>
        public TimeSpan CurrentTime
        {
            get { return currentTimeValue; }
        }

        public Skeleton skeleton { get; set; }

        public Model model { get; set; }
    }
}
