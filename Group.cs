using Info.Blockchain.API.BlockExplorer;
using Info.Blockchain.API.Models;
using NBitcoin;
using NBitcoin.RPC;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static NBitcoin.Scripting.OutputDescriptor;

namespace MixerFront
{
    public class Group
    {
        public string SessionId;
        private object _lock = new object();

        private Dictionary<string, Participant> Particiapnts = new Dictionary<string, Participant>();

        public int maxSize = 32;

        public static Network SelectedNetwork = Network.Main;

        // TODO: make the output type Money?
        public decimal GroupOutput; // output requested by user

        // hardcoded shit for now?
        public int avrgSizeOutput = 65;
        public int satsPerByte = 5;

        private bool isSigning = false;
        private Dictionary<string, TransactionSignature> SigningProgress = new Dictionary<string, TransactionSignature>();
        private string ExpectedNextSigner = "";

        // TODO: move to options/config
        //private Network SelectedNetwork = Network.Main;
        public TransactionBuilder txb;

        // to complete request the server will poll from SignerIndex to nr of
        // participants for signing requests. Once fulfilled it will continue
        // to the next until target is reached (or timeout is hit?)
        //
        private string UnsignedMessage = "";
        public string SignedMessage = "";
        private int SignerIndex = 0;
        public NBitcoin.Transaction CurrentTransaction = null;

        public string DebugLog = string.Empty;

        public Group(string sessionId, Network net = null)
        {
            if (net == null)
                net = Bitcoin.Instance.Mainnet;

            SelectedNetwork = net;
            SessionId = sessionId;
        }
        public Group(string sessionId, decimal amount, Network net)
        {
            SelectedNetwork = net;
            GroupOutput = amount;
            SessionId = sessionId;
        }

        public string GetSignedMessage()
        {
            if (CurrentTransaction == null)
                return null;

            if (IsFinishedSigning())
            {
                if(SignedMessage == "")
                    SignedMessage = CurrentTransaction.ToHex();

                return SignedMessage;
            }
                
            return null;
        }

        public bool AddParticipant(Participant p)
        {
            lock(_lock)
            {
                if (Particiapnts.ContainsKey(p.SessionId))
                    return false;

                Particiapnts.Add(p.SessionId, p);
            }
            return true;
        }

        public bool RemoveParticipant(Participant p)
        {
            lock (_lock)
            {
                if (!Particiapnts.ContainsKey(p.SessionId))
                    return false;

                Particiapnts.Remove(p.SessionId);
            }
            return true;
        }

        public bool IsSigning()
        {
            return isSigning;
        }
        public bool CanMultiSig()
        {
            // check if already is signing
            if (isSigning)
                return false;

            // check if group members are ready
            lock (_lock)
            {
                var ps = GetParticipants();
                if (ps.Length == 0)
                    return false;

                foreach (var p in ps)
                {
                    if (!p.IsReady())
                        return false;
                }
            }
            return true;
        }

        public bool StartMiltiSig()
        {
            lock (_lock)
            {
                if(!CanMultiSig())
                    return false;

                isSigning = true; 
            }

            // Start signing
            txb = SelectedNetwork.CreateTransactionBuilder();
            txb.ShuffleInputs = false;
            txb.ShuffleOutputs = false;

            // add fixed fee for each output?
            //Money txTotalFee = new Money(0);
            decimal txTotalFee = 0;

            // Random change address, in the unlikely event we DO have change? (should not happen)
            var changeaddr = SelectedNetwork.CreateBitcoinAddress("1FeribRHR98Crux3DEZPXzjLBpfmHTHKqJ");
            txb.SetChange(changeaddr);

            // get unspent coins
            var participants = GetParticipants();
            var coins = new List<Coin>[participants.Length];
            for (int i = 0; i < participants.Length; i++)
            {
                var p = participants[i];             
                coins[i] = GetUnspentCoins(p.MainAddress);
                
                // only get one?
                participants[i].Coins = coins[i];

                // INVALID BALANCE!
                if (coins[i].Count == 0)
                {
                    DebugLog += $"No balance found for {participants[i].MainAddress}!\n";
                    return false;
                }

                participants[i].RedeemScript = coins[i].FirstOrDefault().ScriptPubKey;

                txb.AddCoins(coins[i]);
            }

            // spend outputs
            DebugLog = string.Empty;
            for (int i = 0; i < participants.Length; i++)
            {
                var p = participants[i];
                var input = coins[i];
                var output = p.GetOutputs();
                var currAddr = participants[i].MainAddress;

                // add output
                decimal total = input.Sum(x => x.Amount);
                decimal totalSats = 0;
                foreach (var o in output)
                {
                    if (total - o.Value < 0)
                    {
                        // underflow!
                        DebugLog += $"[!] FAILED Sending {o.Value} to {o.Key} (insufficient balance for {currAddr}!)\n";
                        return false; // abort!?
                        continue;
                    }

                    totalSats += o.Value;
                    total -= o.Value;

                    DebugLog += $"[+] Sending {o.Value} to {o.Key}\n";
                    txb.Send(BitcoinAddress.Create(o.Key, SelectedNetwork), Money.FromUnit(o.Value, MoneyUnit.Satoshi).Satoshi);
                }

                // TODO: check if below zero or zero!

                // return everything that is left to original wallet
                decimal userSpill = input.Sum(x => x.Amount) - totalSats;
                
                // NOTE: always user spill!?
                //if (userSpill > 0)
                {

                    {
                        decimal fee = avrgSizeOutput * satsPerByte * (1 + output.Count);
                        txTotalFee += fee;
                        userSpill -= fee;
                    }
                    if (userSpill < 0)
                    {
                        DebugLog += $"[!] FAILED missing {Math.Abs(userSpill)} sats in {currAddr}!\n";
                        return false;
                    }
                    DebugLog += $"[+] Sending user spill {userSpill} to self ({currAddr})\n";
                    txb.Send(BitcoinAddress.Create(currAddr, SelectedNetwork), Money.FromUnit(userSpill, MoneyUnit.Satoshi).Satoshi);
                }
            }

            //Console.WriteLine(DebugLog);

            // Next, have users sign?
            txb = txb.SendFees(Money.FromUnit(txTotalFee, MoneyUnit.Satoshi));
            var txPrototype = txb.BuildTransaction(false); // no sign
            //int size = txb.EstimateSize(txPrototype);
            ///Money txFee = Money.FromUnit(size * 3, MoneyUnit.Satoshi);
            // TODO: subtract fees?
            CurrentTransaction = txb.BuildTransaction(false);

            // unsigned raw transcation
            UnsignedMessage = CurrentTransaction.ToHex();

            return true;
        }
        public string GetFinalUserMessage()
        {
            return "-------------------------------------------------\n" + 
                   DebugLog +
                   "-------------------------------------------------\n" +
                   GetSignedMessage() +
                 "\n-------------------------------------------------\n";

        }

