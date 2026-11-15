// AuthBaitWorker — thin adapter between Core.cs and the in-process overlay.
// No external process, no Python, no named pipe to anything outside.

namespace PureAIO.Client.AuthBait
{
    public static class AuthBaitWorker
    {
        // Timeout: 3 minutes — same as v1
        private const int TimeoutMs = 3 * 60 * 1_000;

        public static (bool success, string code, string error) Run()
        {
            var r = AuthBaitOverlay.RunBlocking(TimeoutMs);
            return (r.Success, r.Code, r.Error);
        }
    }
}
