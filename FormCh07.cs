using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using OpenCvSharp;

using Point = System.Drawing.Point;
using Size = System.Drawing.Size;
using FontStyle = System.Drawing.FontStyle;

namespace OpenCVSharp
{
    public class FormCh07 : Form
    {
        private readonly TabControl tabControl = new TabControl();
        private readonly TabPage tabTheory = new TabPage();
        private readonly TabPage tabLab = new TabPage();
        private readonly PictureBox picDiagram = new PictureBox();

        // Simulated/Interactive IplImage instance
        private IplImage simImage = null;
        private IplImage splitR = null;
        private IplImage splitG = null;
        private IplImage splitB = null;
        private IplImage grayImg = null;

        // Selected pixel coordinates
        private int selectedX = 0;
        private int selectedY = 0;
        private bool updatingSliders = false;

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

        // --- Lab Tab Controls ---
        // Left column controls
        private readonly NumericUpDown numWidth = new NumericUpDown();
        private readonly NumericUpDown numHeight = new NumericUpDown();
        private readonly ComboBox cmbBitDepth = new ComboBox();
        private readonly ComboBox cmbChannels = new ComboBox();
        private readonly Button btnCreateImage = new Button();

        private readonly Label lblAttrWidth = new Label();
        private readonly Label lblAttrHeight = new Label();
        private readonly Label lblAttrWidthStep = new Label();
        private readonly Label lblAttrDepth = new Label();
        private readonly Label lblAttrNChannels = new Label();
        private readonly Label lblAttrImageSize = new Label();
        private readonly Label lblAttrImageData = new Label();

        private readonly Label lblCalcFormula = new Label();
        private readonly Label lblCalcSteps = new Label();
        private readonly Label lblCalcResult = new Label();
        private readonly Label lblMemBytes = new Label();

        // Right column controls
        private readonly PictureBox pbPixelGrid = new PictureBox();
        private readonly GroupBox grpPixelEdit = new GroupBox();
        private readonly Label lblSelectedPixelInfo = new Label();
        
        private readonly Label lblRName = new Label();
        private readonly TrackBar trValR = new TrackBar();
        private readonly Label lblValR = new Label();
        
        private readonly Label lblGName = new Label();
        private readonly TrackBar trValG = new TrackBar();
        private readonly Label lblValG = new Label();
        
        private readonly Label lblBName = new Label();
        private readonly TrackBar trValB = new TrackBar();
        private readonly Label lblValB = new Label();

        private readonly PictureBox pbColorPreview = new PictureBox();
        private readonly Button btnApplyPixel = new Button();
        private readonly Button btnFillColor = new Button();

        // Channel visualize controls
        private readonly Button btnSplit = new Button();
        private readonly Button btnCvtGray = new Button();
        private readonly Button btnResetGrid = new Button();
        
        private readonly PictureBox pbRed = new PictureBox();
        private readonly PictureBox pbGreen = new PictureBox();
        private readonly PictureBox pbBlue = new PictureBox();
        private readonly PictureBox pbGray = new PictureBox();

        private enum ColorMask
        {
            Red,
            Green,
            Blue,
            Gray
        }

        public FormCh07()
        {
            Text = "CH07 - OpenCV IplImage 구조의 이해 실습";
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

            InitializeTheoryTab();
            InitializeLabTab();

            // Create default simulation image on load
            CreateSimulationImage(8, 8, BitDepth.U8, 3);
        }

        private void InitializeTheoryTab()
        {
            // Title Header
            var lblTitle = new Label
            {
                Text = "007. OpenCV IplImage 구조의 이해 핵심 이론 학습",
                Font = new Font("Malgun Gothic", 14F, FontStyle.Bold),
                Location = new Point(20, 20),
                AutoSize = true,
                ForeColor = Color.DarkSlateBlue
            };
            tabTheory.Controls.Add(lblTitle);

            // Theory Text Box
            var txtTheoryText = new RichTextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = RichTextBoxScrollBars.Vertical,
                Location = new Point(20, 60),
                Size = new Size(550, 290),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(252, 252, 248),
                Font = new Font("Malgun Gothic", 9.5F)
            };
            tabTheory.Controls.Add(txtTheoryText);

            string theoryText = "■ [IplImage 생성을 위한 3대 요소]\r\n" +
                               "`IplImage` 객체를 올바르게 생성하고 메모리를 정상 할당하기 위해 3가지 요소를 설정해야 합니다.\r\n\r\n" +
                               "  - **1. 이미지 크기 (src.Size)**: 이미지 변수의 가로(Width)와 세로(Height) 크기를 픽셀 단위로 설정합니다. (예: `Size(4, 2)`)\r\n" +
                               "  - **2. 정밀도 (BitDepth)**: 비트 깊이를 통해 각 화소 데이터가 몇 비트로 표현되는지, 부호가 있는지 여부를 설정합니다. (예: `U8` - 8비트 부호 없음, `S16` - 16비트 부호 있음, `F32` - 32비트 실수)\r\n" +
                               "  - **3. 채널 (Channels)**: 이미지의 색상 채널 수를 설정합니다. (예: `1` - 그레이스케일 흑백, `3` - RGB/BGR 컬러)\r\n\r\n" +
                               "--------------------------------------------------\r\n\r\n" +
                               "■ [이미지 색상 및 채널 구조]\r\n" +
                               "컬러 이미지에서 그레이스케일 이미지로 변환할 때, 3개 채널(BGR)의 색상 정보를 단일 채널 밝기 정보로 계산 및 압축합니다.\r\n" +
                               "  - **Cv.CvtColor**: 이미지의 색상 공간을 변환하는 함수로, 컬러에서 그레이스케일로의 변환(`BgrToGray`) 등을 담당합니다.\r\n" +
                               "  - **BGR 채널 순서**: OpenCV의 기본 색상 표현 순서는 RGB가 아니라 **BGR** 순서(Blue, Green, Red)입니다. 메모리 상에 첫 번째 바이트는 파랑(B), 두 번째는 초록(G), 세 번째는 빨강(R) 값이 저장됩니다.\r\n\r\n" +
                               "--------------------------------------------------\r\n\r\n" +
                               "■ [픽셀 색상 편집 및 메모리 오프셋]\r\n" +
                               "  - **픽셀 색상 편집**: 각 화소의 R, G, B 채널 값을 직접 조정하여 원하는 색상을 조합해 냅니다.\r\n" +
                               "  - **메모리 오프셋 계산 공식**:\r\n" +
                               "    $Offset = Y \\times WidthStep + X \\times (NChannels \\times BytesPerPixel)$\r\n" +
                               "  - **WidthStep**: 이미지의 한 행(Row)을 메모리 상에서 가리키는 데 필요한 실제 바이트 크기입니다. 윈도우 OS/OpenCV에서는 4바이트 단위 정렬(Alignment) 규칙으로 인해 `Width * NChannels * BytesPerPixel`보다 큰 값으로 자동 보정될 수 있습니다.";
            
