using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ProjectOxford.Face.Contract;
using System.IO;
using Microsoft.ProjectOxford.Face;

namespace FaceNoise
{
    // TODO: Gaussian blur noise to make it more smooth
    // TODO: Deterministic blur w/ seed.
    class FaceDetector
    {
        private readonly IFaceServiceClient _faceServiceClient = new FaceServiceClient("6d686c7882a04cb3bee40d2ba944ff05");
        private string _filePath;
        public FaceDetector(string filePath)
        {
            _filePath = filePath;
        }

        public async Task<Face[]> UploadAndDetectFaces()
        {
            try
            {
                using (Stream imageFileStream = File.OpenRead(_filePath))
                {
                    var faces = await _faceServiceClient.DetectAsync(imageFileStream);
                    return faces.ToArray();
                }
            }
            catch (Exception)
            {
                return new Face[0];
            }
        }
    }
}
