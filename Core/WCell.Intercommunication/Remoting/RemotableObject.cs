using System;

namespace WCell.Intercommunication.Remoting
{
    /// <summary>
    /// The base class for all remotable services.
    /// </summary>
    public abstract class RemotableObject : MarshalByRefObject, IRemotableObject
    {
        /// <summary>
        /// Pings the service. Returns the time it took to send the request.
        /// </summary>
        /// <param name="sent">The time at which the ping request was sent.</param>
        public TimeSpan Ping(DateTime sent)
        {
            // This is simply to let a message call travel over the wire. Do not
            // mark this method with a OneWayAttribute; that defeats the purpose!
            return DateTime.Now - sent;
        }

        public override object InitializeLifetimeService()
        {
            // No lifetime; exists forever.
            return null;
        }
    }
}
