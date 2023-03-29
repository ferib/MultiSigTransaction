
using MixerFront;




GroupManager groupManager = new GroupManager();

// Create a group that agrees on 10k satoshi output
decimal amount = 600;
Group testGroup = groupManager.CreateNewGroup("test", amount);

// Add 2 participants
Participant Alice = new Participant("alice");
testGroup.AddParticipant(Alice);

Participant Bob = new Participant("bob");
testGroup.AddParticipant(Bob);

// Alice configuration
Alice.UpdateMainAddress("bc1qrk259cv8m7hgcejqyw2g8772ngfmc6n32hu4gx");      // Alice send addr
Alice.UpdateReturnAddress("bc1qrk259cv8m7hgcejqyw2g8772ngfmc6n32hu4gx");    // Alice recv addr (same as main for now)
Alice.AddOutputAddress("3LFaWZ1zoxzAXkG94E5Hw5PfjnhQz2aeMR", amount);       // Alice sends 10k sats to target
bool readyA = Alice.Ready(true);
if (!readyA)
{
    Console.WriteLine("Alice failed getting ready");
    return;
}

// Bob configuration
Bob.UpdateReturnAddress("bc1qq2g23nufzygkgc78rzpsghmx0egvfr4wkf6n3u");
Bob.UpdateMainAddress("bc1qq2g23nufzygkgc78rzpsghmx0egvfr4wkf6n3u");
Bob.AddOutputAddress("3H9LV7w89mqWP4HTjGtRamvbdhRr72KXXh", amount);
bool readyB = Bob.Ready(true);
if (!readyB)
{
    Console.WriteLine("Bob failed getting ready");
    return;
}

bool canStart = testGroup.CanMultiSig();
if (!canStart)
{
    Console.WriteLine("Something wrong on group signing!");
    return;
}

// start mult-ish
testGroup.StartMiltiSig();

bool isFinished = testGroup.IsFinishedSigning();
if(!isFinished)
{
    Console.WriteLine("Failed finalising the group");
}

// The current raw (unsigned) transcation
string rawTranscationHex = testGroup.CurrentTransaction.ToHex();
Console.WriteLine(testGroup.DebugLog);

Console.WriteLine("Done!");

