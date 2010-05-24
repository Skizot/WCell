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
        public delegate bool ConnectingIdentityAuthenticationHandler(IIdentity identity);

        public event ConnectingEndPointAuthenticationHandler AuthenticatingEndPoint;
        public event ConnectingIdentityAuthenticationHandler AuthenticatingIdentity;

        public bool IsConnectingEndPointAuthorized(EndPoint endPoint)
        {
            // Default to true, since they still need to go through
            // the identity phase.
            var value = true;

            var evnt = AuthenticatingEndPoint;
            if (evnt != null)
                value = evnt(endPoint);

            return value;
        }

        public bool IsConnectingIdentityAuthorized(IIdentity identity)
        {
            // Default to true, because usually you wouldn't even
            // make it through to here with an invalid identity.
            var value = true;

            var evnt = AuthenticatingIdentity;
            if (evnt != null)
                value = evnt(identity);

            return value;
        }
    }
}
