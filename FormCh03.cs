using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using OpenCvSharp;
using OpenCvSharp.CPlusPlus;
using OpenCvSharp.Extensions;

using Point = System.Drawing.Point;
using Size = System.Drawing.Size;
using FontStyle = System.Drawing.FontStyle;

namespace OpenCVSharp
{
    public class FormCh03 : Form
    {
        private readonly PictureBox pictureBox = new PictureBox();
        private readonly NumericUpDown nudCameraIndex = new NumericUpDown();
        private readonly ComboBox cmbResolution = new ComboBox();
        private readonly ComboBox cmbFilter = new ComboBox();
        private readonly ComboBox cmbSizeMode = new ComboBox();
        private readonly NumericUpDown nudInterval = new NumericUpDown();

        private readonly Label lblResolutionInfo = new Label();
        private readonly Label lblMeasuredFps = new Label();
        private readonly Label lblStatus = new Label();

        private readonly Button btnStart = new Button();
        private readonly Button btnStop = new Button();

        private readonly Timer renderTimer = new Timer();
        private readonly Stopwatch fpsWatch = new Stopwatch();
        private int frameCounter;

        private CvCapture capture;
        private IplImage frame;

        public FormCh03()
        {
            Text = "CH03 - Camera 출력 실습";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(1100, 600);
            Font = new Font("Malgun Gothic", 9F, FontStyle.Regular);

            pictureBox.Location = new Point(20, 20);
            pictureBox.Size = new Size(720, 480);
            pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox.BorderStyle = BorderStyle.FixedSingle;
            pictureBox.BackColor = Color.Black;

            var panel = new Panel
            {
                Location = new Point(760, 20),
                Size = new Size(310, 560),
                BorderStyle = BorderStyle.FixedSingle
            };

            int y = 15;
            Action<string> addHeader = text =>
            {
                panel.Controls.Add(new Label
                {
                    Text = text,
                    Font = new Font("Malgun Gothic", 10F, FontStyle.Bold),
                    Location = new Point(15, y),
                    AutoSize = true,
                    ForeColor = Color.DarkBlue
                });
                y += 25;
            };

            addHeader("1. 카메라 장치 번호 (Index)");
            nudCameraIndex.Location = new Point(15, y);
            nudCameraIndex.Size = new Size(270, 25);
            nudCameraIndex.Minimum = 0;
            nudCameraIndex.Maximum = 5;
            nudCameraIndex.Value = 0;
            panel.Controls.Add(nudCameraIndex);
            y += 35;

            addHeader("2. 카메라 목표 해상도");
            cmbResolution.Location = new Point(15, y);
            cmbResolution.Size = new Size(270, 25);
            cmbResolution.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbResolution.Items.AddRange(new object[] { "Default (기본 해상도)", "320 x 240", "640 x 480", "1280 x 720" });
            cmbResolution.SelectedIndex = 0;
            panel.Controls.Add(cmbResolution);
            y += 35;

            addHeader("3. 실시간 처리 필터");
            cmbFilter.Location = new Point(15, y);
            cmbFilter.Size = new Size(270, 25);
            cmbFilter.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbFilter.Items.AddRange(new object[] { "None (원본 영상)", "Grayscale (흑백 변환)", "Canny Edge (경계선 감지)" });
            cmbFilter.SelectedIndex = 0;
            panel.Controls.Add(cmbFilter);
            y += 35;

            addHeader("4. 프레임 주기 (Interval, ms)");
            nudInterval.Location = new Point(15, y);
            nudInterval.Size = new Size(270, 25);
            nudInterval.Minimum = 10;
            nudInterval.Maximum = 100;
            nudInterval.Value = 33;
            nudInterval.ValueChanged += (s, e) => renderTimer.Interval = (int)nudInterval.Value;
            panel.Controls.Add(nudInterval);
            y += 35;

            var sizeModes = new[] { PictureBoxSizeMode.Normal, PictureBoxSizeMode.StretchImage, PictureBoxSizeMode.Zoom, PictureBoxSizeMode.CenterImage };
            addHeader("5. PictureBox 출력 모드");
            cmbSizeMode.Location = new Point(15, y);
            cmbSizeMode.Size = new Size(270, 25);
            cmbSizeMode.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbSizeMode.Items.AddRange(new object[] { "Normal (좌상단 정렬)", "StretchImage (비율무시 채움)", "Zoom (비율유지 채움)", "CenterImage (중앙 배치)" });
            cmbSizeMode.SelectedIndex = 2;
            cmbSizeMode.SelectedIndexChanged += (s, e) => pictureBox.SizeMode = sizeModes[cmbSizeMode.SelectedIndex];
            panel.Controls.Add(cmbSizeMode);
            y += 45;

            addHeader("■ 카메라 상태 및 성능");
            lblResolutionInfo.Location = new Point(15, y);
            lblResolutionInfo.Size = new Size(275, 20);
            lblResolutionInfo.Font = new Font("Malgun Gothic", 9F, FontStyle.Bold);
            lblResolutionInfo.Text = "출력 해상도: -";
            panel.Controls.Add(lblResolutionInfo);
            y += 20;

            lblMeasuredFps.Location = new Point(15, y);
            lblMeasuredFps.Size = new Size(275, 20);
            lblMeasuredFps.Font = new Font("Malgun Gothic", 9F, FontStyle.Bold);
            lblMeasuredFps.ForeColor = Color.Red;
            lblMeasuredFps.Text = "측정 FPS: 0 (정지됨)";
            panel.Controls.Add(lblMeasuredFps);
            y += 20;

            lblStatus.Location = new Point(15, y);
            lblStatus.Size = new Size(275, 20);
            lblStatus.Font = new Font("Malgun Gothic", 9F, FontStyle.Bold);
            lblStatus.Text = "상태: 연결 대기 중";
            panel.Controls.Add(lblStatus);
            y += 30;

            btnStart.Text = "장치 켜기";
            btnStart.Location = new Point(15, y);
            btnStart.Size = new Size(130, 35);
            btnStart.Click += BtnStart_Click;
            panel.Controls.Add(btnStart);

            btnStop.Text = "장치 끄기";
            btnStop.Location = new Point(155, y);
            btnStop.Size = new Size(130, 35);
            btnStop.Enabled = false;
            btnStop.Click += BtnStop_Click;
            panel.Controls.Add(btnStop);
            y += 45;

            panel.Controls.Add(new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Location = new Point(15, y),
                Size = new Size(275, 130),
                BorderStyle = BorderStyle.Fixed3D,
                BackColor = Color.WhiteSmoke,
                Text = "【핵심 이론 및 원리】\r\n" +
                       "1. 카메라 연결: CvCapture.FromCamera(idx)\r\n" +
                       "   - 하드웨어 웹캠으로부터 비디오 입력을 초기화합니다.\r\n" +
                       "2. 해상도 제어: SetCaptureProperty(..)\r\n" +
                       "   - FrameWidth, FrameHeight 속성을 지정합니다.\r\n" +
                       "3. 리소스 해제: Dispose() 필수!\r\n" +
                       "   - 사용이 끝난 웹캠은 반납하지 않으면 락(Lock)이 걸려 다른 앱에서 사용 불가능합니다."
            });

