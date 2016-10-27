using Microsoft.ProjectOxford.Face.Contract;
using System;
using System.Drawing;

namespace FaceNoise
{
    class FaceDenoiser
    {
        private Bitmap _bitmap;
        private double _intensity;
        private Random _random;
        private FaceRectangle[] _faceRectangles;

        public FaceDenoiser(
            int seed, 
            Bitmap bitmap, 
            double intensity, 
            FaceRectangle[] faceRectangles)
        {
            _random = new Random(seed);
            _bitmap = bitmap;
            _intensity = intensity;
            _faceRectangles = faceRectangles;
        }

        // From -255 to 255
        public double RandomColorValue()
        {
            return 255 * (_random.NextDouble() * 2 - 1);
        }

        public int Wrap(int value)
        {
            if (value > 255)
            {
                return (value - 255);
            }

            if (value < 0)
            {
                return (value + 255);
            }

            return value;
        }

        public Color GetUnRandomNoise(Color pixel)
        {
            var rChange = (int)(RandomColorValue() * _intensity);
            var rValue = Wrap(pixel.R - rChange);

            var gChange = (int)(RandomColorValue() * _intensity);
            var gValue = Wrap(pixel.G - gChange);

            var bChange = (int)(RandomColorValue() * _intensity);
            var bValue = Wrap(pixel.B - bChange);

            var color = Color.FromArgb(rValue, gValue, bValue);
            return color;
        }

        public Bitmap Denoise()
        {
            var newBitmap = new Bitmap(_bitmap);

            for (int i = 0; i < _faceRectangles.Length; i++)
            {
                var rectangle = _faceRectangles[i];

                for (int j = rectangle.Top; j < rectangle.Top + rectangle.Height; j++)
                {
                    for (int k = rectangle.Left; k < rectangle.Left + rectangle.Width; k++)
                    {
                        var pixel = _bitmap.GetPixel(k, j);
                        var color = GetUnRandomNoise(pixel);
                        newBitmap.SetPixel(k, j, color);
                    }
                }
            }

            return newBitmap;
        }

        public static Bitmap Denoise(String file)
        {
            var b = new Bitmap(file);
            return Denoise(b);
        }

        public static Bitmap Denoise(Bitmap b)
        {
            // First, get the text.
            var text = Steganographer.extractText(b);
            var textArray = text.Split();
            Console.WriteLine("Extracted text: " + text);

            // Parse the text for rectangles.
            var idx = 0;
            var n = Int32.Parse(textArray[idx++]);

            var faceRectangles = new FaceRectangle[n]; 
            for (int i = 0; i < n; i++)
            {
                var faceRectangle = new FaceRectangle();
                faceRectangle.Top = Int32.Parse(textArray[idx++]);
                faceRectangle.Height = Int32.Parse(textArray[idx++]);
                faceRectangle.Left = Int32.Parse(textArray[idx++]);
                faceRectangle.Width = Int32.Parse(textArray[idx++]);

                faceRectangles[i] = faceRectangle;
            }

            // Then, parse the text for the seed.
            int seed = Int32.Parse(textArray[idx++]);
            double intensity = Double.Parse(textArray[idx]);

            var denoiser = new FaceDenoiser(seed, b, intensity, faceRectangles);
            var decryptedB = denoiser.Denoise();
            return decryptedB;
        }
    }
}
