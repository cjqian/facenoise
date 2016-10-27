using Microsoft.ProjectOxford.Face.Contract;
using System;
using System.Drawing;
using System.Linq;

namespace FaceNoise
{
    class FaceNoiser
    {
        // Probabilities:
        private double NORMAL_PROBABILITY = .6;
        private double LANDMARK_PROBABILITY = .8;
        private double GAUSSIAN_RADIUS = 2;

        private String _seedColor;
        private String _seedProbability;
        private string _inputName;

        private Bitmap _bitmap;
        private int _bitsModded;
        private Random _randomColor;
        private Random _randomProbability;
        private FaceDetector _faceDetector;

        // Face circles
        private Face[] _faces;
        private FaceCircle[] _faceCircles;

        public FaceNoiser(String fileName)
        {
            _inputName = fileName;
            _bitmap = new Bitmap(fileName);
            _bitsModded = 0;

            _faceDetector = new FaceDetector(fileName);
            _faces = _faceDetector.UploadAndDetectFaces().GetAwaiter().GetResult();
            Console.WriteLine(_faces.Length + " faces found.");

            // Make the face circles
            _faceCircles = new FaceCircle[_faces.Length];
            for (var i = 0; i < _faceCircles.Length; i++)
            {
                _faceCircles[i] = new FaceCircle(_faces[i]);
                Console.WriteLine(_faceCircles[i].GetStringRepresentation());
            }

            // Generate randomness variables
            int seed = (int)DateTime.Now.Ticks & 0x0000FFFF;
            _seedColor = seed.ToString();
            _randomColor = new Random(seed);

            seed = (int)DateTime.Now.Ticks & 0x0000FFFF;
            _seedProbability = seed.ToString();
            _randomProbability = new Random();
        }

        public string GetExportName(double intensity)
        {
            var parts = _inputName.Split('.');
            var name = parts[0] + "-" + intensity + "." + parts[1];
            Console.WriteLine("Exporting to " + name);
            return name;
        }

        private string GetEncryptedText()
        {
            /*
            var text = GetRectangleString() + _seedColor;
            return text;
            */
            return "";
        }

        public String GetRectangleString(FaceRectangle rectangle)
        {
            // Returns something of the format:
            // 4 height width left top height width left top ....
            var rectString = String.Format("{0} {1} {2} {3}",
                rectangle.Top,
                rectangle.Height,
                rectangle.Left,
                rectangle.Width);

            return rectString;
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

        // From -255 to 255
        public double RandomColorValue()
        {
            return 255 * (_randomColor.NextDouble() * 2 - 1);
        }

        public Color GetRandomNoise(Color pixel, double intensity, double probability)
        {
            var p = _randomProbability.NextDouble();

            if (p < probability) return GetRandomNoise(pixel, intensity);
            return pixel;
        }

        public Color GetRandomNoise(Color pixel, double intensity)
        {
            // 0 will have no change.
            var rChange = (int)(RandomColorValue() * intensity);
            var rValue = Wrap(pixel.R + rChange);

            var gChange = (int)(RandomColorValue() * intensity);
            var gValue = Wrap(pixel.G + gChange);

            var bChange = (int)(RandomColorValue() * intensity);
            var bValue = Wrap(pixel.B + bChange);

            var color = Color.FromArgb(rValue, gValue, bValue);

            _bitsModded += Math.Abs(rChange) + Math.Abs(gChange) + Math.Abs(bChange);
            return color;
        }

        // Make noise within circle
        public void MakeFaceCircleNoise(Bitmap newBitmap, FaceCircle circle, double intensity)
        {
            var xOffset = circle.BoundingSquare.X;
            var yOffset = circle.BoundingSquare.Y;

            // First pass makes noise within the face
            for (var i = 0; i < circle.LocationGuide.Length; i++)
            {
                for (var j = 0; j < circle.LocationGuide[i].Length; j++)
                {
                    var x = xOffset + j;
                    var y = yOffset + i;

                    if (InBounds(x, y))
                    {
                        var color = _bitmap.GetPixel(x, y);
                        var guide = circle.LocationGuide[i][j];
                        Color newColor;
                        switch (guide)
                        {
                            case FaceCircle.Guide.NOT_PRESENT:
                                newColor = color;
                                break;
                            case FaceCircle.Guide.FACE:
                                newColor = GetRandomNoise(color, intensity, NORMAL_PROBABILITY);
                                break;
                            // Default: is a landmark
                            default:
                                newColor = GetRandomNoise(color, intensity, LANDMARK_PROBABILITY);
                                break;
                        }

                        newBitmap.SetPixel(x, y, newColor);
                    }
                }
            }

            // Second pass makes gaussian noise
            var gaussianRadius = (int) (circle.BoundingCircle.Radius * GAUSSIAN_RADIUS);
            var gaussianCircle = new Circle(circle.BoundingCircle.Center, gaussianRadius);

            for (var i = circle.BoundingCircle.Center.Y - gaussianRadius; 
                i < circle.BoundingCircle.Center.Y + gaussianRadius; i++)
            {
                for (var j = circle.BoundingCircle.Center.X - gaussianRadius;
                    j < circle.BoundingCircle.Center.X + gaussianRadius; j++)
                {
                    if (InBounds(j, i) 
                        && !circle.BoundingCircle.Contains(j, i)
                        && gaussianCircle.Contains(j, i))
                    {
                        var constant = GetGaussianConstant(circle, gaussianRadius, j, i);
                        var color = _bitmap.GetPixel(j, i);
                        var newColor = GetRandomNoise(color, intensity * constant, NORMAL_PROBABILITY);
                        newBitmap.SetPixel(j, i, newColor);
                    }
                }
            }
        }

        public double GetGaussianConstant(FaceCircle circle, int gaussianRadius, int x, int y)
        {
            var distance = circle.BoundingCircle.GetDistanceFromCircle(x, y);
            var maxDistance = gaussianRadius - circle.BoundingCircle.Radius;
            var exponent = -1 * (distance / maxDistance);

            // .367 to 1
            double constant = Math.Exp(exponent);
            return constant;
        }

        // Make noise in the image around detected faces
        public Bitmap MakeFacesNoise(double intensity)
        {
            Bitmap newBitmap = new Bitmap(_bitmap);
            for (int i = 0; i < _faceCircles.Length; i++)
            {
                FaceCircle circle = _faceCircles[i];
                MakeFaceCircleNoise(newBitmap, circle, intensity);
            }

            Console.WriteLine(_bitsModded + " bits modded.");
            return newBitmap;
        }

        // Noise and save.
        public Bitmap Noise(double intensity)
        {
            Bitmap b = MakeFacesNoise(intensity);
/*
            var encryptedText = GetEncryptedText() + " " + intensity;
            Bitmap encryptedB = Steganographer.embedText(encryptedText, b);
            var exportName = GetExportName(intensity);
            Console.WriteLine("Written to " + exportName);
            encryptedB.Save(exportName);

            return encryptedB;
            */
            return b;
        }

        public bool InBounds(int x, int y)
        {
            if (x < 0 || y < 0) return false;
            if (x >= _bitmap.Width || y >= _bitmap.Height) return false;
            return true;
        }


        public bool InBounds(Point point)
        {
            if (point.X < 0 || point.Y < 0) return false;
            if (point.X >= _bitmap.Width || point.Y >= _bitmap.Height) return false;
            return true;
        }
    }
}
