using Microsoft.ProjectOxford.Face.Contract;
using System;
using System.Drawing;

namespace FaceNoise
{
    class NoiseGenerator
    {
        private Bitmap _bitmap;
        private int _bitsModded;
        private Random _random;
        private FaceDetector _faceDetector;
        private FaceRectangle[] _faceRectangles;

        public NoiseGenerator(String fileName)
        {
            _bitmap = new Bitmap(fileName);
            _bitsModded = 0;
            _faceDetector = new FaceDetector(fileName);
            _random = new Random();
        }

        public int Clamp(int value)
        {
            return Math.Min(255, Math.Max(0, value));
        }

        // From -255 to 255
        public double RandomColorValue()
        {
            return 255 * (_random.NextDouble() * 2 - 1);
        }

        public Color NaiveNoise(Color pixel, double intensity)
        {
            // 0 will have no change.
            var rChange = (int)(RandomColorValue() * intensity);
            var rValue = Clamp(pixel.R + rChange);

            var gChange = (int)(RandomColorValue() * intensity);
            var gValue = Clamp(pixel.G + rChange);

            var bChange = (int)(RandomColorValue() * intensity);
            var bValue = Clamp(pixel.B + rChange);

            var color = Color.FromArgb(rValue, gValue, bValue);

            //Console.WriteLine("Chagne: " + rChange + ", " + gChange + ", " + bChange);  
            _bitsModded += Math.Abs(rChange) + Math.Abs(gChange) + Math.Abs(bChange);
            return color;
        }

        public void MakeNoise(double intensity, double probability)
        {
            for (int i = 0; i < _bitmap.Height; i++)
            {
                for (int j = 0; j < _bitmap.Width; j++)
                {
                    var p = _random.NextDouble();
                    if (p < probability)
                    {
                        var color = NaiveNoise(_bitmap.GetPixel(j, i), intensity);
                        _bitmap.SetPixel(j, i, color);
                    }
                }
            }
        }

        public void MakeFaceNoise(FaceRectangle rectangle, double intensity, double probability)
        {
            for (int i = rectangle.Top; i < rectangle.Top + rectangle.Height; i++)
            {
                for (int j = rectangle.Left; j < rectangle.Left + rectangle.Width; j++)
                {
                    //Console.WriteLine(j + ", " + i);
                    var color = NaiveNoise(_bitmap.GetPixel(j, i), intensity);
                    _bitmap.SetPixel(j, i, color);
                }
            }
        }

        public void MakeFacesNoise(double intensity, double probability)
        {
            _faceRectangles = _faceDetector.UploadAndDetectFaces().GetAwaiter().GetResult();
            Console.WriteLine(_faceRectangles.Length + " faces found.");
            for (int i = 0; i < _faceRectangles.Length; i++)
            {
                MakeFaceNoise(_faceRectangles[i], intensity, probability);
            }
        }

        public void Export(String fileName)
        {
            Console.WriteLine("Bits modded: " + _bitsModded);
            _bitmap.Save(fileName);
        }
    }
}
