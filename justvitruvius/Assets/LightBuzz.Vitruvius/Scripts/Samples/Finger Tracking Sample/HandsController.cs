using Windows.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using Joint = Windows.Kinect.Joint;
using UnityEngine;

namespace LightBuzz.Vitruvius.FingerTracking
{
    /// <summary>
    /// Detects human hands in the 3D and 2D space.
    /// </summary>
    public class HandsController
    {
        private readonly ushort MIN_DEPTH = 500;
        private readonly ushort MAX_DEPTH = ushort.MaxValue;
        private readonly float DEPTH_THRESHOLD = 80; // equals to 8cm

        private GrahamScan grahamScan = new GrahamScan();

        public bool HandLeftDetected { get; private set; }
        public bool HandRightDetected { get; private set; }

        byte[] handPixels = null;
        public byte[] HandArray { get { return handPixels; } }

        public FilterModuleManager moduleManager;

        /// <summary>
        /// The coordinate mapper that will be used during the finger detection process.
        /// </summary>
        CoordinateMapper CoordinateMapper;

        /// <summary>
        /// Determines whether the algorithm will detect the left hand.
        /// </summary>
        public bool DetectLeftHand { get; set; }

        /// <summary>
        /// Determines whether the algorithm will detect the right hand.
        /// </summary>
        public bool DetectRightHand { get; set; }

        /// <summary>
        /// Raised when a new pair of hands is detected.
        /// </summary>
        public event EventHandler<HandCollection> HandsDetected;

        /// <summary>
        /// Creates a new instance of <see cref="HandsControllerOld"/>.
        /// </summary>
        public HandsController()
        {
            CoordinateMapper = KinectSensor.GetDefault().CoordinateMapper;
            DetectLeftHand = true;
            DetectRightHand = true;
        }

        /// <summary>
        /// Creates a new instance of <see cref="HandsControllerOld"/> with the specified coordinate mapper.
        /// </summary>
        /// <param name="coordinateMapper">The coordinate mapper that will be used during the finger detection process.</param>
        public HandsController(CoordinateMapper coordinateMapper)
        {
            CoordinateMapper = coordinateMapper;
        }

