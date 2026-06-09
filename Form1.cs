using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace OpenCVSharp
{
    public partial class Form1 : Form
    {
        private CvCapture capture;
        private readonly Stopwatch fpsWatch = new Stopwatch();
        private int frameCounter;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            cmbSizeMode.Items.Clear();
            cmbSizeMode.Items.AddRange(new object[] { "0", "1", "2", "3" });
            cmbSizeMode.SelectedIndex = 0;
            renderTimer.Interval = (int)nudInterval.Value;
            SetRunningState(false, "상태: 대기");
            lblMeasuredFps.Text = "측정 FPS: 0";
            UpdateFormulaText();
        }

        private void nudInterval_ValueChanged(object sender, EventArgs e)
        {
            renderTimer.Interval = (int)nudInterval.Value;
            UpdateFormulaText();
        }

        private void renderTimer_Tick(object sender, EventArgs e)
        {
            if (capture == null)
            {
                return;
            }

            using (var frame = capture.QueryFrame())
            {
                if (frame == null)
                {
                    return;
                }

                frameCounter++;
                var old = pictureBoxIpl1.Image;
                pictureBoxIpl1.Image = BitmapConverter.ToBitmap(frame);
                old?.Dispose();
            }

            if (fpsWatch.ElapsedMilliseconds >= 1000)
            {
                lblMeasuredFps.Text = $"측정 FPS: {frameCounter}";
                frameCounter = 0;
                fpsWatch.Restart();
            }
        }

        private void UpdateFormulaText()
        {
            var interval = (int)nudInterval.Value;
            var calcFps = 1000.0 / interval;
            lblFormula.Text = $"FPS 계산: 1000 / {interval} = {calcFps:F1}";
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            try
            {
                StopCamera();
                var cameraIndex = cmbSizeMode.SelectedIndex;
                capture = CvCapture.FromCamera(cameraIndex);
                if (capture == null)
                {
                    SetRunningState(false, "상태: 카메라 연결 실패");
                    return;
                }

                frameCounter = 0;
                fpsWatch.Restart();
                renderTimer.Start();
                SetRunningState(true, $"상태: 실행 중 (Camera {cameraIndex})");
            }
            catch (Exception ex)
            {
                SetRunningState(false, "상태: 시작 오류");
                MessageBox.Show(ex.Message, "Camera Start Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            StopCamera();
        }

        private void StopCamera()
        {
            renderTimer.Stop();

            if (capture != null)
            {
                capture.Dispose();
                capture = null;
            }

            SetRunningState(false, "상태: 중지");
        }

        private void SetRunningState(bool running, string status)
        {
            btnStart.Enabled = !running;
            btnStop.Enabled = running;
            lblStatus.Text = status;
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            StopCamera();
            pictureBoxIpl1.Image?.Dispose();
            base.OnFormClosed(e);
        }
    }
}
