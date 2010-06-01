using System;
using WCell.Intercommunication.Interfaces;
using WCell.Intercommunication.Remoting;
using WCell.RealmServer.Localization;
using WCell.Util.NLog;

namespace WCell.RealmServer.IPC
{
    public static class AuthServiceClient
    {
        public static IAuthService Instance
        {
            get { return _client == null ? null : _client.Instance; }
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
