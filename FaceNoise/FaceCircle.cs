using Microsoft.ProjectOxford.Face.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceNoise
{
    class FaceCircle
    {
        public double Radius { get; set; }
        public int CenterX { get; set; }
        public int CenterY { get; set; }

        public FaceCircle(FaceRectangle rectangle)
        {
            Radius = Math.Max(rectangle.Height, rectangle.Width) / 2;
            CenterX = (2 * rectangle.Left + rectangle.Width) / 2;
            CenterY = (2 * rectangle.Top + rectangle.Height) / 2;
        }

        private double GetDistanceBetweenPoints(
            int x1, int y1,
            int x2, int y2)
        {
            // Distance formula!
            var deltaX = x2 - x1;
            var deltaY = y2 - y1;
            var distance = Math.Sqrt((deltaX * deltaX) + (deltaY * deltaY));
            return distance;
        }

        // Returns 0 if in circle, or distance otherwise
        public double GetDistanceFromCircle(int x, int y)
        {
            var distanceFromCenter = GetDistanceBetweenPoints(
                x, y, CenterX, CenterY);
            return (distanceFromCenter <= Radius)
                ? 0.0 : distanceFromCenter;
        }

        public bool Contains(int x, int y)
        {
            return (GetDistanceFromCircle(x, y) == 0);
        }

        // Returns the string representation
        public String GetStringRepresentation()
        {
            var str = String.Format("{0} {1} {2}", Radius, CenterX, CenterY);
            return str;
        }
    }
}
