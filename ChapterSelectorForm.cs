using System;
using System.Drawing;
using System.Windows.Forms;

namespace OpenCVSharp
{
    public class ChapterSelectorForm : Form
    {
        public ChapterSelectorForm()
        {
            Text = "OpenCVSharp 실습 챕터 선택";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(380, 425);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;

            var lblTitle = new Label
            {
                AutoSize = true,
                Text = "학습할 실습 챕터를 선택하세요",
                Font = new Font("Malgun Gothic", 12F, FontStyle.Bold),
                Location = new Point(20, 20)
            };

            var btnCh02 = new Button
            {
                Text = "002. Image Size & FPS 실습",
                Font = new Font("Malgun Gothic", 9.75F, FontStyle.Regular),
                Size = new Size(340, 42),
                Location = new Point(20, 60)
            };
            btnCh02.Click += (s, e) => OpenChapter(() => new FormCh02());

            var btnCh03 = new Button
            {
                Text = "003. Camera 출력 실습",
                Font = new Font("Malgun Gothic", 9.75F, FontStyle.Regular),
                Size = new Size(340, 42),
                Location = new Point(20, 110)
            };
            btnCh03.Click += (s, e) => OpenChapter(() => new FormCh03());

            var btnCh04 = new Button
            {
                Text = "004. 동영상 파일 출력 실습",
                Font = new Font("Malgun Gothic", 9.75F, FontStyle.Regular),
                Size = new Size(340, 42),
                Location = new Point(20, 160)
            };
            btnCh04.Click += (s, e) => OpenChapter(() => new FormCh04());

            var btnCh05 = new Button
            {
                Text = "005. 이미지 파일 출력 실습",
                Font = new Font("Malgun Gothic", 9.75F, FontStyle.Regular),
                Size = new Size(340, 42),
                Location = new Point(20, 210)
            };
            btnCh05.Click += (s, e) => OpenChapter(() => new FormCh05());

            var btnCh06 = new Button
            {
                Text = "006. 클래스 생성 & GrayScale 실습",
                Font = new Font("Malgun Gothic", 9.75F, FontStyle.Regular),
                Size = new Size(340, 42),
                Location = new Point(20, 260)
            };
            btnCh06.Click += (s, e) => OpenChapter(() => new FormCh06());

            var btnCh07 = new Button
            {
                Text = "007. IplImage 구조의 이해 실습",
                Font = new Font("Malgun Gothic", 9.75F, FontStyle.Regular),
                Size = new Size(340, 42),
                Location = new Point(20, 310)
            };
            btnCh07.Click += (s, e) => OpenChapter(() => new FormCh07());

            var btnCh08 = new Button
            {
                Text = "008. 색상 반전 & Binary 실습",
                Font = new Font("Malgun Gothic", 9.75F, FontStyle.Regular),
                Size = new Size(340, 42),
                Location = new Point(20, 360)
            };
            btnCh08.Click += (s, e) => OpenChapter(() => new FormCh08());

            Controls.Add(lblTitle);
            Controls.Add(btnCh02);
            Controls.Add(btnCh03);
            Controls.Add(btnCh04);
            Controls.Add(btnCh05);
            Controls.Add(btnCh06);
            Controls.Add(btnCh07);
            Controls.Add(btnCh08);
        }

        private void OpenChapter(Func<Form> formFactory)
        {
            this.Hide();
            try
            {
                using (var form = formFactory())
                {
                    form.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "오류 발생", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            this.Show();
        }
    }
}
