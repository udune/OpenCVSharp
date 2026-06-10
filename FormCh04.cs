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
    public class FormCh04 : Form
    {
        private readonly TabControl tabControl = new TabControl();
        private readonly TabPage tabPlayback = new TabPage();
        private readonly TabPage tabRecord = new TabPage();

        // --- Playback Tab ---
        private readonly PictureBox pbPlay = new PictureBox();
        private readonly Button btnPlayOpenFile = new Button();
        private readonly Button btnPlayStart = new Button();
        private readonly Button btnPlayPause = new Button();
        private readonly Button btnPlayStop = new Button();
        private readonly TrackBar trackFrame = new TrackBar();
        private readonly NumericUpDown nudPlayInterval = new NumericUpDown();
        private readonly Label lblPlayFrameInfo = new Label();
        private readonly Label lblPlayStatus = new Label();
        private readonly Timer playTimer = new Timer();
        private CvCapture playCapture;
        private string tempVideoPath;
        private int playTotalFrames;
        private double playFps;
        private bool isTracking;

        // --- Record Tab ---
        private readonly PictureBox pbRec = new PictureBox();
        private readonly NumericUpDown nudRecCameraIndex = new NumericUpDown();
        private readonly ComboBox cmbRecResolution = new ComboBox();
        private readonly ComboBox cmbRecCodec = new ComboBox();
        private readonly Button btnRecStart = new Button();
        private readonly Button btnRecStop = new Button();
        private readonly Label lblRecStatus = new Label();
        private readonly Label lblRecInfo = new Label();
        private readonly Timer recTimer = new Timer();
        private CvCapture recCapture;
        private CvVideoWriter recWriter;
        private int recFrameCount;
        private string recFilePath;
        private int recW;
        private int recH;

        public FormCh04()
        {
            Text = "CH04 - 동영상 파일 출력 (재생/저장) 실습";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(1100, 640);
            Font = new Font("Malgun Gothic", 9F, FontStyle.Regular);

            tabControl.Dock = DockStyle.Fill;
            tabPlayback.Text = "동영상 파일 재생 (Screen Output)";
            tabRecord.Text = "동영상 파일 저장 (File Output)";
            tabControl.Controls.Add(tabPlayback);
            tabControl.Controls.Add(tabRecord);
            Controls.Add(tabControl);

            InitializePlaybackTab();
            InitializeRecordTab();
        }

        // =====================================================================
        // Playback Tab
        // =====================================================================
        private void InitializePlaybackTab()
        {
            pbPlay.Location = new Point(20, 20);
            pbPlay.Size = new Size(700, 420);
            pbPlay.SizeMode = PictureBoxSizeMode.Zoom;
            pbPlay.BorderStyle = BorderStyle.FixedSingle;
            pbPlay.BackColor = Color.Black;
            tabPlayback.Controls.Add(pbPlay);

            trackFrame.Location = new Point(20, 455);
            trackFrame.Size = new Size(700, 45);
            trackFrame.TickStyle = TickStyle.None;
            trackFrame.Enabled = false;
            trackFrame.Scroll += TrackFrame_Scroll;
            trackFrame.MouseDown += (s, e) => isTracking = true;
            trackFrame.MouseUp += (s, e) => isTracking = false;
            tabPlayback.Controls.Add(trackFrame);

            lblPlayFrameInfo.Location = new Point(20, 500);
            lblPlayFrameInfo.Size = new Size(700, 25);
            lblPlayFrameInfo.Font = new Font("Malgun Gothic", 9.75F, FontStyle.Bold);
            lblPlayFrameInfo.Text = "프레임: 0 / 0 (00:00:00 / 00:00:00)";
            tabPlayback.Controls.Add(lblPlayFrameInfo);

            var panel = new Panel
            {
                Location = new Point(740, 20),
                Size = new Size(330, 540),
                BorderStyle = BorderStyle.FixedSingle
            };
            tabPlayback.Controls.Add(panel);

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
                y += 25;
            };

            addHeader("1. 동영상 파일 선택");
            btnPlayOpenFile.Text = "파일 열기 (Open File)";
            btnPlayOpenFile.Location = new Point(15, y);
            btnPlayOpenFile.Size = new Size(290, 35);
            btnPlayOpenFile.Click += BtnPlayOpenFile_Click;
            panel.Controls.Add(btnPlayOpenFile);
            y += 45;

            addHeader("2. 재생 제어");
            btnPlayStart.Text = "재생 (Play)";
            btnPlayStart.Location = new Point(15, y);
            btnPlayStart.Size = new Size(90, 32);
            btnPlayStart.Enabled = false;
            btnPlayStart.Click += BtnPlayStart_Click;
            panel.Controls.Add(btnPlayStart);

            btnPlayPause.Text = "일시정지";
            btnPlayPause.Location = new Point(115, y);
            btnPlayPause.Size = new Size(90, 32);
            btnPlayPause.Enabled = false;
            btnPlayPause.Click += BtnPlayPause_Click;
            panel.Controls.Add(btnPlayPause);

            btnPlayStop.Text = "정지 (Stop)";
            btnPlayStop.Location = new Point(215, y);
            btnPlayStop.Size = new Size(90, 32);
            btnPlayStop.Enabled = false;
            btnPlayStop.Click += BtnPlayStop_Click;
            panel.Controls.Add(btnPlayStop);
            y += 45;

            addHeader("3. 재생 속도 조절 (Interval, ms)");
            nudPlayInterval.Location = new Point(15, y);
            nudPlayInterval.Size = new Size(290, 25);
            nudPlayInterval.Minimum = 5;
            nudPlayInterval.Maximum = 500;
            nudPlayInterval.Value = 33;
            nudPlayInterval.ValueChanged += (s, e) => playTimer.Interval = (int)nudPlayInterval.Value;
            panel.Controls.Add(nudPlayInterval);
            y += 40;

            addHeader("■ 재생 정보 및 상태");
            lblPlayStatus.Location = new Point(15, y);
            lblPlayStatus.Size = new Size(290, 45);
            lblPlayStatus.BorderStyle = BorderStyle.Fixed3D;
            lblPlayStatus.BackColor = Color.LightYellow;
            lblPlayStatus.Padding = new Padding(5);
            lblPlayStatus.Text = "상태: 파일 대기 중";
            panel.Controls.Add(lblPlayStatus);
            y += 60;

            panel.Controls.Add(new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Location = new Point(15, y),
                Size = new Size(290, 180),
                BorderStyle = BorderStyle.Fixed3D,
                BackColor = Color.WhiteSmoke,
                Text = "【핵심 이론 및 원리】\r\n" +
                       "1. 동영상 읽기: CvCapture.FromFile(path)\r\n" +
                       "   - 동영상 컨테이너(mp4, avi)를 열어 디코딩합니다.\r\n" +
                       "2. 재생 원리: Timer 루프 + QueryFrame()\r\n" +
                       "   - 지정된 Interval마다 디코더로부터 프레임을 가져와 비트맵으로 렌더링합니다.\r\n" +
                       "3. 위치 이동(Seek): SetCaptureProperty(..)\r\n" +
                       "   - CaptureProperty.PosFrames 속성을 수정해 재생 헤드를 임의의 프레임 위치로 이동시킵니다."
            });

            playTimer.Tick += PlayTimer_Tick;
        }

        private void BtnPlayOpenFile_Click(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = "Video Files|*.mp4;*.avi;*.mov;*.wmv;*.mkv|All Files|*.*";
                dialog.Title = "재생할 동영상 파일 선택";
                if (dialog.ShowDialog() == DialogResult.OK)
                    LoadVideo(dialog.FileName);
            }
        }

        // OpenCV 2.4 FFmpeg does not support H.265/HEVC. Scan file start+end for hvc1/hev1 codec box.
        private static bool IsHevcVideo(string filePath)
        {
            const int scanSize = 65536;
            try
            {
                using (var stream = File.OpenRead(filePath))
                {
                    int firstLen = (int)Math.Min(stream.Length, scanSize);
                    var first = new byte[firstLen];
                    stream.Read(first, 0, firstLen);
                    if (ContainsHevcMarker(first)) return true;

                    if (stream.Length > scanSize)
                    {
                        stream.Seek(-scanSize, SeekOrigin.End);
                        var last = new byte[scanSize];
                        stream.Read(last, 0, scanSize);
                        if (ContainsHevcMarker(last)) return true;
                    }
                }
            }
            catch { }
            return false;
        }

        private static bool ContainsHevcMarker(byte[] buf)
        {
            for (int i = 0; i < buf.Length - 4; i++)
            {
                if (buf[i] == 'h' && buf[i + 3] == '1' &&
                    ((buf[i + 1] == 'v' && buf[i + 2] == 'c') ||
                     (buf[i + 1] == 'e' && buf[i + 2] == 'v')))
                    return true;
            }
            return false;
        }

        private void LoadVideo(string filepath)
        {
            try
            {
                StopPlayback();

                if (IsHevcVideo(filepath))
                {
                    MessageBox.Show(
                        "이 동영상은 H.265(HEVC) 코덱으로 인코딩되어 있습니다.\n" +
                        "OpenCV 2.4는 H.265를 지원하지 않아 재생할 수 없습니다.\n\n" +
                        "※ 해결 방법: HandBrake, VLC, FFmpeg 등으로\n" +
                        "H.264(AVC) 코덱의 MP4 또는 AVI로 변환해 주세요.",
                        "지원하지 않는 코덱 (H.265/HEVC)", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // OpenCV 2.4 does not support Unicode paths on Windows — copy to ASCII temp path
                string pathToOpen = filepath;
                foreach (char c in filepath)
                {
                    if (c > 127)
                    {
                        string ext = Path.GetExtension(filepath);
                        tempVideoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "temp_video_playback" + ext);
                        if (File.Exists(tempVideoPath)) File.Delete(tempVideoPath);
                        File.Copy(filepath, tempVideoPath, true);
                        pathToOpen = tempVideoPath;
                        break;
                    }
                }

                // OpenCV C/C++ path parser prefers forward slashes
                pathToOpen = pathToOpen.Replace('\\', '/');

                playCapture = CvCapture.FromFile(pathToOpen);
                if (playCapture == null)
                {
                    MessageBox.Show(
                        "비디오를 로드하는 데 실패했습니다.\n\n" +
                        "※ OpenCV 2.4 지원 형식: H.264(AVC) MP4, MJPEG/MPEG-4 AVI",
                        "비디오 로드 에러", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    CleanupTempFile();
                    return;
                }

                // Verify the video can actually be decoded
                var testFrame = playCapture.QueryFrame();
                if (testFrame == null)
                {
                    playCapture.Dispose();
                    playCapture = null;
                    CleanupTempFile();
                    MessageBox.Show(
                        "동영상 첫 프레임을 읽을 수 없습니다.\n지원하지 않는 코덱이거나 파일이 손상되었을 수 있습니다.\n\n" +
                        "※ 권장 형식: H.264 코덱의 MP4, MJPEG/MPEG-4의 AVI",
                        "비디오 디코딩 실패", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                playCapture.SetCaptureProperty(CaptureProperty.PosFrames, 0);

                playTotalFrames = (int)playCapture.GetCaptureProperty(CaptureProperty.FrameCount);
                playFps = playCapture.GetCaptureProperty(CaptureProperty.Fps);
                if (playFps <= 0) playFps = 30;

                nudPlayInterval.Value = (decimal)Math.Max(5, Math.Min(500, 1000.0 / playFps));
                playTimer.Interval = (int)nudPlayInterval.Value;

                trackFrame.Minimum = 0;
                trackFrame.Maximum = Math.Max(0, playTotalFrames - 1);
                trackFrame.Value = 0;
                trackFrame.Enabled = true;

                btnPlayStart.Enabled = true;
                btnPlayPause.Enabled = false;
                btnPlayStop.Enabled = true;

                lblPlayStatus.Text = $"파일명: {Path.GetFileName(filepath)}\nFPS: {playFps:F2} / 총 {playTotalFrames} 프레임";
                UpdatePlayFrameInfo(0);
                ShowFrame(0);
            }
            catch (Exception ex)
            {
                CleanupTempFile();
                MessageBox.Show(
                    "비디오 로딩 실패: " + ex.Message + "\n\n" +
                    "※ OpenCV 2.4 지원 형식: H.264(AVC) MP4, MJPEG/MPEG-4 AVI",
                    "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CleanupTempFile()
        {
            if (tempVideoPath != null && File.Exists(tempVideoPath))
            {
                try { File.Delete(tempVideoPath); } catch { }
                tempVideoPath = null;
            }
        }

        private void ShowFrame(int frameIndex)
        {
            if (playCapture == null) return;
            playCapture.SetCaptureProperty(CaptureProperty.PosFrames, frameIndex);
            var ipl = playCapture.QueryFrame();
            if (ipl == null) return;

            var old = pbPlay.Image;
            pbPlay.Image = BitmapConverter.ToBitmap(ipl);
            old?.Dispose();
        }

        private void UpdatePlayFrameInfo(int currentFrame)
        {
            lblPlayFrameInfo.Text =
                $"프레임: {currentFrame} / {playTotalFrames} " +
                $"({TimeSpan.FromSeconds(currentFrame / playFps):hh\\:mm\\:ss} / " +
                $"{TimeSpan.FromSeconds(playTotalFrames / playFps):hh\\:mm\\:ss})";
        }

        private void BtnPlayStart_Click(object sender, EventArgs e)
        {
            if (playCapture == null) return;
            playTimer.Start();
            btnPlayStart.Enabled = false;
            btnPlayPause.Enabled = true;
            btnPlayStop.Enabled = true;
            lblPlayStatus.Text = "상태: 재생 중";
        }

        private void BtnPlayPause_Click(object sender, EventArgs e)
        {
            playTimer.Stop();
            btnPlayStart.Enabled = true;
            btnPlayPause.Enabled = false;
            lblPlayStatus.Text = "상태: 일시 정지";
        }

        private void BtnPlayStop_Click(object sender, EventArgs e) => StopPlayback();

        private void StopPlayback()
        {
            playTimer.Stop();
            if (playCapture != null)
            {
                playCapture.Dispose();
                playCapture = null;
            }
            pbPlay.Image?.Dispose();
            pbPlay.Image = null;
            trackFrame.Value = 0;
            trackFrame.Enabled = false;
            btnPlayStart.Enabled = false;
            btnPlayPause.Enabled = false;
            btnPlayStop.Enabled = false;
            lblPlayStatus.Text = "상태: 정지됨";
            lblPlayFrameInfo.Text = "프레임: 0 / 0 (00:00:00 / 00:00:00)";
            CleanupTempFile();
        }

        private void PlayTimer_Tick(object sender, EventArgs e)
        {
            if (playCapture == null || isTracking) return;

            // Query frame first — correctly detects end-of-video even when FrameCount == 0
            var ipl = playCapture.QueryFrame();
            if (ipl == null)
            {
                StopPlayback();
                lblPlayStatus.Text = "상태: 재생 완료";
                return;
            }

            var old = pbPlay.Image;
            pbPlay.Image = BitmapConverter.ToBitmap(ipl);
            old?.Dispose();

            int currentFrame = (int)playCapture.GetCaptureProperty(CaptureProperty.PosFrames);
            trackFrame.Value = Math.Min(currentFrame, trackFrame.Maximum);
            UpdatePlayFrameInfo(currentFrame);

            if (playTotalFrames > 0 && currentFrame >= playTotalFrames)
            {
                StopPlayback();
                lblPlayStatus.Text = "상태: 재생 완료";
            }
        }

        private void TrackFrame_Scroll(object sender, EventArgs e)
        {
            if (playCapture == null) return;
            ShowFrame(trackFrame.Value);
            UpdatePlayFrameInfo(trackFrame.Value);
        }

        // =====================================================================
        // Record Tab
        // =====================================================================
        private void InitializeRecordTab()
        {
            pbRec.Location = new Point(20, 20);
            pbRec.Size = new Size(700, 420);
            pbRec.SizeMode = PictureBoxSizeMode.Zoom;
            pbRec.BorderStyle = BorderStyle.FixedSingle;
            pbRec.BackColor = Color.Black;
            tabRecord.Controls.Add(pbRec);

            var panel = new Panel
            {
                Location = new Point(740, 20),
                Size = new Size(330, 540),
                BorderStyle = BorderStyle.FixedSingle
            };
            tabRecord.Controls.Add(panel);

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
                y += 25;
            };

            addHeader("1. 카메라 선택 (Index)");
            nudRecCameraIndex.Location = new Point(15, y);
            nudRecCameraIndex.Size = new Size(290, 25);
            nudRecCameraIndex.Minimum = 0;
            nudRecCameraIndex.Maximum = 5;
            nudRecCameraIndex.Value = 0;
            panel.Controls.Add(nudRecCameraIndex);
            y += 35;

            addHeader("2. 녹화 해상도 (Resolution)");
            cmbRecResolution.Location = new Point(15, y);
            cmbRecResolution.Size = new Size(290, 25);
            cmbRecResolution.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbRecResolution.Items.AddRange(new object[] { "320 x 240", "640 x 480" });
            cmbRecResolution.SelectedIndex = 1;
            panel.Controls.Add(cmbRecResolution);
            y += 35;

            addHeader("3. 비디오 코덱 (Codec)");
            cmbRecCodec.Location = new Point(15, y);
            cmbRecCodec.Size = new Size(290, 25);
            cmbRecCodec.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbRecCodec.Items.AddRange(new object[] { "XVID (AVI)", "DIB / Raw (AVI)", "MJPG (AVI)" });
            cmbRecCodec.SelectedIndex = 2;
            panel.Controls.Add(cmbRecCodec);
            y += 45;

            addHeader("4. 녹화 시작 / 종료");
            btnRecStart.Text = "녹화 시작 (Record)";
            btnRecStart.Location = new Point(15, y);
            btnRecStart.Size = new Size(130, 35);
            btnRecStart.Click += BtnRecStart_Click;
            panel.Controls.Add(btnRecStart);

            btnRecStop.Text = "녹화 중지 (Stop)";
            btnRecStop.Location = new Point(175, y);
            btnRecStop.Size = new Size(130, 35);
            btnRecStop.Enabled = false;
            btnRecStop.Click += BtnRecStop_Click;
            panel.Controls.Add(btnRecStop);
            y += 50;

            addHeader("■ 녹화 진행 상태");
            lblRecStatus.Location = new Point(15, y);
            lblRecStatus.Size = new Size(290, 45);
            lblRecStatus.BorderStyle = BorderStyle.Fixed3D;
            lblRecStatus.BackColor = Color.LightYellow;
            lblRecStatus.Padding = new Padding(5);
            lblRecStatus.Text = "상태: 녹화 준비 완료";
            panel.Controls.Add(lblRecStatus);
            y += 55;

            lblRecInfo.Location = new Point(15, y);
            lblRecInfo.Size = new Size(290, 20);
            lblRecInfo.Font = new Font("Malgun Gothic", 9F, FontStyle.Bold);
            lblRecInfo.Text = "저장 프레임 수: 0 | 크기: 0 KB";
            panel.Controls.Add(lblRecInfo);
            y += 30;

            panel.Controls.Add(new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Location = new Point(15, y),
                Size = new Size(290, 150),
                BorderStyle = BorderStyle.Fixed3D,
                BackColor = Color.WhiteSmoke,
                Text = "【핵심 이론 및 원리】\r\n" +
                       "1. 파일 작성: CvVideoWriter(path, codec, fps, size)\r\n" +
                       "   - 지정된 코덱(예: MJPG, XVID), 속도, 크기로 출력 동영상 스트림 파일을 초기화합니다.\r\n" +
                       "2. 프레임 주입: WriteFrame(iplImage)\r\n" +
                       "   - 매 캡처 프레임을 라이터 객체에 써넣습니다.\r\n" +
                       "3. 최종 저장: Dispose() 필수!\r\n" +
                       "   - 라이터 객체를 닫을 때 파일 헤더 정보와 인덱스가 완전하게 기록되어 재생 가능한 정상 비디오가 됩니다."
            });

            tabRecord.Controls.Add(new Label
            {
                Text = "실습 가이드: 녹화 시작 시 대화상자에서 저장할 경로를 정한 후 녹화를 시작할 수 있습니다.",
                Location = new Point(20, 520),
                Size = new Size(720, 40),
                BorderStyle = BorderStyle.Fixed3D,
                BackColor = Color.LightYellow,
                Padding = new Padding(5)
            });

            recTimer.Interval = 33;
            recTimer.Tick += RecTimer_Tick;
        }

        private void BtnRecStart_Click(object sender, EventArgs e)
        {
            try
            {
                StopRecording();

                using (var dialog = new SaveFileDialog())
                {
                    dialog.Filter = "AVI Video File|*.avi";
                    dialog.Title = "저장할 동영상 파일 명 지정";
                    dialog.FileName = "output_record.avi";
                    if (dialog.ShowDialog() != DialogResult.OK) return;
                    recFilePath = dialog.FileName;
                }

                recCapture = CvCapture.FromCamera((int)nudRecCameraIndex.Value);
                if (recCapture == null)
                {
                    MessageBox.Show("카메라 장치를 열 수 없습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                recW = cmbRecResolution.SelectedIndex == 0 ? 320 : 640;
                recH = cmbRecResolution.SelectedIndex == 0 ? 240 : 480;
                recCapture.SetCaptureProperty(CaptureProperty.FrameWidth, recW);
                recCapture.SetCaptureProperty(CaptureProperty.FrameHeight, recH);

                if (recCapture.QueryFrame() == null)
                {
                    recCapture.Dispose();
                    recCapture = null;
                    MessageBox.Show("카메라 영상 획득에 실패했습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string codecStr;
                switch (cmbRecCodec.SelectedIndex)
                {
                    case 0: codecStr = "XVID"; break;
                    case 1: codecStr = "\0\0\0\0"; break; // DIB/Raw uncompressed
                    default: codecStr = "MJPG"; break;
                }

                recWriter = new CvVideoWriter(recFilePath, codecStr, 30.0, new CvSize(recW, recH));
                recFrameCount = 0;
                recTimer.Start();

                btnRecStart.Enabled = false;
                btnRecStop.Enabled = true;
                nudRecCameraIndex.Enabled = false;
                cmbRecResolution.Enabled = false;
                cmbRecCodec.Enabled = false;
                lblRecStatus.Text = $"녹화 진행 중: {Path.GetFileName(recFilePath)}";
            }
            catch (Exception ex)
            {
                StopRecording();
                MessageBox.Show("녹화 초기화 오류: " + ex.Message, "에러", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnRecStop_Click(object sender, EventArgs e)
        {
            string savedPath = recFilePath;
            StopRecording();
            MessageBox.Show("녹화가 종료되었습니다.\n파일 저장 위치: " + savedPath, "녹화 완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void StopRecording()
        {
            recTimer.Stop();

            if (recWriter != null)
            {
                recWriter.Dispose();
                recWriter = null;
            }

            if (recCapture != null)
            {
                recCapture.Dispose();
                recCapture = null;
            }

            pbRec.Image?.Dispose();
            pbRec.Image = null;

            btnRecStart.Enabled = true;
            btnRecStop.Enabled = false;
            nudRecCameraIndex.Enabled = true;
            cmbRecResolution.Enabled = true;
            cmbRecCodec.Enabled = true;
            lblRecStatus.Text = "상태: 녹화 종료됨";
        }

        private void RecTimer_Tick(object sender, EventArgs e)
        {
            if (recCapture == null || recWriter == null) return;

            var ipl = recCapture.QueryFrame();
            if (ipl == null) return;

            if (ipl.Width != recW || ipl.Height != recH)
            {
                using (var resized = new IplImage(recW, recH, ipl.Depth, ipl.NChannels))
                {
                    ipl.Resize(resized);
                    recWriter.WriteFrame(resized);
                    var old = pbRec.Image;
                    pbRec.Image = BitmapConverter.ToBitmap(resized);
                    old?.Dispose();
                }
            }
            else
            {
                recWriter.WriteFrame(ipl);
                var old = pbRec.Image;
                pbRec.Image = BitmapConverter.ToBitmap(ipl);
                old?.Dispose();
            }

            recFrameCount++;

            long fileSizeKB = 0;
            if (File.Exists(recFilePath))
                fileSizeKB = new FileInfo(recFilePath).Length / 1024;
            lblRecInfo.Text = $"저장 프레임 수: {recFrameCount} | 크기: {fileSizeKB:N0} KB";
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            StopPlayback();
            StopRecording();
            playTimer.Dispose();
            recTimer.Dispose();
            base.OnFormClosed(e);
        }
    }
}
