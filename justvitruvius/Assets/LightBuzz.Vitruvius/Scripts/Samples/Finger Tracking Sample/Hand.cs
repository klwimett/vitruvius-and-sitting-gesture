using Windows.Kinect;
using System.Collections.Generic;
using System.Linq;

namespace LightBuzz.Vitruvius.FingerTracking
{
    /// <summary>
    /// Represents a hand.
    /// </summary>
    public class Hand
    {
        /// <summary>
        /// The tracking ID of the body the current hand belongs to.
        /// </summary>
        public ulong TrackingId { get; protected set; }

        /// <summary>
        /// A list of the detected fingers.
        /// </summary>
        public IList<Finger> Fingers { get; protected set; }

        /// <summary>
        /// A list of the contour points in the 3D Camera space.
        /// </summary>
        public IList<CameraSpacePoint> ContourCamera { get; protected set; }

        /// <summary>
        /// A list of the contour points in the 2D Depth space.
        /// </summary>
        public IList<DepthSpacePoint> ContourDepth { get; protected set; }

        /// <summary>
        /// A list of the countour points in the 2D Color space.
        /// </summary>
        public IList<ColorSpacePoint> ContourColor { get; protected set; }

        /// <summary>
        /// The hand joint position in 2D Depth space.
        /// </summary>
        public DepthSpacePoint HandPosition { get; protected set; }

        /// <summary>
        /// The wrist joint position in 2D Depth space.
        /// </summary>
        public DepthSpacePoint WristPosition { get; protected set; }

        /// <summary>
        /// The state of the hand.
        /// </summary>
        public HandState State { get; protected set; }

        internal Hand(ulong trackingID, HandState state, DepthPointEx handPosition, DepthPointEx wristPosition,
            IList<DepthPointEx> contour, IList<DepthPointEx> fingers, CoordinateMapper coordinateMapper)
        {
            TrackingId = trackingID;

            HandPosition = handPosition.ToDepthSpacePoint();
            WristPosition = wristPosition.ToDepthSpacePoint();
            State = state;

            Fingers = new List<Finger>();

            if (state == HandState.Open || (state == HandState.Unknown && fingers.Count > 0))
            {
                Fingers.Add(new Finger(fingers[0], wristPosition, 2, coordinateMapper));
                Fingers.Add(new Finger(fingers[1], wristPosition, 3, coordinateMapper));
                Fingers.Add(new Finger(fingers[2], wristPosition, 3, coordinateMapper));
                Fingers.Add(new Finger(fingers[3], wristPosition, 3, coordinateMapper));
                Fingers.Add(new Finger(fingers[4], wristPosition, 3, coordinateMapper));
            }

            ushort[] depths = contour.Select(d => (ushort)d.Depth).ToArray();

            ContourDepth = contour.Select(p => p.ToDepthSpacePoint()).ToArray();

            ContourCamera = new CameraSpacePoint[ContourDepth.Count];
            coordinateMapper.MapDepthPointsToCameraSpace((DepthSpacePoint[])ContourDepth, depths, (CameraSpacePoint[])ContourCamera);

            ContourColor = new ColorSpacePoint[ContourDepth.Count];
            coordinateMapper.MapDepthPointsToColorSpace((DepthSpacePoint[])ContourDepth, depths, (ColorSpacePoint[])ContourColor);
        }
    }
}