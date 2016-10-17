using System;
namespace FaceNoise
{
    class Program
    {
        static void Main(string[] args)
        {
            var importFile = args[0];
            var exportFile = args[1];
            var intensity = Double.Parse(args[2]);

            var noiseGenerator = new NoiseGenerator(importFile);
            noiseGenerator.MakeNoise(intensity);
            noiseGenerator.Export(exportFile);
        }
    }
}
