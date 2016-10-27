﻿using Microsoft.ProjectOxford.Face.Contract;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceNoise
{
    class FaceNoiser
    {
        private String _seed;
        private string _inputName;

        private Bitmap _bitmap;
        private int _bitsModded;
        private Random _random;
        private FaceDetector _faceDetector;
        private FaceRectangle[] _faceRectangles;
        private FaceCircle[] _faceCircles;
        
        public FaceNoiser(String fileName)
        {
            _inputName = fileName;
            _bitmap = new Bitmap(fileName);
            _bitsModded = 0;

            // Make the face circles
            _faceDetector = new FaceDetector(fileName);
            _faceRectangles = _faceDetector.UploadAndDetectFaces().GetAwaiter().GetResult();
            Console.WriteLine(_faceRectangles.Length + " faces found.");
            _faceCircles = new FaceCircle[_faceRectangles.Length];
            for (var i = 0; i < _faceCircles.Length; i++)
            {
                _faceCircles[i] = new FaceCircle(_faceRectangles[i]);
                var rectangle = _faceRectangles[i];
                var rectString = String.Format("{0} {1} {2} {3}",
                    rectangle.Top,
                    rectangle.Height,
                    rectangle.Left,
                    rectangle.Width);

                Console.WriteLine(rectString);
                Console.WriteLine(_faceCircles[i].GetStringRepresentation());
            }

            // Random seed based on time.
            int seed = (int)DateTime.Now.Ticks & 0x0000FFFF;
            Console.WriteLine("Random seed is: " + seed);
            _seed = seed.ToString();
            _random = new Random(seed);
        }

        private string GetExportName(double intensity)
        {
            var parts = _inputName.Split('.');
            var name = parts[0] + "-" + intensity + "." + parts[1];
            Console.WriteLine("Exporting to " + name);
            return name;
        }

        private string GetEncryptedText()
        {
            /*
            var text = GetRectangleString() + _seed;
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
            return 255 * (_random.NextDouble() * 2 - 1);
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

        // Make noise in the image around detected faces
        public Bitmap MakeFacesNoise(double intensity)
        {
            Bitmap newBitmap = new Bitmap(_bitmap);
            for (int i = 0; i < _faceCircles.Length; i++)
            {
                FaceCircle circle = _faceCircles[i];
                for (var j = (int)(circle.CenterY - circle.Radius); 
                    j < (int)(circle.CenterY + circle.Radius); j++)
                {
                    for (var k = (int)(circle.CenterX - circle.Radius); 
                        k < (int)(circle.CenterX + circle.Radius); k++)
                    {
                        if (circle.Contains(k, j))
                        {
                            var color = GetRandomNoise(_bitmap.GetPixel(k, j), intensity);
                            newBitmap.SetPixel(k, j, color);
                        }
                    }
                }
            }

            Console.WriteLine(_bitsModded + " bits modded.");
            return newBitmap;
        }

        // Noise and save.
        public Bitmap Noise(double intensity)
        {
            Bitmap b = MakeFacesNoise(intensity);

            var encryptedText = GetEncryptedText() + " " + intensity;
            Bitmap encryptedB = Steganographer.embedText(encryptedText, b);
            var exportName = GetExportName(intensity);
            Console.WriteLine("Written to " + exportName);
            encryptedB.Save(exportName);

            return encryptedB;
        }
    }
}
