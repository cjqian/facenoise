using Microsoft.ProjectOxford.Face.Contract;
using System;
using System.Diagnostics;

namespace FaceNoise
{
    class Program
    {
        static void Main(string[] args)
        {
            //Debugger.Launch();

            /*
            var importFile = args[0];
            var exportFile = args[1];
            var intensity = Double.Parse(args[2]);
            var probability = Double.Parse(args[3]);
            */

            var importFile = "pictures/lp0.jpg";
            var exportFile = "pictures/test.jpg";
            var intensity = .12;
            var probability = .5;

            var noiseGenerator = new NoiseGenerator(importFile);
            noiseGenerator.MakeFacesNoise(intensity, probability);
            noiseGenerator.Export(exportFile);
        }
    }
}
