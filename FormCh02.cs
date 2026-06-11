using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace OpenCVSharp
{
    public class FormCh02 : Form
    {
        private readonly TabControl tabControl = new TabControl();
        private readonly TabPage tabTheory = new TabPage();
        private readonly TabPage tabLab = new TabPage();

        // --- Theory Tab Controls ---
        private readonly GroupBox grpQuiz1 = new GroupBox();
        private readonly RadioButton rdoQ1O = new RadioButton();
        private readonly RadioButton rdoQ1X = new RadioButton();
        private readonly Label lblQ1Result = new Label();

        private readonly GroupBox grpQuiz2 = new GroupBox();
        private readonly RadioButton rdoQ2O = new RadioButton();
        private readonly RadioButton rdoQ2X = new RadioButton();
        private readonly Label lblQ2Result = new Label();

        private readonly Button btnCheckAnswers = new Button();
        private readonly Button btnGoToLab = new Button();
        private readonly PictureBox picDiagram = new PictureBox();

        // --- Lab Tab Controls ---
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
            Text = "CH02 - Image Size & FPS 학습 및 실습";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(1200, 740);
            Font = new Font("Malgun Gothic", 9F, FontStyle.Regular);

            // Setup Tab Control
            tabControl.Dock = DockStyle.Fill;
            tabTheory.Text = "📚 1. 핵심 이론 및 자가진단 퀴즈";
            tabTheory.BackColor = Color.White;
            tabLab.Text = "🧪 2. 실습 실험실 (Interactive Lab)";
            tabLab.BackColor = Color.FromArgb(240, 244, 248);

            tabControl.Controls.Add(tabTheory);
            tabControl.Controls.Add(tabLab);
            Controls.Add(tabControl);

            // Initialize Tabs
            InitializeTheoryTab();
            InitializeLabTab();

            renderTimer.Tick += RenderTimer_Tick;
            UpdateCalculations();
        }

        private void InitializeTheoryTab()
        {
            // Title Header
            var lblTitle = new Label
            {
                Text = "002. Image Size & FPS 핵심 이론 학습",
                Font = new Font("Malgun Gothic", 14F, FontStyle.Bold),
                Location = new Point(20, 20),
                AutoSize = true,
                ForeColor = Color.DarkSlateBlue
            };
            tabTheory.Controls.Add(lblTitle);

            // Theory Text Box
            var txtTheoryText = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Location = new Point(20, 60),
                Size = new Size(550, 280),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(252, 252, 248),
                Font = new Font("Malgun Gothic", 10F),
                Text = "■ [이미지 크기와 용량 (Memory Size)]\r\n" +
                       "디지털 이미지는 픽셀(Pixel)이라고 부르는 작은 '색상 점'들이 가로, 세로 격자 판 위에 가득 모여서 이루어집니다.\r\n" +
                       "이미지가 차지하는 메모리 용량은 가로 점 개수, 세로 점 개수, 그리고 색상 표현 방식(채널)을 곱해서 계산합니다.\r\n\r\n" +
                       "★ 계산 공식: 이미지 용량(바이트) = 가로 크기 × 세로 크기 × 채널 수\r\n" +
                       "  - 명암/흑백 (Grayscale): 1채널. 각 점이 검은색~흰색 밝기만 표현하므로 점당 1바이트만 사용합니다.\r\n" +
                       "  - 컬러 (BGR): 3채널. 빛의 삼원색인 파랑(B), 초록(G), 빨강(R) 3가지 색상을 섞어 표시하므로 점당 3바이트(3채널)를 사용합니다.\r\n" +
                       "  - 컬러 이미지는 흑백 이미지보다 정보량이 정확히 3배 더 무겁습니다!\r\n\r\n" +
                       "--------------------------------------------------\r\n\r\n" +
                       "■ [FPS와 프레임 대기 시간 (Interval)]\r\n" +
                       "동영상은 낱장 카드 그림책을 빠르게 스르륵 넘기듯, 정지 사진 여러 장을 순서대로 빠르게 보여주는 것입니다.\r\n" +
                       "  - FPS (Frames Per Second): 1초 동안 화면에 스쳐 지나가는 사진(프레임)의 장수입니다.\r\n" +
                       "  - 프레임 대기 시간 (Interval): 사진 한 장을 띄우고 다음 사진으로 넘어가기 전까지 멈춰 있는 대기 시간(ms, 1000분의 1초)입니다.\r\n\r\n" +
                       "★ 관계 공식: 목표 FPS = 1000 / 대기 시간(ms)\r\n" +
                       "  - 33ms 주기인 경우: 1000 / 33 ≒ 30.3 FPS (TV나 유튜브 표준으로 부드럽게 보임)\r\n" +
                       "  - 16ms 주기인 경우: 1000 / 16 ≒ 62.5 FPS (게임 등에서 고주사율로 매끄럽게 보임)\r\n\r\n" +
                       "※ 실제 성능 오차: 컴퓨터의 연산 속도 한계나 윈도우 창을 그리는 부하 때문에 실제 측정되는 FPS는 공식 이론값보다 더 떨어질 수 있습니다.\r\n\r\n" +
                       "--------------------------------------------------\r\n\r\n" +
                       "■ [화면 크기 조절 모드 (PictureBoxSizeMode)]\r\n" +
                       "내가 불러온 사진 크기(해상도)와 화면의 액자 크기가 맞지 않을 때 채우는 네 가지 방법입니다.\r\n" +
                       "  - Normal: 사진의 원본 크기 그대로 액자 왼쪽 꼭대기부터 띄웁니다. 액자보다 큰 부분은 잘려 나갑니다.\r\n" +
                       "  - StretchImage: 사진 비율이 찌그러지더라도 액자 크기에 맞게 억지로 늘려 꽉 채웁니다.\r\n" +
                       "  - Zoom: 사진 가로/세로 비율을 예쁘게 유지하면서 액자 안에 가득 들어가도록 늘리거나 줄입니다. 비율이 안 맞으면 빈 여백(검은 띠)이 생깁니다.\r\n" +
                       "  - CenterImage: 사진을 찌그러뜨리지 않고 정중앙을 기준으로 액자에 정렬해 표시합니다."
            };
            tabTheory.Controls.Add(txtTheoryText);

            // Quiz Section Panel
            var pnlQuiz = new Panel
            {
                Location = new Point(20, 360),
                Size = new Size(550, 330),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(245, 245, 250)
            };
            tabTheory.Controls.Add(pnlQuiz);

            var lblQuizTitle = new Label
            {
                Text = "✍ 자가 진단 퀴즈 (이론 검증)",
                Font = new Font("Malgun Gothic", 11F, FontStyle.Bold),
                Location = new Point(15, 10),
                AutoSize = true,
                ForeColor = Color.DarkBlue
            };
            pnlQuiz.Controls.Add(lblQuizTitle);

            // Quiz 1
            grpQuiz1.Text = "질문 1. BGR 이미지 메모리 크기 계산";
            grpQuiz1.Font = new Font("Malgun Gothic", 9F, FontStyle.Bold);
            grpQuiz1.Location = new Point(10, 35);
            grpQuiz1.Size = new Size(530, 100);
            pnlQuiz.Controls.Add(grpQuiz1);

            var txtQ1Text = new TextBox
            {
                Text = "1280 x 720 해상도를 가지는 3채널 BGR 컬러 이미지의 이론적인 메모리 크기는 약 2.64MB (2,764,800 Bytes)이다.",
                Font = new Font("Malgun Gothic", 9F, FontStyle.Regular),
                Location = new Point(10, 20),
                Size = new Size(510, 40),
                Multiline = true,
                ReadOnly = true,
                BorderStyle = BorderStyle.None,
                BackColor = Color.FromArgb(245, 245, 250)
            };
            grpQuiz1.Controls.Add(txtQ1Text);

            rdoQ1O.Text = "O (참)";
            rdoQ1O.Location = new Point(20, 65);
            rdoQ1O.Size = new Size(80, 20);
            rdoQ1O.Font = new Font("Malgun Gothic", 9F, FontStyle.Regular);
            grpQuiz1.Controls.Add(rdoQ1O);

            rdoQ1X.Text = "X (거짓)";
            rdoQ1X.Location = new Point(120, 65);
            rdoQ1X.Size = new Size(80, 20);
            rdoQ1X.Font = new Font("Malgun Gothic", 9F, FontStyle.Regular);
            grpQuiz1.Controls.Add(rdoQ1X);

            lblQ1Result.Location = new Point(220, 65);
            lblQ1Result.Size = new Size(300, 25);
            lblQ1Result.Font = new Font("Malgun Gothic", 8.5F, FontStyle.Regular);
            lblQ1Result.ForeColor = Color.DarkGray;
            lblQ1Result.Text = "정답 확인 시 해설이 여기에 표시됩니다.";
            grpQuiz1.Controls.Add(lblQ1Result);

            // Quiz 2
            grpQuiz2.Text = "질문 2. PictureBoxSizeMode 속성 이해";
            grpQuiz2.Font = new Font("Malgun Gothic", 9F, FontStyle.Bold);
            grpQuiz2.Location = new Point(10, 140);
            grpQuiz2.Size = new Size(530, 100);
            pnlQuiz.Controls.Add(grpQuiz2);

            var txtQ2Text = new TextBox
            {
                Text = "PictureBox의 StretchImage 모드는 이미지의 고유 비율(종횡비)을 유지하면서 화면 크기에 맞춰 최대화하여 출력한다.",
                Font = new Font("Malgun Gothic", 9F, FontStyle.Regular),
                Location = new Point(10, 20),
                Size = new Size(510, 40),
                Multiline = true,
                ReadOnly = true,
                BorderStyle = BorderStyle.None,
                BackColor = Color.FromArgb(245, 245, 250)
            };
            grpQuiz2.Controls.Add(txtQ2Text);

            rdoQ2O.Text = "O (참)";
            rdoQ2O.Location = new Point(20, 65);
            rdoQ2O.Size = new Size(80, 20);
            rdoQ2O.Font = new Font("Malgun Gothic", 9F, FontStyle.Regular);
            grpQuiz2.Controls.Add(rdoQ2O);

            rdoQ2X.Text = "X (거짓)";
            rdoQ2X.Location = new Point(120, 65);
            rdoQ2X.Size = new Size(80, 20);
            rdoQ2X.Font = new Font("Malgun Gothic", 9F, FontStyle.Regular);
            grpQuiz2.Controls.Add(rdoQ2X);

            lblQ2Result.Location = new Point(220, 65);
            lblQ2Result.Size = new Size(300, 25);
            lblQ2Result.Font = new Font("Malgun Gothic", 8.5F, FontStyle.Regular);
            lblQ2Result.ForeColor = Color.DarkGray;
            lblQ2Result.Text = "정답 확인 시 해설이 여기에 표시됩니다.";
            grpQuiz2.Controls.Add(lblQ2Result);

            // Check Answers Button
            btnCheckAnswers.Text = "정답 확인 및 해설 보기";
            btnCheckAnswers.Location = new Point(10, 245);
            btnCheckAnswers.Size = new Size(530, 35);
            btnCheckAnswers.Font = new Font("Malgun Gothic", 10F, FontStyle.Bold);
            btnCheckAnswers.BackColor = Color.SteelBlue;
            btnCheckAnswers.ForeColor = Color.White;
            btnCheckAnswers.FlatStyle = FlatStyle.Flat;
            btnCheckAnswers.Click += BtnCheckAnswers_Click;
            pnlQuiz.Controls.Add(btnCheckAnswers);

            // Go to Lab Button
            btnGoToLab.Text = "실습 실험실로 이동하기 (Go to Lab) ▶";
            btnGoToLab.Location = new Point(10, 285);
            btnGoToLab.Size = new Size(530, 38);
            btnGoToLab.Font = new Font("Malgun Gothic", 11F, FontStyle.Bold);
            btnGoToLab.BackColor = Color.ForestGreen;
            btnGoToLab.ForeColor = Color.White;
            btnGoToLab.FlatStyle = FlatStyle.Flat;
            btnGoToLab.Click += (s, e) => tabControl.SelectedTab = tabLab;
            pnlQuiz.Controls.Add(btnGoToLab);

            // --- Right Side Infographic Diagram Area ---
            var lblDiagramTitle = new Label
            {
                Text = "🖼 시각 자료: 이미지 구조 및 픽셀 매핑",
                Font = new Font("Malgun Gothic", 11F, FontStyle.Bold),
                Location = new Point(590, 20),
                AutoSize = true,
                ForeColor = Color.DarkBlue
            };
            tabTheory.Controls.Add(lblDiagramTitle);

            picDiagram.Location = new Point(590, 45);
            picDiagram.Size = new Size(580, 480);
            picDiagram.SizeMode = PictureBoxSizeMode.Zoom;
            picDiagram.BorderStyle = BorderStyle.FixedSingle;
            picDiagram.BackColor = Color.FromArgb(20, 20, 20);
            string imgPath = GetImagePath("ch02_pixel_structure_ko.png");
            if (imgPath != null)
            {
                picDiagram.Image = Image.FromFile(imgPath);
            }
            tabTheory.Controls.Add(picDiagram);

            var txtDiagramDesc = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Location = new Point(590, 535),
                Size = new Size(580, 150),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(250, 250, 252),
                Font = new Font("Malgun Gothic", 9.5F),
                Text = "【인포그래픽 설명】\r\n" +
                       "1. 왼쪽(단일 채널 - 그레이스케일): 픽셀 하나당 1바이트(8비트, 0~255) 데이터만을 사용합니다. 명암 강도만을 표현하므로 메모리를 최소한으로 소모합니다.\r\n" +
                       "2. 오른쪽(3채널 - BGR 컬러): 픽셀 하나를 구성하기 위해 Blue(청색), Green(녹색), Red(적색) 각각 1바이트씩 총 3바이트(24비트)의 메모리를 소모합니다. 픽셀 당 3개 성분의 조합을 통해 다채로운 색을 구현합니다.\r\n" +
                       "3. 결론: 컬러 이미지는 동일 해상도의 흑백 이미지보다 데이터양이 정확히 3배가 되어 하드웨어 대역폭과 계산 성능에 상당한 부담을 줍니다. 따라서 고속 처리가 생명인 컴퓨터 비전 파이프라인에서는 컬러 이미지를 그레이스케일로 변환하는 작업을 전처리 1순위로 수행합니다."
            };
            tabTheory.Controls.Add(txtDiagramDesc);
        }

        private void BtnCheckAnswers_Click(object sender, EventArgs e)
        {
            // Q1 Check (Answer: O)
            if (rdoQ1O.Checked)
            {
                lblQ1Result.ForeColor = Color.Green;
                lblQ1Result.Text = "정답! 1280 * 720 * 3 = 2,764,800 Bytes 이며, 1024 두 번 나누면 약 2.64MB가 정확합니다.";
            }
            else if (rdoQ1X.Checked)
            {
                lblQ1Result.ForeColor = Color.Red;
                lblQ1Result.Text = "오답입니다. 가로 x 세로 x 채널수(3) 공식을 다시 한 번 확인해 보세요.";
            }
            else
            {
                lblQ1Result.ForeColor = Color.OrangeRed;
                lblQ1Result.Text = "답안을 먼저 체크해 주세요.";
            }

            // Q2 Check (Answer: X)
            if (rdoQ2X.Checked)
            {
                lblQ2Result.ForeColor = Color.Green;
                lblQ2Result.Text = "정답! StretchImage는 종횡비를 파괴하여 이미지를 왜곡시킵니다. 비율 유지는 Zoom입니다.";
            }
            else if (rdoQ2O.Checked)
            {
                lblQ2Result.ForeColor = Color.Red;
                lblQ2Result.Text = "오답입니다. StretchImage는 비율을 유지하지 않고 화면 전체에 찌그러트립니다.";
            }
            else
            {
                lblQ2Result.ForeColor = Color.OrangeRed;
                lblQ2Result.Text = "답안을 먼저 체크해 주세요.";
            }
        }

        private void InitializeLabTab()
        {
            pictureBox.Location = new Point(20, 20);
            pictureBox.Size = new Size(720, 480);
            pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox.BorderStyle = BorderStyle.FixedSingle;
            pictureBox.BackColor = Color.Black;
            tabLab.Controls.Add(pictureBox);

            var panel = new Panel
            {
                Location = new Point(760, 20),
                Size = new Size(310, 560),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };
            tabLab.Controls.Add(panel);

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
                Size = new Size(275, 120),
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

            tabLab.Controls.Add(new TextBox
            {
                Text = "실습 가이드: 각 옵션을 설정한 뒤 [시작] 버튼을 눌러 공의 움직임과 FPS 변동을 관찰해 보세요.",
                Location = new Point(20, 520),
                Size = new Size(720, 45),
                Multiline = true,
                ReadOnly = true,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.LightYellow,
                Font = new Font("Malgun Gothic", 9.5F)
            });
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

        private string GetImagePath(string filename)
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename);
            if (!File.Exists(path))
            {
                path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\" + filename);
            }
            return File.Exists(path) ? path : null;
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            StopTimer();
            pictureBox.Image?.Dispose();
            picDiagram.Image?.Dispose();
            renderTimer.Dispose();
            base.OnFormClosed(e);
        }
    }
}
