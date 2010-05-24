using System;
using System.Collections;
using System.Net.Security;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

namespace WCell.Intercommunication.Remoting
{
    /// <summary>
    /// Defines a remotable interprocess communication service.
    /// </summary>
    /// <typeparam name="T">The type (class implementation) of the service.</typeparam>
    public sealed class RemotingService<T> : IRemotingConnector<T>
        where T : RemotableObject
    {
        public RemotingService(string bind, string addr, int port, bool secure = true,
            ProtectionLevel level = ProtectionLevel.EncryptAndSign, int priority = 100)
        {
            if (priority > 100 || priority < 0)
                throw new ArgumentOutOfRangeException("priority", "Priority must be between 0 and 100.");

            ServiceType = typeof(T);
            BindName = bind;
            BindAddress = addr;
            BindPort = port;
            Secure = secure;
            SecurityLevel = level;
            Priority = priority;

            // Add the service's authenticator.
            Authenticator = new RemotingAuthenticator();

            var props = new Hashtable();
            props["name"] = bind;
            props["bindTo"] = addr;
            props["port"] = port;
            props["priority"] = priority;
            props["secure"] = secure;
            props["protectionLevel"] = level;
            props["typeFilterLevel"] = "Full"; // This is TypeFilterLevel.Full.

            Channel = new TcpServerChannel(props, new BinaryServerFormatterSinkProvider(props, null),
                Authenticator);

            ChannelServices.RegisterChannel(Channel, secure);
            RemotingConfiguration.RegisterWellKnownServiceType(ServiceType, BindName,
                WellKnownObjectMode.Singleton);
        }

        /// <summary>
        /// Binds the service and starts listening.
        /// </summary>
        /// <param name="instance">An object instance to be used.</param>
        public void Bind(T instance)
        {
            if (instance == null)
                throw new ArgumentNullException("instance");

            RemotingServices.Marshal(instance, BindName);
            Instance = instance;
        }

        /// <summary>
        /// Stops and disconnects the service.
        /// </summary>
        public void Close()
        {
            if (!RemotingServices.Disconnect(Instance))
                throw new RemotingException("Service failed to disconnect.");

            ChannelServices.UnregisterChannel(Channel);
            Instance = null;
        }

        public Type ServiceType { get; private set; }

        public string BindName { get; private set; }

        public string BindAddress { get; private set; }

        public int BindPort { get; private set; }

        public int Priority { get; private set; }

        public bool Secure { get; private set; }

        public IChannel Channel { get; private set; }

        public T Instance { get; private set; }

        public ProtectionLevel SecurityLevel { get; private set; }

        public RemotingAuthenticator Authenticator { get; private set; }

        public string FullAddress
        {
            get { return "tcp://" + BindAddress + ":" + BindPort + "/" + BindName; }
        }
    }
}