        /// <summary>
        /// Updates the finger-detection engine with the new data.
        /// </summary>
        /// <param name="data">A pointer to an array of depth data.</param>
        /// <param name="body">The body to search for hands and fingers.</param>
        public void Update(ushort[] data, Body body)
        {
            if (data == null || body == null) return;

            int dataSize = Constants.DEFAULT_DEPTH_WIDTH * Constants.DEFAULT_DEPTH_HEIGHT;

            if (handPixels == null)
            {
                handPixels = new byte[dataSize];
            }

            Hand handLeft = null;
            Hand handRight = null;

            Joint jointHandLeft = body.Joints[JointType.HandLeft];
            Joint jointHandRight = body.Joints[JointType.HandRight];
            Joint jointWristLeft = body.Joints[JointType.WristLeft];
            Joint jointWristRight = body.Joints[JointType.WristRight];
            Joint jointElbowLeft = body.Joints[JointType.ElbowLeft];
            Joint jointElbowRight = body.Joints[JointType.ElbowRight];
            Joint jointTipLeft = body.Joints[JointType.HandTipLeft];
            Joint jointTipRight = body.Joints[JointType.HandTipRight];
            Joint jointThumbLeft = body.Joints[JointType.ThumbLeft];
            Joint jointThumbRight = body.Joints[JointType.ThumbRight];

            DepthSpacePoint depthPointHandLeft = CoordinateMapper.MapCameraPointToDepthSpace(jointHandLeft.Position);
            DepthSpacePoint depthPointWristLeft = CoordinateMapper.MapCameraPointToDepthSpace(jointWristLeft.Position);
            DepthSpacePoint depthPointElbowLeft = CoordinateMapper.MapCameraPointToDepthSpace(jointElbowLeft.Position);
            DepthSpacePoint depthPointTipLeft = CoordinateMapper.MapCameraPointToDepthSpace(jointTipLeft.Position);
            DepthSpacePoint depthPointThumbLeft = CoordinateMapper.MapCameraPointToDepthSpace(jointThumbLeft.Position);

            DepthSpacePoint depthPointHandRight = CoordinateMapper.MapCameraPointToDepthSpace(jointHandRight.Position);
            DepthSpacePoint depthPointWristRight = CoordinateMapper.MapCameraPointToDepthSpace(jointWristRight.Position);
            DepthSpacePoint depthPointElbowRight = CoordinateMapper.MapCameraPointToDepthSpace(jointElbowRight.Position);
            DepthSpacePoint depthPointTipRight = CoordinateMapper.MapCameraPointToDepthSpace(jointTipRight.Position);
            DepthSpacePoint depthPointThumbRight = CoordinateMapper.MapCameraPointToDepthSpace(jointThumbRight.Position);

            float elbowLeftX = depthPointElbowLeft.X;
            float elbowLeftY = depthPointElbowLeft.Y;
            float wristLeftX = elbowLeftX + (depthPointWristLeft.X - elbowLeftX) * 0.8f;
            float wristLeftY = elbowLeftY + (depthPointWristLeft.Y - elbowLeftY) * 0.8f;
            float handLeftX = wristLeftX + (depthPointHandLeft.X - wristLeftX) * 0.8f;
            float handLeftY = wristLeftY + (depthPointHandLeft.Y - wristLeftY) * 0.8f;
            float tipLeftX = depthPointTipLeft.X;
            float tipLeftY = depthPointTipLeft.Y;
            float thumbLeftX = depthPointThumbLeft.X;
            float thumbLeftY = depthPointThumbLeft.Y;

            float elbowRightX = depthPointElbowRight.X;
            float elbowRightY = depthPointElbowRight.Y;
            float wristRightX = elbowRightX + (depthPointWristRight.X - elbowRightX) * 0.8f;
            float wristRightY = elbowRightY + (depthPointWristRight.Y - elbowRightY) * 0.8f;
            float handRightX = wristRightX + (depthPointHandRight.X - wristRightX) * 0.8f;
            float handRightY = wristRightY + (depthPointHandRight.Y - wristRightY) * 0.8f;
            float tipRightX = depthPointTipRight.X;
            float tipRightY = depthPointTipRight.Y;
            float thumbRightX = depthPointThumbRight.X;
            float thumbRightY = depthPointThumbRight.Y;

            bool searchForLeftHand = DetectLeftHand && !float.IsInfinity(handLeftX) && !float.IsInfinity(handLeftY) && !float.IsInfinity(wristLeftX) && !float.IsInfinity(wristLeftY) && !float.IsInfinity(tipLeftX) && !float.IsInfinity(tipLeftY) && !float.IsInfinity(thumbLeftX) && !float.IsInfinity(thumbLeftY);
            bool searchForRightHand = DetectRightHand && !float.IsInfinity(handRightX) && !float.IsInfinity(handRightY) && !float.IsInfinity(wristRightX) && !float.IsInfinity(wristRightY) && !float.IsInfinity(tipRightX) && !float.IsInfinity(tipRightY) && !float.IsInfinity(thumbRightX) && !float.IsInfinity(thumbRightY);

            if (searchForLeftHand || searchForRightHand)
            {
                double angleLeft = searchForLeftHand ? DepthPointEx.Angle(wristLeftX, wristLeftY, wristLeftX, 0, handLeftX, handLeftY) : 0.0;
                double angleRight = searchForRightHand ? DepthPointEx.Angle(wristRightX, wristRightY, wristRightX, 0, handRightX, handRightY) : 0.0;

                double distanceLeft = searchForLeftHand ? CalculateDistance(handLeftX, handLeftY, tipLeftX, tipLeftY, thumbLeftX, thumbLeftY) : 0.0;
                double distanceRight = searchForRightHand ? CalculateDistance(handRightX, handRightY, tipRightX, tipRightY, thumbRightX, thumbRightY) : 0.0;

                int minLeftX = searchForLeftHand ? (int)(handLeftX - distanceLeft) : 0;
                int minLeftY = searchForLeftHand ? (int)(handLeftY - distanceLeft) : 0;
                int maxLeftX = searchForLeftHand ? (int)(handLeftX + distanceLeft) : 0;
                int maxLeftY = searchForLeftHand ? (int)(handLeftY + distanceLeft) : 0;

                int minRightX = searchForRightHand ? (int)(handRightX - distanceRight) : 0;
                int minRightY = searchForRightHand ? (int)(handRightY - distanceRight) : 0;
                int maxRightX = searchForRightHand ? (int)(handRightX + distanceRight) : 0;
                int maxRightY = searchForRightHand ? (int)(handRightY + distanceRight) : 0;

                float depthLeft = jointHandLeft.Position.Z * 1000; // m to mm
                float depthRight = jointHandRight.Position.Z * 1000;

                ushort depth = 0;

                int depthX = 0;
                int depthY = 0;

                bool isInBounds = false;
                bool conditionLeft = false;
                bool conditionRight = false;

                List<DepthPointEx> contourLeft = new List<DepthPointEx>();
                List<DepthPointEx> contourRight = new List<DepthPointEx>();

                byte top = 255;
                byte bottom = 255;
                byte left = 255;
                byte right = 255;

                bool readLeftHandData = false;
                bool readRightHandData = false;

                DepthPointEx depthPointEx = new DepthPointEx();

                for (int i = 0; i < dataSize; i++)
                {
                    depth = data[i];

                    depthX = i % Constants.DEFAULT_DEPTH_WIDTH;
                    depthY = i / Constants.DEFAULT_DEPTH_WIDTH;

                    isInBounds = depth >= MIN_DEPTH && depth <= MAX_DEPTH;

                    conditionLeft = depth >= depthLeft - DEPTH_THRESHOLD && depth <= depthLeft + DEPTH_THRESHOLD &&
                        depthX >= minLeftX && depthX <= maxLeftX && depthY >= minLeftY && depthY <= maxLeftY;

                    conditionRight = depth >= depthRight - DEPTH_THRESHOLD && depth <= depthRight + DEPTH_THRESHOLD &&
                        depthX >= minRightX && depthX <= maxRightX && depthY >= minRightY && depthY <= maxRightY;

                    readLeftHandData = searchForLeftHand && conditionLeft;
                    readRightHandData = searchForRightHand && conditionRight;

                    handPixels[i] = (byte)(!isInBounds ? 255 : readLeftHandData && readRightHandData ? (depthLeft <= depthRight ? 0 : 2) : readLeftHandData ? 0 : readRightHandData ? 2 : 255);
                }

                for (int i = 0; i < dataSize; i++)
                {
                    depth = data[i];

                    depthX = i % Constants.DEFAULT_DEPTH_WIDTH;
                    depthY = i / Constants.DEFAULT_DEPTH_WIDTH;

                    if (searchForLeftHand && handPixels[i] == 0)
                    {
                        top = i - Constants.DEFAULT_DEPTH_WIDTH >= 0 ? handPixels[i - Constants.DEFAULT_DEPTH_WIDTH] : (byte)255;
                        bottom = i + Constants.DEFAULT_DEPTH_WIDTH < dataSize ? handPixels[i + Constants.DEFAULT_DEPTH_WIDTH] : (byte)255;
                        left = i - 1 >= 0 ? handPixels[i - 1] : (byte)255;
                        right = i + 1 < dataSize ? handPixels[i + 1] : (byte)255;

                        if (top == 255 || bottom == 255 || left == 255 || right == 255)
                        {
                            handPixels[i] = 1;
                            depthPointEx.X = depthX;
                            depthPointEx.Y = depthY;
                            depthPointEx.Depth = depth;
                            contourLeft.Add(depthPointEx);
                        }
                    }

                    if (searchForRightHand && handPixels[i] == 2)
                    {
                        top = i - Constants.DEFAULT_DEPTH_WIDTH >= 0 ? handPixels[i - Constants.DEFAULT_DEPTH_WIDTH] : (byte)255;
                        bottom = i + Constants.DEFAULT_DEPTH_WIDTH < dataSize ? handPixels[i + Constants.DEFAULT_DEPTH_WIDTH] : (byte)255;
                        left = i - 1 >= 0 ? handPixels[i - 1] : (byte)255;
                        right = i + 1 < dataSize ? handPixels[i + 1] : (byte)255;

                        if (top == 255 || bottom == 255 || left == 255 || right == 255)
                        {
                            handPixels[i] = 3;
                            depthPointEx.X = depthX;
                            depthPointEx.Y = depthY;
                            depthPointEx.Depth = depth;
                            contourRight.Add(depthPointEx);
                        }
                    }
                }

                if (searchForLeftHand)
                {
                    handLeft = GetHand(body.TrackingId, body.HandLeftState, contourLeft, angleLeft, new DepthPointEx(handLeftX, handLeftY, 0),
                        new DepthPointEx(wristLeftX, wristLeftY, 0), new DepthPointEx(elbowLeftX, elbowLeftY, 0));
                }

                if (searchForRightHand)
                {
                    handRight = GetHand(body.TrackingId, body.HandRightState, contourRight, angleRight, new DepthPointEx(handRightX, handRightY, 0),
                        new DepthPointEx(wristRightX, wristRightY, 0), new DepthPointEx(elbowRightX, elbowRightY, 0));
                }
            }

            HandLeftDetected = handLeft != null;
            HandRightDetected = handRight != null;

            if (HandLeftDetected || HandRightDetected)
            {
                HandCollection hands = new HandCollection
                {
                    TrackingId = body.TrackingId,
                    HandLeft = handLeft,
                    HandRight = handRight
                };

                if (HandsDetected != null)
                {
                    HandsDetected(this, hands);
                }
            }
        }

