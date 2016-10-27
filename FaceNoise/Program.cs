using System;
using System.Diagnostics;

namespace FaceNoise
{
    class Program
    {
        static void Main(string[] args)
        {
           // Debugger.Launch();

            var type = args[0];
            var file = args[1];

            // -e is to encrypt
            if (type.Equals("-e"))
            {
                Double intensity = Double.Parse(args[2]);
                var faceNoiser = new FaceNoiser(file);
                faceNoiser.Noise(intensity);
            }

            // -d is to decrypt
            else if (type.Equals("-d"))
            {
                var outputFile = args[2];
                var b = FaceDenoiser.Denoise(file);
                b.Save(outputFile);
            }
        }
    }
}
