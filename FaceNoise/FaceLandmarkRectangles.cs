using Microsoft.ProjectOxford.Face.Contract;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceNoise
{
    class FaceLandmarkRectangles
    {
        public bool ContainsLandmarks;
        public Rectangle Eye;
        public Rectangle Nose;
        public Rectangle Mouth;

        public FaceLandmarkRectangles(Face face)
        {
            var landmarks = face.FaceLandmarks;
            if (landmarks == null)
            {
                ContainsLandmarks = false;
                return;
            }

            ContainsLandmarks = true;

            // Set eye
            Eye = MakeRectangle(landmarks.EyeLeftTop, landmarks.EyeRightBottom);
            Nose = MakeRectangle(landmarks.NoseLeftAlarTop, landmarks.NoseRootRight);

            var mouthX = (int)landmarks.MouthLeft.X;
            var mouthY = (int)landmarks.UpperLipTop.Y;
            var mouthWidth = (int)landmarks.MouthRight.X - mouthX;
            var mouthHeight = (int)landmarks.UnderLipBottom.Y - mouthY;
            Mouth = new Rectangle(mouthX, mouthY, mouthWidth, mouthHeight);
        }

        private Rectangle MakeRectangle(FeatureCoordinate coord1, FeatureCoordinate coord2)
        {
            var x = (int)coord1.X;
            var y = (int)coord1.Y;
            var width = (int)coord2.X - x;
            var height = (int)coord2.Y - y;

            return new Rectangle(x, y, width, height);
        }
    }
}
