using System;

namespace WCell.Intercommunication.Remoting
{
    public interface IRemotableObject
    {
        TimeSpan Ping(DateTime sent);
    }
}
