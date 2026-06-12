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
    public class FormCh06 : Form
    {
        private readonly TabControl tabControl = new TabControl();
        private readonly TabPage tabTheory = new TabPage();
        private readonly TabPage tabLab = new TabPage();
        private readonly PictureBox picDiagram = new PictureBox();

        // OpenCV_CLASS instance
        private OpenCV_CLASS openCV = null;

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
        private readonly PictureBox pbOriginal = new PictureBox();
        private readonly PictureBox pbGray = new PictureBox();
        private readonly Button btnOpen = new Button();
        private readonly Button btnLoadDefault = new Button();
        private readonly Label lblFilePath = new Label();

        // Step buttons
        private readonly Button btnCreate = new Button();
        private readonly Button btnLoadImage = new Button();
        private readonly Button btnConvert = new Button();
        private readonly Button btnDispose = new Button();

        // Status labels
        private readonly Label lblClassStatus = new Label();
        private readonly Label lblImageStatus = new Label();
        private readonly Label lblMemInfo = new Label();

        // Live code preview RichTextBox
        private readonly RichTextBox txtCodePreview = new RichTextBox();

        // File path state
        private string selectedFilePath = "";

        public FormCh06()
        {
            Text = "CH06 - 클래스 생성 & GrayScale 변환 실습";
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
            
            UpdateStatus();
            UpdateCodePreview(0);
        }

        private void InitializeTheoryTab()
        {
            // Title Header
            var lblTitle = new Label
            {
                Text = "006. 클래스 설계 및 GrayScale 변환 핵심 이론 학습",
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
                Size = new Size(550, 280),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(252, 252, 248),
                Font = new Font("Malgun Gothic", 10F)
            };
            tabTheory.Controls.Add(txtTheoryText);

            string theoryText = "■ [IDisposable 인터페이스와 비관리 자원 해제]\r\n" +
                               "OpenCV의 `IplImage` 객체는 C++로 작성된 **비관리(Unmanaged) 네이티브 자원**을 참조합니다.\r\n" +
                               "C#의 가비지 컬렉터(GC)는 C# 클래스 자체는 수집할 수 있지만, 네이티브 픽셀 데이터를 제때 해제하지 못하므로 명시적인 해제 구조가 필수적입니다.\r\n\r\n" +
                               "  - **IDisposable 상속**: C#에서 파일, 네트워크 연결, 네이티브 DLL 리소스 등 GC의 제어 밖 자원을 명시적으로 해제하기 위해 구현하는 표준 인터페이스입니다.\r\n" +
                               "  - **Dispose() 구현**: 이 인터페이스를 구현하는 주 목적은 `Dispose()` 내에서 네이티브 자원을 해제하도록 강제하는 것입니다. 사용 완료 후 `using` 블록을 활용하거나 직접 `Dispose()`를 호출해 줍니다.\r\n\r\n" +
                               "--------------------------------------------------\r\n\r\n" +
                               "■ [Cv.ReleaseImage(ref IplImage)의 ref 키워드 원리]\r\n" +
                               "OpenCVSharp 2.4에서 `Cv.ReleaseImage`는 `ref` 키워드로 이미지를 수신합니다.\r\n" +
                               "  - **참조(ref)에 의한 전달**: 함수 내부에서 네이티브 포인터를 해제(C++ `cvReleaseImage`)한 뒤, 호출한 곳의 C# `IplImage` 변수 참조 자체를 **null**로 자동 초기화하기 위함입니다.\r\n" +
                               "  - **이중 해제(Double Free) 방지**: 해제된 포인터를 가리키는 쓰레기 주소(Dangling Pointer)가 남아있을 경우 발생할 수 있는 이중 해제 크래시를 컴파일러 수준에서 방지해 줍니다.\r\n\r\n" +
                               "--------------------------------------------------\r\n\r\n" +
                               "■ [컬러 BGR에서 Grayscale(흑백) 변환]\r\n" +
                               "  - **그레이스케일**: 색상 대비만 나타내는 8비트 1채널 밝기 강도 이미지입니다.\r\n" +
                               "  - **변환 원리**: BGR 3채널의 빛을 가중치 합산하여 단일 채널 밝기값(Gray)으로 계산합니다.\r\n" +
                               "  - **공식**: $Gray = 0.299 \\times R + 0.587 \\times G + 0.114 \\times B$\r\n" +
                               "  - **용량 절약**: 데이터량이 정확히 1/3로 축소되므로 에지 검출, 기하 연산 등의 계산 효율을 극대화합니다.";
            RichTextHelper.SetMarkdown(txtTheoryText, theoryText);

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
            grpQuiz1.Text = "질문 1. IDisposable 인터페이스 상속 목적";
            grpQuiz1.Font = new Font("Malgun Gothic", 9F, FontStyle.Bold);
            grpQuiz1.Location = new Point(10, 35);
            grpQuiz1.Size = new Size(530, 100);
            pnlQuiz.Controls.Add(grpQuiz1);

            var txtQ1Text = new TextBox
            {
                Text = "클래스에 IDisposable 인터페이스를 상속받아 구현하는 가장 주된 목적은 가비지 컬렉터(GC)의 한계를 보완하여 비관리 네이티브 자원을 수동으로 즉시 해제하기 위함이다.",
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
            grpQuiz2.Text = "질문 2. Cv.ReleaseImage(ref IplImage)에서 ref 키워드";
            grpQuiz2.Font = new Font("Malgun Gothic", 9F, FontStyle.Bold);
            grpQuiz2.Location = new Point(10, 140);
            grpQuiz2.Size = new Size(530, 100);
            pnlQuiz.Controls.Add(grpQuiz2);

            var txtQ2Text = new TextBox
            {
                Text = "Cv.ReleaseImage 함수에 ref 키워드가 전달되는 이유는 네이티브 메모리를 정상 소멸시킨 이후, 전달한 C# 변수 참조에 null을 주입해 댕글링 포인터나 이중 해제를 막기 위함이다.",
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

            // --- Right Side Diagram ---
            var lblDiagramTitle = new Label
            {
                Text = "🖼 시각 자료: 클래스 설계 및 GrayScale 변환 파이프라인",
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
            string imgPath = GetImagePath("ch06_class_grayscale_ko.png");
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
                                 "1. **클래스 설계**: `IDisposable`을 상속한 `OpenCV_CLASS`를 정의하여 비관리 이미지 자원의 해제 라이프사이클을 안전하게 캡슐화합니다.\r\n" +
                                 "2. **GrayScale 변환**: BGR 이미지를 `Cv.CvtColor`와 `BgrToGray` 파라미터를 사용해 Grayscale(1채널) 이미지 데이터로 변환합니다.\r\n" +
                                 "3. **UI 연동**: 원본 이미지(`_srcImage`)와 변환 이미지(`_grayImage`)를 각각 `BitmapConverter.ToBitmap`을 거쳐 C# 윈폼 `PictureBox`에 동시에 나열 출력합니다.";
            RichTextHelper.SetMarkdown(txtDiagramDesc, diagramDesc);
        }

        private void BtnCheckAnswers_Click(object sender, EventArgs e)
        {
            // Q1 Check (Answer: O)
            if (rdoQ1O.Checked)
            {
                lblQ1Result.ForeColor = Color.Green;
                lblQ1Result.Text = "정답! GC는 C++ 네이티브 포인터 메모리의 크기를 몰라 해제 시기를 놓치므로, 수동 Dispose가 절대적으로 요구됩니다.";
            }
            else if (rdoQ1X.Checked)
            {
                lblQ1Result.ForeColor = Color.Red;
                lblQ1Result.Text = "오답입니다. C# 객체 참조 제거만으로는 실제 네이티브 픽셀 정보가 담긴 비관리 메모리가 정리되지 않습니다.";
            }
            else
            {
                lblQ1Result.ForeColor = Color.OrangeRed;
                lblQ1Result.Text = "답안을 먼저 체크해 주세요.";
            }

            // Q2 Check (Answer: O)
            if (rdoQ2O.Checked)
            {
                lblQ2Result.ForeColor = Color.Green;
                lblQ2Result.Text = "정답! ref 포인터를 통해 함수 내부에서 외부 변수 자체를 null로 강제 세팅하여, 실수로 중복 해제하는 런타임 크래시를 원천 차단합니다.";
            }
            else if (rdoQ2X.Checked)
            {
                lblQ2Result.ForeColor = Color.Red;
                lblQ2Result.Text = "오답입니다. 속도와는 무관하며, 해제된 자원의 재사용 또는 이중 해제를 막기 위한 참조 전달입니다.";
            }
            else
            {
                lblQ2Result.ForeColor = Color.OrangeRed;
                lblQ2Result.Text = "답안을 먼저 체크해 주세요.";
            }
        }

        private void InitializeLabTab()
        {
            // Set two PictureBoxes side by side
            pbOriginal.Location = new Point(20, 20);
            pbOriginal.Size = new Size(350, 480);
            pbOriginal.SizeMode = PictureBoxSizeMode.Zoom;
            pbOriginal.BorderStyle = BorderStyle.FixedSingle;
            pbOriginal.BackColor = Color.Black;
            tabLab.Controls.Add(pbOriginal);

            pbGray.Location = new Point(390, 20);
            pbGray.Size = new Size(350, 480);
            pbGray.SizeMode = PictureBoxSizeMode.Zoom;
            pbGray.BorderStyle = BorderStyle.FixedSingle;
            pbGray.BackColor = Color.Black;
            tabLab.Controls.Add(pbGray);

            // Right sidebar options panel
            var panel = new Panel
            {
                Location = new Point(760, 20),
                Size = new Size(310, 680),
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
                    Font = new Font("Malgun Gothic", 9.75F, FontStyle.Bold),
                    Location = new Point(15, y),
                    AutoSize = true,
                    ForeColor = Color.DarkBlue
                });
                y += 23;
            };

            // 1. File Selection
            addHeader("1. 이미지 파일 선택");
            btnOpen.Text = "파일 열기 (Open File)";
            btnOpen.Location = new Point(15, y);
            btnOpen.Size = new Size(135, 30);
            btnOpen.Click += BtnOpen_Click;
            panel.Controls.Add(btnOpen);

            btnLoadDefault.Text = "기본 이미지";
            btnLoadDefault.Location = new Point(155, y);
            btnLoadDefault.Size = new Size(130, 30);
            btnLoadDefault.Click += BtnLoadDefault_Click;
            panel.Controls.Add(btnLoadDefault);
            y += 35;

            lblFilePath.Text = "선택된 파일: 없음";
            lblFilePath.Location = new Point(15, y);
            lblFilePath.Size = new Size(270, 15);
            lblFilePath.ForeColor = Color.Gray;
            panel.Controls.Add(lblFilePath);
            y += 25;

            // 2. Class Instance Controls
            addHeader("2. OpenCV_CLASS 인스턴스 제어");
            btnCreate.Text = "1단계: 클래스 객체 생성";
            btnCreate.Location = new Point(15, y);
            btnCreate.Size = new Size(270, 28);
            btnCreate.Click += BtnCreate_Click;
            panel.Controls.Add(btnCreate);
            y += 33;

            btnLoadImage.Text = "2단계: 이미지 로드 (Load)";
            btnLoadImage.Location = new Point(15, y);
            btnLoadImage.Size = new Size(270, 28);
            btnLoadImage.Click += BtnLoadImage_Click;
            panel.Controls.Add(btnLoadImage);
            y += 33;

            btnConvert.Text = "3단계: Grayscale 변환";
            btnConvert.Location = new Point(15, y);
            btnConvert.Size = new Size(270, 28);
            btnConvert.Click += BtnConvert_Click;
            panel.Controls.Add(btnConvert);
            y += 33;

            btnDispose.Text = "4단계: 자원 해제 (Dispose)";
            btnDispose.Location = new Point(15, y);
            btnDispose.Size = new Size(270, 28);
            btnDispose.Click += BtnDispose_Click;
            panel.Controls.Add(btnDispose);
            y += 40;

            // 3. Status
            addHeader("■ 인스턴스 및 메모리 상태");
            lblClassStatus.Location = new Point(15, y);
            lblClassStatus.Size = new Size(270, 18);
            lblClassStatus.Font = new Font("Malgun Gothic", 9F, FontStyle.Bold);
            panel.Controls.Add(lblClassStatus);
            y += 18;

            lblImageStatus.Location = new Point(15, y);
            lblImageStatus.Size = new Size(270, 18);
            lblImageStatus.Font = new Font("Malgun Gothic", 9F, FontStyle.Bold);
            panel.Controls.Add(lblImageStatus);
            y += 18;

            lblMemInfo.Location = new Point(15, y);
            lblMemInfo.Size = new Size(270, 18);
            lblMemInfo.Font = new Font("Malgun Gothic", 9F, FontStyle.Bold);
            lblMemInfo.ForeColor = Color.DarkSlateBlue;
            panel.Controls.Add(lblMemInfo);
            y += 30;

            // 4. Code Preview
            addHeader("3. 실행 코드 프리뷰 (C#)");
            txtCodePreview.Location = new Point(15, y);
            txtCodePreview.Size = new Size(270, 100);
            txtCodePreview.ReadOnly = true;
            txtCodePreview.BackColor = Color.FromArgb(30, 30, 30);
            txtCodePreview.ForeColor = Color.LightGreen;
            txtCodePreview.Font = new Font("Consolas", 8F);
            txtCodePreview.BorderStyle = BorderStyle.FixedSingle;
            panel.Controls.Add(txtCodePreview);
            y += 115;

            // 5. Summary
            var txtTheory = new RichTextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = RichTextBoxScrollBars.Vertical,
                Location = new Point(15, y),
                Size = new Size(270, 110),
                BorderStyle = BorderStyle.Fixed3D,
                BackColor = Color.WhiteSmoke,
                Font = new Font("Malgun Gothic", 8.5F)
            };
            panel.Controls.Add(txtTheory);

            string theorySummaryText = "【핵심 이론 및 원리】\r\n" +
                                       "1. **클래스 캡슐화**: 이미지와 해제 수단을 단일 클래스(`OpenCV_CLASS`) 안에 모아 안전하게 관리합니다.\r\n" +
                                       "2. **Grayscale 채널 축소**: `Cv.CvtColor`로 BGR 3채널 이미지를 흑백 1채널 데이터로 계산해 크기를 3분의 1로 압축합니다.\r\n" +
                                       "3. **안전한 해제 흐름**: 자원 해제 시 `Cv.ReleaseImage` 호출 후 변수에 `null`을 대입하여 이중 해제와 댕글링 포인터를 방지합니다.";
            RichTextHelper.SetMarkdown(txtTheory, theorySummaryText);

            tabLab.Controls.Add(new TextBox
            {
                Text = "실습 가이드: 이미지를 지정한 뒤, 1단계부터 4단계까지 순서대로 실행하며 폼의 출력 결과와 메모리 상태의 동적 변화를 모니터링하세요.",
                Location = new Point(20, 680),
                Size = new Size(720, 25),
                ReadOnly = true,
                BorderStyle = BorderStyle.None,
                BackColor = Color.FromArgb(240, 244, 248),
                Font = new Font("Malgun Gothic", 9.5F, FontStyle.Bold)
            });
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
            UpdateStatus();
        }

        private void BtnCreate_Click(object sender, EventArgs e)
        {
            if (openCV != null) return;
            openCV = new OpenCV_CLASS();
            UpdateStatus();
            UpdateCodePreview(1);
        }

        private void BtnLoadImage_Click(object sender, EventArgs e)
        {
            if (openCV == null) return;
            if (string.IsNullOrEmpty(selectedFilePath) || !File.Exists(selectedFilePath))
            {
                MessageBox.Show("이미지 파일을 먼저 선택해 주세요.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                openCV.LoadImage(selectedFilePath);
                
                // Load original to PictureBoxOriginal
                var old = pbOriginal.Image;
                pbOriginal.Image = BitmapConverter.ToBitmap(openCV.SrcImage);
                old?.Dispose();

                // Clear Gray PictureBox (new load)
                var oldGray = pbGray.Image;
                pbGray.Image = null;
                oldGray?.Dispose();

                UpdateStatus();
                UpdateCodePreview(2);
            }
            catch (Exception ex)
            {
                MessageBox.Show("이미지 로드 중 오류 발생:\n" + ex.Message, "에러", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnConvert_Click(object sender, EventArgs e)
        {
            if (openCV == null || openCV.SrcImage == null) return;

            try
            {
                openCV.ConvertToGray();

                // Load Gray to PictureBoxGray
                var old = pbGray.Image;
                pbGray.Image = BitmapConverter.ToBitmap(openCV.GrayImage);
                old?.Dispose();

                UpdateStatus();
                UpdateCodePreview(3);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Grayscale 변환 중 오류 발생:\n" + ex.Message, "에러", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnDispose_Click(object sender, EventArgs e)
        {
            if (openCV == null) return;

            openCV.Dispose();
            openCV = null;

            var oldOrig = pbOriginal.Image;
            pbOriginal.Image = null;
            oldOrig?.Dispose();

            var oldGray = pbGray.Image;
            pbGray.Image = null;
            oldGray?.Dispose();

            UpdateStatus();
            UpdateCodePreview(4);
        }

        private void UpdateStatus()
        {
            if (openCV == null)
            {
                lblClassStatus.Text = "인스턴스 상태: 미생성 (Null)";
                lblClassStatus.ForeColor = Color.Red;
                lblImageStatus.Text = "이미지 상태: 미로드";
                lblImageStatus.ForeColor = Color.Red;
                lblMemInfo.Text = "네이티브 메모리: 0 Bytes";
                
                btnCreate.Enabled = true;
                btnLoadImage.Enabled = false;
                btnConvert.Enabled = false;
                btnDispose.Enabled = false;
            }
            else
            {
                lblClassStatus.Text = "인스턴스 상태: 생성됨 (Active)";
                lblClassStatus.ForeColor = Color.Green;
                
                long totalBytes = 0;
                string imgStatus = "미로드";
                lblImageStatus.ForeColor = Color.Red;

                if (openCV.SrcImage != null)
                {
                    totalBytes += openCV.SrcImage.Width * openCV.SrcImage.Height * openCV.SrcImage.NChannels;
                    imgStatus = $"원본 로드됨 ({openCV.SrcImage.Width}x{openCV.SrcImage.Height})";
                    lblImageStatus.ForeColor = Color.Orange;
                }

                if (openCV.GrayImage != null)
                {
                    totalBytes += openCV.GrayImage.Width * openCV.GrayImage.Height * openCV.GrayImage.NChannels;
                    imgStatus = "Grayscale 변환 완료";
                    lblImageStatus.ForeColor = Color.Green;
                }

                lblImageStatus.Text = $"이미지 상태: {imgStatus}";
                lblMemInfo.Text = $"네이티브 메모리: {totalBytes:N0} Bytes ({totalBytes / 1024.0 / 1024.0:F2} MB)";

                btnCreate.Enabled = false;
                btnLoadImage.Enabled = !string.IsNullOrEmpty(selectedFilePath);
                btnConvert.Enabled = (openCV.SrcImage != null);
                btnDispose.Enabled = true;
            }
        }

        private void UpdateCodePreview(int step)
        {
            txtCodePreview.Clear();
            txtCodePreview.SelectionFont = txtCodePreview.Font;
            txtCodePreview.SelectionColor = txtCodePreview.ForeColor;

            string code = "";
            switch (step)
            {
                case 0:
                    code = "// OpenCV_CLASS 미생성 상태\r\n" +
                           "OpenCV_CLASS openCV = null;";
                    break;
                case 1:
                    code = "// 1단계: 클래스 인스턴스 생성\r\n" +
                           "**openCV = new OpenCV_CLASS();**\r\n" +
                           "// (인스턴스가 메모리에 적재됨)";
                    break;
                case 2:
                    code = "// 2단계: 이미지 로드\r\n" +
                           "// filePath = \"" + Path.GetFileName(selectedFilePath) + "\"\r\n" +
                           "**openCV.LoadImage(filePath);**\r\n" +
                           "// (_srcImage 생성됨)";
                    break;
                case 3:
                    code = "// 3단계: Grayscale 변환 실행\r\n" +
                           "**openCV.ConvertToGray();**\r\n" +
                           "// (_grayImage 생성 및 BgrToGray 변환 완료)";
                    break;
                case 4:
                    code = "// 4단계: Dispose 호출하여 리소스 수동 회수\r\n" +
                           "**openCV.Dispose();**\r\n" +
                           "**openCV = null;**\r\n" +
                           "// (네이티브 힙 메모리 즉각 해제 완료)";
                    break;
            }
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

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            if (openCV != null)
            {
                openCV.Dispose();
                openCV = null;
            }
            pbOriginal.Image?.Dispose();
            pbGray.Image?.Dispose();
            picDiagram.Image?.Dispose();
            base.OnFormClosed(e);
        }
    }
}
