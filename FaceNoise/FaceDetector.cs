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
    // TODO: Gaussian blur noise for smoothing reasons
    // TODO: Deterministic blur w/ seed.
    class FaceDetector
    {
        private readonly IFaceServiceClient _faceServiceClient = new FaceServiceClient("6d686c7882a04cb3bee40d2ba944ff05");

        private string _filePath;
        private string _fileCached;

        public FaceDetector(string filePath)
        {
            _filePath = filePath;
        }

        public Face[] GetFaces()
        {
            if (!IsCached())
            {
            var faces = UploadAndDetectFaces().GetAwaiter().GetResult();
            CacheFaces(faces);
            }

            return GetCachedFaces();
        }

        public bool IsCached()
        {
            var file = Directory.GetFiles("cache/", _filePath + "-faces.txt", SearchOption.AllDirectories).FirstOrDefault();
            if (file == null) return false;

            _fileCached = File.ReadAllText(file);
            return true;
        }

        public Face[] GetCachedFaces()
        {
            var tokens = _fileCached.Split();

            int nFaces = Int32.Parse(tokens[0]);
            var faces = new Face[nFaces];

            for (int i = 0; i < nFaces; i++)
            {
                var face = new Face();
                face.FaceRectangle = new FaceRectangle();

                face.FaceRectangle.Height = Int32.Parse(tokens[i * 4 + 1]);
                face.FaceRectangle.Width = Int32.Parse(tokens[i * 4 + 2]);
                face.FaceRectangle.Left = Int32.Parse(tokens[i * 4 + 3]);
                face.FaceRectangle.Top = Int32.Parse(tokens[i * 4 + 4]);

                faces[i] = face;
            }

            return faces;
        }

        public void CacheFaces(Face[] faces)
        {
            var sb = new StringBuilder();
            sb.Append(faces.Length + " ");

            for (int i = 0; i < faces.Length; i++)
            {
                var rectangle = faces[i].FaceRectangle;
                sb.Append(rectangle.Height + " " 
                    + rectangle.Width + " " 
                    + rectangle.Left + " " 
                    + rectangle.Top + " ");
            }

            var path = "cache/" + _filePath + "-faces.txt";
            var text = sb.ToString();
            File.WriteAllText(path, text);
        }

        public async Task<Face[]> UploadAndDetectFaces()
        {
            try
            {
                using (Stream imageFileStream = File.OpenRead("input/" + _filePath))
                {
                    var faces = await _faceServiceClient.DetectAsync(imageFileStream);
                    Console.WriteLine("Detection called: " + faces.Length + " faces found.");
                    return faces.ToArray();
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Detection called: No faces found.");
                return new Face[0];
            }
        }
    }
}
