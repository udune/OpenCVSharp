using System.Drawing;
using System.Windows.Forms;

namespace OpenCVSharp
{
    public static class RichTextHelper
    {
        public static void SetMarkdown(RichTextBox rtb, string text)
        {
            rtb.Clear();
            rtb.SelectionFont = rtb.Font;
            rtb.SelectionColor = rtb.ForeColor;

            int i = 0;
            while (i < text.Length)
            {
                // Parse bold: **text**
                if (text.Length - i >= 4 && text.Substring(i, 2) == "**")
                {
                    int closingIdx = text.IndexOf("**", i + 2);
                    if (closingIdx != -1)
                    {
                        string boldText = text.Substring(i + 2, closingIdx - (i + 2));
                        rtb.SelectionFont = new Font(rtb.Font, FontStyle.Bold);
                        rtb.AppendText(boldText);
                        rtb.SelectionFont = new Font(rtb.Font, FontStyle.Regular);
                        i = closingIdx + 2;
                        continue;
                    }
                }

                // Highlight section headers: lines starting with ■ or ★
                if (text[i] == '■' || text[i] == '★')
                {
                    int lineEnd = text.IndexOf("\n", i);
                    if (lineEnd == -1) lineEnd = text.Length;
                    string headerText = text.Substring(i, lineEnd - i);
                    
                    rtb.SelectionFont = new Font(rtb.Font, FontStyle.Bold);
                    rtb.SelectionColor = text[i] == '■' ? Color.DarkSlateBlue : Color.Navy;
                    rtb.AppendText(headerText);
                    rtb.SelectionColor = rtb.ForeColor;
                    rtb.SelectionFont = new Font(rtb.Font, FontStyle.Regular);
                    i = lineEnd;
                    continue;
                }

                rtb.AppendText(text[i].ToString());
                i++;
            }
        }
    }
}
