using System;
using System.Drawing;
using System.Windows.Forms;

namespace QuickLauncher
{
    public class ToastNotification : Form
    {
        private Label _messageLabel;
        private Timer _fadeTimer;
        private Timer _displayTimer;
        private double _opacity = 0.0;
        private bool _fadingIn = true;

        public ToastNotification(string message, int displayDuration = 2000)
        {
            InitializeComponent();
            _messageLabel.Text = message;
            
            // Position at bottom-right of screen
            Screen screen = Screen.PrimaryScreen ?? Screen.AllScreens[0];
            int x = screen.WorkingArea.Right - this.Width - 20;
            int y = screen.WorkingArea.Bottom - this.Height - 20;
            this.Location = new Point(x, y);

            // Setup timers
            _displayTimer = new Timer();
            _displayTimer.Interval = displayDuration;
            _displayTimer.Tick += DisplayTimer_Tick;

            _fadeTimer = new Timer();
            _fadeTimer.Interval = 50;
            _fadeTimer.Tick += FadeTimer_Tick;

            this.Opacity = 0;
            _fadeTimer.Start();
        }

        private void InitializeComponent()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.Manual;
            this.Size = new Size(300, 80);
            this.BackColor = Color.FromArgb(45, 45, 48);
            this.ShowInTaskbar = false;
            this.TopMost = true;

            _messageLabel = new Label
            {
                Text = "",
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Location = new Point(15, 15),
                Size = new Size(270, 50),
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                TextAlign = ContentAlignment.MiddleLeft,
                AutoSize = false
            };
            this.Controls.Add(_messageLabel);

            // Rounded corners (simple version)
            this.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 10, 10));
        }

        [System.Runtime.InteropServices.DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(
            int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

        private void FadeTimer_Tick(object? sender, EventArgs e)
        {
            if (_fadingIn)
            {
                _opacity += 0.1;
                if (_opacity >= 0.95)
                {
                    _opacity = 0.95;
                    _fadingIn = false;
                    _fadeTimer.Stop();
                    _displayTimer.Start();
                }
            }
            else
            {
                _opacity -= 0.1;
                if (_opacity <= 0)
                {
                    _opacity = 0;
                    _fadeTimer.Stop();
                    this.Close();
                }
            }
            this.Opacity = _opacity;
        }

        private void DisplayTimer_Tick(object? sender, EventArgs e)
        {
            _displayTimer.Stop();
            _fadingIn = false;
            _fadeTimer.Start();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _fadeTimer?.Dispose();
                _displayTimer?.Dispose();
            }
            base.Dispose(disposing);
        }

        public static void Show(string message, int displayDuration = 2000)
        {
            var toast = new ToastNotification(message, displayDuration);
            toast.Show();
        }
    }
}
