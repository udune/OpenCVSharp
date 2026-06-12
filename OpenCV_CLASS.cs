using System;
using System.IO;
using OpenCvSharp;

namespace OpenCVSharp
{
    public class OpenCV_CLASS : IDisposable
    {
        private IplImage _srcImage;
        private IplImage _grayImage;

        public IplImage SrcImage => _srcImage;
        public IplImage GrayImage => _grayImage;

        public void LoadImage(string filePath)
        {
            // Release existing source image if loaded
            if (_srcImage != null)
            {
                Cv.ReleaseImage(_srcImage);
                _srcImage = null;
            }

            // Handle non-ASCII path (OpenCV 2.4 limit on Windows)
            string pathToOpen = filePath;
            foreach (char c in filePath)
            {
                if (c > 127)
                {
                    string ext = Path.GetExtension(filePath);
                    string tempPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "temp_image_ch06" + ext);
                    if (File.Exists(tempPath)) File.Delete(tempPath);
                    File.Copy(filePath, tempPath, true);
                    pathToOpen = tempPath;
                    break;
                }
            }
            pathToOpen = pathToOpen.Replace('\\', '/');

            _srcImage = new IplImage(pathToOpen, LoadMode.Color);
        }

        public void ConvertToGray()
        {
            if (_srcImage == null)
                throw new InvalidOperationException("원본 이미지가 로드되지 않았습니다.");

            // Release existing gray image if created
            if (_grayImage != null)
            {
                Cv.ReleaseImage(_grayImage);
                _grayImage = null;
            }

            // Create 8-bit single channel (Grayscale) image
            _grayImage = new IplImage(_srcImage.Size, BitDepth.U8, 1);
            Cv.CvtColor(_srcImage, _grayImage, ColorConversion.BgrToGray);
        }

        public void Dispose()
        {
            // Safe release of native memory to prevent memory leaks
            if (_srcImage != null)
            {
                Cv.ReleaseImage(_srcImage);
                _srcImage = null;
            }
            if (_grayImage != null)
            {
                Cv.ReleaseImage(_grayImage);
                _grayImage = null;
            }
        }
    }
}
