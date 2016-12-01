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

        /*
        public Face[] GetBrianFaces()
        {
            var faces = new Face[2];

            var face0 = new Face();
            face0.FaceRectangle = new FaceRectangle();
            face0.FaceRectangle.Height = 81;
            face0.FaceRectangle.Width = 81;
            face0.FaceRectangle.Left = 165;
            face0.FaceRectangle.Top = 249;
            faces[0] = face0;

            var face1 = new Face();
            face1.FaceRectangle = new FaceRectangle();
            face1.FaceRectangle.Height = 72;
            face1.FaceRectangle.Width = 72;
            face1.FaceRectangle.Left = 350;
            face1.FaceRectangle.Top = 254;
            faces[1] = face1;

            return faces;
        }*/

        public Face[] GetFaces()
        {
            if (IsCached()) return GetCachedFaces();
            var faces = UploadAndDetectFaces().GetAwaiter().GetResult();
            CacheFaces(faces);
            return faces;
        }

        public bool IsCached()
        {
            var file = Directory.GetFiles("cache/", _filePath + ".txt", SearchOption.AllDirectories).FirstOrDefault();
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

            var path = "cache/" + _filePath + ".txt";
            var text = sb.ToString();
            File.WriteAllText(path, text);
        }

        /*
        public Face[] GetLawnPartiesFaces()
        {
            var faces = new Face[5];

            var face0 = new Face();
            face0.FaceRectangle = new FaceRectangle();
            face0.FaceRectangle.Height = 141;
            face0.FaceRectangle.Width = 141;
            face0.FaceRectangle.Left = 708;
            face0.FaceRectangle.Top = 199;
            faces[0] = face0;

            var face1 = new Face();
            face1.FaceRectangle = new FaceRectangle();
            face1.FaceRectangle.Height = 122;
            face1.FaceRectangle.Width = 122;
            face1.FaceRectangle.Left = 148;
            face1.FaceRectangle.Top = 233;
            faces[1] = face1;

            var face2 = new Face();
            face2.FaceRectangle = new FaceRectangle();
            face2.FaceRectangle.Height = 108;
            face2.FaceRectangle.Width = 108;
            face2.FaceRectangle.Left = 322;
            face2.FaceRectangle.Top = 217;
            faces[2] = face2;

            var face3 = new Face();
            face3.FaceRectangle = new FaceRectangle();
            face3.FaceRectangle.Height = 108;
            face3.FaceRectangle.Width = 108;
            face3.FaceRectangle.Left = 322;
            face3.FaceRectangle.Top = 217;
            faces[3] = face3;

            var face4 = new Face();
            face4.FaceRectangle = new FaceRectangle();
            face4.FaceRectangle.Height = 73;
            face4.FaceRectangle.Width = 73;
            face4.FaceRectangle.Left = 249;
            face4.FaceRectangle.Top = 190;
            faces[4] = face4;

            return faces;
        }*/

        public async Task<Face[]> UploadAndDetectFaces()
        {
            try
            {
                using (Stream imageFileStream = File.OpenRead(_filePath))
                {
                    var faces = await _faceServiceClient.DetectAsync(imageFileStream);
                    Console.WriteLine(faces.Length + " faces found.");
                    return faces.ToArray();
                }
            }
            catch (Exception)
            {
                Console.WriteLine("NO faces found.");
                return new Face[0];
            }
        }
    }
}
