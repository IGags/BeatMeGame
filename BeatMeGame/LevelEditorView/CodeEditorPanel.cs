using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BeatMeGame.EditorView
{
    public class CodeEditorPanel : Panel
    {
        public TextBox CodeEditor { get; }

        public CodeEditorPanel(int textSize)
        {
            CodeEditor = new TextBox()
            {
                ForeColor = Color.Green,
                BackColor = Color.Black,
                BorderStyle = BorderStyle.None,
                AutoCompleteMode = AutoCompleteMode.Suggest,
                ScrollBars = ScrollBars.Both,
                Multiline = true,
                Font = new Font(FontFamily.GenericSansSerif, textSize, FontStyle.Regular),
                WordWrap = false
            };

            SizeChanged += (sender, args) =>
            {
                CodeEditor.Size = ClientSize;
            };

            Controls.Add(CodeEditor);
        }
    }
}
