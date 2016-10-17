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
    class FaceDetector
    {
        private readonly IFaceServiceClient _faceServiceClient = new FaceServiceClient("apikey");
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
