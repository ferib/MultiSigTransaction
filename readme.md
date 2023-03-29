# Bitcoin Multi-Sig Transaction

Quick PoC to transact Bitcoins in a single transaction with multiple participants. Helps obscure coin flow and might even reduce transaction size _(and thus transaction costs)_.

Part of this blog post: [Bringing Obfuscation to the Bitcoin Blockchain](https://ferib.dev/blog.php?l=post/Bringing_Obfuscation_to_the_Bitcoin_Blockchain)


## Example

Alice and Bob both agreed to send exactly `0.00000600` BTC to their target of choice. The below transaction obfuscated the coin flow as you can no longer differentiate who owns which output of `0.00000600` BTC.

```json
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
```