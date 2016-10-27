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
        public Circle BoundingCircle { get; set; }

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

            var radius = diameter / 2;
            var center = new Point(centerX, centerY);
            BoundingCircle = new FaceNoise.Circle(center, radius);
            BoundingSquare = new Rectangle(centerX - radius, centerY - radius, diameter, diameter);
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
                    if (!BoundingCircle.Contains(j + BoundingSquare.X, i + BoundingSquare.Y))
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

        // Returns the string representation
        public String GetStringRepresentation()
        {
            var str = String.Format("{0} {1} {2}", BoundingCircle.Radius, BoundingCircle.Center.X, BoundingCircle.Center.Y);
            return str;
        }
    }
}
