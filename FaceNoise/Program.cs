using Microsoft.ProjectOxford.Face.Contract;
using System;
using System.Diagnostics;
using System.Drawing;

namespace FaceNoise
{
    class Program
    {
        static void Main(string[] args)
        {
            //Debugger.Launch();


            Debugger.Launch();
            var importFile = args[0];
            var intensity = Double.Parse(args[1]);

            var faceNoiser = new FaceNoiser(importFile);

            var b = faceNoiser.Noise(intensity);
            var t = FaceDenoiser.Denoise(b);
            t.Save("did.jpg");
            Console.WriteLine("Denoised!");
        }
    }
}
