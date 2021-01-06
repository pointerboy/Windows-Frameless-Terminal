using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace WindowsFramelessTerminal.Experimental
{
    public partial class WindowHighlighter : Form
    {
        private Rectangle innerRectangle;
        private Rectangle outerRectangle;
        private int BorderWidth;

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll")]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cX, int cY, uint uFlags);

        public WindowHighlighter(Color fillColor, Color borderColor)
        {
            BackColor = fillColor;
            ForeColor = borderColor;

            base.FormBorderStyle = FormBorderStyle.None;
            base.MaximizeBox = false;
            base.MinimizeBox = false;
            base.ShowInTaskbar = false;
            base.StartPosition = FormStartPosition.Manual;
            base.Text = String.Empty;
        }

        public void Highlight(Rectangle rectangle, bool desktop, int borderWidth)
        {
            BorderWidth = borderWidth;
            SetLocation(rectangle, desktop);

            SetWindowPos(this.Handle, new IntPtr(-1), 0, 0, 0, 0, 0x43); // topmost flag.
            Show();
        }

        public void SetLocation(Rectangle rectangle, bool desktop)
        {

            int width = BorderWidth;
            if (desktop)
            {
                this.TopMost = true;

                //Make the chrome window surface transparent

                //Define rectangle for chrome window border
                outerRectangle = new Rectangle(new Point(0, 0), rectangle.Size);
                //Define rectangle for chrome window content (everything inside of the window border)
                innerRectangle = new Rectangle(new Point(BorderWidth, BorderWidth), rectangle.Size - new Size(BorderWidth * 2, BorderWidth * 2));

                Region region = new Region(outerRectangle);
                //Exclude the contents of chrome window from region
                region.Exclude(innerRectangle);

                base.Location = rectangle.Location;
                base.Size = outerRectangle.Size;
                base.Region = region;
            }
            else
            {
                this.TopMost = false;

                //Make the chrome window surface transparent
                //Define rectangle for chrome window border
                outerRectangle = new Rectangle(new Point(0, 0), rectangle.Size + new Size(width * 2, width * 2));
                //Define rectangle for chrome window content (everything inside of the window border)
                innerRectangle = new Rectangle(new Point(BorderWidth, BorderWidth), rectangle.Size);
                Region region = new Region(outerRectangle);
                //Exclude the contents of chrome window from region
                region.Exclude(innerRectangle);
                base.Location = rectangle.Location - new Size(width, width);
                base.Size = outerRectangle.Size;
                base.Region = region;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Rectangle rect = new Rectangle(outerRectangle.Left, outerRectangle.Top, outerRectangle.Width, outerRectangle.Height);
            Rectangle rectangle2 = new Rectangle(innerRectangle.Left, innerRectangle.Top, innerRectangle.Width, innerRectangle.Height);
            e.Graphics.DrawRectangle(new Pen(ForeColor), rectangle2);
            e.Graphics.DrawRectangle(new Pen(ForeColor), rect);
        }

        public void Show()
        {
            SetWindowPos(base.Handle, IntPtr.Zero, 0, 0, 0, 0, 0x53);
        }

        private void Chrome_Load(object sender, EventArgs e)
        {
            // TODO: Logging 
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // WindowHighlighter
            // 
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Name = "WindowHighlighter";
            this.Load += new System.EventHandler(this.WindowHighlighter_Load);
            this.ResumeLayout(false);

        }

        private void WindowHighlighter_Load(object sender, EventArgs e)
        {

        }
    }
}