/*********************************************************************************
    This file is part of Open Mobile.

    Open Mobile is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    Open Mobile is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with Open Mobile.  If not, see <http://www.gnu.org/licenses/>.
 
    There is one additional restriction when using this framework regardless of modifications to it.
    The About Panel or its contents must be easily accessible by the end users.
    This is to ensure all project contributors are given due credit not only in the source code.
*********************************************************************************/
using System;
using System.Drawing;
using System.Windows.Forms;

namespace OpenMobile.Controls
{
    public partial class TransparentColorPicker : UserControl
    {
        private Color selectedColor;
        /// <summary>
        /// Occurs when a new color is selected
        /// </summary>
        /// <param name="c"></param>
        public delegate void ColorChanged(Color c);

        /// <summary>
        /// A new color has been selected
        /// </summary>
        public event ColorChanged SelectedColorChanged;

        /// <summary>
        /// Creates a new color selection control
        /// </summary>
        /// <param name="selected"></param>
        public TransparentColorPicker(Color selected)
        {
            selectedColor = selected;
            InitializeComponent();
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            foreach (string s in Enum.GetNames(typeof(KnownColor)))
            {
                if (Color.FromName(s).IsSystemColor == false)
                    if (listBox1.Items.Contains(s)==false)
                        listBox1.Items.Add(s);
            }
            listBox1.Sorted = true;
            for (int i = 255; i >=0; i--)
                domainUpDown1.Items.Add(i.ToString());
            domainUpDown2.Items.AddRange(domainUpDown1.Items.GetRange(0, 256));
            domainUpDown3.Items.AddRange(domainUpDown1.Items.GetRange(0, 256));
            luminositySlider1.ForeColor =panel1.ForeColor= selectedColor;
            setDomains(selectedColor);
            trackBar1.Value = (int)(selectedColor.A / 2.55);
            updateTransparency();
            if ((selectedColor != null)&&(selectedColor.IsKnownColor==true))
                listBox1.SelectedItem = selectedColor.Name;
        }

        private void TransparentColorPicker_Resize(object sender, EventArgs e)
        {
            tabControl1.Width = this.Width;
            tabControl1.Height = this.Height;
        }

        private void listBox1_DrawItem(object sender, DrawItemEventArgs e)
        {
            Graphics g = e.Graphics;
            Rectangle r = e.Bounds;
            if (listBox1.SelectedIndex==e.Index)
            {
                g.FillRectangle(SystemBrushes.Highlight, e.Bounds);
            }
            Rectangle shape = new Rectangle(r.Left, r.Top + (r.Height / 6), (int)(1.5 * r.Height), (int)((float)r.Height / 1.5));
            g.FillRectangle(new SolidBrush(Color.FromName(listBox1.Items[e.Index].ToString())), shape);
            g.DrawRectangle(Pens.Black, shape);
            if (listBox1.SelectedIndex==e.Index)
            {
                g.DrawString(listBox1.Items[e.Index].ToString(), new Font(e.Font.FontFamily,e.Font.Size,FontStyle.Bold), Brushes.White, new PointF(r.Left + (int)(1.5*r.Height)+1, r.Top));
            }
            else
            {
                g.DrawString(listBox1.Items[e.Index].ToString(), e.Font, Brushes.Black, new PointF(r.Left + (int)(1.5 * r.Height) + 1, r.Top));
            }
        }

        private void listBox1_MouseDown(object sender, MouseEventArgs e)
        {
            listBox1.Invalidate();
        }

        private void listBox1_Click(object sender, EventArgs e)
        {
            SelectedColorChanged(Color.FromName(listBox1.SelectedItem.ToString()));
        }

        private void listBox1_KeyDown(object sender, KeyEventArgs e)
        {
            listBox1.Invalidate();
        }

        private void customColor1_SelectedColorChanged(Color c)
        {
            selectedColor = panel1.BackColor=luminositySlider1.ForeColor = c;
            setDomains(c);
            updateTransparency();
        }

