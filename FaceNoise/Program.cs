using System;
using System.Diagnostics;

namespace FaceNoise
{
    class Program
    {
        static void Main(string[] args)
        {
           Debugger.Launch();

            var type = args[0];
            var file = args[1];
            var output_file = "output/" + file;

            // -e is to encrypt
            if (type.Equals("-e"))
            {
                Double intensity = Double.Parse(args[2]);
                var b = FaceNoiser.Noise(file, intensity);
                b.Save(output_file);
            }

            // -d is to decrypt
            else if (type.Equals("-d"))
            {
                var b = FaceDenoiser.Denoise(file);
                b.Save(output_file);
            }
        }
    }
}
