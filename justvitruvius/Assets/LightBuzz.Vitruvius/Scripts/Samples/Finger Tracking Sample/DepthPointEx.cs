using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Kinect;

namespace LightBuzz.Vitruvius.FingerTracking
{
    public struct DepthPointEx
    {
        public double X;
        public double Y;
        public double Depth;

        public static DepthPointEx Zero = new DepthPointEx(0, 0, 0);

        public double Magnitude
        {
            get
            {
                return X * X + Y * Y;
            }
        }

        public DepthPointEx(DepthPointEx point) : this()
        {
            X = point.X;
            Y = point.Y;
            Depth = point.Depth;
        }

        public DepthPointEx(UnityEngine.Vector3 point) : this()
        {
            X = point.x;
            Y = point.y;
            Depth = point.z;
        }

        public DepthPointEx(double x, double y, double depth) : this()
        {
            X = x;
            Y = y;
            Depth = depth;
        }

        public DepthPointEx(float x, float y, float depth) : this()
        {
            X = x;
            Y = y;
            Depth = depth;
        }

        public DepthPointEx FindNearestPoint(IEnumerable<DepthPointEx> points)
        {
            var pointList = points.ToList();
            return pointList[FindIndexOfNearestPoint(pointList)];
        }

        public int FindIndexOfNearestPoint(IList<DepthPointEx> points)
        {
            int index = 0;
            int resultIndex = -1;
            double minDist = double.MaxValue;
            foreach (DepthPointEx p in points)
            {
                var distance = Distance(p.X, p.Y, X, Y);
                if (distance < minDist)
                {
                    resultIndex = index;
                    minDist = distance;
                }
                index++;
            }
            return resultIndex;
        }

        public double GetMagnitude(DepthPointEx other)
        {
            return (X - other.X) * (X - other.X) + (Y - other.Y) * (Y - other.Y);
        }

        public double Distance(DepthPointEx other)
        {
            return Distance(X, Y, other.X, other.Y);
        }

        public static double Distance(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));
        }

        public double Dot(DepthPointEx otherDir)
        {
            return Dot(X, Y, otherDir.X, otherDir.Y);
        }

        public static double Dot(double x1, double y1, double x2, double y2)
        {
            return x1 * x2 + y1 * y2;
        }

        public static double Cross(double x1, double y1, double x2, double y2)
        {
            return x1 * y2 - y1 * x2;
        }

        public double Angle(DepthPointEx other)
        {
            return Angle(X, Y, other.X, other.Y);
        }

        public static double Angle(DepthPointEx center, DepthPointEx start, DepthPointEx end)
        {
            return Angle(center.X, center.Y, start.X, start.Y, end.X, end.Y);
        }

        public static double Angle(double centerX, double centerY, double startX, double startY, double endX, double endY)
        {
            return Angle(startX - centerX, startY - centerY, endX - centerX, endY - centerY);
        }

        public static double Angle(double x1, double y1, double x2, double y2)
        {
            return Math.Abs(Math.Floor(Math.Atan2(Cross(x1, y1, x2, y2), Dot(x1, y1, x2, y2)) * 180.0 / Math.PI + 0.5));
        }

        public DepthPointEx Lerp(DepthPointEx target, double delta)
        {
            return new DepthPointEx(X + (target.X - X) * delta, Y + (target.Y - Y) * delta, Depth + (target.Depth - Depth) * delta);
        }

        public DepthPointEx Center(DepthPointEx other)
        {
            return new DepthPointEx((X + other.X) * 0.5, (Y + other.Y) * 0.5, (Depth + other.Depth) * 0.5);
        }

        public static DepthPointEx Center(IList<DepthPointEx> points)
        {
            var center = Zero;
            if (points.Count > 0)
            {
                for (int index = 0; index < points.Count; index++)
                {
                    var p = points[index];
                    center.X += p.X;
                    center.Y += p.Y;
                    center.Depth += p.Depth;
                }

                center.X /= points.Count;
                center.Y /= points.Count;
                center.Depth /= points.Count;
            }
            return center;
        }

        public DepthSpacePoint ToDepthSpacePoint()
        {
            return new DepthSpacePoint() { X = (float)X, Y = (float)Y };
        }

        public static DepthPointEx operator +(DepthPointEx p1, DepthPointEx p2)
        {
            return new DepthPointEx(p1.X + p2.X, p1.Y + p2.Y, p1.Depth + p2.Depth);
        }

        public static DepthPointEx operator -(DepthPointEx p1, DepthPointEx p2)
        {
            return new DepthPointEx(p2.X - p1.X, p2.Y - p1.Y, p2.Depth - p1.Depth);
        }

        public static DepthPointEx operator *(DepthPointEx p, double m)
        {
            return new DepthPointEx(p.X * m, p.Y * m, p.Depth * m);
        }

        public static DepthPointEx operator *(double m, DepthPointEx p)
        {
            return new DepthPointEx(p.X * m, p.Y * m, p.Depth * m);
        }

        public static DepthPointEx operator /(DepthPointEx p, double d)
        {
            return new DepthPointEx(p.X / d, p.Y / d, p.Depth / d);
        }

        public static DepthPointEx operator /(double d, DepthPointEx p)
        {
            return new DepthPointEx(p.X / d, p.Y / d, p.Depth / d);
        }
    }
}