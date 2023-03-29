//using BitcoinLib.Responses;
//using FluidMixer.AddressUtil;
using NBitcoin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MixerFront
{
    public class GroupManager
    {

        private object _lock = new object();

        private Dictionary<string, Group> Groups = new Dictionary<string, Group>();


        public static NBitcoin.Network SelectedNetwork = NBitcoin.Network.Main;

        public GroupManager()
        {

        }

        public Group CreateNewGroup(string gid, decimal amount, NBitcoin.Network n = null)
        {
            // TODO: group manager to decide if 'amount' is OK
            // based on active usage and how much is in the vault
            //
            if (n == null)
                n = Bitcoin.Instance.Mainnet;

            Group g = new Group(gid, amount, n);
            if (!AddGroup(g))
                return null;

            return g;
        }

        public Group CreateNewGroup(decimal amount, NBitcoin.Network n = null)
        {
            Random rnd = new Random();
            string gid = $"grp_{(rnd.Next(0, int.MaxValue)).ToString("X8")}";
            return CreateNewGroup(gid, amount, n);
        }


        public Group GetGroupBySessionId(string sessionId)
        {
            lock (_lock)
            {
                //foreach(var g in )
                if (!Groups.ContainsKey(sessionId))
                    return null;
                return Groups[sessionId];
            }
            //return null;
        }

        // NOTE: only do on empty group?
        public bool AddGroup(Group g)
        {
            lock (_lock)
            {
                if (Groups.ContainsKey(g.SessionId))
                    return false;

                Groups.Add(g.SessionId, g);
            }
            return true;
        }

        public bool RemoveGroups(Group g)
        {
            lock (_lock)
            {
                if (!Groups.ContainsKey(g.SessionId))
                    return false;

                Groups.Remove(g.SessionId);
            }
            return true;
        }

    }
}
