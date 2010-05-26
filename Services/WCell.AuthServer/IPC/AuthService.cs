using System;
using System.Linq;
using NLog;
using WCell.AuthServer.Accounts;
using WCell.AuthServer.Commands;
using WCell.AuthServer.Localization;
using WCell.AuthServer.Privileges;
using WCell.Constants;
using WCell.Constants.Login;
using WCell.Constants.Realm;
using WCell.Core.Cryptography;
using WCell.Intercommunication.DataTypes;
using WCell.Intercommunication.Interfaces;
using WCell.Intercommunication.Remoting;

namespace WCell.AuthServer.IPC
{
    public sealed class AuthService : RemotableObject, IAuthService
    {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Incremental ID to be assigned to realms.
        /// </summary>
        private volatile int _lastRealmId = 1;

        private int GenerateNewRealmId()
        {
            return _lastRealmId++;
        }

        public AuthenticationInfo GetAuthenticationInfo(string accName)
        {
            var authInfo = AuthenticationServer.Instance.GetAuthenticationInfo(accName);

            if (authInfo == null)
                AuthenticationServer.Instance.Error(null, Resources.CannotRetrieveAuthenticationInfo,
                    accName);

            return authInfo;
        }

        public AccountInfo GetAccountInfo(string accName, byte[] requestAddr)
        {
            var acc = AccountMgr.GetAccount(accName);

            if (acc == null)
            {
                _log.Warn(string.Format(Resources.AttemptedRequestForUnknownAccount, accName, requestAddr));
                return null;
            }

            var info = new AccountInfo
            {
                AccountId = acc.AccountId,
                EmailAddress = acc.EmailAddress,
                ClientId = acc.ClientId,
                RoleGroupName = acc.RoleGroupName,
                LastIP = acc.LastIP,
                LastLogin = acc.LastLogin,
                Locale = acc.Locale
            };

            return info;
        }

        public FullAccountInfo GetFullAccountInfo(string accName)
        {
            var acc = AccountMgr.GetAccount(accName);

            if (acc == null)
            {
                _log.Error(string.Format(Resources.AttemptedRequestForUnknownAccount, accName));
                return null;
            }

            var info = new FullAccountInfo
            {
                Name = acc.Name,
                IsActive = acc.IsActive,
                StatusUntil = acc.StatusUntil,
                AccountId = acc.AccountId,
                EmailAddress = acc.EmailAddress,
                ClientId = acc.ClientId,
                RoleGroupName = acc.RoleGroupName,
                LastIP = acc.LastIP,
                LastLogin = acc.LastLogin,
                Locale = acc.Locale
            };

            return info;
        }

        public RoleGroupInfo GetRoleGroup(string name)
        {
            return PrivilegeMgr.Instance.GetRoleGroup(name);
        }

        public RoleGroupInfo[] GetRoleGroups()
        {
            return PrivilegeMgr.Instance.RoleGroups.Values.ToArray();
        }

        public int RegisterOrUpdateRealmService(IRealmService service, string srvName, string addr,
            int port, int chrCnt, int capacity, RealmServerType type, RealmFlags flags,
            RealmCategory category, RealmStatus status, ClientVersion version)
        {
            var realm = AuthenticationServer.GetRealmByName(srvName);
            var isNew = realm == null;
            int id;
            if (isNew) // This is a new realm.
            {
                id = GenerateNewRealmId();
                realm = new RealmEntry(id);
            }
            else // Just an existing realm re-connecting.
                id = realm.Id;

            realm.Service = service;
            realm.Address = addr;
            realm.Port = port;
            realm.Name = srvName;
            realm.Category = category;
            realm.ServerType = type;
            realm.Flags = flags;
            realm.Status = status;
            realm.CharCapacity = capacity;
            realm.CharCount = chrCnt;
            realm.ClientVersion = version;

            if (isNew)
                AuthenticationServer.Realms.Add(realm.Id, realm);

            _log.Info(Resources.RealmRegistered, realm);
            return id;
        }

        public bool UnregisterRealmService(int id)
        {
            var realm = AuthenticationServer.GetRealmById(id);
            if (realm == null)
                return false;

            realm.SetOffline(true);
            _log.Info(Resources.RealmUnregistered, realm);

            return true;
        }

        public void SetAccountLoggedIn(int realmId, string accName)
        {
            var realm = AuthenticationServer.GetRealmById(realmId);
            if (realm == null)
                return;

            AuthenticationServer.Instance.SetAccountLoggedIn(realm, accName);
        }

        public void SetAccountLoggedOut(string accName)
        {
            AuthenticationServer.Instance.SetAccountLoggedOut(accName);
        }

        public void SetAccountsLoggedIn(int realmId, string[] accNames)
        {
            var realm = AuthenticationServer.GetRealmById(realmId);
            if (realm == null)
                return;

            foreach (var acc in accNames)
                AuthenticationServer.Instance.SetAccountLoggedIn(realm, acc);
        }

        public void SetAccountsLoggedOut(string[] accNames)
        {
            foreach (var acc in accNames)
                AuthenticationServer.Instance.SetAccountLoggedOut(acc);
        }

        public bool SetAccountEmail(long id, string email)
        {
            var acc = AccountMgr.GetAccount(id);
            if (acc != null)
            {
                acc.EmailAddress = email;

                AuthenticationServer.Instance.AddMessage(acc.SaveAndFlush);
                return true;
            }

            return false;
        }

        public bool SetAccountActive(long id, bool active, DateTime? statusUntil)
        {
            var acc = AccountMgr.GetAccount(id);
            if (acc == null)
                return false;

            if (acc.IsActive != active || acc.StatusUntil != statusUntil)
            {
                acc.IsActive = active;
                acc.StatusUntil = statusUntil;

                AuthenticationServer.Instance.AddMessage(acc.SaveAndFlush);
                return true;
            }

            return false;
        }

        public void SetAccountsActive(long[] ids, bool active, DateTime? statusUntil)
        {
            foreach (var id in ids)
                SetAccountActive(id, active, statusUntil);
        }

        public bool SetAccountPassword(long id, string oldPassStr, byte[] newPass)
        {
            var acc = AccountMgr.GetAccount(id);
            if (acc != null)
            {
                if (oldPassStr != null)
                {
                    var oldPass = SecureRemotePassword.GenerateCredentialsHash(acc.Name, oldPassStr);
                    if (!oldPass.SequenceEqual(acc.Password))
                        return false;
                }

                acc.Password = newPass;

                AuthenticationServer.Instance.AddMessage(acc.SaveAndFlush);
                return true;
            }

            return false;
        }

        public void SetAccountHighestLevel(long id, int level)
        {
            var acc = AccountMgr.GetAccount(id);
            if (acc == null)
                return;

            acc.HighestCharLevel = level;
            AuthenticationServer.Instance.AddMessage(acc.SaveAndFlush);
        }

        public bool SetAccountRole(long id, string roleName)
        {
            var acc = AccountMgr.GetAccount(id);
            if (acc != null)
            {
                acc.RoleGroupName = roleName;

                AuthenticationServer.Instance.AddMessage(acc.SaveAndFlush);
                return true;
            }

            return false;
        }

        public BufferedCommandResponse ExecuteCommand(string command)
        {
            return AuthCommandHandler.ExecuteBufferedCommand(command);
        }
    }
}