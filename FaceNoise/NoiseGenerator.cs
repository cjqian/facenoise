using System;
using System.Drawing;

namespace FaceNoise
{
    class NoiseGenerator
    {
        private Bitmap _bitmap;
        private Random _random;
        public NoiseGenerator(String fileName)
        {
            _bitmap = new Bitmap(fileName);
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
            var rChange = (int) (RandomColorValue() * intensity);
            var rValue = Clamp(pixel.R + rChange);

            var gChange = (int) (RandomColorValue() * intensity);
            var gValue = Clamp(pixel.G + rChange);

            var bChange = (int) (RandomColorValue() * intensity);
            var bValue = Clamp(pixel.B + rChange);

            var color = Color.FromArgb(rValue, gValue, bValue);
            return color;
        }

        public void MakeNoise(double intensity)
        {
            for (int i = 0; i < _bitmap.Height; i++)
            {
                for (int j = 0; j < _bitmap.Width; j++)
                {
                    var color = NaiveNoise(_bitmap.GetPixel(j, i), intensity);
                    _bitmap.SetPixel(j, i, color);
                }
            }
        }

        public void Export(String fileName)
        {
            _bitmap.Save(fileName);
        }
    }
}
