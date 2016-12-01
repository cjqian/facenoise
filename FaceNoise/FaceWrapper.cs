using Microsoft.ProjectOxford.Face.Contract;
using System;
using System.Drawing;

namespace FaceNoise
{
    class FaceWrapper
    {
        // GLOBAL VALUES:
        private double GAUSSIAN_MULTIPLIER = 1.5;
        private double NORMAL_PROBABILITY = .6;
        private double LANDMARK_PROBABILITY = .8;

        // 0 when not in circle, 0 - 1 in Gaussian radius, 1 for in face.
        public double[][] NoiseChart { get; set; }

        // 0 for not in circle, NORMAL for in face, LANDMARK for landmark
        public double[][] ProbabilityChart { get; set; }

        public Rectangle Square { get; set; }
        public Circle FaceCircle { get; set; }
        public Circle GaussianCircle { get; set; }
        public FaceWrapper(Face face)
        {
            // Make the things
            var rectangle = face.FaceRectangle;

            var diameter = Math.Max(rectangle.Height, rectangle.Width);
            var radius = diameter / 2;

            var centerX = (2 * rectangle.Left + rectangle.Width) / 2;
            var centerY = (2 * rectangle.Top + rectangle.Height) / 2;
            var center = new Point(centerX, centerY);

            FaceCircle = new Circle(center, radius);

            var gaussianRadius = radius * GAUSSIAN_MULTIPLIER;
            GaussianCircle = new Circle(center, gaussianRadius);

            var startX = centerX - gaussianRadius;
            var startY = centerY - gaussianRadius;
            var height = 2 * gaussianRadius;
            var width = 2 * gaussianRadius;

            Square = new Rectangle((int)startX, (int)startY, (int)width, (int)height);

            // Populate the noise chart
            NoiseChart = new double[Square.Height][];
            ProbabilityChart = new double[Square.Height][];

            for (var i = 0; i < Square.Height; i++)
            {
                NoiseChart[i] = new double[Square.Width];
                ProbabilityChart[i] = new double[Square.Width];
            }

            for (var i = 0; i < Square.Height; i++)
            {
                for (var j = 0; j < Square.Width; j++)
                {
                    NoiseChart[i][j] = GetNoiseFactor(
                        j + Square.Left,
                        i + Square.Top);

                    // Probability is always 1.
                    ProbabilityChart[i][j] = 1.0;
                }
            }
        }

        // Returns 1 if in circle, 0 if not, some value inbetween if in gaussian
        private double GetNoiseFactor(int x, int y)
        {
            if (FaceCircle.Contains(x, y)) return 1.0;
            else if (GaussianCircle.Contains(x, y))
            {
                var curPoint = new Point(x, y);
                var distanceFromCircle = DistanceBetweenPoints(GaussianCircle.Center, curPoint) - FaceCircle.Radius;

                // Scale it
                var scaleValue = GaussianCircle.Radius - FaceCircle.Radius;

                var result =  1.0 - (distanceFromCircle / scaleValue);
                return result;
            }

            return 0.0;
        }

        // Distance between two points
        private double DistanceBetweenPoints(Point a, Point b)
        {
            return Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));
        }
    }
}
