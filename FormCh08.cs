using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using OpenCvSharp;
using OpenCvSharp.Extensions;

using Point = System.Drawing.Point;
using Size = System.Drawing.Size;
using FontStyle = System.Drawing.FontStyle;

namespace OpenCVSharp
{
    public class FormCh08 : Form
    {
        private readonly TabControl tabControl = new TabControl();
        private readonly TabPage tabTheory = new TabPage();
        private readonly TabPage tabLab = new TabPage();
        private readonly PictureBox picDiagram = new PictureBox();

        // Native OpenCV Image variables
        private IplImage srcImage = null;
        private IplImage invertImage = null;
        private IplImage grayImage = null;
        private IplImage binaryImage = null;

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
        // PictureBoxes
        private readonly PictureBox pbOriginal = new PictureBox();
        private readonly PictureBox pbInverted = new PictureBox();
        private readonly PictureBox pbBinarized = new PictureBox();

        // Controls at the bottom
        private readonly GroupBox grpFile = new GroupBox();
        private readonly Button btnOpen = new Button();
        private readonly Button btnLoadDefault = new Button();
        private readonly Label lblFilePath = new Label();

        private readonly GroupBox grpParams = new GroupBox();
        private readonly Label lblThreshold = new Label();
        private readonly TrackBar trThreshold = new TrackBar();
        private readonly Label lblThresholdVal = new Label();
        private readonly ComboBox cmbThresholdType = new ComboBox();
        private readonly Label lblOtsuResult = new Label();
        private readonly Label lblTypeDesc = new Label();

        private readonly GroupBox grpCode = new GroupBox();
        private readonly RichTextBox txtCodePreview = new RichTextBox();

        // File path state
        private string selectedFilePath = "";

        public FormCh08()
        {
            Text = "CH08 - 이미지 색상 반전 & 이진화(Binary) 실습";
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

            UpdateCodePreview();
        }

