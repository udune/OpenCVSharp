using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
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
            Text = "CH03 - Camera 출력 학습 및 실습";
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

            renderTimer.Interval = (int)nudInterval.Value;
            renderTimer.Tick += RenderTimer_Tick;
        }

        private void InitializeTheoryTab()
        {
            // Title Header
            var lblTitle = new Label
            {
                Text = "003. Camera 출력 및 실시간 필터 핵심 이론 학습",
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
                Text = "■ [카메라 불러오기 및 설정 (CvCapture)]\r\n" +
                       "OpenCV에서 컴퓨터나 노트북에 달린 카메라(웹캠)를 켜고 제어할 때 사용하는 주인공이 바로 'CvCapture'입니다.\r\n\r\n" +
                       "★ 연결 API: capture = CvCapture.FromCamera(cameraIndex);\r\n" +
                       "  - 번호(cameraIndex): 내 컴퓨터에 카메라가 여러 개 있을 수 있으므로 0번, 1번 등 번호로 연결할 카메라를 고릅니다.\r\n" +
                       "  - 해상도(크기) 설정: 카메라에게 '가로 몇 픽셀, 세로 몇 픽셀 크기로 찍어줘'라고 요청할 수 있습니다.\r\n" +
                       "  - 단, 카메라도 기계이기 때문에 자신이 찍을 수 없는 이상한 크기를 요청받으면, 자신이 지원하는 크기 중 가장 비슷한 크기로 알아서 맞추어 촬영합니다.\r\n\r\n" +
                       "--------------------------------------------------\r\n\r\n" +
                       "■ [실시간 사진 찰칵! (캡처 루프)]\r\n" +
                       "카메라는 쉬지 않고 실시간으로 영상을 보내오므로, 프로그램은 정해진 시간(타이머)마다 빠르게 사진을 한 장씩 가져와야 합니다.\r\n" +
                       "  - 캡처 API: frame = capture.QueryFrame();\r\n" +
                       "  - QueryFrame(): 카메라가 지금 막 찍은 따끈따끈한 최신 사진 한 장(프레임)을 쏙 빼내서 컴퓨터 메모리에 담습니다.\r\n" +
                       "  - 화면 띄우기: 가져온 사진 데이터를 C# 화면에 띄울 수 있는 비트맵 그림 형식으로 매번 변환해 PictureBox에 그려줍니다.\r\n\r\n" +
                       "--------------------------------------------------\r\n\r\n" +
                       "■ [카메라 독점과 해제 (Lock)]\r\n" +
                       "카메라나 마이크 같은 하드웨어는 학교 화장실 칸처럼 '한 번에 딱 한 프로그램만' 들어와서 쓸 수 있습니다.\r\n" +
                       "  - 우리 프로그램이 카메라를 켜서 쓰고 있는 동안에는, 다른 앱(예: 줌, 카카오톡 페이스톡)에서 동일한 카메라를 켤 수 없습니다.\r\n" +
                       "  - 만약 프로그램을 끌 때 카메라를 안전하게 꺼주지 않고(Dispose) 억지로 종료해 버리면, 카메라는 여전히 잠겨 있는 상태(Lock)가 됩니다. 이 경우 프로그램 재실행 시 에러가 나거나 화면이 나오지 않습니다. 폼이 닫힐 때 꼭 꺼주는 처리가 필수적입니다!\r\n\r\n" +
                       "--------------------------------------------------\r\n\r\n" +
                       "■ [실시간 필터와 컴퓨터가 느끼는 무게]\r\n" +
                       "  - 흑백 변환 (Grayscale): 색깔을 빼고 밝기만 계산하는 가벼운 일입니다. 연산 시간이 거의 들지 않아 화면이 부드럽게 유지됩니다.\r\n" +
                       "  - 테두리 찾기 (Canny Edge): 사진 속에서 선을 찾기 위해 먼지 제거(블러), 미분(수학 연산), 얇은 선 남기기 등 복잡하고 무거운 수학 계산을 매 장마다 해야 합니다. CPU가 열심히 일하느라 머리를 많이 쓰기 때문에 속도(FPS)가 느려질 수 있습니다."
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
            grpQuiz1.Text = "질문 1. 웹캠 장치 리소스 해제";
            grpQuiz1.Font = new Font("Malgun Gothic", 9F, FontStyle.Bold);
            grpQuiz1.Location = new Point(10, 35);
            grpQuiz1.Size = new Size(530, 100);
            pnlQuiz.Controls.Add(grpQuiz1);

            var txtQ1Text = new TextBox
            {
                Text = "웹캠 장치는 운영체제의 독점 리소스이므로, 사용이 완료되면 반드시 capture.Dispose()를 호출해 주어야 장치 락(Lock)을 예방할 수 있다.",
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
            grpQuiz2.Text = "질문 2. 실시간 필터 적용과 FPS 관계";
            grpQuiz2.Font = new Font("Malgun Gothic", 9F, FontStyle.Bold);
            grpQuiz2.Location = new Point(10, 140);
            grpQuiz2.Size = new Size(530, 100);
            pnlQuiz.Controls.Add(grpQuiz2);

            var txtQ2Text = new TextBox
            {
                Text = "실시간 비디오 프레임 루프 내부에서 Cv2.Canny와 같은 에지 감지 연산을 추가로 실행하더라도, CPU 연산 부하가 전혀 없어 실제 측정 FPS에는 아무런 영향이 없다.",
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
                Text = "🖼 시각 자료: 카메라 프레임 획득 & 필터 파이프라인",
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
            string imgPath = GetImagePath("ch03_camera_pipeline_ko.png");
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
                       "1. 웹캠 장치: 아날로그 빛 신호를 디지털 전기 신호로 실시간 캡처하여 스트림으로 방출합니다.\r\n" +
                       "2. 네이티브 버퍼 (CvCapture): 시스템 드라이버로부터 카메라 프레임을 초고속 획득하여 C++ 네이티브 메모리 영역(Unmanaged Heap)에 IplImage 형식의 포인터로 적재합니다.\r\n" +
                       "3. 필터 연산 (Canny / Grayscale): 매 프레임 Tick 루프 돌 때마다 흑백(Grayscale) 변환 및 캐니(Canny) 에지 검출 연산이 비관리 힙 내부에서 수행되어 이미지의 형태를 변화시킵니다.\r\n" +
                       "4. 화면 출력 (BitmapConverter): C++의 네이티브 포인터 메모리 구조를 C# 윈폼이 이해할 수 있도록 GDI+ Bitmap으로 고속 복사(Marshalling)하여 PictureBox UI에 실시간 갱신합니다."
            };
            tabTheory.Controls.Add(txtDiagramDesc);
        }

        private void BtnCheckAnswers_Click(object sender, EventArgs e)
        {
            // Q1 Check (Answer: O)
            if (rdoQ1O.Checked)
            {
                lblQ1Result.ForeColor = Color.Green;
                lblQ1Result.Text = "정답! 장치를 제때 Dispose하지 않으면 카메라가 점유 상태로 풀리지 않아 재시행 및 타 앱 연결 시 에러를 유발합니다.";
            }
            else if (rdoQ1X.Checked)
            {
                lblQ1Result.ForeColor = Color.Red;
                lblQ1Result.Text = "오답입니다. 하드웨어 외부 장치는 반드시 사용 후 Dispose를 명시해 락을 방지해야 합니다.";
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
                lblQ2Result.Text = "정답! Canny 에지 필터 등 복잡한 영상 처리 필터는 프레임당 높은 수학적 연산비용을 요구하므로 FPS 하락을 초래합니다.";
            }
            else if (rdoQ2O.Checked)
            {
                lblQ2Result.ForeColor = Color.Red;
                lblQ2Result.Text = "오답입니다. 매 프레임마다 대량의 행렬 연산이 얹어지면 처리 지연이 발생해 실시간 성능이 떨어집니다.";
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
                Size = new Size(275, 120),
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

            tabLab.Controls.Add(new TextBox
            {
                Text = "실습 가이드: PC에 연결된 웹캠 번호를 입력한 뒤 [장치 켜기]를 누르세요. 흑백 및 에지 필터 적용 시의 FPS 차이와 실제 획득되는 해상도를 학습해 보세요.",
                Location = new Point(20, 520),
                Size = new Size(720, 45),
                Multiline = true,
                ReadOnly = true,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.LightYellow,
                Font = new Font("Malgun Gothic", 9.5F)
            });
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
            StopCamera();
            pictureBox.Image?.Dispose();
            picDiagram.Image?.Dispose();
            renderTimer.Dispose();
            base.OnFormClosed(e);
        }
    }
}
