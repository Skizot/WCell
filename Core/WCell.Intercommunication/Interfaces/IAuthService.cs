using System;
using WCell.Constants;
using WCell.Constants.Login;
using WCell.Constants.Realm;
using WCell.Intercommunication.DataTypes;
using WCell.Intercommunication.Remoting;
using WCell.Util.Commands;

namespace WCell.Intercommunication.Interfaces
{
    /// <summary>
    /// Defines the IPC service for the AuthServer.
    /// 
    /// Can be used to retrieve/set information about accounts, roles, and realms.
    /// </summary>
    public interface IAuthService : IRemotableObject
    {
        #region Retrieval

        AuthenticationInfo GetAuthenticationInfo(string accName);

        AccountInfo GetAccountInfo(string accName, byte[] requestAddr);

        FullAccountInfo GetFullAccountInfo(string accName);

        RoleGroupInfo GetRoleGroup(string name);

        RoleGroupInfo[] GetRoleGroups();

        #endregion

        #region Realms

        int RegisterOrUpdateRealmService(IRealmService service, string srvName, string addr, int port,
            int chrCnt, int capacity, RealmServerType type, RealmFlags flags, RealmCategory category,
            RealmStatus status, ClientVersion version);

        bool UnregisterRealmService(int id);

        IRealmService GetRealmById(int id);

        IRealmService GetRealmByName(string name);

        #endregion

        #region Accounts

        void SetAccountLoggedIn(int realmId, string accName);

        void SetAccountLoggedOut(string accName);

        void SetAccountsLoggedIn(int realmId, string[] accNames);

        void SetAccountsLoggedOut(string[] accNames);

        bool SetAccountEmail(long id, string email);

        bool SetAccountActive(long id, bool active, DateTime? statusUntil);

        void SetAccountsActive(long[] ids, bool active, DateTime? statusUntil);

        bool SetAccountPassword(long id, string oldPassStr, byte[] newPass);

        void SetAccountHighestLevel(long id, int level);

        bool SetAccountRole(long id, string roleName);

        #endregion

        #region Miscellaneous

        IBufferedCommandResponse ExecuteCommand(string command);

        #endregion
    }
}
