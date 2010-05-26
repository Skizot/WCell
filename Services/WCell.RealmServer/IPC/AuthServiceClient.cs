using System;
using WCell.Intercommunication.Interfaces;
using WCell.Intercommunication.Remoting;
using WCell.RealmServer.Localization;
using WCell.Util.NLog;
using WCell.Util.Variables;

namespace WCell.RealmServer.IPC
{
    public static class AuthServiceClient
    {
        public static int RealmId { get; set; }

        public static TimeSpan LastPingTime { get; private set; }

        public static IAuthService Instance
        {
            get
            {
                if (_client == null)
                    return null;

                var instance = _client.Instance;

                try
                {
                    // There could be a race condition here, but it's not important.
                    LastPingTime = instance.Ping(DateTime.Now);
                }
                catch (Exception)
                {
                    Connect();
                }

                // In the worst case, may return null.
                return _client.Instance;
            }
        }

        public static bool IsOpen { get; private set; }

        private static RemotingClient<IAuthService> _client;

        private static readonly object _lock = new object();

        public static void Connect()
        {
            lock (_lock)
            {
                try
                {
                    _client = new RemotingClient<IAuthService>(RealmServerConfiguration.AuthIPCBind,
                        RealmServerConfiguration.AuthIPCAddress, RealmServerConfiguration.AuthIPCPort);

                    _client.Connect();

                    IsOpen = true;
                }
                catch (Exception ex)
                {
                    LogUtil.ErrorException(ex, Resources.IPCProxyFailedException);
                }
            }
        }

        public static void Disconnect()
        {
            lock (_lock)
            {
                try
                {
                    _client.Close();
                }
                catch (Exception)
                {
                }

                IsOpen = false;
            }
        }
    }
}
