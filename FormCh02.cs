using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace OpenCVSharp
{
    public class FormCh02 : Form
    {
        private readonly PictureBox pictureBox = new PictureBox();
        private readonly ComboBox cmbResolution = new ComboBox();
        private readonly ComboBox cmbChannels = new ComboBox();
        private readonly NumericUpDown nudInterval = new NumericUpDown();
        private readonly ComboBox cmbSizeMode = new ComboBox();

        private readonly Label lblMemoryCalc = new Label();
        private readonly Label lblTargetFps = new Label();
        private readonly Label lblMeasuredFps = new Label();

        private readonly Button btnStart = new Button();
        private readonly Button btnStop = new Button();

        private readonly Timer renderTimer = new Timer();
        private readonly Stopwatch fpsWatch = new Stopwatch();
        private int frameCounter;

        private float ballX = 100;
        private float ballY = 100;
        private float ballSpeedX = 6f;
        private float ballSpeedY = 4f;
        private const int BallRadius = 20;

        public FormCh02()
        {
            Text = "CH02 - Image Size & FPS 실습";
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

            addHeader("1. 이미지 해상도 선택 (Width x Height)");
            cmbResolution.Location = new Point(15, y);
            cmbResolution.Size = new Size(270, 25);
            cmbResolution.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbResolution.Items.AddRange(new object[] { "320 x 240 (QVGA)", "640 x 480 (VGA)", "1280 x 720 (HD)", "1920 x 1080 (FHD)" });
            cmbResolution.SelectedIndex = 1;
            cmbResolution.SelectedIndexChanged += (s, e) => UpdateCalculations();
            panel.Controls.Add(cmbResolution);
            y += 35;

            addHeader("2. 컬러 채널 수 (Color Channels)");
            cmbChannels.Location = new Point(15, y);
            cmbChannels.Size = new Size(270, 25);
            cmbChannels.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbChannels.Items.AddRange(new object[] { "1 Channel (Grayscale)", "3 Channels (BGR Color)" });
            cmbChannels.SelectedIndex = 1;
            cmbChannels.SelectedIndexChanged += (s, e) => UpdateCalculations();
            panel.Controls.Add(cmbChannels);
            y += 35;

            addHeader("3. 프레임 갱신 주기 (Interval, ms)");
            nudInterval.Location = new Point(15, y);
            nudInterval.Size = new Size(270, 25);
            nudInterval.Minimum = 5;
            nudInterval.Maximum = 1000;
            nudInterval.Value = 33;
            nudInterval.ValueChanged += (s, e) =>
            {
                renderTimer.Interval = (int)nudInterval.Value;
                UpdateCalculations();
            };
            panel.Controls.Add(nudInterval);
            y += 35;

            var sizeModes = new[] { PictureBoxSizeMode.Normal, PictureBoxSizeMode.StretchImage, PictureBoxSizeMode.Zoom, PictureBoxSizeMode.CenterImage };
            addHeader("4. PictureBox 출력 모드 (SizeMode)");
            cmbSizeMode.Location = new Point(15, y);
            cmbSizeMode.Size = new Size(270, 25);
            cmbSizeMode.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbSizeMode.Items.AddRange(new object[] { "Normal (좌상단 정렬)", "StretchImage (비율무시 채움)", "Zoom (비율유지 채움)", "CenterImage (중앙 배치)" });
            cmbSizeMode.SelectedIndex = 2;
            cmbSizeMode.SelectedIndexChanged += (s, e) => pictureBox.SizeMode = sizeModes[cmbSizeMode.SelectedIndex];
            panel.Controls.Add(cmbSizeMode);
            y += 45;

            addHeader("■ 실시간 계산 결과");
            lblMemoryCalc.Location = new Point(15, y);
            lblMemoryCalc.Size = new Size(275, 20);
            lblMemoryCalc.Font = new Font("Malgun Gothic", 9F, FontStyle.Bold);
            panel.Controls.Add(lblMemoryCalc);
            y += 20;

            lblTargetFps.Location = new Point(15, y);
            lblTargetFps.Size = new Size(275, 20);
            lblTargetFps.Font = new Font("Malgun Gothic", 9F, FontStyle.Bold);
            panel.Controls.Add(lblTargetFps);
            y += 20;

            lblMeasuredFps.Location = new Point(15, y);
            lblMeasuredFps.Size = new Size(275, 20);
            lblMeasuredFps.Font = new Font("Malgun Gothic", 9.75F, FontStyle.Bold);
            lblMeasuredFps.ForeColor = Color.Red;
            lblMeasuredFps.Text = "실제 측정 FPS: 0 (정지됨)";
            panel.Controls.Add(lblMeasuredFps);
            y += 35;

            btnStart.Text = "시작 (Start)";
            btnStart.Location = new Point(15, y);
            btnStart.Size = new Size(130, 35);
            btnStart.Click += BtnStart_Click;
            panel.Controls.Add(btnStart);

            btnStop.Text = "중지 (Stop)";
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
                Size = new Size(275, 220),
                BorderStyle = BorderStyle.Fixed3D,
                BackColor = Color.WhiteSmoke,
                Text = "【핵심 이론 및 원리】\r\n" +
                       "1. 이미지 크기 (Memory Size)\r\n" +
                       "   공식: 가로 × 세로 × 채널 수 (Bytes)\r\n" +
                       "   - 그레이스케일: 1채널 (명암값)\r\n" +
                       "   - 컬러(BGR): 3채널 (Blue, Green, Red)\r\n\r\n" +
                       "2. FPS 와 Interval 관계\r\n" +
                       "   공식: Target FPS = 1000 / Interval(ms)\r\n" +
                       "   - 33ms ≒ 30.3 FPS (표준 비디오)\r\n" +
                       "   - 16ms ≒ 62.5 FPS\r\n\r\n" +
                       "3. 렌더링 오차\r\n" +
                       "   - PC 성능, 화면 크기 조절, UI 타이머 오차로 인해 실제 측정 FPS는 계산값보다 작을 수 있습니다."
            });

            Controls.Add(pictureBox);
            Controls.Add(panel);
            Controls.Add(new Label
            {
                Text = "실습 가이드: 각 옵션을 설정한 뒤 [시작] 버튼을 눌러 공의 움직임과 FPS 변동을 관찰해 보세요.",
                Location = new Point(20, 520),
                Size = new Size(720, 40),
                BorderStyle = BorderStyle.Fixed3D,
                BackColor = Color.LightYellow,
                Padding = new Padding(5)
            });

            renderTimer.Tick += RenderTimer_Tick;
            UpdateCalculations();
        }

        private void GetResolution(out int w, out int h)
        {
            switch (cmbResolution.SelectedIndex)
            {
                case 0: w = 320; h = 240; break;
                case 2: w = 1280; h = 720; break;
                case 3: w = 1920; h = 1080; break;
                default: w = 640; h = 480; break;
            }
        }

        private int GetChannels() => cmbChannels.SelectedIndex == 0 ? 1 : 3;

        private void UpdateCalculations()
        {
            GetResolution(out int w, out int h);
            int ch = GetChannels();
            long bytes = (long)w * h * ch;
            lblMemoryCalc.Text = $"이론 메모리: {bytes:N0} B ({bytes / 1024.0 / 1024.0:F2} MB)";

            int interval = (int)nudInterval.Value;
            lblTargetFps.Text = $"목표 FPS: {1000.0 / interval:F1} (주기 {interval}ms)";
        }

        private void BtnStart_Click(object sender, EventArgs e)
        {
            frameCounter = 0;
            fpsWatch.Restart();
            renderTimer.Interval = (int)nudInterval.Value;
            renderTimer.Start();
            btnStart.Enabled = false;
            btnStop.Enabled = true;
            cmbResolution.Enabled = false;
            cmbChannels.Enabled = false;
        }

        private void BtnStop_Click(object sender, EventArgs e) => StopTimer();

        private void StopTimer()
        {
            renderTimer.Stop();
            btnStart.Enabled = true;
            btnStop.Enabled = false;
            cmbResolution.Enabled = true;
            cmbChannels.Enabled = true;
            lblMeasuredFps.Text = "실제 측정 FPS: 0 (정지됨)";
        }

        private void RenderTimer_Tick(object sender, EventArgs e)
        {
            GetResolution(out int w, out int h);
            int ch = GetChannels();

            var bitmap = new Bitmap(w, h);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.Clear(ch == 1 ? Color.FromArgb(40, 40, 40) : Color.FromArgb(20, 30, 40));

                ballX += ballSpeedX * (w / 640f);
                ballY += ballSpeedY * (h / 480f);
                float r = Math.Max(10f, BallRadius * (w / 640f));

                if (ballX - r < 0) { ballX = r; ballSpeedX = -ballSpeedX; }
                else if (ballX + r > w) { ballX = w - r; ballSpeedX = -ballSpeedX; }

                if (ballY - r < 0) { ballY = r; ballSpeedY = -ballSpeedY; }
                else if (ballY + r > h) { ballY = h - r; ballSpeedY = -ballSpeedY; }

                g.FillEllipse(ch == 1 ? Brushes.LightGray : Brushes.OrangeRed, ballX - r, ballY - r, r * 2, r * 2);

                using (var gridPen = new Pen(Color.FromArgb(50, 255, 255, 255), 1))
                {
                    g.DrawLine(gridPen, w / 2, 0, w / 2, h);
                    g.DrawLine(gridPen, 0, h / 2, w, h / 2);
                }

                float fontSize = Math.Max(10f, w / 40f);
                using (var font = new Font("Malgun Gothic", fontSize, FontStyle.Bold))
                {
                    g.DrawString($"Simulated Frame: {w} x {h} ({ch}Ch)", font, Brushes.White, 20, 20);
                    g.DrawString(DateTime.Now.ToString("HH:mm:ss.fff"), font, Brushes.White, 20, 20 + fontSize * 1.5f);
                    g.DrawString($"Memory: {w * h * ch / 1024.0 / 1024.0:F2} MB", font, Brushes.White, 20, 20 + fontSize * 3.0f);
                }
            }

            var old = pictureBox.Image;
            pictureBox.Image = bitmap;
            old?.Dispose();

            frameCounter++;
            if (fpsWatch.ElapsedMilliseconds >= 1000)
            {
                lblMeasuredFps.Text = $"실제 측정 FPS: {frameCounter / (fpsWatch.ElapsedMilliseconds / 1000.0):F1}";
                frameCounter = 0;
                fpsWatch.Restart();
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            StopTimer();
            pictureBox.Image?.Dispose();
            renderTimer.Dispose();
            base.OnFormClosed(e);
        }
    }
}
