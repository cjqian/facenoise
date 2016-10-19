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
        private string _filePath;
        public FaceDetector(string filePath)
        {
            _filePath = filePath;
        }

        public async Task<FaceRectangle[]> UploadAndDetectFaces()
        {
            try
            {
                using (Stream imageFileStream = File.OpenRead(_filePath))
                {
                    var faces = await _faceServiceClient.DetectAsync(imageFileStream);
                    var facesList = faces.Select(face => face.FaceRectangle);
                    return facesList.ToArray();
                }
            }
            catch (Exception)
            {
                return new FaceRectangle[0];
            }
        }
    }
}
