using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using WCell.AuthServer.Localization;
using WCell.Intercommunication.Interfaces;
using WCell.Intercommunication.Remoting;
using WCell.Util.NLog;
using WCell.Util.Threading.TaskParallel;
using WCell.Util.Variables;

namespace WCell.AuthServer.IPC
{
    public static class AuthServiceHost
    {
        [Variable("IPCReconnectDelay")]
        public static int ReconnectDelay = 30;

        public static IAuthService Instance
        {
            get { return _service != null ? _service.Instance : null; }
        }

        public static bool IsOpen { get; private set; }

        private static RemotingService<AuthService> _service;

        private static readonly object _lock = new object();

        public static void StartService()
        {
            lock (_lock)
            {
                try
                {
                    _service = new RemotingService<AuthService>(AuthServerConfiguration.IPCBind,
                        AuthServerConfiguration.IPCAddress, AuthServerConfiguration.IPCPort);

                    _service.Authenticator.AuthenticatingEndPoint += AuthorizeIPAddress;
                    var instance = new AuthService();
                    _service.Bind(instance);

                    IsOpen = true;

                    var evnt = Connected;
                    if (evnt != null)
                        evnt();
                }
                catch (Exception ex)
                {
                    LogUtil.ErrorException(ex, Resources.IPCServiceFailed, ReconnectDelay);

                    // Try again in 30 seconds.
                    Task.Factory.StartNewDelayed(ReconnectDelay * 1000, StartService);
                }
            }
        }

        public static void StopService()
        {
            lock (_lock)
            {
                try
                {
                    _service.Close();
                }
                catch (Exception)
                {
                }

                IsOpen = false;

                var evnt = Disconnected;
                if (evnt != null)
                    evnt();
            }
        }

        private static bool AuthorizeIPAddress(EndPoint ep)
        {
            var ip = ((IPEndPoint)ep).Address.ToString();
            return AuthServerConfiguration.RealmIPs.Any(addr => addr.Equals(ip));
        }

        public delegate void ConnectionChangeHandler();

        public static event ConnectionChangeHandler Connected;

        public static event ConnectionChangeHandler Disconnected;
    }
}
