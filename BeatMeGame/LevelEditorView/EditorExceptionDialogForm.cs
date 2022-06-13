using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BeatMeGame.LevelEditorView
{
    public class EditorExceptionDialogForm : Form
    {
        public EditorExceptionDialogForm(string exceptionText)
        {
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Size = new Size(200, 130);
            var label = new Label()
            {
                Text = exceptionText,
                Size = new Size(200, 70),
                TextAlign = ContentAlignment.MiddleCenter
            };

            var button = new Button()
            {
                Text = "OK",
                Location = new Point(75, label.Bottom),
                Size = new Size(50, 20)
            };

            button.Click += (sender, args) =>
            {
                Close();
            };

            Controls.Add(label);
            Controls.Add(button);
        }
    }
}
