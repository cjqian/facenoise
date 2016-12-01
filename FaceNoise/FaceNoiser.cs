using Microsoft.ProjectOxford.Face.Contract;
using System;
using System.Drawing;

namespace FaceNoise
{
    class FaceNoiser
    {
        private Bitmap _bitmap;

        // 0 for no noise, 0 - 1 in Gaussian blur area, and 1 in face circle area
        private double[][] _noiseChart;
        private double[][] _probabilityChart;

        private Random _randomColor;
        private Random _randomProbability;
        private FaceDetector _faceDetector;

        // Face circles
        private FaceWrapper[] _faceWrappers;

        public FaceNoiser(String fileName)
        {
            _bitmap = new Bitmap("input/" + fileName);

            // Detect faces and make wrappers for each face
            _faceDetector = new FaceDetector(fileName);

            var faces = _faceDetector.GetFaces();

            // Initialize noise and probability charts
            _noiseChart = new double[_bitmap.Height][];
            _probabilityChart = new double[_bitmap.Height][];
            for (var i = 0; i < _noiseChart.Length; i++)
            {
                _noiseChart[i] = new double[_bitmap.Width];
                _probabilityChart[i] = new double[_bitmap.Width];
            }

            _faceWrappers = new FaceWrapper[faces.Length];
            // Populate the noise and probability charts
            for (var i = 0; i < _faceWrappers.Length; i++)
            {
                var faceWrapper = new FaceWrapper(faces[i]);
                for (var j = 0; j < faceWrapper.Square.Height; j++)
                {
                    for (var k = 0; k < faceWrapper.Square.Width; k++)
                    {
                        var curNoise = faceWrapper.NoiseChart[j][k];
                        var curProbability = faceWrapper.ProbabilityChart[j][k];

                        var xPos = k + faceWrapper.Square.X;
                        var yPos = j + faceWrapper.Square.Y;

                        if (xPos < _bitmap.Width && yPos < _bitmap.Height)
                        {
                            var globalNoise = _noiseChart[yPos][xPos];
                            var globalProbability = _probabilityChart[yPos][xPos];

                            _noiseChart[yPos][xPos] = Math.Max(curNoise, globalNoise);
                            _probabilityChart[yPos][xPos] = Math.Max(curNoise, globalNoise);
                        }
                    }
                }

                _faceWrappers[i] = faceWrapper;
            }

            // TODO: Populate a probability chart. 0 for out of bounds, 1 in gaussian,
            // 2 for landmarks.

            // Generate randomness variables,
            // one for color and one for probability.
            int seed = (int)DateTime.Now.Ticks & 0x0000FFFF;
            _randomColor = new Random(seed);
            seed = (int)DateTime.Now.Ticks & 0x0000FFFF;
            _randomProbability = new Random(seed);
        }

        /********************************************
         * Helper methods
         * ******************************************/
        // If out of bounds, we circle around 0, 255.
        private int Wrap(int value)
        {
            /*
            if (value > 255)
            {
                return (value - 255);
            }

            if (value < 0)
            {
                return (value + 255);
            }

            return value;
            */

            return Math.Min(Math.Max(0, value), 255);
        }

        // Returns a color value from -255 to 255
        private double GetRandomColor()
        {
            return 255 * (_randomColor.NextDouble() * 2 - 1);
        }

        // Return a pixel value dependent on pixel and intensity with probability
        private Color GetRandomNoise(Color pixel, double intensity, double probability)
        {
            var p = _randomProbability.NextDouble();
            if (p < probability)
            {
                // 0 will have no change.
                var rChange = (int)(GetRandomColor() * intensity);
                var rValue = Wrap(pixel.R + rChange);

                var gChange = (int)(GetRandomColor() * intensity);
                var gValue = Wrap(pixel.G + gChange);

                var bChange = (int)(GetRandomColor() * intensity);
                var bValue = Wrap(pixel.B + bChange);

                var color = Color.FromArgb(rValue, gValue, bValue);
                return color;
            }

            else
            {
                return pixel;
            }
        }

        public Bitmap Noise(double intensity)
        {
            Bitmap newBitmap = new Bitmap(_bitmap);
            for (int i = 0; i < _noiseChart.Length; i++)
            {
                for (var j = 0; j < _noiseChart[i].Length; j++)
                {
                    var color = newBitmap.GetPixel(j, i);
                    var curIntensity = _noiseChart[i][j] * intensity;
                    var newColor = GetRandomNoise(color, curIntensity, _probabilityChart[i][j]);
                    newBitmap.SetPixel(j, i, newColor);
                }
            }

            return newBitmap;
        }

        public static Bitmap Noise(String file, double intensity)
        {
            var noiser = new FaceNoiser(file);
            return noiser.Noise(intensity);
        }
    }
}
