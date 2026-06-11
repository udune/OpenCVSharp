using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using OpenCvSharp;
using OpenCvSharp.UserInterface;

using Point = System.Drawing.Point;
using Size = System.Drawing.Size;
using FontStyle = System.Drawing.FontStyle;

namespace OpenCVSharp
{
    public class FormCh05 : Form
    {
        // Setup Tab Control
        private readonly TabControl tabControl = new TabControl();
        private readonly TabPage tabTheory = new TabPage();
        private readonly TabPage tabLab = new TabPage();
        private readonly PictureBox picDiagram = new PictureBox();

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
        private readonly PictureBoxIpl pictureBox = new PictureBoxIpl();
        private readonly Button btnOpen = new Button();
        private readonly Button btnLoadDefault = new Button();
        
        // Step-by-Step Interactive Tutorial Buttons
        private readonly Button btnStep1 = new Button();
        private readonly Button btnStep2 = new Button();
        private readonly Button btnStep3 = new Button();
        
        // Code snippet displays
        private readonly TextBox txtCode1 = new TextBox();
        private readonly TextBox txtCode2 = new TextBox();
        private readonly TextBox txtCode3 = new TextBox();

        // Memory Tracker Visuals
        private readonly Label lblActiveMemInfo = new Label();
        private readonly Panel pnlActiveBg = new Panel();
        private readonly Panel pnlActiveFill = new Panel();

        private readonly Label lblLeakedMemInfo = new Label();
        private readonly Panel pnlLeakedBg = new Panel();
        private readonly Panel pnlLeakedFill = new Panel();

        private readonly Label lblWarning = new Label();
        private readonly Button btnResetLeaks = new Button();
        private readonly Button btnFreeNow = new Button();

        // Settings and Info
        private readonly ComboBox cmbSizeMode = new ComboBox();
        private readonly Label lblResolution = new Label();
        private readonly Label lblChannels = new Label();
        private readonly Label lblFilePath = new Label();

        // State variables
        private string selectedFilePath = "";
        private IplImage src = null;
        private long currentAllocatedBytes = 0;
        private long totalLeakedBytes = 0;
        private const long MaxBytes = 10 * 1024 * 1024; // 10 MB limit for the visualizer bar

        public FormCh05()
        {
            Text = "CH05 - 이미지 출력 & 메모리 누수 학습 및 실습";
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
        }

