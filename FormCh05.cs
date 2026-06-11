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
        private readonly PictureBoxIpl pictureBox = new PictureBoxIpl();
        private readonly Button btnOpen = new Button();
        private readonly Button btnLoadDefault = new Button();
        private readonly ComboBox cmbSizeMode = new ComboBox();

        private readonly Label lblFileName = new Label();
        private readonly Label lblResolution = new Label();
        private readonly Label lblChannels = new Label();
        private readonly Label lblMemorySize = new Label();

        private IplImage src;

        public FormCh05()
        {
            Text = "CH05 - 이미지 파일 출력 실습";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(1100, 600);
            Font = new Font("Malgun Gothic", 9F, FontStyle.Regular);

            pictureBox.Location = new Point(20, 20);
            pictureBox.Size = new Size(720, 480);
            pictureBox.SizeMode = PictureBoxSizeMode.StretchImage; // Default size mode as requested in the guide
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

            addHeader("1. 이미지 파일 선택");
            
            btnOpen.Text = "이미지 열기 (Open File)";
            btnOpen.Location = new Point(15, y);
            btnOpen.Size = new Size(270, 35);
            btnOpen.Click += BtnOpen_Click;
            panel.Controls.Add(btnOpen);
            y += 40;

            btnLoadDefault.Text = "기본 이미지 로드 (Italia.jpg)";
            btnLoadDefault.Location = new Point(15, y);
            btnLoadDefault.Size = new Size(270, 35);
            btnLoadDefault.Click += BtnLoadDefault_Click;
            panel.Controls.Add(btnLoadDefault);
            y += 45;

            var sizeModes = new[] { PictureBoxSizeMode.Normal, PictureBoxSizeMode.StretchImage, PictureBoxSizeMode.Zoom, PictureBoxSizeMode.CenterImage };
            addHeader("2. PictureBox 출력 모드 (SizeMode)");
            cmbSizeMode.Location = new Point(15, y);
            cmbSizeMode.Size = new Size(270, 25);
            cmbSizeMode.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbSizeMode.Items.AddRange(new object[] { "Normal (좌상단 정렬)", "StretchImage (비율무시 채움)", "Zoom (비율유지 채움)", "CenterImage (중앙 배치)" });
            cmbSizeMode.SelectedIndex = 1; // StretchImage is default in the guide
            cmbSizeMode.SelectedIndexChanged += (s, e) => pictureBox.SizeMode = sizeModes[cmbSizeMode.SelectedIndex];
            panel.Controls.Add(cmbSizeMode);
            y += 45;

            addHeader("■ 이미지 정보");
            
            lblFileName.Location = new Point(15, y);
            lblFileName.Size = new Size(275, 20);
            lblFileName.Font = new Font("Malgun Gothic", 9F, FontStyle.Bold);
            lblFileName.Text = "파일명: -";
            panel.Controls.Add(lblFileName);
            y += 20;

            lblResolution.Location = new Point(15, y);
            lblResolution.Size = new Size(275, 20);
            lblResolution.Font = new Font("Malgun Gothic", 9F, FontStyle.Bold);
            lblResolution.Text = "해상도: -";
            panel.Controls.Add(lblResolution);
            y += 20;

            lblChannels.Location = new Point(15, y);
            lblChannels.Size = new Size(275, 20);
            lblChannels.Font = new Font("Malgun Gothic", 9F, FontStyle.Bold);
            lblChannels.Text = "채널 수: -";
            panel.Controls.Add(lblChannels);
            y += 20;

            lblMemorySize.Location = new Point(15, y);
            lblMemorySize.Size = new Size(275, 20);
            lblMemorySize.Font = new Font("Malgun Gothic", 9F, FontStyle.Bold);
            lblMemorySize.Text = "메모리 크기: -";
            panel.Controls.Add(lblMemorySize);
            y += 35;

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
                       "1. 이미지 로드: new IplImage(path, LoadMode)\r\n" +
                       "   - 지정된 경로에서 이미지 파일을 메모리로 읽어옵니다.\r\n" +
                       "   - LoadMode.Color는 이미지를 3채널 BGR 컬러로 로드합니다.\r\n\r\n" +
                       "2. PictureBoxIpl 컨트롤 사용\r\n" +
                       "   - OpenCvSharp.UserInterface.PictureBoxIpl은 IplImage를 직접 렌더링할 수 있는 확장 PictureBox입니다.\r\n" +
                       "   - ImageIpl 속성에 IplImage 객체를 전달합니다.\r\n\r\n" +
                       "3. 메모리 관리 (Resource Release)\r\n" +
                       "   - C++ 엔진을 래핑하고 있으므로, 가비지 컬렉터에만 의존하면 native 메모리 누수가 발생할 수 있습니다.\r\n" +
                       "   - Cv.ReleaseImage(ref image) 또는 image.Dispose()를 호출하여 사용이 끝난 native 리소스를 안전하게 해제해야 합니다."
            });

            Controls.Add(pictureBox);
            Controls.Add(panel);
            Controls.Add(new Label
            {
                Text = "실습 가이드: [이미지 열기]를 눌러 이미지 파일을 선택하거나 [기본 이미지 로드]를 눌러 'Italia.jpg'를 불러오세요. PictureBoxIpl의 ImageIpl 속성을 통해 이미지가 출력됩니다.",
                Location = new Point(20, 520),
                Size = new Size(720, 40),
                BorderStyle = BorderStyle.Fixed3D,
                BackColor = Color.LightYellow,
                Padding = new Padding(5)
            });
        }

        private void BtnOpen_Click(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif;*.tif;*.tiff|All Files|*.*";
                dialog.Title = "출력할 이미지 파일 선택";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    LoadImage(dialog.FileName);
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
                LoadImage(defaultPath);
            }
            else
            {
                MessageBox.Show("기본 이미지 'Italia.jpg'를 찾을 수 없습니다.\n프로젝트 폴더 또는 실행 파일 위치에 저장해 주세요.", "파일 없음", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void LoadImage(string filepath)
        {
            try
            {
                ReleaseCurrentImage();

                // OpenCV 2.4 does not support Unicode paths on Windows — copy to ASCII temp path if needed
                string pathToOpen = filepath;
                foreach (char c in filepath)
                {
                    if (c > 127)
                    {
                        string ext = Path.GetExtension(filepath);
                        string tempPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "temp_image_playback" + ext);
                        if (File.Exists(tempPath)) File.Delete(tempPath);
                        File.Copy(filepath, tempPath, true);
                        pathToOpen = tempPath;
                        break;
                    }
                }

                pathToOpen = pathToOpen.Replace('\\', '/');

                src = new IplImage(pathToOpen, LoadMode.Color);
                if (src == null)
                {
                    MessageBox.Show("이미지를 로드하는 데 실패했습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                pictureBox.ImageIpl = src;

                lblFileName.Text = $"파일명: {Path.GetFileName(filepath)}";
                lblResolution.Text = $"해상도: {src.Width} x {src.Height}";
                lblChannels.Text = $"채널 수: {src.NChannels} ({GetChannelName(src.NChannels)})";
                long bytes = (long)src.Width * src.Height * src.NChannels;
                lblMemorySize.Text = $"메모리 크기: {bytes:N0} B ({bytes / 1024.0 / 1024.0:F2} MB)";
            }
            catch (Exception ex)
            {
                MessageBox.Show("이미지 로딩 중 오류가 발생했습니다:\n" + ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string GetChannelName(int channels)
        {
            switch (channels)
            {
                case 1: return "Grayscale";
                case 3: return "BGR Color";
                case 4: return "BGRA Color (Alpha)";
                default: return "Unknown";
            }
        }

        private void ReleaseCurrentImage()
        {
            if (src != null)
            {
                pictureBox.ImageIpl = null;
                
                // Call ReleaseImage as shown in the guide
                Cv.ReleaseImage(src);
                src = null;
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            ReleaseCurrentImage();
            base.OnFormClosed(e);
        }
    }
}
