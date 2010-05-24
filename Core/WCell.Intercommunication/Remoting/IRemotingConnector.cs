using System;
using System.Runtime.Remoting.Channels;

namespace WCell.Intercommunication.Remoting
{
    /// <summary>
    /// Represents the interface for classes that bridge a
    /// remotable service connection.
    /// </summary>
    /// <typeparam name="T">The service type.</typeparam>
    public interface IRemotingConnector<out T>
        where T : IRemotableObject
    {
        Type ServiceType { get; }

        string BindAddress { get; }

        string BindName { get; }

        int BindPort { get; }

        string FullAddress { get; }

        bool Secure { get; }

        IChannel Channel { get; }

        T Instance { get; }
    }
}
