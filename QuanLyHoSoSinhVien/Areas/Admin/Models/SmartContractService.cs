// SmartContractService.cs

using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using System;
using System.Linq;
using System.Threading.Tasks;
using Nethereum.Hex.HexTypes;
using Nethereum.Util;
using System.Numerics;

namespace CamTrai.Areas.Admin.Models
{
    public class SmartContractService
    {
        private readonly string contractAddress = "0xd1ebF760E18851cA9769FA8BbB02B97fC4CF1aBD"; // Địa chỉ của smart contract
        private readonly string web3Url = "https://rpc-mumbai.maticvigil.com"; // URL của nút Ethereum
        private readonly string SmartContractAbi = @"[
    {
        ""inputs"": [
            {
                ""internalType"": ""uint256"",
                ""name"": ""_mssv"",
                ""type"": ""uint256""
            },
            {
                ""internalType"": ""bytes32"",
                ""name"": ""_infoHash"",
                ""type"": ""bytes32""
            }
        ],
        ""name"": ""addStudent"",
        ""outputs"": [],
        ""stateMutability"": ""nonpayable"",
        ""type"": ""function""
    },
    {
        ""inputs"": [
            {
                ""internalType"": ""uint256"",
                ""name"": ""_mssv"",
                ""type"": ""uint256""
            }
        ],
        ""name"": ""getStudent"",
        ""outputs"": [
            {
                ""internalType"": ""uint256"",
                ""name"": """",
                ""type"": ""uint256""
            },
            {
                ""internalType"": ""bytes32"",
                ""name"": """",
                ""type"": ""bytes32""
            }
        ],
        ""stateMutability"": ""view"",
        ""type"": ""function""
    },
    {
        ""inputs"": [
            {
                ""internalType"": ""uint256"",
                ""name"": """",
                ""type"": ""uint256""
            }
        ],
        ""name"": ""students"",
        ""outputs"": [
            {
                ""internalType"": ""uint256"",
                ""name"": ""mssv"",
                ""type"": ""uint256""
            },
            {
                ""internalType"": ""bytes32"",
                ""name"": ""infoHash"",
                ""type"": ""bytes32""
            }
        ],
        ""stateMutability"": ""view"",
        ""type"": ""function""
    }
]";
        // Thay thế bằng ABI của smart contract

        private readonly HexBigInteger gas = new HexBigInteger(21000);
        private readonly BigInteger gasPrice = Web3.Convert.ToWei(20, UnitConversion.EthUnit.Gwei);


        public async Task<string> GetHashFromSmartContractAsync(string mssv)
        {
            try
            {
                var web3 = new Web3(web3Url);

                var addressToWatch = "0x8AB812f8EE5bDF5BAe7d0741Df36298094911294";
                var eventSubscription = web3.Eth.GetEvent<SinhVienAddedEventDTO>(contractAddress);
                var filterInput = eventSubscription.CreateFilterInput(addressToWatch);

                var events = await eventSubscription.GetAllChangesAsync(filterInput);
                var relevantEvent = events.FirstOrDefault(e => e.Event.Mssv == mssv);

                if (relevantEvent != null)
                {
                    return relevantEvent.Event.HashCode;
                }
                else
                {
                    Console.WriteLine("Event not found for MSSV: " + mssv);
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting hash from smart contract: {ex.Message}");
                return null;
            }
        }

        public async Task UpdateHashCodeOnSmartContractAsync(string mssv, string newHashCode)
        {
            try
            {
                var web3 = new Web3(web3Url);
                var contract = web3.Eth.GetContract(SmartContractAbi, contractAddress);

                var defaultAccount = await web3.Eth.CoinBase.SendRequestAsync();
                var fromAddress = defaultAccount ?? "0x8AB812f8EE5bDF5BAe7d0741Df36298094911294";

                var function = contract.GetFunction("updateHashCode");
                var transactionInput = function.CreateTransactionInput(fromAddress, gas, gasPrice, new HexBigInteger(0), mssv, new HexBigInteger(newHashCode));

                var transactionHash = await web3.Eth.Transactions.SendTransaction.SendRequestAsync(transactionInput);
                Console.WriteLine($"Transaction Hash: {transactionHash}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending transaction: {ex.Message}");
            }
        }

        [Function("getHashCode", "string")]
        public class GetHashCodeFunctionInputDTO : FunctionMessage
        {
            [Parameter("string", "mssv", 1)]
            public string Mssv { get; set; }
        }

        [Event("SinhVienAdded")]
        public class SinhVienAddedEventDTO : IEventDTO
        {
            [Parameter("address", "studentAddress", 1, true)]
            public string StudentAddress { get; set; }

            [Parameter("string", "mssv", 2, false)]
            public string Mssv { get; set; }

            [Parameter("string", "hashCode", 3, false)]
            public string HashCode { get; set; }
        }
    }
}
