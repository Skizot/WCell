using System;
using System.Collections;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Security.Principal;

namespace WCell.Intercommunication.Remoting
{
    /// <summary>
    /// Represents a connection to a remotable service.
    /// </summary>
    /// <typeparam name="T">The type (interface or class) of the remote service.</typeparam>
    public sealed class RemotingClient<T> : IRemotingConnector<T>
        where T : class, IRemotableObject
    {
        public RemotingClient(string bind, string addr, int port, string user,
            string pass, bool secure = true)
        {
            ServiceType = typeof(T);
            BindName = bind;
            BindAddress = addr;
            BindPort = port;
            Username = user;
            Password = pass;
            Secure = secure;

            var props = new Hashtable();
            props["username"] = user;
            props["password"] = pass;
            props["retryCount"] = 0; // If the connection doesn't succeed at first, something's wrong.
            props["socketCachePolicy"] = SocketCachePolicy.Default;
            props["socketCacheTimeout"] = 600; // 10 minutes.
            props["tokenImpersonationLevel"] = TokenImpersonationLevel.Impersonation;
            props["typeFilterLevel"] = "Full"; // This is TypeFilterLevel.Full.

            Channel = new TcpClientChannel(props, new BinaryClientFormatterSinkProvider(props, null));

            ChannelServices.RegisterChannel(Channel, secure);
            RemotingConfiguration.RegisterWellKnownClientType(ServiceType, FullAddress);
        }

        /// <summary>
        /// Gets an instance of the service.
        /// </summary>
        public T Connect()
        {
            return Instance = (T)Activator.GetObject(ServiceType, FullAddress);
        }

        /// <summary>
        /// Stops and disconnects the client.
        /// </summary>
        public void Close()
        {
            ChannelServices.UnregisterChannel(Channel);
            Instance = null;
        }

        public Type ServiceType { get; private set; }

        public string BindName { get; private set; }

        public string BindAddress { get; private set; }

        public int BindPort { get; private set; }

        public bool Secure { get; private set; }

        public IChannel Channel { get; private set; }

        public T Instance { get; private set; }

        public string Username { get; private set; }

        public string Password { get; private set; }

        public string FullAddress
        {
            get { return "tcp://" + BindAddress + ":" + BindPort + "/" + BindName; }
        }
    }
}