// #==================#
// #     Evaluate     #
// #==================#
/*

# Raw (unsigned) Transcation:

0100000002a7ab1a4cd06fed32f231936cb205caa8a323ffdb3dfe8682ab3a5e33e218a88e0100000000ffffffffa7ab1a4cd06fed32f231936cb205caa8a323ffdb3dfe8682ab3a5e33e218a88e0300000000ffffffff03580200000000000017a914cb9ac6d00534e4595960b77259678296c9094b1887580200000000000017a914a984323e2d9b262fda7694cdeddcef3ac3ce204c87b00e0000000000001600140290a8cf8911116463c71883045f667e50c48eae00000000


# Decoded Transcation:

{
  "txid": "b5e757cb0a310cce731f184388cac1cf6f82f032067bcda7e108391810e05a71",
  "hash": "b5e757cb0a310cce731f184388cac1cf6f82f032067bcda7e108391810e05a71",
  "version": 1,
  "size": 187,
  "vsize": 187,
  "weight": 748,
  "locktime": 0,
  "vin": [
    {
      "txid": "8ea818e2335e3aab8286fe3ddbff23a3a8ca05b26c9331f232ed6fd04c1aaba7",
      "vout": 1,
      "scriptSig": {
        "asm": "",
        "hex": ""
      },
      "sequence": 4294967295
    },
    {
      "txid": "8ea818e2335e3aab8286fe3ddbff23a3a8ca05b26c9331f232ed6fd04c1aaba7",
      "vout": 3,
      "scriptSig": {
        "asm": "",
        "hex": ""
      },
      "sequence": 4294967295
    }
  ],
  "vout": [
    {
      "value": 0.00000600,
      "n": 0,
      "scriptPubKey": {
        "asm": "OP_HASH160 cb9ac6d00534e4595960b77259678296c9094b18 OP_EQUAL",
        "hex": "a914cb9ac6d00534e4595960b77259678296c9094b1887",
        "address": "3LFaWZ1zoxzAXkG94E5Hw5PfjnhQz2aeMR",
        "type": "scripthash"
      }
    },
    {
      "value": 0.00000600,
      "n": 1,
      "scriptPubKey": {
        "asm": "OP_HASH160 a984323e2d9b262fda7694cdeddcef3ac3ce204c OP_EQUAL",
        "hex": "a914a984323e2d9b262fda7694cdeddcef3ac3ce204c87",
        "address": "3H9LV7w89mqWP4HTjGtRamvbdhRr72KXXh",
        "type": "scripthash"
      }
    },
    {
      "value": 0.00003760,
      "n": 2,
      "scriptPubKey": {
        "asm": "0 0290a8cf8911116463c71883045f667e50c48eae",
        "hex": "00140290a8cf8911116463c71883045f667e50c48eae",
        "address": "bc1qq2g23nufzygkgc78rzpsghmx0egvfr4wkf6n3u",
        "type": "witness_v0_keyhash"
      }
    }
  ]
}

# Signed Raw Transaction

01000000000102a7ab1a4cd06fed32f231936cb205caa8a323ffdb3dfe8682ab3a5e33e218a88e0100000000ffffffffa7ab1a4cd06fed32f231936cb205caa8a323ffdb3dfe8682ab3a5e33e218a88e0300000000ffffffff03580200000000000017a914cb9ac6d00534e4595960b77259678296c9094b1887580200000000000017a914a984323e2d9b262fda7694cdeddcef3ac3ce204c87b00e0000000000001600140290a8cf8911116463c71883045f667e50c48eae02473044022074c999c6a4f758d4c841507feaa91014066e2dc6177e1c11362573d5019d272402203646e1e781b12912a017c81fc349bf4287789a739f14f2aa4bf5ce3064442153012102fbded889a1f26d5b362977ddeb508c7a586cdaca8893b82ca4b9be67cf7da9e002473044022023ce79f494e6e78a67e3c47269da0db414381e8b8b81bf403ad74046c24c285e022023b02c0405015e6f8b2d07dfde0125b3e8eea40f15b4621f193cb38bbc52727a012103c48926b53c84327780f935c574525906e6de0c87981177d5fa7a953e0e0c248e00000000


# Decoded Signed Transcation

{
  "txid": "b5e757cb0a310cce731f184388cac1cf6f82f032067bcda7e108391810e05a71",
  "hash": "cdc747ed21f398f533b45b42adf47b9db909ea88313e2a6c6f76a2764ed93053",
  "version": 1,
  "size": 403,
  "vsize": 241,
  "weight": 964,
  "locktime": 0,
  "vin": [
    {
      "txid": "8ea818e2335e3aab8286fe3ddbff23a3a8ca05b26c9331f232ed6fd04c1aaba7",
      "vout": 1,
      "scriptSig": {
        "asm": "",
        "hex": ""
      },
      "txinwitness": [
        "3044022074c999c6a4f758d4c841507feaa91014066e2dc6177e1c11362573d5019d272402203646e1e781b12912a017c81fc349bf4287789a739f14f2aa4bf5ce306444215301",
        "02fbded889a1f26d5b362977ddeb508c7a586cdaca8893b82ca4b9be67cf7da9e0"
      ],
      "sequence": 4294967295
    },
    {
      "txid": "8ea818e2335e3aab8286fe3ddbff23a3a8ca05b26c9331f232ed6fd04c1aaba7",
      "vout": 3,
      "scriptSig": {
        "asm": "",
        "hex": ""
      },
      "txinwitness": [
        "3044022023ce79f494e6e78a67e3c47269da0db414381e8b8b81bf403ad74046c24c285e022023b02c0405015e6f8b2d07dfde0125b3e8eea40f15b4621f193cb38bbc52727a01",
        "03c48926b53c84327780f935c574525906e6de0c87981177d5fa7a953e0e0c248e"
      ],
      "sequence": 4294967295
    }
  ],
  "vout": [
    {
      "value": 0.00000600,
      "n": 0,
      "scriptPubKey": {
        "asm": "OP_HASH160 cb9ac6d00534e4595960b77259678296c9094b18 OP_EQUAL",
        "hex": "a914cb9ac6d00534e4595960b77259678296c9094b1887",
        "address": "3LFaWZ1zoxzAXkG94E5Hw5PfjnhQz2aeMR",
        "type": "scripthash"
      }
    },
    {
      "value": 0.00000600,
      "n": 1,
      "scriptPubKey": {
        "asm": "OP_HASH160 a984323e2d9b262fda7694cdeddcef3ac3ce204c OP_EQUAL",
        "hex": "a914a984323e2d9b262fda7694cdeddcef3ac3ce204c87",
        "address": "3H9LV7w89mqWP4HTjGtRamvbdhRr72KXXh",
        "type": "scripthash"
      }
    },
    {
      "value": 0.00003760,
      "n": 2,
      "scriptPubKey": {
        "asm": "0 0290a8cf8911116463c71883045f667e50c48eae",
        "hex": "00140290a8cf8911116463c71883045f667e50c48eae",
        "address": "bc1qq2g23nufzygkgc78rzpsghmx0egvfr4wkf6n3u",
        "type": "witness_v0_keyhash"
      }
    }
  ]
}

# Notes

Looking at the transaction we can see "Sending user spill 90 to self (bc1qrk259cv8m7hgcejqyw2g8772ngfmc6n32hu4gx)"
is not true as these 90 satoshis are so low in value that they are considered to be 'dust'. So instead of wasting bytes
on chain we simply do not collect them and use them as extra fee.

*/
// #============#
// #   STEP 2   #
// #============#
//
// TODO: figure out how Bob and Alice should sign the transcation
//