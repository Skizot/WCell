using System;
using WCell.Intercommunication.DataTypes;
using WCell.Intercommunication.Interfaces;
using WCell.Intercommunication.Remoting;

namespace WCell.RealmServer.IPC
{
    public sealed class RealmService : RemotableObject, IRealmService
    {
        public BufferedCommandResponse ExecuteCommand(string command)
        {
            throw new NotImplementedException();
        }
    }
}