            Controls.Add(pictureBox);
            Controls.Add(panel);
            Controls.Add(new Label
            {
                Text = "실습 가이드: PC에 연결된 웹캠 번호를 입력한 뒤 [장치 켜기]를 누르세요. 흑백 및 에지 필터 적용 시의 FPS 차이와 실제 획득되는 해상도를 학습해 보세요.",
                Location = new Point(20, 520),
                Size = new Size(720, 40),
                BorderStyle = BorderStyle.Fixed3D,
                BackColor = Color.LightYellow,
                Padding = new Padding(5)
            });

            renderTimer.Interval = (int)nudInterval.Value;
            renderTimer.Tick += RenderTimer_Tick;
        }

        private void BtnStart_Click(object sender, EventArgs e)
        {
            try
            {
                StopCamera();

                capture = CvCapture.FromCamera((int)nudCameraIndex.Value);
                if (capture == null)
                {
                    MessageBox.Show("카메라 장치를 열 수 없습니다.", "장치 연결 실패", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    lblStatus.Text = "상태: 카메라 연결 실패";
                    return;
                }

                if (cmbResolution.SelectedIndex > 0)
                {
                    int targetW, targetH;
                    switch (cmbResolution.SelectedIndex)
                    {
                        case 1: targetW = 320; targetH = 240; break;
                        case 3: targetW = 1280; targetH = 720; break;
                        default: targetW = 640; targetH = 480; break;
                    }
                    capture.SetCaptureProperty(CaptureProperty.FrameWidth, targetW);
                    capture.SetCaptureProperty(CaptureProperty.FrameHeight, targetH);
                }

                frame = capture.QueryFrame();
                if (frame == null)
                {
                    capture.Dispose();
                    capture = null;
                    MessageBox.Show("카메라로부터 프레임을 가져올 수 없습니다. 권한 혹은 연결 상태를 확인하세요.", "프레임 획득 실패", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    lblStatus.Text = "상태: 프레임 획득 실패";
                    return;
                }

                int actualW = (int)capture.GetCaptureProperty(CaptureProperty.FrameWidth);
                int actualH = (int)capture.GetCaptureProperty(CaptureProperty.FrameHeight);
                lblResolutionInfo.Text = $"실제 획득 해상도: {actualW} x {actualH}";

                frameCounter = 0;
                fpsWatch.Restart();
                renderTimer.Interval = (int)nudInterval.Value;
                renderTimer.Start();

                btnStart.Enabled = false;
                btnStop.Enabled = true;
                nudCameraIndex.Enabled = false;
                cmbResolution.Enabled = false;
                lblStatus.Text = "상태: 카메라 캡처 중";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "카메라 초기화 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "상태: 장치 시작 에러";
            }
        }

        private void BtnStop_Click(object sender, EventArgs e) => StopCamera();

        private void StopCamera()
        {
            renderTimer.Stop();
            frame = null;

            if (capture != null)
            {
                capture.Dispose();
                capture = null;
            }

            btnStart.Enabled = true;
            btnStop.Enabled = false;
            nudCameraIndex.Enabled = true;
            cmbResolution.Enabled = true;
            lblStatus.Text = "상태: 연결 종료";
            lblMeasuredFps.Text = "측정 FPS: 0 (정지됨)";
        }

        private void RenderTimer_Tick(object sender, EventArgs e)
        {
            if (capture == null) return;

            frame = capture.QueryFrame();
            if (frame == null) return;

            Bitmap processedBitmap;
            using (var mat = new Mat(frame))
            {
                switch (cmbFilter.SelectedIndex)
                {
                    case 1:
                        using (var gray = new Mat())
                        {
                            Cv2.CvtColor(mat, gray, ColorConversion.BgrToGray);
                            processedBitmap = BitmapConverter.ToBitmap(gray);
                        }
                        break;
                    case 2:
                        using (var gray = new Mat())
                        using (var edge = new Mat())
                        {
                            Cv2.CvtColor(mat, gray, ColorConversion.BgrToGray);
                            Cv2.Canny(gray, edge, 50, 150);
                            processedBitmap = BitmapConverter.ToBitmap(edge);
                        }
                        break;
                    default:
                        processedBitmap = BitmapConverter.ToBitmap(mat);
                        break;
                }
            }

            var old = pictureBox.Image;
            pictureBox.Image = processedBitmap;
            old?.Dispose();

            frameCounter++;
            if (fpsWatch.ElapsedMilliseconds >= 1000)
            {
                lblMeasuredFps.Text = $"측정 FPS: {frameCounter / (fpsWatch.ElapsedMilliseconds / 1000.0):F1}";
                frameCounter = 0;
                fpsWatch.Restart();
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            StopCamera();
            pictureBox.Image?.Dispose();
            renderTimer.Dispose();
            base.OnFormClosed(e);
        }
    }
}
