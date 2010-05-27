using WCell.Intercommunication.DataTypes;
using WCell.Intercommunication.Remoting;
using WCell.Util.Commands;

namespace WCell.Intercommunication.Interfaces
{
    /// <summary>
    /// Defines the IPC interface for the RealmServer.
    /// </summary>
    public interface IRealmService : IRemotableObject
    {
        #region Statistics

        int GetCharacterCount();

        int GetAllianceCharacterCount();

        int GetHordeCharacterCount();

        int GetStaffMemberCount();

        int GetRegionCount();

        int GetInstancedRegionCount();

        #endregion

        #region World

        void TogglePause(bool value);

        void Save();

        void Broadcast(string message, params object[] args);

        #endregion

        #region Miscellaneous

        IBufferedCommandResponse ExecuteCommand(string command);

        #endregion
    }
}