        private void InitializeTheoryTab()
        {
            // Title Header
            var lblTitle = new Label
            {
                Text = "005. 이미지 출력 & 네이티브 메모리 관리 핵심 이론 학습",
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
                Text = "■ [컴퓨터 방 청소기와 외부 창고 (네이티브 메모리)]\r\n" +
                       "C# 언어는 컴퓨터가 알아서 메모리 방 청소를 해 주는 '가비지 컬렉터(청소기)'를 가지고 있어 무척 편리합니다.\r\n" +
                       "하지만 OpenCV는 원래 C++ 언어로 만들어진 외부 도구이기 때문에, 메모리를 쓰는 구역이 다릅니다.\r\n\r\n" +
                       "  - 도서관 대출 대장과 외부 창고:\r\n" +
                       "    C#의 청소기(GC)는 자기 도서관 책상 위의 얇은 '대출 대장(C# 객체 변수)'만 찾아내 치워 줍니다.\r\n" +
                       "    하지만 실제 거대하고 무거운 '도서 박스(C++ 원본 픽셀 데이터)'는 저 멀리 떨어진 **외부 창고(비관리 네이티브 힙)**에 보관됩니다.\r\n" +
                       "  - 수동 반납 필수: 외부 창고에 직접 들어간 도서 박스는 C# 청소기가 건드릴 수 없습니다. 따라서 사진을 다 봤다면 개발자가 직접 `Dispose()` 또는 `ReleaseImage()`를 호출해서 '이 책 반납할게요!'라고 명시적으로 창고 박스를 비워 주어야 합니다.\r\n\r\n" +
                       "--------------------------------------------------\r\n\r\n" +
                       "■ [메모리 누수(Memory Leak)가 생기는 원리]\r\n" +
                       "  - C# 변수 `src = new IplImage(...)`로 새 사진을 가져오면 외부 창고에 박스가 하나 생깁니다.\r\n" +
                       "  - 이 사진을 다 쓴 뒤에 반납 처리를 하지 않고, 그냥 똑같은 변수에 다시 새 이미지(`src = new IplImage(...)`)를 덮어쓰게 되면:\r\n" +
                       "    1. 변수 `src`는 새로 만든 상자만 가리키게 되며, 이전 상자의 열쇠(참조 포인터)를 잃어버립니다.\r\n" +
                       "    2. 열쇠가 없어진 이전 도서 박스는 외부 창고 구석에 갇힌 채 영원히 버릴 수도, 찾을 수도 없는 미아 상태가 됩니다.\r\n" +
                       "    3. 이것이 계속 반복되어 외부 창고에 버려진 박스들이 가득 차 결국 컴퓨터 메모리가 통째로 꽉 차서 뻗어버리는 현상(Out of Memory)을 바로 **메모리 누수(Memory Leak)**라고 합니다.\r\n\r\n" +
                       "--------------------------------------------------\r\n\r\n" +
                       "■ [액자 컨트롤(PictureBoxIpl)의 함정]\r\n" +
                       "  - PictureBoxIpl은 단지 외부 창고에 있는 이미지를 액자에 넣어 거실에 보여주는 조수 역할을 합니다.\r\n" +
                       "  - 액자에 새 사진을 끼워 넣는다고 해서, 액자 조수가 알아서 예전 사진 원본(네이티브 메모리)을 버려주지는 않습니다. 액자 사용이 끝났을 때 원본 이미지를 수동으로 지워 주는 주체는 항상 개발자 자신이어야 합니다."
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
            grpQuiz1.Text = "질문 1. 가비지 컬렉터(GC)와 네이티브 리소스";
            grpQuiz1.Font = new Font("Malgun Gothic", 9F, FontStyle.Bold);
            grpQuiz1.Location = new Point(10, 35);
            grpQuiz1.Size = new Size(530, 100);
            pnlQuiz.Controls.Add(grpQuiz1);

            var txtQ1Text = new TextBox
            {
                Text = "C#의 가비지 컬렉터(GC)는 CLR의 통제를 벗어나는 C++ 네이티브 힙 영역의 OpenCV 이미지(IplImage) 메모리까지 실시간으로 즉각 감지하고 완벽히 회수해 준다.",
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
            grpQuiz2.Text = "질문 2. PictureBoxIpl의 메모리 해제 범위";
            grpQuiz2.Font = new Font("Malgun Gothic", 9F, FontStyle.Bold);
            grpQuiz2.Location = new Point(10, 140);
            grpQuiz2.Size = new Size(530, 100);
            pnlQuiz.Controls.Add(grpQuiz2);

            var txtQ2Text = new TextBox
            {
                Text = "PictureBoxIpl 컨트롤의 ImageIpl 속성에 새 IplImage를 연속 대입하면, 기존에 대입되었던 이전 이미지의 네이티브 메모리는 뷰어에 의해 자동으로 안전 해제(소멸)된다.",
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
                Text = "🖼 시각 자료: 네이티브 메모리 관리 & 메모리 누수 메커니즘",
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
            string imgPath = GetImagePath("ch05_memory_leak_ko.png");
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
                       "1. 왼쪽(정상 메모리 해제 흐름): C#에서 IplImage 변수를 생성하면 관리 힙에는 작은 래퍼 객체가 생성되고, 실제 대용량 픽셀 데이터는 네이티브 힙에 저장됩니다. 사용 완료 후 `ReleaseImage` 또는 `Dispose()`를 수동으로 명시 호출하면 네이티브 메모리가 즉각 온전히 해제됩니다.\r\n" +
                       "2. 오른쪽(메모리 누수 발생 흐름): 네이티브 힙에 이미지가 떠 있는 상태에서, 수동 해제 없이 변수에 새 이미지 대입(`new IplImage`)을 반복 수행하면, 이전 래퍼의 포인터만 유실되고 네이티브 힙의 실데이터는 갈 곳을 잃고 그대로 박혀 유실됩니다. C# GC(가비지 컬렉터)는 이 네이티브 힙의 영역을 인지하지 못합니다.\r\n" +
                       "3. 핵심 요약: 컴퓨터 비전 처리 파이프라인에서 수백 FPS 속도로 대량의 이미지 할당/대입이 반복되므로, 단 몇 프레임의 Dispose() 누락만으로도 초 단위 내에 기가바이트 급의 메모리가 고갈되어 OOM(Out of Memory) 크래시를 맞닥뜨리게 됩니다."
            };
            tabTheory.Controls.Add(txtDiagramDesc);
        }

        private void BtnCheckAnswers_Click(object sender, EventArgs e)
        {
            // Q1 Check (Answer: X)
            if (rdoQ1X.Checked)
            {
                lblQ1Result.ForeColor = Color.Green;
                lblQ1Result.Text = "정답! GC는 C++ 네이티브 리소스를 실시간 추적하지 못하므로, 수동 해제인 ReleaseImage를 직접 거쳐야만 합니다.";
            }
            else if (rdoQ1O.Checked)
            {
                lblQ1Result.ForeColor = Color.Red;
                lblQ1Result.Text = "오답입니다. GC는 오로지 Managed 힙만 제어하므로 비관리 영역은 명시적 해제가 절대적으로 요구됩니다.";
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
                lblQ2Result.Text = "정답! PictureBoxIpl은 단지 픽셀 화면 연동만을 거들 뿐이며 원본 이미지 수명은 개발자가 수동 소멸시켜야 합니다.";
            }
            else if (rdoQ2O.Checked)
            {
                lblQ2Result.ForeColor = Color.Red;
                lblQ2Result.Text = "오답입니다. ImageIpl에 새 이미지를 대입한다고 하여 이전 이미지가 소멸(Release)되는 것이 아닙니다.";
            }
            else
            {
                lblQ2Result.ForeColor = Color.OrangeRed;
                lblQ2Result.Text = "답안을 먼저 체크해 주세요.";
            }
        }

        private void InitializeLabTab()
        {
            // PictureBox setup
            pictureBox.Location = new Point(20, 20);
            pictureBox.Size = new Size(640, 480);
            pictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox.BorderStyle = BorderStyle.FixedSingle;
            pictureBox.BackColor = Color.Black;
            tabLab.Controls.Add(pictureBox);

            // Main learning panel on the right
            var panel = new Panel
            {
                Location = new Point(680, 20),
                Size = new Size(390, 560),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };
            tabLab.Controls.Add(panel);

            int y = 10;
            
            // Title Header
            var lblTitle = new Label
            {
                Text = "005. 이미지 출력 & 메모리 누수 실험실",
                Font = new Font("Malgun Gothic", 11F, FontStyle.Bold),
                Location = new Point(15, y),
                AutoSize = true,
                ForeColor = Color.DarkSlateBlue
            };
            panel.Controls.Add(lblTitle);
            y += 28;

            // 1. File Selection Section
            var lblSection1 = new Label
            {
                Text = "1. 이미지 파일 준비",
                Font = new Font("Malgun Gothic", 9.5F, FontStyle.Bold),
                Location = new Point(15, y),
                AutoSize = true,
                ForeColor = Color.Navy
            };
            panel.Controls.Add(lblSection1);
            y += 20;

            btnOpen.Text = "이미지 선택 (Open File)";
            btnOpen.Location = new Point(15, y);
            btnOpen.Size = new Size(170, 30);
            btnOpen.Click += BtnOpen_Click;
            panel.Controls.Add(btnOpen);

            btnLoadDefault.Text = "기본 이미지 (Italia.jpg)";
            btnLoadDefault.Location = new Point(195, y);
            btnLoadDefault.Size = new Size(170, 30);
            btnLoadDefault.Click += BtnLoadDefault_Click;
            panel.Controls.Add(btnLoadDefault);
            y += 35;

            lblFilePath.Text = "선택된 파일: 없음 (대기 중)";
            lblFilePath.Location = new Point(15, y);
            lblFilePath.Size = new Size(350, 15);
            lblFilePath.ForeColor = Color.Gray;
            panel.Controls.Add(lblFilePath);
            y += 20;

            // 2. Step-by-Step Learning Section
            var lblSection2 = new Label
            {
                Text = "2. 단계별 실행 학습 (Interactive Steps)",
                Font = new Font("Malgun Gothic", 9.5F, FontStyle.Bold),
                Location = new Point(15, y),
                AutoSize = true,
                ForeColor = Color.Navy
            };
            panel.Controls.Add(lblSection2);
            y += 20;

            // Step 1: Load Image
            btnStep1.Text = "1단계: 로드";
            btnStep1.Location = new Point(15, y);
            btnStep1.Size = new Size(90, 25);
            btnStep1.Font = new Font("Malgun Gothic", 8.5F, FontStyle.Bold);
            btnStep1.Enabled = false;
            btnStep1.Click += BtnStep1_Click;
            panel.Controls.Add(btnStep1);

            SetupCodeTextBox(txtCode1, "src = new IplImage(path, LoadMode.Color);", new Point(110, y));
            panel.Controls.Add(txtCode1);
            y += 30;

            // Step 2: Bind to PictureBox
            btnStep2.Text = "2단계: 출력";
            btnStep2.Location = new Point(15, y);
            btnStep2.Size = new Size(90, 25);
            btnStep2.Font = new Font("Malgun Gothic", 8.5F, FontStyle.Bold);
            btnStep2.Enabled = false;
            btnStep2.Click += BtnStep2_Click;
            panel.Controls.Add(btnStep2);

            SetupCodeTextBox(txtCode2, "pictureBox.ImageIpl = src;", new Point(110, y));
            panel.Controls.Add(txtCode2);
            y += 30;

            // Step 3: Release Resource
            btnStep3.Text = "3단계: 해제";
            btnStep3.Location = new Point(15, y);
            btnStep3.Size = new Size(90, 25);
            btnStep3.Font = new Font("Malgun Gothic", 8.5F, FontStyle.Bold);
            btnStep3.Enabled = false;
            btnStep3.Click += BtnStep3_Click;
            panel.Controls.Add(btnStep3);

            SetupCodeTextBox(txtCode3, "Cv.ReleaseImage(src); src = null;", new Point(110, y));
            panel.Controls.Add(txtCode3);
            y += 35;

            // 3. Memory Monitoring Section
            var lblSection3 = new Label
            {
                Text = "3. 실시간 네이티브 힙 모니터",
                Font = new Font("Malgun Gothic", 9.5F, FontStyle.Bold),
                Location = new Point(15, y),
                AutoSize = true,
                ForeColor = Color.Navy
            };
            panel.Controls.Add(lblSection3);
            y += 20;

            // Active Memory Bar
            lblActiveMemInfo.Text = "활성 네이티브 메모리: 0 Bytes";
            lblActiveMemInfo.Location = new Point(15, y);
            lblActiveMemInfo.Size = new Size(350, 15);
            lblActiveMemInfo.Font = new Font("Malgun Gothic", 8.5F, FontStyle.Bold);
            panel.Controls.Add(lblActiveMemInfo);
            y += 20;

            pnlActiveBg.Location = new Point(15, y);
            pnlActiveBg.Size = new Size(360, 12);
            pnlActiveBg.BackColor = Color.FromArgb(40, 40, 40);
            pnlActiveFill.Location = new Point(0, 0);
            pnlActiveFill.Size = new Size(0, 12);
            pnlActiveFill.BackColor = Color.LimeGreen;
            pnlActiveBg.Controls.Add(pnlActiveFill);
            panel.Controls.Add(pnlActiveBg);
            y += 20;

            // Leaked Memory Bar
            lblLeakedMemInfo.Text = "누출(누수) 메모리 합계: 0 Bytes";
            lblLeakedMemInfo.Location = new Point(15, y);
            lblLeakedMemInfo.Size = new Size(350, 15);
            lblLeakedMemInfo.ForeColor = Color.Crimson;
            lblLeakedMemInfo.Font = new Font("Malgun Gothic", 8.5F, FontStyle.Bold);
            panel.Controls.Add(lblLeakedMemInfo);
            y += 20;

            pnlLeakedBg.Location = new Point(15, y);
            pnlLeakedBg.Size = new Size(360, 12);
            pnlLeakedBg.BackColor = Color.FromArgb(40, 40, 40);
            pnlLeakedFill.Location = new Point(0, 0);
            pnlLeakedFill.Size = new Size(0, 12);
            pnlLeakedFill.BackColor = Color.Crimson;
            pnlLeakedBg.Controls.Add(pnlLeakedFill);
            panel.Controls.Add(pnlLeakedBg);
            y += 20;

            // Log / Warning Status
            lblWarning.Location = new Point(15, y);
            lblWarning.Size = new Size(360, 32);
            lblWarning.BackColor = Color.LightYellow;
            lblWarning.ForeColor = Color.DarkGoldenrod;
            lblWarning.BorderStyle = BorderStyle.FixedSingle;
            lblWarning.Text = "상태: 파일을 선택한 후 [1단계: 로드]를 실행하세요.";
            lblWarning.Padding = new Padding(3);
            lblWarning.Font = new Font("Malgun Gothic", 8.5F, FontStyle.Regular);
            panel.Controls.Add(lblWarning);
            y += 37;

            // Reset/Action Buttons
            btnResetLeaks.Text = "누수 상태 초기화";
            btnResetLeaks.Location = new Point(15, y);
            btnResetLeaks.Size = new Size(120, 25);
            btnResetLeaks.Click += BtnResetLeaks_Click;
            panel.Controls.Add(btnResetLeaks);

            btnFreeNow.Text = "즉시 안전 해제";
            btnFreeNow.Location = new Point(145, y);
            btnFreeNow.Size = new Size(120, 25);
            btnFreeNow.Click += BtnFreeNow_Click;
            panel.Controls.Add(btnFreeNow);
            y += 33;

            // 4. Output Options and Info
            var lblSection4 = new Label
            {
                Text = "4. 출력 옵션 및 정보",
                Font = new Font("Malgun Gothic", 9.5F, FontStyle.Bold),
                Location = new Point(15, y),
                AutoSize = true,
                ForeColor = Color.Navy
            };
            panel.Controls.Add(lblSection4);
            y += 20;

            var lblSizeName = new Label
            {
                Text = "SizeMode:",
                Location = new Point(15, y),
                Size = new Size(65, 20)
            };
            panel.Controls.Add(lblSizeName);

            cmbSizeMode.Location = new Point(80, y);
            cmbSizeMode.Size = new Size(110, 25);
            cmbSizeMode.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbSizeMode.Items.AddRange(new object[] { "Normal", "StretchImage", "Zoom", "CenterImage" });
            cmbSizeMode.SelectedIndex = 1; // StretchImage default
            var sizeModes = new[] { PictureBoxSizeMode.Normal, PictureBoxSizeMode.StretchImage, PictureBoxSizeMode.Zoom, PictureBoxSizeMode.CenterImage };
            cmbSizeMode.SelectedIndexChanged += (s, e) => pictureBox.SizeMode = sizeModes[cmbSizeMode.SelectedIndex];
            panel.Controls.Add(cmbSizeMode);

            lblResolution.Text = "해상도: -";
            lblResolution.Location = new Point(200, y);
            lblResolution.Size = new Size(170, 15);
            lblResolution.Font = new Font("Malgun Gothic", 8.5F, FontStyle.Bold);
            panel.Controls.Add(lblResolution);

            lblChannels.Text = "채널 수: -";
            lblChannels.Location = new Point(200, y + 17);
            lblChannels.Size = new Size(170, 15);
            lblChannels.Font = new Font("Malgun Gothic", 8.5F, FontStyle.Bold);
            panel.Controls.Add(lblChannels);
            y += 40;

            // 5. Theory TextBox
            var lblSection5 = new Label
            {
                Text = "5. 핵심 이론 요약",
                Font = new Font("Malgun Gothic", 9.5F, FontStyle.Bold),
                Location = new Point(15, y),
                AutoSize = true,
                ForeColor = Color.Navy
            };
            panel.Controls.Add(lblSection5);
            y += 20;

            var txtTheory = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Location = new Point(15, y),
                Size = new Size(360, 75),
                BorderStyle = BorderStyle.Fixed3D,
                BackColor = Color.WhiteSmoke,
                Text = "【핵심 이론 및 원리】\r\n" +
                       "1. 가비지 컬렉션의 한계\r\n" +
                       "   C# GC는 네이티브 힙(C++ OpenCV)에 생성된 픽셀 데이터를 제때 해제하지 못하므로, 누수가 누적됩니다.\r\n" +
                       "2. 메모리 누수 재현 실험\r\n" +
                       "   이전 이미지를 해제(3단계)하지 않고 1단계를 반복 클릭하면, 사용되지 않는 메모리가 네이티브 힙에 방치되어 붉은색 누출 게이지가 올라갑니다.\r\n" +
                       "3. 안전 해제\r\n" +
                       "   Cv.ReleaseImage(src)를 호출하면 활성 픽셀 데이터가 즉각 소멸합니다."
            };
            panel.Controls.Add(txtTheory);

            tabLab.Controls.Add(new TextBox
            {
                Text = "실습 가이드: 파일 선택 후 [1단계: 로드] -> [2단계: 출력] 순서대로 실행하세요. 3단계를 생략하고 1단계를 다시 실행하면 '메모리 누수'가 유발되는 현상을 관찰할 수 있습니다.",
                Location = new Point(20, 520),
                Size = new Size(640, 45),
                Multiline = true,
                ReadOnly = true,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.LightYellow,
                Font = new Font("Malgun Gothic", 9.5F)
            });
        }

        private void SetupCodeTextBox(TextBox txt, string code, Point loc)
        {
            txt.Multiline = true;
            txt.ReadOnly = true;
            txt.BackColor = Color.FromArgb(30, 30, 30);
            txt.ForeColor = Color.LightGreen;
            txt.Font = new Font("Consolas", 8.5F);
            txt.Text = code;
            txt.Location = loc;
            txt.Size = new Size(265, 25);
            txt.BorderStyle = BorderStyle.None;
        }

        private void BtnOpen_Click(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif;*.tif;*.tiff|All Files|*.*";
                dialog.Title = "출력할 이미지 파일 선택";
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
                // Fallback to project root if run from bin/Debug during development
                defaultPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\Italia.jpg");
            }

            if (File.Exists(defaultPath))
            {
                SelectFile(defaultPath);
            }
            else
            {
                MessageBox.Show("기본 이미지 'Italia.jpg'를 찾을 수 없습니다.\n프로젝트 폴더 또는 실행 파일 위치에 저장해 주세요.", "파일 없음", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void SelectFile(string filepath)
        {
            selectedFilePath = filepath;
            lblFilePath.Text = $"선택됨: {Path.GetFileName(filepath)} (대기 중)";
            lblWarning.Text = "상태: 파일 준비 완료. [1단계: 로드] 버튼을 눌러 메모리에 적재하세요.";
            lblWarning.BackColor = Color.LightYellow;
            lblWarning.ForeColor = Color.DarkGoldenrod;
            
            btnStep1.Enabled = true;
            btnStep2.Enabled = false;
            btnStep3.Enabled = false;
        }

        // [Step 1] Load Image to Native Memory
        private void BtnStep1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(selectedFilePath) || !File.Exists(selectedFilePath))
            {
                MessageBox.Show("파일을 먼저 선택해 주세요.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Check for MEMORY LEAK!
                // If src is already loaded and we overwrite it without Cv.ReleaseImage, it's a leak.
                if (src != null)
                {
                    long leakSize = src.Width * src.Height * src.NChannels;
                    totalLeakedBytes += leakSize;
                    
                    lblWarning.Text = $"[경고] 메모리 누출 발생! 이전 이미지({leakSize:N0} Bytes)를 해제하지 않아 힙에 유실되었습니다.";
                    lblWarning.BackColor = Color.MistyRose;
                    lblWarning.ForeColor = Color.Red;
                }
                else
                {
                    lblWarning.Text = "상태: 1단계 완료. 이미지가 네이티브 힙에 로드되었습니다. (화면 미출력)";
                    lblWarning.BackColor = Color.LightCyan;
                    lblWarning.ForeColor = Color.DarkBlue;
                }

                // Handle non-ASCII path (OpenCV 2.4 limit)
                string pathToOpen = selectedFilePath;
                foreach (char c in selectedFilePath)
                {
                    if (c > 127)
                    {
                        string ext = Path.GetExtension(selectedFilePath);
                        string tempPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "temp_image_playback" + ext);
                        if (File.Exists(tempPath)) File.Delete(tempPath);
                        File.Copy(selectedFilePath, tempPath, true);
                        pathToOpen = tempPath;
                        break;
                    }
                }
                pathToOpen = pathToOpen.Replace('\\', '/');

                // Load image using legacy IplImage
                src = new IplImage(pathToOpen, LoadMode.Color);
                if (src == null)
                {
                    MessageBox.Show("이미지 디코딩에 실패했습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                currentAllocatedBytes = src.Width * src.Height * src.NChannels;

                // Update Info Labels
                lblResolution.Text = $"해상도: {src.Width} x {src.Height}";
                lblChannels.Text = $"채널 수: {src.NChannels} (BGR)";

                // Enable subsequent steps
                btnStep2.Enabled = true;
                btnStep3.Enabled = true;

                UpdateMemoryGauges();
            }
            catch (Exception ex)
            {
                MessageBox.Show("이미지 로드 중 오류 발생:\n" + ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // [Step 2] Bind Image to PictureBox
        private void BtnStep2_Click(object sender, EventArgs e)
        {
            if (src == null)
            {
                MessageBox.Show("메모리에 로드된 이미지가 없습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            pictureBox.ImageIpl = src;
            
            lblWarning.Text = "상태: 2단계 완료. IplImage가 PictureBoxIpl의 ImageIpl 속성에 성공적으로 바인딩되었습니다.";
            lblWarning.BackColor = Color.LightGreen;
            lblWarning.ForeColor = Color.DarkGreen;
        }

        // [Step 3] Release Image Memory
        private void BtnStep3_Click(object sender, EventArgs e)
        {
            if (src == null) return;

            pictureBox.ImageIpl = null;
            Cv.ReleaseImage(src);
            src = null;

            currentAllocatedBytes = 0;

            lblWarning.Text = "상태: 3단계 완료. Cv.ReleaseImage(src)를 통해 네이티브 리소스가 회수되었습니다.";
            lblWarning.BackColor = Color.LightCyan;
            lblWarning.ForeColor = Color.DarkBlue;

            btnStep2.Enabled = false;
            btnStep3.Enabled = false;

            UpdateMemoryGauges();
        }

        private void BtnResetLeaks_Click(object sender, EventArgs e)
        {
            totalLeakedBytes = 0;
            lblWarning.Text = "상태: 누수 메모리 기록을 초기화했습니다.";
            lblWarning.BackColor = Color.LightYellow;
            lblWarning.ForeColor = Color.DarkGoldenrod;
            UpdateMemoryGauges();
        }

        private void BtnFreeNow_Click(object sender, EventArgs e)
        {
            if (src != null)
            {
                pictureBox.ImageIpl = null;
                Cv.ReleaseImage(src);
                src = null;
                currentAllocatedBytes = 0;
                
                lblWarning.Text = "상태: 활성 네이티브 메모리를 즉시 안전하게 회수했습니다.";
                lblWarning.BackColor = Color.LightCyan;
                lblWarning.ForeColor = Color.DarkBlue;
                
                btnStep2.Enabled = false;
                btnStep3.Enabled = false;
                UpdateMemoryGauges();
            }
        }

        private void UpdateMemoryGauges()
        {
            lblActiveMemInfo.Text = $"활성 네이티브 메모리: {currentAllocatedBytes:N0} Bytes ({currentAllocatedBytes / 1024.0 / 1024.0:F2} MB)";
            lblLeakedMemInfo.Text = $"누출(누수) 메모리 합계: {totalLeakedBytes:N0} Bytes ({totalLeakedBytes / 1024.0 / 1024.0:F2} MB)";

            // Calculate fill bar widths
            int activeWidth = (int)Math.Min(360, (currentAllocatedBytes * 360) / MaxBytes);
            int leakedWidth = (int)Math.Min(360, (totalLeakedBytes * 360) / MaxBytes);

            pnlActiveFill.Width = activeWidth;
            pnlLeakedFill.Width = leakedWidth;
        }

        private void ReleaseCurrentImage()
        {
            if (src != null)
            {
                pictureBox.ImageIpl = null;
                Cv.ReleaseImage(src);
                src = null;
                currentAllocatedBytes = 0;
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
            ReleaseCurrentImage();
            picDiagram.Image?.Dispose();
            base.OnFormClosed(e);
        }
    }
}
