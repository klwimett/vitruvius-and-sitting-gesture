using System.Collections.Generic;
using Windows.Kinect;

namespace LightBuzz.Vitruvius.FingerTracking
{
    /// <summary>
    /// Represents a finger tip.
    /// </summary>
    public class Finger
    {
        public FingerJoint[] joints;

        /// <summary>
        /// The position of the fingertip in the 3D Camera space.
        /// </summary>
        public CameraSpacePoint CameraPoint { get; set; }

        /// <summary>
        /// The position of the fingertip in the 2D Depth space.
        /// </summary>
        public DepthSpacePoint DepthPoint { get; set; }

        /// <summary>
        /// The position of the fingertip in the 2D Color space.
        /// </summary>
        public ColorSpacePoint ColorPoint { get; set; }

        public Finger(DepthPointEx point, DepthPointEx wrist, int jointCount, CoordinateMapper coordinateMapper)
        {
            DepthPoint = point.ToDepthSpacePoint();

            ColorPoint = coordinateMapper.MapDepthPointToColorSpace(DepthPoint, (ushort)point.Depth);

            CameraPoint = coordinateMapper.MapDepthPointToCameraSpace(DepthPoint, (ushort)point.Depth);

            joints = GetJoints(point, wrist.Lerp(point, 0.6), jointCount, coordinateMapper);
        }

        FingerJoint[] GetJoints(DepthPointEx point, DepthPointEx firstJoint, int jointCount, CoordinateMapper coordinateMapper)
        {
            List<FingerJoint> joints = new List<FingerJoint>();
            joints.Add(new FingerJoint(firstJoint, coordinateMapper));

            double step = 1.0 / jointCount;

            for (int i = 1; i < jointCount; i++)
            {
                joints.Add(new FingerJoint(firstJoint.Lerp(point, step * i), coordinateMapper));
            }

            return joints.ToArray();
        }
    }

    public struct FingerJoint
    {
        public CameraSpacePoint CameraPoint { get; set; }

        public DepthSpacePoint DepthPoint { get; set; }

        public ColorSpacePoint ColorPoint { get; set; }

        public FingerJoint(DepthPointEx point, CoordinateMapper coordinateMapper)
        {
            DepthPoint = point.ToDepthSpacePoint();

            ColorPoint = coordinateMapper.MapDepthPointToColorSpace(DepthPoint, (ushort)point.Depth);

            CameraPoint = coordinateMapper.MapDepthPointToCameraSpace(DepthPoint, (ushort)point.Depth);
        }
    }
}