using System.Net;
using System.Runtime.Remoting.Channels;
using System.Security.Principal;

namespace WCell.Intercommunication.Remoting
{
    /// <summary>
    /// Used to authorize access to services based on Windows user
    /// credentials and IP endpoint. Contains hookable events.
    /// </summary>
    public sealed class RemotingAuthenticator : IAuthorizeRemotingConnection
    {
        public delegate bool ConnectingEndPointAuthenticationHandler(EndPoint endPoint);

        public event ConnectingEndPointAuthenticationHandler AuthenticatingEndPoint;

        public bool IsConnectingEndPointAuthorized(EndPoint endPoint)
        {
            var value = true;

            var evnt = AuthenticatingEndPoint;
            if (evnt != null)
                value = evnt(endPoint);

            return value;
        }

        public bool IsConnectingIdentityAuthorized(IIdentity identity)
        {
            return true;
        }
    }
}