        private void InitializeTheoryTab()
        {
            // Title Header
            var lblTitle = new Label
            {
                Text = "008. 이미지 색상 반전 & 이진화 핵심 이론 학습",
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

            string theoryText = "■ [색상 반전 (Image Inversion)]\r\n" +
                                "색상 반전은 이미지의 각 화소(Pixel) 밝기/색상 값을 보색 관계로 변환하는 기법입니다.\r\n\r\n" +
                                "  - **공식**: $반전\\ 픽셀\\ 값 = 255 - 원본\\ 픽셀\\ 값$\r\n" +
                                "  - **Cv.Not**: OpenCV에서 이미지 데이터의 모든 비트를 반전(Bitwise NOT)시키는 함수입니다. (`Cv.Not(src, dst)`)\r\n" +
                                "  - BGR 컬러 이미지에서는 Blue, Green, Red 모든 채널에 연산이 각각 적용되어 보색 대비가 생성되고, 그레이스케일 이미지에서는 밝기가 정반대로 전환됩니다.\r\n\r\n" +
                                "--------------------------------------------------\r\n\r\n" +
                                "■ [이미지 이진화 (Image Binarization)]\r\n" +
                                "이진화는 다계조의 이미지를 흑색(0)과 백색(255) 두 가지 값만 가진 단순한 형태로 분류 및 단순화하는 기법입니다.\r\n\r\n" +
                                "  - **Cv.Threshold**: 기준이 되는 임계값(Threshold)에 따라 픽셀 값을 결정하는 핵심 함수입니다.\r\n" +
                                "  - **이진화 타입 (ThresholdType)**:\r\n" +
                                "    1. **Binary**: 픽셀 값이 임계값보다 크면 맥스값(255), 작으면 0으로 변환합니다.\r\n" +
                                "    2. **BinaryInv**: 임계값보다 크면 0, 작으면 맥스값(255)으로 변환합니다.\r\n" +
                                "    3. **Truncate**: 임계값보다 큰 픽셀은 임계값 자체로 깎아내고(절단), 작으면 원본 값을 유지합니다.\r\n" +
                                "    4. **ToZero**: 임계값보다 크면 원본 값을 유지하고, 작으면 0으로 만듭니다.\r\n" +
                                "    5. **ToZeroInv**: 임계값보다 크면 0으로 만들고, 작으면 원본 값을 유지합니다.\r\n\r\n" +
                                "--------------------------------------------------\r\n\r\n" +
                                "■ [Otsu 알고리즘 (오초 알고리즘)]\r\n" +
                                "  - 사용자가 직접 임계값(스레시홀드)을 수동 지정하는 대신, 이미지 내 픽셀 분포(명암 분포)를 분석하여 배경과 전경의 분산(Variance)이 최대가 되는 최적의 임계 한계를 컴퓨터가 자동으로 결정해 줍니다.\r\n" +
                                "  - OpenCV에서는 `ThresholdType.Binary | ThresholdType.Otsu`와 같이 플래그 조합 형태로 사용합니다.";

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
            grpQuiz1.Text = "질문 1. Cv.Not 색상 반전 공식";
            grpQuiz1.Font = new Font("Malgun Gothic", 9F, FontStyle.Bold);
            grpQuiz1.Location = new Point(10, 35);
            grpQuiz1.Size = new Size(530, 95);
            pnlQuiz.Controls.Add(grpQuiz1);

            var txtQ1Text = new TextBox
            {
                Text = "Cv.Not 함수를 이용하여 이미지의 색상을 반전시킬 때, 원본 픽셀 값과 변환 후 픽셀 값의 합은 항상 255가 된다.",
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
            grpQuiz2.Text = "질문 2. Otsu 자동 이진화 알고리즘의 동작 방식";
            grpQuiz2.Font = new Font("Malgun Gothic", 9F, FontStyle.Bold);
            grpQuiz2.Location = new Point(10, 135);
            grpQuiz2.Size = new Size(530, 95);
            pnlQuiz.Controls.Add(grpQuiz2);

            var txtQ2Text = new TextBox
            {
                Text = "Otsu 알고리즘은 사용자가 임계값(Threshold)을 직접 수동 설정해 주어야만 최적의 명암 경계선을 계산해 낼 수 있는 반자동 필터 기법이다.",
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
                Text = "🖼 시각 자료: 색상 반전 및 이미지 이진화 원리 인포그래픽",
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
            picDiagram.BackColor = Color.FromArgb(240, 244, 248);
            string imgPath = GetImagePath("008 색상 반전 & Binary.png");
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
                                 "1. **색상 반전(Image Inversion)**: 모든 RGB 채널의 픽셀 밝기 값을 $255 - 원본$ 연산하여 보색으로 역전시킵니다. `Cv.Not(input, output)` 함수를 활용합니다.\r\n" +
                                 "2. **이미지 이진화(Image Binarization)**: 설정한 임계값(Threshold)을 기준으로 흑백을 선별합니다. 임계값을 넘어가는 부분은 백색(255), 나머지는 흑색(0)으로 단순화해 에지 추출 및 형상 인식의 전처리를 돕습니다.\r\n" +
                                 "3. **Otsu 알고리즘**: 히스토그램을 토대로 명암을 가장 잘 양분하는 경계 임계값을 알고리즘 내부적으로 자동 검색해내는 기술입니다.";
            RichTextHelper.SetMarkdown(txtDiagramDesc, diagramDesc);
        }

        private void BtnCheckAnswers_Click(object sender, EventArgs e)
        {
            // Q1 Check (Answer: O)
            if (rdoQ1O.Checked)
            {
                lblQ1Result.ForeColor = Color.Green;
                lblQ1Result.Text = "정답! $반전 = 255 - 원본$이므로, 원본 값과 반전 값의 합은 항상 255가 됩니다.";
            }
            else if (rdoQ1X.Checked)
            {
                lblQ1Result.ForeColor = Color.Red;
                lblQ1Result.Text = "오답입니다. 모든 비트 반전 연산(NOT) 결과의 총합은 8비트 상한인 255로 유지됩니다.";
            }
            else
            {
                lblQ1Result.ForeColor = Color.OrangeRed;
                lblQ1Result.Text = "답안을 체크해 주세요.";
            }

            // Q2 Check (Answer: X)
            if (rdoQ2X.Checked)
            {
                lblQ2Result.ForeColor = Color.Green;
                lblQ2Result.Text = "정답! Otsu 알고리즘은 사용자의 입력 없이도 스스로 히스토그램을 분석해 최적 임계값을 결정해 줍니다.";
            }
            else if (rdoQ2O.Checked)
            {
                lblQ2Result.ForeColor = Color.Red;
                lblQ2Result.Text = "오답입니다. Otsu는 수동 지정 없이 완전히 컴퓨터가 자동화 연산해내는 자동 임계값 기법입니다.";
            }
            else
            {
                lblQ2Result.ForeColor = Color.OrangeRed;
                lblQ2Result.Text = "답안을 체크해 주세요.";
            }
        }

        private void InitializeLabTab()
        {
            // Set three PictureBoxes side-by-side
            int pbW = 370;
            int pbH = 350;

            var lblOrigTitle = new Label { Text = "원본 이미지 (Original)", Location = new Point(20, 15), Font = new Font("Malgun Gothic", 9.5F, FontStyle.Bold), AutoSize = true };
            pbOriginal.Location = new Point(20, 40);
            pbOriginal.Size = new Size(pbW, pbH);
            pbOriginal.SizeMode = PictureBoxSizeMode.Zoom;
            pbOriginal.BorderStyle = BorderStyle.FixedSingle;
            pbOriginal.BackColor = Color.Black;

            var lblInvertTitle = new Label { Text = "색상 반전 이미지 (Cv.Not)", Location = new Point(415, 15), Font = new Font("Malgun Gothic", 9.5F, FontStyle.Bold), AutoSize = true, ForeColor = Color.DarkSlateBlue };
            pbInverted.Location = new Point(415, 40);
            pbInverted.Size = new Size(pbW, pbH);
            pbInverted.SizeMode = PictureBoxSizeMode.Zoom;
            pbInverted.BorderStyle = BorderStyle.FixedSingle;
            pbInverted.BackColor = Color.Black;

            var lblBinaryTitle = new Label { Text = "이진화 이미지 (Cv.Threshold)", Location = new Point(810, 15), Font = new Font("Malgun Gothic", 9.5F, FontStyle.Bold), AutoSize = true, ForeColor = Color.Navy };
            pbBinarized.Location = new Point(810, 40);
            pbBinarized.Size = new Size(pbW, pbH);
            pbBinarized.SizeMode = PictureBoxSizeMode.Zoom;
            pbBinarized.BorderStyle = BorderStyle.FixedSingle;
            pbBinarized.BackColor = Color.Black;

            tabLab.Controls.Add(lblOrigTitle);
            tabLab.Controls.Add(pbOriginal);
            tabLab.Controls.Add(lblInvertTitle);
            tabLab.Controls.Add(pbInverted);
            tabLab.Controls.Add(lblBinaryTitle);
            tabLab.Controls.Add(pbBinarized);

            // Control GroupBoxes at the bottom
            int grpY = 410;
            int grpH = 280;

            // Group 1: File Loading & Image Info
            grpFile.Text = "1. 이미지 파일 준비";
            grpFile.Location = new Point(20, grpY);
            grpFile.Size = new Size(370, grpH);
            grpFile.Font = new Font("Malgun Gothic", 9.5F, FontStyle.Bold);
            tabLab.Controls.Add(grpFile);

            btnOpen.Text = "새 이미지 파일 열기...";
            btnOpen.Location = new Point(20, 35);
            btnOpen.Size = new Size(330, 40);
            btnOpen.BackColor = Color.LightSteelBlue;
            btnOpen.FlatStyle = FlatStyle.Flat;
            btnOpen.Click += BtnOpen_Click;
            grpFile.Controls.Add(btnOpen);

            btnLoadDefault.Text = "기본 학습 이미지 로드 (Italia.jpg)";
            btnLoadDefault.Location = new Point(20, 90);
            btnLoadDefault.Size = new Size(330, 40);
            btnLoadDefault.BackColor = Color.Gainsboro;
            btnLoadDefault.FlatStyle = FlatStyle.Flat;
            btnLoadDefault.Click += BtnLoadDefault_Click;
            grpFile.Controls.Add(btnLoadDefault);

            lblFilePath.Text = "선택된 파일: 없음 (대기 중)";
            lblFilePath.Location = new Point(20, 150);
            lblFilePath.Size = new Size(330, 40);
            lblFilePath.Font = new Font("Malgun Gothic", 9F, FontStyle.Regular);
            lblFilePath.ForeColor = Color.Gray;
            grpFile.Controls.Add(lblFilePath);

            var lblFileTip = new Label
            {
                Text = "※ 이미지 파일을 선택하여 로드하면, 자동으로 색상 반전(Cv.Not) 연산과 그레이스케일 기준 이진화(Cv.Threshold) 연산이 실시간 업데이트되어 3가지 뷰어에 나란히 출력됩니다.",
                Location = new Point(20, 200),
                Size = new Size(330, 70),
                Font = new Font("Malgun Gothic", 8.5F, FontStyle.Regular),
                ForeColor = Color.DimGray
            };
            grpFile.Controls.Add(lblFileTip);

            // Group 2: Parameters Control (Threshold)
            grpParams.Text = "2. 실시간 이진화(Threshold) 제어";
            grpParams.Location = new Point(415, grpY);
            grpParams.Size = new Size(370, grpH);
            grpParams.Font = new Font("Malgun Gothic", 9.5F, FontStyle.Bold);
            tabLab.Controls.Add(grpParams);

            lblThreshold.Text = "임계값 (Threshold) 조절 슬라이더:";
            lblThreshold.Location = new Point(20, 30);
            lblThreshold.AutoSize = true;
            grpParams.Controls.Add(lblThreshold);

            trThreshold.Location = new Point(20, 55);
            trThreshold.Size = new Size(270, 45);
            trThreshold.Minimum = 0;
            trThreshold.Maximum = 255;
            trThreshold.Value = 127;
            trThreshold.TickStyle = TickStyle.None;
            trThreshold.Scroll += TrThreshold_Scroll;
            grpParams.Controls.Add(trThreshold);

            lblThresholdVal.Text = "127";
            lblThresholdVal.Location = new Point(300, 60);
            lblThresholdVal.Size = new Size(50, 20);
            lblThresholdVal.Font = new Font("Consolas", 11F, FontStyle.Bold);
            grpParams.Controls.Add(lblThresholdVal);

            var lblComboTitle = new Label { Text = "이진화 연산 타입 (ThresholdType):", Location = new Point(20, 105), AutoSize = true };
            grpParams.Controls.Add(lblComboTitle);

            cmbThresholdType.Location = new Point(20, 130);
            cmbThresholdType.Size = new Size(330, 25);
            cmbThresholdType.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbThresholdType.Items.AddRange(new object[] {
                "Binary (일반 이진화)",
                "BinaryInv (역 이진화)",
                "Truncate (임계값 상한 절단)",
                "ToZero (임계값 하한 제로화)",
                "ToZeroInv (임계값 상한 제로화)",
                "Otsu (오초 자동 임계값 결정)"
            });
            cmbThresholdType.SelectedIndex = 0;
            cmbThresholdType.SelectedIndexChanged += CmbThresholdType_SelectedIndexChanged;
            grpParams.Controls.Add(cmbThresholdType);

            lblOtsuResult.Text = "Otsu 계산 결과: -";
            lblOtsuResult.Location = new Point(20, 170);
            lblOtsuResult.Size = new Size(330, 20);
            lblOtsuResult.Font = new Font("Malgun Gothic", 9F, FontStyle.Bold);
            lblOtsuResult.ForeColor = Color.DarkSlateBlue;
            grpParams.Controls.Add(lblOtsuResult);

            lblTypeDesc.Location = new Point(20, 195);
            lblTypeDesc.Size = new Size(330, 75);
            lblTypeDesc.BorderStyle = BorderStyle.FixedSingle;
            lblTypeDesc.BackColor = Color.WhiteSmoke;
            lblTypeDesc.Padding = new Padding(5);
            lblTypeDesc.Font = new Font("Malgun Gothic", 8.5F);
            lblTypeDesc.ForeColor = Color.DarkSlateGray;
            grpParams.Controls.Add(lblTypeDesc);

            // Group 3: Code Preview & Theory summary
            grpCode.Text = "3. OpenCV C# 실행 코드 프리뷰";
            grpCode.Location = new Point(810, grpY);
            grpCode.Size = new Size(370, grpH);
            grpCode.Font = new Font("Malgun Gothic", 9.5F, FontStyle.Bold);
            tabLab.Controls.Add(grpCode);

            txtCodePreview.Location = new Point(15, 30);
            txtCodePreview.Size = new Size(340, 130);
            txtCodePreview.ReadOnly = true;
            txtCodePreview.BackColor = Color.FromArgb(30, 30, 30);
            txtCodePreview.ForeColor = Color.LightGreen;
            txtCodePreview.Font = new Font("Consolas", 8.5F);
            txtCodePreview.BorderStyle = BorderStyle.FixedSingle;
            grpCode.Controls.Add(txtCodePreview);

            var txtSummary = new RichTextBox
            {
                Location = new Point(15, 175),
                Size = new Size(340, 95),
                ReadOnly = true,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.WhiteSmoke,
                Font = new Font("Malgun Gothic", 8.5F)
            };
            grpCode.Controls.Add(txtSummary);

            string summaryText = "【핵심 원리 요약】\r\n" +
                                 "1. **색상 반전**: `Cv.Not`을 통해 BGR 각 채널 픽셀의 비트를 정반대로 뒤집어 보색을 생성합니다.\r\n" +
                                 "2. **이진화**: `Cv.Threshold`를 수행해 회색조 이미지에서 밝기가 특정 임계값 이상인 대상을 분리합니다.\r\n" +
                                 "3. **Otsu 이진화**: 임계점 산출이 난해할 때 이미지의 히스토그램을 토대로 수학적 최적의 분리 벽을 자동으로 검출합니다.";
            RichTextHelper.SetMarkdown(txtSummary, summaryText);

            UpdateTypeDescription();
        }

        private void BtnOpen_Click(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif;*.tif;*.tiff|All Files|*.*";
                dialog.Title = "로드할 이미지 선택";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    SelectFile(dialog.FileName);
                }
            }
        }

        private void BtnLoadDefault_Click(object sender, EventArgs e)
        {
            string defaultPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Italia.jpg");
            if (!File.Exists(defaultPath))
            {
                defaultPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\Italia.jpg");
            }

            if (File.Exists(defaultPath))
            {
                SelectFile(defaultPath);
            }
            else
            {
                MessageBox.Show("기본 이미지 'Italia.jpg'를 찾을 수 없습니다.", "파일 없음", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void SelectFile(string filepath)
        {
            selectedFilePath = filepath;
            lblFilePath.Text = $"선택됨: {Path.GetFileName(filepath)}";
            ProcessImages();
        }

        private void ProcessImages()
        {
            if (string.IsNullOrEmpty(selectedFilePath) || !File.Exists(selectedFilePath)) return;

            try
            {
                // Clear previous native resources
                SafeDisposeImages();

                // Workaround for Korean unicode paths in OpenCV 2.4 on Windows
                string pathToOpen = selectedFilePath;
                foreach (char c in selectedFilePath)
                {
                    if (c > 127)
                    {
                        string ext = Path.GetExtension(selectedFilePath);
                        string tempPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "temp_image_ch08" + ext);
                        if (File.Exists(tempPath)) File.Delete(tempPath);
                        File.Copy(selectedFilePath, tempPath, true);
                        pathToOpen = tempPath;
                        break;
                    }
                }
                pathToOpen = pathToOpen.Replace('\\', '/');

                // Load source image
                srcImage = new IplImage(pathToOpen, LoadMode.Color);
                invertImage = new IplImage(srcImage.Size, srcImage.Depth, srcImage.NChannels);
                grayImage = new IplImage(srcImage.Size, BitDepth.U8, 1);
                binaryImage = new IplImage(srcImage.Size, BitDepth.U8, 1);

                // 1. Color Inversion
                Cv.Not(srcImage, invertImage);

                // 2. Grayscale Conversion (Threshold requires 1-channel for standard binary, although it runs on color as well)
                Cv.CvtColor(srcImage, grayImage, ColorConversion.BgrToGray);

                // 3. Thresholding
                ApplyThresholding();

                // Bind to UI
                var oldO = pbOriginal.Image;
                pbOriginal.Image = BitmapConverter.ToBitmap(srcImage);
                oldO?.Dispose();

                var oldI = pbInverted.Image;
                pbInverted.Image = BitmapConverter.ToBitmap(invertImage);
                oldI?.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show("이미지 처리 중 오류 발생:\n" + ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ApplyThresholding()
        {
            if (grayImage == null || binaryImage == null) return;

            double thresholdVal = trThreshold.Value;
            ThresholdType type = GetSelectedThresholdType();

            double calculatedThresh;

            if (cmbThresholdType.SelectedIndex == 5) // Otsu selected
            {
                // Otsu requires combining Binary or BinaryInv with Otsu flag.
                // Standard Otsu: ThresholdType.Binary | ThresholdType.Otsu
                calculatedThresh = Cv.Threshold(grayImage, binaryImage, 0, 255, ThresholdType.Binary | ThresholdType.Otsu);
                lblOtsuResult.Text = $"Otsu 계산 결과: 자동 결정된 임계값 = {calculatedThresh:F0}";
            }
            else
            {
                calculatedThresh = Cv.Threshold(grayImage, binaryImage, thresholdVal, 255, type);
                lblOtsuResult.Text = "Otsu 계산 결과: - (수동 제어 모드)";
            }

            var oldB = pbBinarized.Image;
            pbBinarized.Image = BitmapConverter.ToBitmap(binaryImage);
            oldB?.Dispose();

            UpdateCodePreview();
        }

        private ThresholdType GetSelectedThresholdType()
        {
            switch (cmbThresholdType.SelectedIndex)
            {
                case 0: return ThresholdType.Binary;
                case 1: return ThresholdType.BinaryInv;
                case 2: return ThresholdType.Truncate;
                case 3: return ThresholdType.ToZero;
                case 4: return ThresholdType.ToZeroInv;
                case 5: return ThresholdType.Binary | ThresholdType.Otsu;
                default: return ThresholdType.Binary;
            }
        }

        private void TrThreshold_Scroll(object sender, EventArgs e)
        {
            lblThresholdVal.Text = trThreshold.Value.ToString();
            ApplyThresholding();
        }

        private void CmbThresholdType_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool isOtsu = (cmbThresholdType.SelectedIndex == 5);
            trThreshold.Enabled = !isOtsu;
            lblThresholdVal.Enabled = !isOtsu;

            UpdateTypeDescription();
            ApplyThresholding();
        }

        private void UpdateTypeDescription()
        {
            switch (cmbThresholdType.SelectedIndex)
            {
                case 0:
                    lblTypeDesc.Text = "【Binary (이진화)】\r\n픽셀 값이 임계값보다 크면 백색(255)으로 설정하고, 임계값 이하인 픽셀은 흑색(0)으로 전환합니다.";
                    break;
                case 1:
                    lblTypeDesc.Text = "【BinaryInv (역 이진화)】\r\n픽셀 값이 임계값보다 크면 흑색(0)으로 설정하고, 임계값 이하인 픽셀은 백색(255)으로 역전환합니다.";
                    break;
                case 2:
                    lblTypeDesc.Text = "【Truncate (절단)】\r\n픽셀 값이 임계값보다 크면 임계값으로 설정하고(상한선 절단), 임계값 이하인 픽셀은 원본 값을 유지합니다.";
                    break;
                case 3:
                    lblTypeDesc.Text = "【ToZero (제로화)】\r\n픽셀 값이 임계값보다 크면 원본 값을 유지하고, 임계값 이하인 픽셀은 모두 흑색(0)으로 만듭니다.";
                    break;
                case 4:
                    lblTypeDesc.Text = "【ToZeroInv (역 제로화)】\r\n픽셀 값이 임계값보다 크면 흑색(0)으로 설정하고, 임계값 이하인 픽셀은 원본 값을 유지합니다.";
                    break;
                case 5:
                    lblTypeDesc.Text = "【Otsu (오초 알고리즘)】\r\n이미지 전체의 명암 분산 데이터를 분석하여 흑과 백을 양분하는 최적의 경계 임계값을 자동으로 역산해 적용합니다.";
                    break;
            }
        }

        private void UpdateCodePreview()
        {
            txtCodePreview.Clear();
            txtCodePreview.SelectionFont = txtCodePreview.Font;
            txtCodePreview.SelectionColor = txtCodePreview.ForeColor;

            int thVal = trThreshold.Value;
            string typeStr = "ThresholdType.Binary";
            if (cmbThresholdType.SelectedIndex == 1) typeStr = "ThresholdType.BinaryInv";
            else if (cmbThresholdType.SelectedIndex == 2) typeStr = "ThresholdType.Truncate";
            else if (cmbThresholdType.SelectedIndex == 3) typeStr = "ThresholdType.ToZero";
            else if (cmbThresholdType.SelectedIndex == 4) typeStr = "ThresholdType.ToZeroInv";
            else if (cmbThresholdType.SelectedIndex == 5) typeStr = "ThresholdType.Binary | ThresholdType.Otsu";

            string code = "// 1. 색상 반전 (Image Inversion)\r\n" +
                           "**Cv.Not(srcImage, invertImage);**\r\n\r\n" +
                           "// 2. 이미지 이진화 (Binarization)\r\n" +
                           "// threshold = " + (cmbThresholdType.SelectedIndex == 5 ? "Auto" : thVal.ToString()) + ", maxValue = 255\r\n" +
                           "**Cv.Threshold(grayImage, binaryImage, " + (cmbThresholdType.SelectedIndex == 5 ? "0" : thVal.ToString()) + ", 255, " + typeStr + ");**";

            RichTextHelper.SetMarkdown(txtCodePreview, code);
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

        private void SafeDisposeImages()
        {
            if (srcImage != null)
            {
                Cv.ReleaseImage(srcImage);
                srcImage = null;
            }
            if (invertImage != null)
            {
                Cv.ReleaseImage(invertImage);
                invertImage = null;
            }
            if (grayImage != null)
            {
                Cv.ReleaseImage(grayImage);
                grayImage = null;
            }
            if (binaryImage != null)
            {
                Cv.ReleaseImage(binaryImage);
                binaryImage = null;
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            SafeDisposeImages();

            pbOriginal.Image?.Dispose();
            pbInverted.Image?.Dispose();
            pbBinarized.Image?.Dispose();
            picDiagram.Image?.Dispose();

            // Delete temporary files if created
            string[] tempPaths = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "temp_image_ch08*");
            foreach (var temp in tempPaths)
            {
                try { File.Delete(temp); } catch { }
            }

            base.OnFormClosed(e);
        }
    }
}
