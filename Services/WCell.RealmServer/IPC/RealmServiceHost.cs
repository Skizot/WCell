using System;
using System.Threading.Tasks;
using WCell.Intercommunication.Interfaces;
using WCell.Intercommunication.Remoting;
using WCell.RealmServer.Localization;
using WCell.Util.NLog;
using WCell.Util.Threading.TaskParallel;
using WCell.Util.Variables;

namespace WCell.RealmServer.IPC
{
    public static class RealmServiceHost
    {
        [Variable("IPCReconnectDelay")]
        public static int ReconnectDelay = 30;

        public static IRealmService Instance
        {
            get { return _service != null ? _service.Instance : null; }
        }

        public static bool IsOpen { get; private set; }

        private static RemotingService<RealmService> _service;

        private static readonly object _lock = new object();

        public static void StartService()
        {
            lock (_lock)
            {
                try
                {
                    _service = new RemotingService<RealmService>(RealmServerConfiguration.IPCBind,
                        RealmServerConfiguration.IPCAddress, RealmServerConfiguration.IPCPort);

                    var instance = new RealmService();
                    _service.Bind(instance);

                    IsOpen = true;
                }
                catch (Exception ex)
                {
                    LogUtil.ErrorException(ex, Resources.IPCFailed, ReconnectDelay);

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
            }
        }
    }
}
