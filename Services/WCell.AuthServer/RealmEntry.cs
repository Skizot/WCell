/*************************************************************************
 *
 *   file		: RealmStruct.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2008-06-19 00:10:11 +0800 (Thu, 19 Jun 2008) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 515 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using WCell.AuthServer.Accounts;
using WCell.AuthServer.Network;
using WCell.Constants;
using WCell.Constants.Login;
using WCell.Constants.Realm;
using WCell.Core;
using WCell.Core.Network;
using System.ServiceModel;
using WCell.Intercommunication.Interfaces;
using WCell.Util.Threading.TaskParallel;
using WCell.Util.Variables;

namespace WCell.AuthServer
{
    /// <summary>
    /// Defines one Entry in the Realm-list.
    /// </summary>
    public class RealmEntry
    {
        private readonly Logger _log = LogManager.GetCurrentClassLogger();

        [Variable("RealmMaintenanceInterval")]
        public static int MaintenanceInterval = 5;

        public RealmEntry(int id)
        {
            Id = id;

            Task.Factory.StartNewDelayed(MaintenanceInterval * 1000, Maintain);
        }

        #region Properties
        public int Id
        {
            get;
            set;
        }

        public IRealmService Service
        {
            get;
            set;
        }

        public TimeSpan LastPingDelay
        {
            get;
            set;
        }

        /// <summary>
        /// Realm icon as shown in the realm list. (aka PVP, PVE, etc) 
        /// </summary>
        public RealmServerType ServerType
        {
            get;
            internal set;
        }

        /// <summary>
        /// Realm status as shown in the realm list. (Good, Locked)
        /// </summary>
        public RealmStatus Status
        {
            get;
            internal set;
        }

        /// <summary>
        /// </summary>
        public RealmFlags Flags
        {
            get;
            internal set;
        }

        /// <summary>
        /// Realm name. 
        /// </summary>
        public string Name
        {
            get;
            internal set;
        }

        /// <summary>
        /// Realm address. ("IP:PORT")
        /// </summary>
        public string Address
        {
            get;
            internal set;
        }

        public int Port
        {
            get;
            internal set;
        }

        public string AddressString
        {
            get { return Address + ":" + Port; }
        }

		/// <summary>
		/// The supported client version of this realm
		/// </summary>
		public ClientVersion ClientVersion
		{
			get;
			internal set;
		}

        public int CharCount
        {
            get;
            internal set;
        }

        public int CharCapacity
        {
            get;
            internal set;
        }

        /// <summary>
        /// Realm population. (1.6+ for high value and 1.6- for lower) 
        /// </summary>
        public float Population
        {
            get
            {
                return (CharCount > CharCapacity * 0.75 ? 1.7f : (CharCount > CharCapacity / 3 ? 1.6f : 1.5f));
            }
        }

        /// <summary>
        /// Realm timezone.
        /// </summary>
        public RealmCategory Category
        {
            get;
            internal set;
		}
        #endregion

        public void NotifyOnline()
        {
            // We're back online, so start checking the connection again.
            Task.Factory.StartNewDelayed(MaintenanceInterval * 1000, Maintain);
        }

        void Maintain()
        {
            try
            {
                LastPingDelay = Service.Ping(DateTime.Now);
            }
            catch (Exception)
            {
                // The server isn't there.
                SetOffline(false);
                _log.Warn("Realm " + Name + "(" + Id + ") went offline (last ping delay = " +
                    LastPingDelay + ").");

                return;
            }

            // Everything's good.
            Task.Factory.StartNewDelayed(MaintenanceInterval * 1000, Maintain);
        }

        /// <summary>
        /// Removes this realm from the RealmList
        /// </summary>
        public void SetOffline(bool remove)
        {
            AuthenticationServer.Instance.ClearAccounts(Id);

			if (remove)
			{
				AuthenticationServer.Realms.Remove(Id);
			}
			else
			{
				Flags = RealmFlags.Offline;
				Status = RealmStatus.Locked;
			}
        }

        #region Send + Receive
        /// <summary>
        /// Handles an incoming realm list request.
        /// </summary>
        /// <param name="client">the client to send to</param>
        /// <param name="packet">the full packet</param>
        [ClientPacketHandler(AuthServerOpCode.REALM_LIST)]
        public static void RealmListRequest(IAuthClient client, PacketIn packet)
        {
            SendRealmList(client);
        }

        /// <summary>
        /// Sends the realm list to the client.
        /// </summary>
        /// <param name="client">the Session the incoming packet belongs to</param>
        public static void SendRealmList(IAuthClient client)
        {
			if (client.Account == null)
			{
				AuthenticationHandler.OnLoginError(client, AccountStatus.Failure);
				return;
			}
            using (var packet = new AuthPacketOut(AuthServerOpCode.REALM_LIST))
            {
            	packet.Position += 2;							// Packet length
                packet.Write(0);								// Unknown Value (0x0000)
            	//var cpos = packet.Position;
            	//packet.Position = cpos + 2;

            	packet.Write((short)client.Server.RealmCount);

            	//var count = 0;
                foreach (var realm in AuthenticationServer.Realms.Values)
                {
                	// check for client version
                	//if (realm.ClientVersion.IsSupported(client.Info.Version))

                	//++count;
                	realm.WriteRealm(client, packet);
                }

            	//packet.Write((byte)0x15);
                packet.Write((byte)0x10);
				packet.Write((byte)0x00);

				//packet.Position = cpos;
				//packet.WriteShort(count);

                packet.Position = 1;
                packet.Write((short)packet.TotalLength - 3);

                client.Send(packet);
            }
        }

        /// <summary>
        /// Writes the realm information to the specified packet.
        /// </summary>
        /// <param name="packet">the packet to write the realm info to</param>
        public void WriteRealm(IAuthClient client, AuthPacketOut packet)
        {
			var status = Status;
			var flags = Flags;
			var name = Name;
			if (!ClientVersion.IsSupported(client.Info.Version))
			{
				flags = RealmFlags.Offline;
				name += " [" + ClientVersion.BasicString + "]";
			}
            else if (Flags.HasFlag(RealmFlags.Offline) && Status == RealmStatus.Locked)
            {
            	var acc = client.Account;
                var role = acc.Role;
                if (role.IsStaff)
                {
                    status = RealmStatus.Open;
                	flags = RealmFlags.None;
                }
			}

            // TODO: Change char-count to amount of Characters of the querying account on this Realm
			packet.Write((byte)ServerType);
            packet.Write((byte)status);
            packet.Write((byte)flags);
            packet.WriteCString(name);
            packet.WriteCString(AddressString);
            packet.Write(Population);
            packet.Write(0); // amount of characters the player has on this realm
            packet.Write((byte)Category);
            packet.Write((byte)0x00); // realm separator?
        }

        #endregion

		public override string ToString()
		{
			return Name + " @ " + Address;
		}
    }
}