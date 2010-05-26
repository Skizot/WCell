using WCell.Intercommunication.DataTypes;
using WCell.Intercommunication.Remoting;

namespace WCell.Intercommunication.Interfaces
{
    /// <summary>
    /// Defines the IPC interface for the RealmServer.
    /// </summary>
    public interface IRealmService : IRemotableObject
    {
        #region Miscellaneous

        BufferedCommandResponse ExecuteCommand(string command);

        #endregion
    }
}
