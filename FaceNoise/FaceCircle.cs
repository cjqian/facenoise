using Microsoft.ProjectOxford.Face.Contract;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceNoise
{
    class FaceCircle
    {
        public int Radius { get; set; }
        public Point Center { get; set; }

        public Rectangle BoundingSquare { get; set; }

        // Also, a 2D array of locations 
        public Point Start { get; set; }
        public Guide[][] LocationGuide { get; set; }

        public FaceLandmarkRectangles Features { get; set; }

        public enum Guide:uint
        {
            NOT_PRESENT = 0,
            FACE = 1,

            // Observation: DeepFace targets 6 fiducial points:
            // center of eyes, tip of nose, and mouth.
            EYES = 2,
            NOSE = 3,
            MOUTH = 4
        }

        public FaceCircle(Face face)
        {
            var rectangle = face.FaceRectangle;
            var diameter = Math.Max(rectangle.Height, rectangle.Width);
            var centerX = (2 * rectangle.Left + rectangle.Width) / 2;
            var centerY = (2 * rectangle.Top + rectangle.Height) / 2;

            Radius = diameter / 2;
            Center = new Point(centerX, centerY);
            BoundingSquare = new Rectangle(centerX - Radius, centerY - Radius, diameter, diameter);
            Start = new Point(rectangle.Left, rectangle.Top);
            Features = new FaceLandmarkRectangles(face);

            // Set up the location guide
            LocationGuide = new Guide[diameter][];
            for (var i = 0; i < LocationGuide.Length; i++)
            {
                LocationGuide[i] = new Guide[diameter];
                for (var j = 0; j < LocationGuide[i].Length; j++)
                {
                    // Check out of bounds
                    if (!this.Contains(j + BoundingSquare.X, i + BoundingSquare.Y))
                    {
                        LocationGuide[i][j] = Guide.NOT_PRESENT;
                    }

                    else if (!Features.ContainsLandmarks)
                    {
                        LocationGuide[i][j] = Guide.FACE;
                    }
                    else
                    {
                        if (Features.Eye.Contains(j, i))
                        {
                            LocationGuide[i][j] = Guide.EYES;
                        }
                        else if (Features.Nose.Contains(j, i))
                        {
                            LocationGuide[i][j] = Guide.NOSE;
                        }
                        else if (Features.Mouth.Contains(j, i))
                        {
                            LocationGuide[i][j] = Guide.MOUTH;
                        }
                        else
                        {
                            LocationGuide[i][j] = Guide.FACE;
                        }
                    }
                }
            }
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
                ? 0.0 : distanceFromCenter; 
        }

        public bool Contains(int x, int y)
        {
            return (GetDistanceFromCircle(x, y) == 0);
        }

        // Returns the string representation
        public String GetStringRepresentation()
        {
            var str = String.Format("{0} {1} {2}", Radius, Center.X, Center.Y);
            return str;
        }
    }
}
