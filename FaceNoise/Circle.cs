using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceNoise
{
    class Circle
    {
        public Point Center {get; set;}
        public double Radius { get; set; }

        public Circle(Point c, double r)
        {
            Center = c;
            Radius = r;
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
                x, y, Center.X, Center.Y);
            return (distanceFromCenter <= Radius)
                ? 0.0 : (distanceFromCenter - Radius);
        }

        public bool Contains(int x, int y)
        {
            return (GetDistanceFromCircle(x, y) == 0);
        }
    }
}
