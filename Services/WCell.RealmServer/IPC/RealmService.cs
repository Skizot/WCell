using System;
using System.Linq;
using NLog;
using WCell.Intercommunication.DataTypes;
using WCell.Intercommunication.Interfaces;
using WCell.Intercommunication.Remoting;
using WCell.RealmServer.Global;
using WCell.Util.Commands;

namespace WCell.RealmServer.IPC
{
    public sealed class RealmService : RemotableObject, IRealmService
    {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        public int GetCharacterCount()
        {
            return World.CharacterCount;
        }

        public int GetAllianceCharacterCount()
        {
            return World.AllianceCharCount;
        }

        public int GetHordeCharacterCount()
        {
            return World.HordeCharCount;
        }

        public int GetStaffMemberCount()
        {
            return World.StaffMemberCount;
        }

        public int GetRegionCount()
        {
            return World.RegionCount;
        }

        public int GetInstancedRegionCount()
        {
            var instances = World.GetAllInstances();
            return instances.Length + instances.Sum(inner => inner.Length);
        }

        public void TogglePause(bool value)
        {
            _log.Info("AuthServer set world pause to " + value + ".");

            World.Paused = value;
        }

        public void Save()
        {
            _log.Info("AuthServer requested a world save.");

            World.Save();
        }

        public void Broadcast(string message, params object[] args)
        {
            World.Broadcast(message, args);
        }

        public IBufferedCommandResponse ExecuteCommand(string command)
        {
            throw new NotImplementedException();
        }
    }
}
