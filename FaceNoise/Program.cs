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
                Console.WriteLine("Encrypting " + file + ", perturbation " + intensity);
                var output_file = "output/" + intensity + "/" + file;
                var b = FaceNoiser.Noise(file, intensity);
                b.Save(output_file);
            }
        }
    }
}
