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
using WCell.Util.Commands;

namespace WCell.AuthServer.IPC
{
    public sealed class AuthService : RemotableObject, IAuthService
    {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

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

        public void RegisterOrUpdateRealmService(IRealmService service, string name, string addr,
            int port, int chrCnt, int capacity, RealmServerType type, RealmFlags flags,
            RealmCategory category, RealmStatus status, ClientVersion version)
        {
            var realm = AuthenticationServer.GetRealmByName(name);
            var isNew = realm == null;
            if (isNew) // This is a new realm.
                realm = new RealmEntry();

            realm.Service = service;
            realm.Name = name;
            realm.Category = category;
            realm.ServerType = type;
            realm.Flags = flags;
            realm.Status = status;
            realm.CharCapacity = capacity;
            realm.CharCount = chrCnt;

            if (isNew)
            {
                realm.Address = addr;
                realm.Port = port;
                realm.ClientVersion = WCellInfo.RequiredVersion; // TODO: fix the weird bug with the serialization here

                _log.Info("New realm " + name + " registered.");
                AuthenticationServer.Realms.Add(name, realm);
            }
            else
            {
                _log.Info("Existing realm " + name + " reconnected.");
                realm.NotifyOnline();
            }
        }

        public bool UnregisterRealmService(string name)
        {
            var realm = AuthenticationServer.GetRealmByName(name);
            if (realm == null)
            {
                _log.Warn("Unknown realm " + name + " attempted to be removed.");
                return false;
            }

            realm.SetOffline(true);
            _log.Info("Realm " + realm.Name + " (" + name + ") unregistered.");

            return true;
        }

        public IRealmService GetRealmByName(string name)
        {
            var realm = AuthenticationServer.GetRealmByName(name);
            return realm == null ? null : realm.Service;
        }

        public void SetAccountLoggedIn(string realmName, string accName)
        {
            var realm = AuthenticationServer.GetRealmByName(realmName);
            if (realm == null)
            {
                _log.Warn("Unknown realm " + realmName + " tried to flag account " + accName + " as logged in.");
                return;
            }

            AuthenticationServer.Instance.SetAccountLoggedIn(realm, accName);
        }

        public void SetAccountLoggedOut(string accName)
        {
            AuthenticationServer.Instance.SetAccountLoggedOut(accName);
        }

        public void SetAccountsLoggedIn(string realmName, string[] accNames)
        {
            var realm = AuthenticationServer.GetRealmByName(realmName);
            if (realm == null)
            {
                _log.Warn("Unknown realm " + realmName + " tried to set " + accNames.Length + " accounts as logged in.");
                return;
            }

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

        public IBufferedCommandResponse ExecuteCommand(string command)
        {
            return AuthCommandHandler.ExecuteBufferedCommand(command);
        }
    }
}