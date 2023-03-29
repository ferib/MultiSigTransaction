using NBitcoin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MixerFront
{
    public class Participant
    {
        public string SessionId;
        private object _lock = new object();

        private bool isReady = false;

        public string ReturnAddress = "";
        public string MainAddress = "";

        private string SignedTranscation = "";

        public Script RedeemScript;

        public List<Coin> Coins;
        public DateTime Created;

        Dictionary<string, decimal> ObfuscatedMapping =
            new Dictionary<string, decimal>(); 


        public Participant(string privateSession)
        {
            SessionId = privateSession;
            Created = DateTime.UtcNow;
        }

        public bool Ready(bool setrdy)
        {
            lock (_lock)
            {
                if (ReturnAddress == "")
                    return false;
                if (MainAddress == "")
                    return false;
                if (ObfuscatedMapping.Count <= 0)
                    return false;

                isReady = true;
                return isReady;
            }
        }

        public Dictionary<string, decimal> GetOutputs()
        {
            // is this threadsafe??
            return ObfuscatedMapping;
        }

        public bool UpdateReturnAddress(string addr)
        {
            if (isReady)
                return false;

            ReturnAddress = addr;
            // TODO: add input check?
            return true;
        }
        public bool UpdateMainAddress(string addr)
        {
            if (isReady)
                return false;

            MainAddress = addr;
            // TODO: add input check?
            return true;
        }

        public bool AddOutputAddress(string addr, decimal amount)
        {
            if (isReady)
                return false;

            // TODO: add input check?
            lock (_lock)
            {
                if (ObfuscatedMapping.ContainsKey(addr))
                    return false;

                ObfuscatedMapping.Add(addr, amount);
            }
            return true;
        }
        public bool RemoveOutputAddress(string addr)
        {
            if (isReady)
                return false;

            // TODO: add input check?
            lock (_lock)
            {
                if (!ObfuscatedMapping.ContainsKey(addr))
                    return false;

                ObfuscatedMapping.Remove(addr);
            }
            return true;
        }
        public bool UpdateOutputAddress(string addr, decimal amount)
        {
            if (isReady)
                return false;

            // TODO: add input check?
            lock (_lock)
            {
                if (!ObfuscatedMapping.ContainsKey(addr))
                    return false;

                ObfuscatedMapping[addr] = amount;
            }
            return true;
        }

        public virtual bool IsReady()
        {
            return isReady;
        }
    }
}
