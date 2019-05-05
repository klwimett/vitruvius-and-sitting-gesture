using System.Collections.Generic;

namespace LightBuzz.Vitruvius.FingerTracking
{
    public static class PointFilter
    {
        public static IList<DepthPointEx> Filter(this IList<DepthPointEx> points, double distance = 1.0)
        {
            bool removed;
            do
            {
                removed = false;

                for (int i = 0; i < points.Count - 1; i++)
                {
                    if (points[i].GetMagnitude(points[i + 1]) < distance)
                    {
                        points[i] = points[i].Center(points[i + 1]);
                        points.RemoveAt(i + 1);
                        i--;
                        removed = true;
                    }
                }

                if (points.Count > 1)
                {
                    if (points[0].GetMagnitude(points[points.Count - 1]) < distance)
                    {
                        points[0] = points[0].Center(points[points.Count - 1]);
                        points.RemoveAt(points.Count - 1);
                        removed = true;
                    }
                }

            } while (removed);


            return points;
        }

        public static IList<DepthPointEx> SmoothPoints(this IList<DepthPointEx> points, double smooth = 0.5)
        {
            for (int i = 1, count = points.Count - 1; i < count; i++)
            {
                points[i] = points[i].Lerp(points[i - 1].Lerp(points[i + 1], 0.5), smooth);
            }

            return points;
        }

        public static IList<DepthPointEx> SimplifyClosePoints(this IList<DepthPointEx> points, double dist = 2.0)
        {
            bool removed;
            do
            {
                removed = false;

                for (int i = 0; i < points.Count - 1; i++)
                {
                    if (points[i].GetMagnitude(points[i + 1]) < dist)
                    {
                        points.RemoveAt(i);
                        removed = true;
                    }
                }

            } while (removed);

            return points;
        }

        public static IList<DepthPointEx> SmoothClosePoints(this IList<DepthPointEx> points, double dist = 2.0, double smooth = 0.5)
        {
            for (int i = 1; i < points.Count - 1; i++)
            {
                if (points[i].GetMagnitude(points[i - 1]) < dist && points[i].GetMagnitude(points[i + 1]) < dist)
                {
                    points[i - 1] = points[i].Lerp(points[i - 1], smooth);
                    points[i + 1] = points[i].Lerp(points[i + 1], smooth);
                }
            }

            return points;
        }

        public static IList<DepthPointEx> SmoothFarPoints(this IList<DepthPointEx> points, double dist = 2.0, double smooth = 0.5)
        {
            for (int i = 1; i < points.Count - 1; i++)
            {
                if (points[i].GetMagnitude(points[i - 1]) > dist || points[i].GetMagnitude(points[i + 1]) > dist)
                {
                    points[i] = points[i].Lerp(points[i - 1].Lerp(points[i + 1], 0.5), smooth);
                }
            }

            return points;
        }

        public static IList<DepthPointEx> SimplifyFarPoints(this IList<DepthPointEx> points, double dist = 500.0, int iterations = -1)
        {
            bool removed;
            do
            {
                removed = false;
                for (int i = points.Count - 1; i > 0; i--)
                {
                    if (points[i].GetMagnitude(points[i - 1]) > dist)
                    {
                        points.RemoveAt(i);
                        removed = true;
                    }
                }

                if (iterations > 0)
                {
                    iterations--;
                }

            } while (removed && (iterations < 0 || iterations > 0));

            return points;
        }

        public static IList<DepthPointEx> RemoveSteepCorners(this IList<DepthPointEx> points, double degrees = 5.0, int iterations = -1)
        {
            bool removed;
            do
            {
                removed = false;
                for (int i = 1; i < points.Count - 1; i++)
                {
                    if (DepthPointEx.Angle(points[i], points[i - 1], points[i + 1]) < degrees)
                    {
                        points.RemoveAt(i++);
                        removed = true;
                    }
                }

                if (iterations > 0)
                {
                    iterations--;
                }

            } while (removed && (iterations < 0 || iterations > 0));

            return points;
        }

        public static IList<DepthPointEx> RemoveWideCorners(this IList<DepthPointEx> points, double degrees = 175.0)
        {
            bool removed;
            do
            {
                removed = false;
                for (int i = 1; i < points.Count - 1; i++)
                {
                    if (DepthPointEx.Angle(points[i], points[i - 1], points[i + 1]) > degrees)
                    {
                        points.RemoveAt(i);
                        removed = true;
                    }
                }

            } while (removed);

            return points;
        }

        public static IList<DepthPointEx> Arrange(this IList<DepthPointEx> points, DepthPointEx startClosest)
        {
            if (points.Count == 0) return points;

            IList<DepthPointEx> newPoints = new List<DepthPointEx>();

            double currentMag = 0;
            double closestMag = double.MaxValue;
            int closestIndex = 0;

            for (int i = 0, count = points.Count; i < count; i++)
            {
                currentMag = points[i].GetMagnitude(startClosest);

                if (currentMag < closestMag)
                {
                    closestMag = currentMag;
                    closestIndex = i;
                }
            }

            newPoints.Add(points[closestIndex]);
            points.RemoveAt(closestIndex);

            DepthPointEx lastPoint;

            while (points.Count > 0)
            {
                closestMag = double.MaxValue;
                closestIndex = 0;
                lastPoint = newPoints[newPoints.Count - 1];

                for (int i = 0, count = points.Count; i < count; i++)
                {
                    currentMag = points[i].GetMagnitude(lastPoint);

                    if (currentMag < closestMag)
                    {
                        closestMag = currentMag;
                        closestIndex = i;
                    }
                }

                if (lastPoint.GetMagnitude(points[closestIndex]) >= 5.0)
                {
                    newPoints.Add(points[closestIndex]);
                }

                points.RemoveAt(closestIndex);
            }

            return newPoints;
        }

        public static IList<DepthPointEx> CutSparePointsAtEnd(this IList<DepthPointEx> points, int depth = 10)
        {
            if (points.Count == 0) return points;

            double currentMag = 0;
            double closestMag = double.MaxValue;
            int closestIndex = 0;

            bool foundClosest = false;

            DepthPointEx closestPoint = points[0];

            for (int i = points.Count - 1; i > 0; i--)
            {
                currentMag = points[i].GetMagnitude(closestPoint);

                if (currentMag < closestMag)
                {
                    closestMag = currentMag;
                    closestIndex = i;
                    foundClosest = true;
                }
                else if (foundClosest)
                {
                    depth--;

                    if (depth <= 0)
                    {
                        break;
                    }
                }
            }

            while (points.Count > closestIndex + 1)
            {
                points.RemoveAt(points.Count - 1);
            }

            return points;
        }
    }
}