        // NOTE: this is unused, but we need soon-ish?
        private bool UpdateNextSigner(bool initialize = false)
        {
            // NOTE: need to check if bot is stuck?
            var previousSigner = ExpectedNextSigner;

            if (initialize)
            {
                Participant p = GetParticipants()[0];

                // check if bot and skip
                ExpectedNextSigner = p.SessionId;
            }
            else
            {
                var list = Particiapnts.Keys.ToList();
                int nextIndex = list.IndexOf(previousSigner) + 1;
                if (Particiapnts.Count() < nextIndex)
                    return false; // err?

                ExpectedNextSigner = list[nextIndex];
            }
            return true;
            
        }
        public string GetUnsignedMessageForParticipant(Participant p)
        {
            // TODO: filter!
            return UnsignedMessage;
        }

        public Participant[] GetParticipants()
        {
            lock(_lock)
            {
                Participant[] result = Particiapnts.Values.ToArray();
                return result;
            }
        }

        public Participant GetParticipant(string sessionUser)
        {
            lock (_lock)
            {
                if (!Particiapnts.ContainsKey(sessionUser))
                    return null;
                return Particiapnts[sessionUser];
            }
        }
    
        public bool IsFinishedSigning()
        {
            lock (_lock)
            {
                // No signing needed, just make sure transcation is set
                return CurrentTransaction != null;

                //return SigningProgress.Keys.Count == Particiapnts.Count;
            }
        }

        public bool SignTransaction(Participant p, NBitcoin.Transaction transaction)
        {
            // TODO: add signing!
            return false;
        }

        //
        // helpers
        //
        public static string ReverseGroupedHex(string hex)
        {
            if (hex.Length % 2 != 0)
            {
                hex = " " + hex;
            }
            int i = 0;
            return string.Join(null, string.Join(null, hex.Select(x => i++ % 2 == 1 ? x.ToString() + " " : x.ToString())).Split(' ', StringSplitOptions.RemoveEmptyEntries).Reverse());
        }
        public static byte[] HexToBytes(string hex)
        {
            if (hex.Length % 2 != 0)
            {
                hex = " " + hex;
            }
            int i = 0;
            return string.Join(null, hex.Select(x => i++ % 2 == 1 ? x.ToString() + " " : x.ToString()))
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => byte.Parse(x, System.Globalization.NumberStyles.HexNumber))
                    .ToArray();
        }
        public static List<Coin> GetUnspentCoins(string addr)
        {
            // TODO: Replace BlockchainInfo with RPC Client!
            List<Coin> coins = new List<Coin>();

            // TODO: convert between addr sizes to base58?
            // create intputs based on output
            BlockExplorer Exp = new BlockExplorer();
            var data = Exp.GetUnspentOutputsAsync(new List<string>() { addr }).Result;

            var allUtxos = new List<UnspentOutput>();
            allUtxos.AddRange(data);

            foreach (var o in allUtxos)
            {
                string inTxHash = ReverseGroupedHex(o.TransactionHash);
                var scriptPubKey = new Script(HexToBytes(o.Script));
                var c = new Coin(uint256.Parse(inTxHash), (uint)o.N, Money.FromUnit(o.Value.Satoshis, MoneyUnit.Satoshi), scriptPubKey);
                coins.Add(c);
            }
            
            return coins;
        }
    }
}
