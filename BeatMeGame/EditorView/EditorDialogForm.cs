using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BeatMeGame.EditorView
{
    public class EditorDialogForm : Form
    {
        public EditorDialogForm(Form parent)
        {
            Size = new Size(parent.ClientSize.Width / 4, parent.ClientSize.Height / 4);
            Location = new Point(parent.ClientSize.Width / 2 - ClientSize.Width / 2,
                parent.ClientSize.Height / 2 - ClientSize.Height / 2);
            BackColor = Color.DarkGray;
            FormBorderStyle = FormBorderStyle.None;
        }
    }
}