        private void luminositySlider1_SelectedColorChanged(Color c)
        {
            selectedColor= panel1.BackColor = c;
            setDomains(c);
            updateTransparency();
        }

        private void setDomains(Color c)
        {
            if (domainUpDown1.SelectedIndex != 255-c.R)
                domainUpDown1.SelectedIndex = 255-c.R;
            if (domainUpDown2.SelectedIndex != 255 - c.G)
                domainUpDown2.SelectedIndex = 255 - c.G;
            if (domainUpDown3.SelectedIndex != 255 - c.B)
                domainUpDown3.SelectedIndex = 255 - c.B;
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            selectedColor = Color.FromArgb((int)Math.Round(trackBar1.Value * 2.55), selectedColor);
            updateTransparency();
        }

        private void updateTransparency()
        {
            trackBar1.Value=(int)Math.Round(selectedColor.A / 2.55);
            textBox1.Text = trackBar1.Value.ToString();
            tabPage3.Invalidate();
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            int i = 0;
            e.Handled = true;
            if ((int)e.KeyChar != (int)Keys.Back)
            {
                if (int.TryParse(e.KeyChar.ToString(), out i) == false)
                    return;
                if (textBox1.Text.Length > 0)
                    if (((int.Parse(textBox1.Text) * 10) + i) > 100)
                        return;
                textBox1.Text += e.KeyChar;
            }
            else
            {
                if (textBox1.Text.Length > 0)
                    textBox1.Text = textBox1.Text.Remove(textBox1.Text.Length - 1);
            }
            if (textBox1.Text.Length > 0)
            {
                selectedColor = Color.FromArgb((int)(int.Parse(textBox1.Text) * 2.55), selectedColor);
                trackBar1.Value = (int)Math.Round(selectedColor.A / 2.55);
                textBox1.SelectionStart = textBox1.Text.Length;
                tabPage3.Invalidate();
            }
        }
        /// <summary>
        /// The selected color
        /// </summary>
        public override Color ForeColor
        {
            get
            {
                return selectedColor;
            }
            set
            {
               selectedColor = value;
            }
        }

        private void tabPage3_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.FillRectangle(Brushes.Black, new Rectangle(5, 5, 78, 25));
            g.FillRectangle(Brushes.White, new Rectangle(83, 5, 78, 25));
            g.DrawString("Foreground Text",new Font(FontFamily.GenericSerif,14F),new SolidBrush(selectedColor),new PointF(5,5));
            g.DrawRectangle(Pens.Black, new Rectangle(5, 5, 156, 25));

            g.DrawString("Background Text", new Font(FontFamily.GenericSerif, 14F), Brushes.Black, new PointF(5, 35));
            g.FillRectangle(new SolidBrush(selectedColor), new Rectangle(5, 35, 156, 25));
            g.DrawRectangle(Pens.Black, new Rectangle(5, 35, 156, 25));
        }

        private void domainUpDown_SelectedItemChanged(object sender, EventArgs e)
        {
            if ((domainUpDown3.SelectedIndex == -1) || (domainUpDown2.SelectedIndex == -1) || (domainUpDown1.SelectedIndex == -1))
                return; //Everythings not loaded yet
            selectedColor = panel1.BackColor = Color.FromArgb(selectedColor.A, 255 - domainUpDown1.SelectedIndex, 255 - domainUpDown2.SelectedIndex, 255 - domainUpDown3.SelectedIndex);
            luminositySlider1.ForeColor = selectedColor;
            updateTransparency();
        }

        private void domainUpDown_TextChanged(object sender, EventArgs e)
        {
            DomainUpDown up= (DomainUpDown)sender;
            int i = 0;
            if (int.TryParse(up.Text, out i) == false)
                return;
            up.SelectedIndex = 255-i;
        }
    }
}