        private double CalculateDistance(float handX, float handY, float tipX, float tipY, float thumbX, float thumbY)
        {
            return Math.Max(DepthPointEx.Distance(tipX, tipY, handX, handY) * 2.0, DepthPointEx.Distance(thumbX, thumbY, handX, handY) * 2.0);
        }

        private Hand GetHand(ulong trackingID, HandState state, IList<DepthPointEx> contour, double angle, DepthPointEx hand, DepthPointEx wrist, DepthPointEx elbow)
        {
            DepthPointEx handDirection = wrist.Lerp(elbow.Lerp(hand, 0.8), 0.5) - elbow;

            contour = contour.
                Where(p => handDirection.Dot(p - wrist) >= 0).ToList()
                .Arrange(wrist)
                .SmoothPoints(0.7)
                .SmoothPoints(0.7)
                .CutSparePointsAtEnd();

            //foreach (FilterModuleManager.FilterModule module in moduleManager.modules)
            //{
            //    switch (module.type)
            //    {
            //        case FilterType.Arrange:
            //            contour = contour.Arrange(wrist);
            //            break;
            //        case FilterType.SmoothPoints:
            //                contour = contour.SmoothPoints(Mathf.Clamp01((float)module.dValue1));
            //            break;
            //        case FilterType.SmoothClosePoints:
            //                contour = contour.SmoothClosePoints(module.dValue1, Mathf.Clamp01((float)module.dValue2));
            //            break;
            //        case FilterType.SmoothFarPoints:
            //                contour = contour.SmoothFarPoints(module.dValue1, Mathf.Clamp01((float)module.dValue2));
            //            break;
            //        case FilterType.SimplifyClosePoints:
            //                contour = contour.SimplifyClosePoints(module.dValue1);
            //            break;
            //        case FilterType.SimplifyFarPoints:
            //                contour = contour.SimplifyFarPoints(module.dValue1, module.intValue);
            //            break;
            //        case FilterType.RemoveSteepCorners:
            //                contour = contour.RemoveSteepCorners(module.dValue1, module.intValue);
            //            break;
            //        case FilterType.RemoveWideCorners:
            //                contour = contour.RemoveWideCorners(module.dValue1);
            //            break;
            //        case FilterType.CutSparePointsAtEnd:
            //                contour = contour.CutSparePointsAtEnd(module.intValue);
            //            break;
            //    }
            //}

            IList<DepthPointEx> convexHull = new List<DepthPointEx>(contour);
            convexHull = grahamScan.ConvexHull(contour)
                .Filter(moduleManager.filterDistance)
                .RemoveWideCorners(165);

            // Toggles the convex hull visualization
            if (!moduleManager.showContour)
            {
                contour = (List<DepthPointEx>)convexHull;
            }

            IList<DepthPointEx> fingers = convexHull.OrderByDescending(p => elbow.GetMagnitude(p)).Take(5).ToList();

            if (fingers.Count < 5 && fingers.Count > 0)
            {
                int addition = 5 - fingers.Count;

                for (int i = 0; i < addition; i++)
                {
                    fingers.Add(fingers[i]);
                }
            }
            
            if (contour.Count > 0 && fingers.Count > 0)
            {
                ArrangeFingers(fingers, wrist);

                return new Hand(trackingID, state, hand, wrist, contour, fingers, CoordinateMapper);
            }

            return null;
        }

        void ArrangeFingers(IList<DepthPointEx> fingers, DepthPointEx toPoint)
        {
            DepthPointEx thumbFinger = NextClosestFinger(fingers, toPoint);
            DepthPointEx indexFinger = NextClosestFinger(fingers, thumbFinger);
            DepthPointEx middleFinger = NextClosestFinger(fingers, indexFinger);
            DepthPointEx ringFinger = NextClosestFinger(fingers, middleFinger);

            fingers.Insert(0, ringFinger);
            fingers.Insert(0, middleFinger);
            fingers.Insert(0, indexFinger);
            fingers.Insert(0, thumbFinger);
        }

        DepthPointEx NextClosestFinger(IList<DepthPointEx> fingers, DepthPointEx toPoint)
        {
            double closestDistance = double.MaxValue;
            int closestIndex = 0;
            double currDistance;

            for (int i = 0; i < fingers.Count; i++)
            {
                currDistance = fingers[i].GetMagnitude(toPoint);

                if (currDistance < closestDistance)
                {
                    closestDistance = currDistance;
                    closestIndex = i;
                }
            }

            DepthPointEx closestFinger = fingers[closestIndex];
            fingers.RemoveAt(closestIndex);
            return closestFinger;
        }
    }
}