            RichTextHelper.SetMarkdown(txtTheoryText, theoryText);

            // Quiz Section Panel
            var pnlQuiz = new Panel
            {
                Location = new Point(20, 365),
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
            grpQuiz1.Text = "질문 1. IplImage 3대 핵심 요소 구성";
            grpQuiz1.Font = new Font("Malgun Gothic", 9F, FontStyle.Bold);
            grpQuiz1.Location = new Point(10, 35);
            grpQuiz1.Size = new Size(530, 95);
            pnlQuiz.Controls.Add(grpQuiz1);

            var txtQ1Text = new TextBox
            {
                Text = "IplImage 객체를 생성하고 메모리를 할당하기 위한 3대 핵심 요소는 이미지 크기(Size), 정밀도(BitDepth), 채널(Channels) 수 이다.",
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
            rdoQ1O.Location = new Point(20, 62);
            rdoQ1O.Size = new Size(80, 20);
            rdoQ1O.Font = new Font("Malgun Gothic", 9F, FontStyle.Regular);
            grpQuiz1.Controls.Add(rdoQ1O);

            rdoQ1X.Text = "X (거짓)";
            rdoQ1X.Location = new Point(120, 62);
            rdoQ1X.Size = new Size(80, 20);
            rdoQ1X.Font = new Font("Malgun Gothic", 9F, FontStyle.Regular);
            grpQuiz1.Controls.Add(rdoQ1X);

            lblQ1Result.Location = new Point(220, 62);
            lblQ1Result.Size = new Size(300, 25);
            lblQ1Result.Font = new Font("Malgun Gothic", 8.5F, FontStyle.Regular);
            lblQ1Result.ForeColor = Color.DarkGray;
            lblQ1Result.Text = "정답 확인 시 해설이 여기에 표시됩니다.";
            grpQuiz1.Controls.Add(lblQ1Result);

            // Quiz 2
            grpQuiz2.Text = "질문 2. OpenCV의 픽셀 채널 배치 순서";
            grpQuiz2.Font = new Font("Malgun Gothic", 9F, FontStyle.Bold);
            grpQuiz2.Location = new Point(10, 135);
            grpQuiz2.Size = new Size(530, 95);
            pnlQuiz.Controls.Add(grpQuiz2);

            var txtQ2Text = new TextBox
            {
                Text = "OpenCV는 메모리 배치 방식으로 RGB 순서를 사용하므로, 3채널 IplImage의 imageData 포인터에서 오프셋 0은 빨간색(Red) 채널 값을 나타낸다.",
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
            rdoQ2O.Location = new Point(20, 62);
            rdoQ2O.Size = new Size(80, 20);
            rdoQ2O.Font = new Font("Malgun Gothic", 9F, FontStyle.Regular);
            grpQuiz2.Controls.Add(rdoQ2O);

            rdoQ2X.Text = "X (거짓)";
            rdoQ2X.Location = new Point(120, 62);
            rdoQ2X.Size = new Size(80, 20);
            rdoQ2X.Font = new Font("Malgun Gothic", 9F, FontStyle.Regular);
            grpQuiz2.Controls.Add(rdoQ2X);

            lblQ2Result.Location = new Point(220, 62);
            lblQ2Result.Size = new Size(300, 25);
            lblQ2Result.Font = new Font("Malgun Gothic", 8.5F, FontStyle.Regular);
            lblQ2Result.ForeColor = Color.DarkGray;
            lblQ2Result.Text = "정답 확인 시 해설이 여기에 표시됩니다.";
            grpQuiz2.Controls.Add(lblQ2Result);

            // Check Answers Button
            btnCheckAnswers.Text = "정답 확인 및 해설 보기";
            btnCheckAnswers.Location = new Point(10, 240);
            btnCheckAnswers.Size = new Size(530, 35);
            btnCheckAnswers.Font = new Font("Malgun Gothic", 10F, FontStyle.Bold);
            btnCheckAnswers.BackColor = Color.SteelBlue;
            btnCheckAnswers.ForeColor = Color.White;
            btnCheckAnswers.FlatStyle = FlatStyle.Flat;
            btnCheckAnswers.Click += BtnCheckAnswers_Click;
            pnlQuiz.Controls.Add(btnCheckAnswers);

            // Go to Lab Button
            btnGoToLab.Text = "실습 실험실로 이동하기 (Go to Lab) ▶";
            btnGoToLab.Location = new Point(10, 280);
            btnGoToLab.Size = new Size(530, 38);
            btnGoToLab.Font = new Font("Malgun Gothic", 11F, FontStyle.Bold);
            btnGoToLab.BackColor = Color.ForestGreen;
            btnGoToLab.ForeColor = Color.White;
            btnGoToLab.FlatStyle = FlatStyle.Flat;
            btnGoToLab.Click += (s, e) => tabControl.SelectedTab = tabLab;
            pnlQuiz.Controls.Add(btnGoToLab);

            // --- Right Side Diagram ---
            var lblDiagramTitle = new Label
            {
                Text = "🖼 시각 자료: OpenCV IplImage 구조 학습 인포그래픽",
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
            string imgPath = GetImagePath("ch07_iplimage_ko.png");
            if (imgPath != null)
            {
                picDiagram.Image = Image.FromFile(imgPath);
            }
            tabTheory.Controls.Add(picDiagram);

            var txtDiagramDesc = new RichTextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = RichTextBoxScrollBars.Vertical,
                Location = new Point(590, 535),
                Size = new Size(580, 150),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(250, 250, 252),
                Font = new Font("Malgun Gothic", 9.5F)
            };
            tabTheory.Controls.Add(txtDiagramDesc);

            string diagramDesc = "【인포그래픽 설명】\r\n" +
                                 "1. **IplImage 생성 3대 요소**: 크기(Size), 정밀도(BitDepth), 채널(Channels)이 결합하여 이미지 변수의 구조를 잡습니다.\r\n" +
                                 "2. **이미지 색상 및 채널 구조**: 3채널 BGR 이미지를 `Cv.CvtColor`로 압축해 1채널 흑백 이미지로 축소하는 원리 및 BGR의 메모리 상 순서 정보를 요약합니다.\r\n" +
                                 "3. **픽셀 색상 편집**: 3개 채널이 쌓여 색상을 조합하는 구조와 채널 분리를 통해 각 픽셀이 R, G, B 강도를 지니고 시각화되는 과정을 다룹니다.";
            RichTextHelper.SetMarkdown(txtDiagramDesc, diagramDesc);
        }

        private void BtnCheckAnswers_Click(object sender, EventArgs e)
        {
            // Q1 Check (Answer: O)
            if (rdoQ1O.Checked)
            {
                lblQ1Result.ForeColor = Color.Green;
                lblQ1Result.Text = "정답! 크기, 비트 깊이, 채널 수의 조합이 이미지의 전체 크기 정보와 픽셀 데이터 포맷을 완벽하게 규정합니다.";
            }
            else if (rdoQ1X.Checked)
            {
                lblQ1Result.ForeColor = Color.Red;
                lblQ1Result.Text = "오답입니다. 이 세 가지 정보는 IplImage의 메모리 할당 및 접근에 필요한 3대 기둥입니다.";
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
                lblQ2Result.Text = "정답! OpenCV는 기본 색상 순서로 BGR(파랑, 초록, 빨강)을 사용하므로, 오프셋 0은 파란색(Blue) 채널입니다.";
            }
            else if (rdoQ2O.Checked)
            {
                lblQ2Result.ForeColor = Color.Red;
                lblQ2Result.Text = "오답입니다. OpenCV는 RGB가 아닌 BGR 순서로 픽셀을 배치하므로 첫 채널은 파란색(Blue)입니다.";
            }
            else
            {
                lblQ2Result.ForeColor = Color.OrangeRed;
                lblQ2Result.Text = "답안을 먼저 체크해 주세요.";
            }
        }

        private void InitializeLabTab()
        {
            // Left sidebar: Setup and Info (X: 20 ~ 530)
            var grpSetup = new GroupBox
            {
                Text = "1. IplImage 3대 요소 설정 (시뮬레이터)",
                Font = new Font("Malgun Gothic", 9.5F, FontStyle.Bold),
                Location = new Point(20, 20),
                Size = new Size(510, 180)
            };
            tabLab.Controls.Add(grpSetup);

            grpSetup.Controls.Add(new Label { Text = "Width:", Location = new Point(15, 30), AutoSize = true, Font = new Font("Malgun Gothic", 9F, FontStyle.Regular) });
            numWidth.Location = new Point(70, 28);
            numWidth.Size = new Size(70, 20);
            numWidth.Minimum = 4;
            numWidth.Maximum = 16;
            numWidth.Value = 8;
            numWidth.Font = new Font("Malgun Gothic", 9F, FontStyle.Regular);
            grpSetup.Controls.Add(numWidth);

            grpSetup.Controls.Add(new Label { Text = "Height:", Location = new Point(160, 30), AutoSize = true, Font = new Font("Malgun Gothic", 9F, FontStyle.Regular) });
            numHeight.Location = new Point(215, 28);
            numHeight.Size = new Size(70, 20);
            numHeight.Minimum = 4;
            numHeight.Maximum = 16;
            numHeight.Value = 8;
            numHeight.Font = new Font("Malgun Gothic", 9F, FontStyle.Regular);
            grpSetup.Controls.Add(numHeight);

            grpSetup.Controls.Add(new Label { Text = "정밀도 (BitDepth):", Location = new Point(15, 70), AutoSize = true, Font = new Font("Malgun Gothic", 9F, FontStyle.Regular) });
            cmbBitDepth.Location = new Point(130, 67);
            cmbBitDepth.Size = new Size(155, 20);
            cmbBitDepth.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbBitDepth.Items.AddRange(new object[] { "BitDepth.U8 (8비트 부호없음)", "BitDepth.S16 (16비트 부호있음)", "BitDepth.F32 (32비트 실수)" });
            cmbBitDepth.SelectedIndex = 0;
            cmbBitDepth.Font = new Font("Malgun Gothic", 9F, FontStyle.Regular);
            grpSetup.Controls.Add(cmbBitDepth);

            grpSetup.Controls.Add(new Label { Text = "채널 (Channels):", Location = new Point(15, 110), AutoSize = true, Font = new Font("Malgun Gothic", 9F, FontStyle.Regular) });
            cmbChannels.Location = new Point(130, 107);
            cmbChannels.Size = new Size(155, 20);
            cmbChannels.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbChannels.Items.AddRange(new object[] { "1 Channel (Grayscale)", "3 Channels (BGR)" });
            cmbChannels.SelectedIndex = 1;
            cmbChannels.Font = new Font("Malgun Gothic", 9F, FontStyle.Regular);
            grpSetup.Controls.Add(cmbChannels);

            btnCreateImage.Text = "IplImage\r\n객체 생성하기";
            btnCreateImage.Location = new Point(300, 28);
            btnCreateImage.Size = new Size(190, 130);
            btnCreateImage.Font = new Font("Malgun Gothic", 11F, FontStyle.Bold);
            btnCreateImage.BackColor = Color.SteelBlue;
            btnCreateImage.ForeColor = Color.White;
            btnCreateImage.FlatStyle = FlatStyle.Flat;
            btnCreateImage.Click += BtnCreateImage_Click;
            grpSetup.Controls.Add(btnCreateImage);

            var grpInfo = new GroupBox
            {
                Text = "2. 네이티브 객체 속성 정보 (생성된 IplImage)",
                Font = new Font("Malgun Gothic", 9.5F, FontStyle.Bold),
                Location = new Point(20, 210),
                Size = new Size(510, 200)
            };
            tabLab.Controls.Add(grpInfo);

            int yOffset = 25;
            Action<Label, string, Point> setupAttrLabel = (lbl, title, pt) =>
            {
                lbl.Text = title;
                lbl.Location = pt;
                lbl.AutoSize = true;
                lbl.Font = new Font("Consolas", 9.5F, FontStyle.Bold);
                lbl.ForeColor = Color.DarkSlateBlue;
                grpInfo.Controls.Add(lbl);
            };

            setupAttrLabel(lblAttrWidth, "Width      : -", new Point(15, yOffset));
            setupAttrLabel(lblAttrHeight, "Height     : -", new Point(15, yOffset + 25));
            setupAttrLabel(lblAttrWidthStep, "WidthStep  : - (Row Stride)", new Point(15, yOffset + 50));
            setupAttrLabel(lblAttrDepth, "Depth      : -", new Point(15, yOffset + 75));
            
            setupAttrLabel(lblAttrNChannels, "NChannels  : -", new Point(260, yOffset));
            setupAttrLabel(lblAttrImageSize, "ImageSize  : - Bytes", new Point(260, yOffset + 25));
            setupAttrLabel(lblAttrImageData, "ImageData  : 0x- (Ptr)", new Point(260, yOffset + 50));

            var lblAlignTip = new Label
            {
                Text = "※ WidthStep(행 바이트 크기)은 성능 최적화를 위해 Windows OS 기준\r\n   4바이트 경계(Padding)로 자동 올림 정렬됩니다.",
                Location = new Point(15, 140),
                Size = new Size(480, 45),
                Font = new Font("Malgun Gothic", 8.5F, FontStyle.Regular),
                ForeColor = Color.Gray
            };
            grpInfo.Controls.Add(lblAlignTip);

            var grpOffsetCalc = new GroupBox
            {
                Text = "3. 실시간 메모리 포인터 오프셋 계산기",
                Font = new Font("Malgun Gothic", 9.5F, FontStyle.Bold),
                Location = new Point(20, 420),
                Size = new Size(510, 260)
            };
            tabLab.Controls.Add(grpOffsetCalc);

            lblCalcFormula.Text = "공식: Address = ImageData + Y * WidthStep + X * (NChannels * BytesPerValue)";
            lblCalcFormula.Location = new Point(15, 30);
            lblCalcFormula.Size = new Size(480, 20);
            lblCalcFormula.Font = new Font("Consolas", 9F, FontStyle.Bold);
            lblCalcFormula.ForeColor = Color.MediumBlue;
            grpOffsetCalc.Controls.Add(lblCalcFormula);

            lblCalcSteps.Text = "대입: -";
            lblCalcSteps.Location = new Point(15, 60);
            lblCalcSteps.Size = new Size(480, 20);
            lblCalcSteps.Font = new Font("Consolas", 9F, FontStyle.Bold);
            grpOffsetCalc.Controls.Add(lblCalcSteps);

            lblCalcResult.Text = "결과: -";
            lblCalcResult.Location = new Point(15, 90);
            lblCalcResult.Size = new Size(480, 20);
            lblCalcResult.Font = new Font("Consolas", 9F, FontStyle.Bold);
            lblCalcResult.ForeColor = Color.DarkGreen;
            grpOffsetCalc.Controls.Add(lblCalcResult);

            lblMemBytes.Text = "메모리 바이트 값: -";
            lblMemBytes.Location = new Point(15, 120);
            lblMemBytes.Size = new Size(480, 90);
            lblMemBytes.Font = new Font("Consolas", 9.5F, FontStyle.Bold);
            lblMemBytes.ForeColor = Color.Purple;
            grpOffsetCalc.Controls.Add(lblMemBytes);

            var lblOffsetTip = new Label
            {
                Text = "※ 픽셀 그리드에서 화소를 마우스로 클릭하면, 해당 픽셀 메모리 주소가 실시간 자동 연산되어 표기됩니다.",
                Location = new Point(15, 220),
                Size = new Size(480, 30),
                Font = new Font("Malgun Gothic", 8.5F, FontStyle.Regular),
                ForeColor = Color.Gray
            };
            grpOffsetCalc.Controls.Add(lblOffsetTip);

            // Right sidebar: Grid and Editing (X: 550 ~ 1160)
            var grpGrid = new GroupBox
            {
                Text = "4. 픽셀 편집 및 시각화 그리드",
                Font = new Font("Malgun Gothic", 9.5F, FontStyle.Bold),
                Location = new Point(550, 20),
                Size = new Size(610, 660)
            };
            tabLab.Controls.Add(grpGrid);

            pbPixelGrid.Location = new Point(20, 30);
            pbPixelGrid.Size = new Size(280, 280);
            pbPixelGrid.BorderStyle = BorderStyle.FixedSingle;
            pbPixelGrid.BackColor = Color.LightGray;
            pbPixelGrid.Paint += PbPixelGrid_Paint;
            pbPixelGrid.MouseDown += PbPixelGrid_MouseDown;
            grpGrid.Controls.Add(pbPixelGrid);

            grpPixelEdit.Text = "선택된 픽셀 색상 편집";
            grpPixelEdit.Location = new Point(320, 22);
            grpPixelEdit.Size = new Size(270, 290);
            grpPixelEdit.Font = new Font("Malgun Gothic", 9F, FontStyle.Bold);
            grpGrid.Controls.Add(grpPixelEdit);

            lblSelectedPixelInfo.Text = "선택된 픽셀: (X: -, Y: -)";
            lblSelectedPixelInfo.Location = new Point(15, 20);
            lblSelectedPixelInfo.Size = new Size(240, 20);
            lblSelectedPixelInfo.ForeColor = Color.DarkBlue;
            grpPixelEdit.Controls.Add(lblSelectedPixelInfo);

            int sY = 45;
            Action<Label, TrackBar, Label, string, int> setupSlider = (lblTitle, tr, lblVal, name, defaultVal) =>
            {
                lblTitle.Text = name;
                lblTitle.Location = new Point(15, sY);
                lblTitle.AutoSize = true;
                lblTitle.Font = new Font("Malgun Gothic", 8.5F, FontStyle.Regular);
                grpPixelEdit.Controls.Add(lblTitle);

                tr.Location = new Point(10, sY + 15);
                tr.Size = new Size(160, 40);
                tr.Minimum = 0;
                tr.Maximum = 255;
                tr.Value = defaultVal;
                tr.TickStyle = TickStyle.None;
                tr.Scroll += (s, e) => {
                    lblVal.Text = tr.Value.ToString();
                    OnSliderScroll();
                };
                grpPixelEdit.Controls.Add(tr);

                lblVal.Text = defaultVal.ToString();
                lblVal.Location = new Point(175, sY + 18);
                lblVal.Size = new Size(30, 20);
                lblVal.Font = new Font("Consolas", 9F, FontStyle.Regular);
                grpPixelEdit.Controls.Add(lblVal);

                sY += 50;
            };

            setupSlider(lblRName, trValR, lblValR, "Red (R) 채널:", 180);
            setupSlider(lblGName, trValG, lblValG, "Green (G) 채널:", 70);
            setupSlider(lblBName, trValB, lblValB, "Blue (B) 채널:", 220);

            pbColorPreview.Location = new Point(215, 60);
            pbColorPreview.Size = new Size(45, 115);
            pbColorPreview.BorderStyle = BorderStyle.FixedSingle;
            pbColorPreview.BackColor = Color.FromArgb(180, 70, 220);
            grpPixelEdit.Controls.Add(pbColorPreview);

            btnApplyPixel.Text = "픽셀 값 적용";
            btnApplyPixel.Location = new Point(15, 205);
            btnApplyPixel.Size = new Size(110, 32);
            btnApplyPixel.BackColor = Color.DarkSlateGray;
            btnApplyPixel.ForeColor = Color.White;
            btnApplyPixel.FlatStyle = FlatStyle.Flat;
            btnApplyPixel.Click += BtnApplyPixel_Click;
            grpPixelEdit.Controls.Add(btnApplyPixel);

            btnFillColor.Text = "단색 채우기";
            btnFillColor.Location = new Point(135, 205);
            btnFillColor.Size = new Size(120, 32);
            btnFillColor.BackColor = Color.Silver;
            btnFillColor.FlatStyle = FlatStyle.Flat;
            btnFillColor.Click += BtnFillColor_Click;
            grpPixelEdit.Controls.Add(btnFillColor);

            var lblEditTip = new Label
            {
                Text = "💡 픽셀 클릭 → 슬라이더로 BGR 조정 → 적용 클릭\r\n  (OpenCV는 내부적으로 Blue-Green-Red 순)",
                Location = new Point(15, 245),
                Size = new Size(240, 40),
                Font = new Font("Malgun Gothic", 8F, FontStyle.Regular),
                ForeColor = Color.Gray
            };
            grpPixelEdit.Controls.Add(lblEditTip);

            // Real-time channel separation visualizer
            var pnlChannels = new Panel
            {
                Location = new Point(20, 320),
                Size = new Size(570, 320),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };
            grpGrid.Controls.Add(pnlChannels);

            var lblSplitTitle = new Label
            {
                Text = "🧪 실시간 BGR 채널 분리 (Cv.Split) & Gray 변환 (Cv.CvtColor) 시각화",
                Font = new Font("Malgun Gothic", 9.5F, FontStyle.Bold),
                Location = new Point(15, 10),
                AutoSize = true,
                ForeColor = Color.Navy
            };
            pnlChannels.Controls.Add(lblSplitTitle);

            btnSplit.Text = "채널 분리 (Cv.Split)";
            btnSplit.Location = new Point(15, 30);
            btnSplit.Size = new Size(160, 30);
            btnSplit.BackColor = Color.Orange;
            btnSplit.ForeColor = Color.White;
            btnSplit.FlatStyle = FlatStyle.Flat;
            btnSplit.Font = new Font("Malgun Gothic", 9F, FontStyle.Bold);
            btnSplit.Click += BtnSplit_Click;
            pnlChannels.Controls.Add(btnSplit);

            btnCvtGray.Text = "Gray 변환 (CvtColor)";
            btnCvtGray.Location = new Point(185, 30);
            btnCvtGray.Size = new Size(160, 30);
            btnCvtGray.BackColor = Color.DimGray;
            btnCvtGray.ForeColor = Color.White;
            btnCvtGray.FlatStyle = FlatStyle.Flat;
            btnCvtGray.Font = new Font("Malgun Gothic", 9F, FontStyle.Bold);
            btnCvtGray.Click += BtnCvtGray_Click;
            pnlChannels.Controls.Add(btnCvtGray);

            btnResetGrid.Text = "기본 그리드 리셋";
            btnResetGrid.Location = new Point(355, 30);
            btnResetGrid.Size = new Size(130, 30);
            btnResetGrid.Click += BtnResetGrid_Click;
            pnlChannels.Controls.Add(btnResetGrid);

            int picY = 70;
            Action<PictureBox, string, int> setupSmallPic = (pb, title, x) =>
            {
                pb.Location = new Point(x, picY);
                pb.Size = new Size(120, 120);
                pb.BorderStyle = BorderStyle.FixedSingle;
                pb.BackColor = Color.LightGray;
                pnlChannels.Controls.Add(pb);

                var lbl = new Label
                {
                    Text = title,
                    Location = new Point(x, picY + 125),
                    Size = new Size(120, 20),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Malgun Gothic", 8.5F, FontStyle.Bold)
                };
                pnlChannels.Controls.Add(lbl);
            };

            setupSmallPic(pbBlue, "Blue (B) 채널", 15);
            setupSmallPic(pbGreen, "Green (G) 채널", 150);
            setupSmallPic(pbRed, "Red (R) 채널", 285);
            setupSmallPic(pbGray, "Grayscale 변환", 420);

            pbRed.Paint += (s, e) => PaintChannelGrid(e, splitR, ColorMask.Red);
            pbGreen.Paint += (s, e) => PaintChannelGrid(e, splitG, ColorMask.Green);
            pbBlue.Paint += (s, e) => PaintChannelGrid(e, splitB, ColorMask.Blue);
            pbGray.Paint += (s, e) => PaintChannelGrid(e, grayImg, ColorMask.Gray);
        }

        private void OnSliderScroll()
        {
            if (updatingSliders) return;
            pbColorPreview.BackColor = Color.FromArgb(trValR.Value, trValG.Value, trValB.Value);
        }

        private void BtnCreateImage_Click(object sender, EventArgs e)
        {
            int w = (int)numWidth.Value;
            int h = (int)numHeight.Value;

            BitDepth depth = BitDepth.U8;
            if (cmbBitDepth.SelectedIndex == 1) depth = BitDepth.S16;
            else if (cmbBitDepth.SelectedIndex == 2) depth = BitDepth.F32;

            int ch = (cmbChannels.SelectedIndex == 0) ? 1 : 3;

            CreateSimulationImage(w, h, depth, ch);
        }

        private void CreateSimulationImage(int w, int h, BitDepth depth, int ch)
        {
            SafeDisposeSimImage();

            try
            {
                simImage = new IplImage(new CvSize(w, h), depth, ch);

                lblAttrWidth.Text = $"Width      : {simImage.Width}";
                lblAttrHeight.Text = $"Height     : {simImage.Height}";
                lblAttrWidthStep.Text = $"WidthStep  : {simImage.WidthStep} Bytes";
                lblAttrDepth.Text = $"Depth      : {simImage.Depth}";
                lblAttrNChannels.Text = $"NChannels  : {simImage.NChannels}";
                lblAttrImageSize.Text = $"ImageSize  : {simImage.ImageSize:N0} Bytes";
                lblAttrImageData.Text = $"ImageData  : 0x{simImage.ImageData.ToInt64():X}";

                ConfigureSlidersForChannel();
                
                // Select first pixel by default
                selectedX = 0;
                selectedY = 0;

                ResetToPattern();
            }
            catch (Exception ex)
            {
                MessageBox.Show("IplImage 생성 중 오류 발생:\n" + ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ResetToPattern()
        {
            if (simImage == null) return;
            int w = simImage.Width;
            int h = simImage.Height;
            int ch = simImage.NChannels;
            int bytesPerVal = GetBytesPerVal(simImage.Depth);

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int offset = y * simImage.WidthStep + x * ch * bytesPerVal;
                    if (ch == 1)
                    {
                        // Checkerboard or diagonal gradient for Gray
                        double gVal = ((x + y) % 2 == 0) ? 200 : 50;
                        SetPixelVal(simImage.ImageData, offset, simImage.Depth, MapFromByte((int)gVal, simImage.Depth));
                    }
                    else
                    {
                        // BGR pattern: interesting color gradient
                        int r = x * 255 / (w - 1);
                        int g = y * 255 / (h - 1);
                        int b = (x + y) * 255 / (w + h - 2);

                        SetPixelVal(simImage.ImageData, offset, simImage.Depth, MapFromByte(b, simImage.Depth));
                        SetPixelVal(simImage.ImageData, offset + bytesPerVal, simImage.Depth, MapFromByte(g, simImage.Depth));
                        SetPixelVal(simImage.ImageData, offset + 2 * bytesPerVal, simImage.Depth, MapFromByte(r, simImage.Depth));
                    }
                }
            }
            pbPixelGrid.Invalidate();
            UpdateOffsetDisplay();

            // Clear old split previews
            if (splitR != null) { Cv.ReleaseImage(splitR); splitR = null; }
            if (splitG != null) { Cv.ReleaseImage(splitG); splitG = null; }
            if (splitB != null) { Cv.ReleaseImage(splitB); splitB = null; }
            if (grayImg != null) { Cv.ReleaseImage(grayImg); grayImg = null; }

            pbRed.Invalidate();
            pbGreen.Invalidate();
            pbBlue.Invalidate();
            pbGray.Invalidate();
        }

        private void BtnResetGrid_Click(object sender, EventArgs e)
        {
            ResetToPattern();
        }

        private void BtnApplyPixel_Click(object sender, EventArgs e)
        {
            if (simImage == null) return;
            int w = simImage.Width;
            int h = simImage.Height;
            if (selectedX < 0 || selectedX >= w || selectedY < 0 || selectedY >= h) return;

            int ch = simImage.NChannels;
            int bytesPerVal = GetBytesPerVal(simImage.Depth);
            int offset = selectedY * simImage.WidthStep + selectedX * ch * bytesPerVal;

            if (ch == 1)
            {
                double val = MapFromByte(trValR.Value, simImage.Depth);
                SetPixelVal(simImage.ImageData, offset, simImage.Depth, val);
            }
            else
            {
                double rVal = MapFromByte(trValR.Value, simImage.Depth);
                double gVal = MapFromByte(trValG.Value, simImage.Depth);
                double bVal = MapFromByte(trValB.Value, simImage.Depth);

                SetPixelVal(simImage.ImageData, offset, simImage.Depth, bVal);
                SetPixelVal(simImage.ImageData, offset + bytesPerVal, simImage.Depth, gVal);
                SetPixelVal(simImage.ImageData, offset + 2 * bytesPerVal, simImage.Depth, rVal);
            }

            pbPixelGrid.Invalidate();
            UpdateOffsetDisplay();
            UpdateChannelPreviews();
        }

        private void BtnFillColor_Click(object sender, EventArgs e)
        {
            if (simImage == null) return;
            int w = simImage.Width;
            int h = simImage.Height;
            int ch = simImage.NChannels;
            int bytesPerVal = GetBytesPerVal(simImage.Depth);

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int offset = y * simImage.WidthStep + x * ch * bytesPerVal;
                    if (ch == 1)
                    {
                        double val = MapFromByte(trValR.Value, simImage.Depth);
                        SetPixelVal(simImage.ImageData, offset, simImage.Depth, val);
                    }
                    else
                    {
                        double rVal = MapFromByte(trValR.Value, simImage.Depth);
                        double gVal = MapFromByte(trValG.Value, simImage.Depth);
                        double bVal = MapFromByte(trValB.Value, simImage.Depth);

                        SetPixelVal(simImage.ImageData, offset, simImage.Depth, bVal);
                        SetPixelVal(simImage.ImageData, offset + bytesPerVal, simImage.Depth, gVal);
                        SetPixelVal(simImage.ImageData, offset + 2 * bytesPerVal, simImage.Depth, rVal);
                    }
                }
            }

            pbPixelGrid.Invalidate();
            UpdateOffsetDisplay();
            UpdateChannelPreviews();
        }

        private void BtnSplit_Click(object sender, EventArgs e)
        {
            if (simImage == null) return;
            if (simImage.NChannels != 3)
            {
                MessageBox.Show("채널 분리는 3채널 이미지(BGR)에서만 동작합니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (splitR != null) { Cv.ReleaseImage(splitR); splitR = null; }
            if (splitG != null) { Cv.ReleaseImage(splitG); splitG = null; }
            if (splitB != null) { Cv.ReleaseImage(splitB); splitB = null; }

            splitR = new IplImage(simImage.Size, simImage.Depth, 1);
            splitG = new IplImage(simImage.Size, simImage.Depth, 1);
            splitB = new IplImage(simImage.Size, simImage.Depth, 1);

            Cv.Split(simImage, splitB, splitG, splitR, null); // OpenCV Split: B, G, R

            pbRed.Invalidate();
            pbGreen.Invalidate();
            pbBlue.Invalidate();
        }

        private void BtnCvtGray_Click(object sender, EventArgs e)
        {
            if (simImage == null) return;
            if (simImage.NChannels == 1)
            {
                MessageBox.Show("이미 1채널 그레이스케일 상태입니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (grayImg != null) { Cv.ReleaseImage(grayImg); grayImg = null; }

            grayImg = new IplImage(simImage.Size, simImage.Depth, 1);
            Cv.CvtColor(simImage, grayImg, ColorConversion.BgrToGray);

            pbGray.Invalidate();
        }

        private void PbPixelGrid_Paint(object sender, PaintEventArgs e)
        {
            if (simImage == null)
            {
                e.Graphics.Clear(Color.DarkGray);
                using (var font = new Font("Malgun Gothic", 10, FontStyle.Bold))
                {
                    var size = e.Graphics.MeasureString("IplImage를 생성해 주세요.", font);
                    e.Graphics.DrawString("IplImage를 생성해 주세요.", font, Brushes.White, (pbPixelGrid.Width - size.Width) / 2, (pbPixelGrid.Height - size.Height) / 2);
                }
                return;
            }

            int w = simImage.Width;
            int h = simImage.Height;
            int cellW = pbPixelGrid.Width / w;
            int cellH = pbPixelGrid.Height / h;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    Color c = GetPixelColor(x, y);
                    using (var brush = new SolidBrush(c))
                    {
                        e.Graphics.FillRectangle(brush, x * cellW, y * cellH, cellW, cellH);
                    }
                    using (var pen = new Pen(Color.FromArgb(80, 80, 80), 1))
                    {
                        e.Graphics.DrawRectangle(pen, x * cellW, y * cellH, cellW, cellH);
                    }
                }
            }

            // Highlight selected pixel
            if (selectedX >= 0 && selectedX < w && selectedY >= 0 && selectedY < h)
            {
                using (var pen = new Pen(Color.Yellow, 3))
                {
                    e.Graphics.DrawRectangle(pen, selectedX * cellW, selectedY * cellH, cellW, cellH);
                }
            }
        }

        private void PbPixelGrid_MouseDown(object sender, MouseEventArgs e)
        {
            if (simImage == null) return;
            int w = simImage.Width;
            int h = simImage.Height;
            int cellW = pbPixelGrid.Width / w;
            int cellH = pbPixelGrid.Height / h;

            int x = e.X / cellW;
            int y = e.Y / cellH;

            if (x >= 0 && x < w && y >= 0 && y < h)
            {
                selectedX = x;
                selectedY = y;
                pbPixelGrid.Invalidate();
                UpdateOffsetDisplay();
            }
        }

        private void PaintChannelGrid(PaintEventArgs e, IplImage channelImg, ColorMask mask)
        {
            if (channelImg == null)
            {
                e.Graphics.Clear(Color.DarkGray);
                using (var font = new Font("Malgun Gothic", 9, FontStyle.Regular))
                {
                    var size = e.Graphics.MeasureString("미활성", font);
                    e.Graphics.DrawString("미활성", font, Brushes.White, (120 - size.Width) / 2, (120 - size.Height) / 2);
                }
                return;
            }

            int w = channelImg.Width;
            int h = channelImg.Height;
            int cellW = 120 / w;
            int cellH = 120 / h;
            int bytesPerVal = GetBytesPerVal(channelImg.Depth);

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int offset = y * channelImg.WidthStep + x * bytesPerVal;
                    double rawVal = GetPixelVal(channelImg.ImageData, offset, channelImg.Depth);
                    int val = MapToByte(rawVal, channelImg.Depth);

                    Color c;
                    if (mask == ColorMask.Red) c = Color.FromArgb(val, 0, 0);
                    else if (mask == ColorMask.Green) c = Color.FromArgb(0, val, 0);
                    else if (mask == ColorMask.Blue) c = Color.FromArgb(0, 0, val);
                    else c = Color.FromArgb(val, val, val); // Gray

                    using (var brush = new SolidBrush(c))
                    {
                        e.Graphics.FillRectangle(brush, x * cellW, y * cellH, cellW, cellH);
                    }
                    using (var pen = new Pen(Color.FromArgb(50, 50, 50), 1))
                    {
                        e.Graphics.DrawRectangle(pen, x * cellW, y * cellH, cellW, cellH);
                    }
                }
            }
        }

        private void ConfigureSlidersForChannel()
        {
            if (simImage == null) return;
            int ch = simImage.NChannels;
            if (ch == 1)
            {
                lblRName.Text = "Brightness (Gray):";
                trValG.Visible = false;
                lblGName.Visible = false;
                lblValG.Visible = false;
                trValB.Visible = false;
                lblBName.Visible = false;
                lblValB.Visible = false;
            }
            else
            {
                lblRName.Text = "Red (R) 채널:";
                trValG.Visible = true;
                lblGName.Visible = true;
                lblValG.Visible = true;
                trValB.Visible = true;
                lblBName.Visible = true;
                lblValB.Visible = true;
            }
        }

        private void UpdateOffsetDisplay()
        {
            if (simImage == null || selectedX < 0 || selectedX >= simImage.Width || selectedY < 0 || selectedY >= simImage.Height)
            {
                lblSelectedPixelInfo.Text = "선택된 픽셀: (X: -, Y: -)";
                lblCalcFormula.Text = "공식: Address = ImageData + Y * WidthStep + X * (NChannels * BytesPerValue)";
                lblCalcSteps.Text = "대입: -";
                lblCalcResult.Text = "결과: -";
                lblMemBytes.Text = "메모리 바이트 값: -";
                return;
            }

            int x = selectedX;
            int y = selectedY;
            int wStep = simImage.WidthStep;
            int ch = simImage.NChannels;
            int bytesPerVal = GetBytesPerVal(simImage.Depth);
            long baseAddr = simImage.ImageData.ToInt64();

            int offset = y * wStep + x * ch * bytesPerVal;
            long pixelAddr = baseAddr + offset;

            lblSelectedPixelInfo.Text = $"선택된 픽셀: (X: {x}, Y: {y})";

            lblCalcFormula.Text = "공식: Address = ImageData + Y * WidthStep + X * (NChannels * BytesPerValue)";
            lblCalcSteps.Text = $"대입: 0x{baseAddr:X} + {y} * {wStep} + {x} * ({ch} * {bytesPerVal})";
            lblCalcResult.Text = $"결과: 0x{baseAddr:X} + {offset} = 0x{pixelAddr:X}";

            if (ch == 1)
            {
                double val = GetPixelVal(simImage.ImageData, offset, simImage.Depth);
                lblMemBytes.Text = $"메모리 바이트 값 (Gray):\r\n  - Offset + 0: {val:F4} (실제 저장 값)";

                updatingSliders = true;
                int byteVal = MapToByte(val, simImage.Depth);
                trValR.Value = byteVal;
                lblValR.Text = byteVal.ToString();
                updatingSliders = false;

                pbColorPreview.BackColor = Color.FromArgb(byteVal, byteVal, byteVal);
            }
            else
            {
                double bVal = GetPixelVal(simImage.ImageData, offset, simImage.Depth);
                double gVal = GetPixelVal(simImage.ImageData, offset + bytesPerVal, simImage.Depth);
                double rVal = GetPixelVal(simImage.ImageData, offset + 2 * bytesPerVal, simImage.Depth);

                int r = MapToByte(rVal, simImage.Depth);
                int g = MapToByte(gVal, simImage.Depth);
                int b = MapToByte(bVal, simImage.Depth);

                lblMemBytes.Text = $"메모리 바이트 값 (BGR 순서):\r\n  - B (Offset + 0): {bVal:F4}\r\n  - G (Offset + {bytesPerVal}): {gVal:F4}\r\n  - R (Offset + {2 * bytesPerVal}): {rVal:F4}";

                updatingSliders = true;
                trValR.Value = r;
                lblValR.Text = r.ToString();
                trValG.Value = g;
                lblValG.Text = g.ToString();
                trValB.Value = b;
                lblValB.Text = b.ToString();
                updatingSliders = false;

                pbColorPreview.BackColor = Color.FromArgb(r, g, b);
            }
        }

        private Color GetPixelColor(int x, int y)
        {
            if (simImage == null) return Color.Black;

            int ch = simImage.NChannels;
            int bytesPerVal = GetBytesPerVal(simImage.Depth);
            int offset = y * simImage.WidthStep + x * ch * bytesPerVal;

            if (ch == 1)
            {
                double grayVal = GetPixelVal(simImage.ImageData, offset, simImage.Depth);
                int g = MapToByte(grayVal, simImage.Depth);
                return Color.FromArgb(g, g, g);
            }
            else
            {
                double bVal = GetPixelVal(simImage.ImageData, offset, simImage.Depth);
                double gVal = GetPixelVal(simImage.ImageData, offset + bytesPerVal, simImage.Depth);
                double rVal = GetPixelVal(simImage.ImageData, offset + 2 * bytesPerVal, simImage.Depth);

                int r = MapToByte(rVal, simImage.Depth);
                int g = MapToByte(gVal, simImage.Depth);
                int b = MapToByte(bVal, simImage.Depth);
                return Color.FromArgb(r, g, b);
            }
        }

        private double GetPixelVal(IntPtr basePtr, int offset, BitDepth depth)
        {
            IntPtr ptr = new IntPtr(basePtr.ToInt64() + offset);
            if (depth == BitDepth.U8)
            {
                return System.Runtime.InteropServices.Marshal.ReadByte(ptr);
            }
            else if (depth == BitDepth.S16)
            {
                return System.Runtime.InteropServices.Marshal.ReadInt16(ptr);
            }
            else if (depth == BitDepth.F32)
            {
                float[] buf = new float[1];
                System.Runtime.InteropServices.Marshal.Copy(ptr, buf, 0, 1);
                return buf[0];
            }
            return 0;
        }

        private void SetPixelVal(IntPtr basePtr, int offset, BitDepth depth, double val)
        {
            IntPtr ptr = new IntPtr(basePtr.ToInt64() + offset);
            if (depth == BitDepth.U8)
            {
                System.Runtime.InteropServices.Marshal.WriteByte(ptr, (byte)Math.Max(0, Math.Min(255, val)));
            }
            else if (depth == BitDepth.S16)
            {
                System.Runtime.InteropServices.Marshal.WriteInt16(ptr, (short)Math.Max(short.MinValue, Math.Min(short.MaxValue, val)));
            }
            else if (depth == BitDepth.F32)
            {
                float[] buf = new float[] { (float)val };
                System.Runtime.InteropServices.Marshal.Copy(buf, 0, ptr, 1);
            }
        }

        private int GetBytesPerVal(BitDepth depth)
        {
            switch (depth)
            {
                case BitDepth.U8: return 1;
                case BitDepth.S16: return 2;
                case BitDepth.F32: return 4;
                default: return 1;
            }
        }

        private int MapToByte(double val, BitDepth depth)
        {
            if (depth == BitDepth.F32)
            {
                return (int)Math.Max(0, Math.Min(255, val * 255.0));
            }
            else
            {
                return (int)Math.Max(0, Math.Min(255, val));
            }
        }

        private double MapFromByte(int val, BitDepth depth)
        {
            if (depth == BitDepth.F32)
            {
                return val / 255.0;
            }
            else
            {
                return val;
            }
        }

        private void SafeDisposeSimImage()
        {
            if (simImage != null)
            {
                Cv.ReleaseImage(simImage);
                simImage = null;
            }
            if (splitR != null) { Cv.ReleaseImage(splitR); splitR = null; }
            if (splitG != null) { Cv.ReleaseImage(splitG); splitG = null; }
            if (splitB != null) { Cv.ReleaseImage(splitB); splitB = null; }
            if (grayImg != null) { Cv.ReleaseImage(grayImg); grayImg = null; }
        }

        private void UpdateChannelPreviews()
        {
            if (simImage == null) return;

            // Auto update split channels if they exist
            if (splitR != null && simImage.NChannels == 3)
            {
                Cv.Split(simImage, splitB, splitG, splitR, null);
                pbRed.Invalidate();
                pbGreen.Invalidate();
                pbBlue.Invalidate();
            }

            // Auto update gray image if it exists
            if (grayImg != null && simImage.NChannels == 3)
            {
                Cv.CvtColor(simImage, grayImg, ColorConversion.BgrToGray);
                pbGray.Invalidate();
            }
        }

        private string GetImagePath(string filename)
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename);
            if (!File.Exists(path))
            {
                path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\" + filename);
            }
            if (!File.Exists(path))
            {
                path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\ref\\" + filename);
            }
            return File.Exists(path) ? path : null;
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            SafeDisposeSimImage();
            picDiagram.Image?.Dispose();
            base.OnFormClosed(e);
        }
    }
}
