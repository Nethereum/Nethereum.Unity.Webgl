using Nethereum.Contracts;
using Nethereum.Hex.HexTypes;
using Nethereum.JsonRpc.Client.RpcMessages;
using Nethereum.JsonRpc.UnityClient;
using Nethereum.Metamask;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Signer;
using Nethereum.Unity.RpcModel;
using Nethereum.Util;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

public class MetamaskRpcRequestMessage : RpcRequestMessage
    {
        public MetamaskRpcRequestMessage(object id, string method, string from, params object[] parameterList) : base(id, method,
            parameterList)
        {
            From = from;
        }

        [JsonProperty("from")]
        public string From { get; private set; }
    }

    public class MetamaskTransactionUnityRequest : UnityRequest<string>
    {
        private readonly EthEstimateGasUnityRequest _ethEstimateGasUnityRequest;
        private string _url;
        private readonly string _account;
        public bool EstimateGas { get; set; } = true;
        

        public MetamaskTransactionUnityRequest(string url, string account, BigInteger? chainId = null, Dictionary<string, string> requestHeaders = null)
        {
            _url = url;
            _ethEstimateGasUnityRequest = new EthEstimateGasUnityRequest(_url);
            _ethEstimateGasUnityRequest.RequestHeaders = requestHeaders;
            _account = account;
        }

        public IEnumerator SendTransaction<TContractFunction>(TContractFunction function, string contractAdress, string gameObject, string callback, string fallback) where TContractFunction : FunctionMessage
        {
            var transactionInput = function.CreateTransactionInput(contractAdress);
            yield return SendTransaction(transactionInput, gameObject, callback, fallback);
        }

        public IEnumerator SendDeploymentContractTransaction<TDeploymentMessage>(TDeploymentMessage deploymentMessage, string gameObject, string callback, string fallback)
            where TDeploymentMessage : ContractDeploymentMessage
        {
            var transactionInput = deploymentMessage.CreateTransactionInput();
            yield return SendTransaction(transactionInput, gameObject, callback, fallback);
        }

        public IEnumerator SendDeploymentContractTransaction<TDeploymentMessage>(string gameObject, string callback, string fallback)
            where TDeploymentMessage : ContractDeploymentMessage, new()
        {
            var deploymentMessage = new TDeploymentMessage();
            yield return SendDeploymentContractTransaction(deploymentMessage, gameObject, callback, fallback);
        }

        public static string DeserialiseTxnHashFromResponse(string rpcResponse)
        {
            var responseObject = JsonConvert.DeserializeObject<RpcResponse>(rpcResponse);
            var txnHash = responseObject.GetResult<string>(true);
            return txnHash;
        }

        public IEnumerator SendTransaction(TransactionInput transactionInput, string gameObject, string callback, string fallback)
        {
            if (transactionInput == null) throw new ArgumentNullException("transactionInput");

            if (string.IsNullOrEmpty(transactionInput.From)) transactionInput.From = _account;

            if (!transactionInput.From.IsTheSameAddress(_account))
            {
                throw new Exception("Transaction Input From address does not match account");
            }

            if (transactionInput.Gas == null)
            {
                if (EstimateGas)
                {
                    yield return _ethEstimateGasUnityRequest.SendRequest(transactionInput);

                    if (_ethEstimateGasUnityRequest.Exception == null)
                    {
                        var gas = _ethEstimateGasUnityRequest.Result;
                        transactionInput.Gas = gas;
                    }
                    else
                    {
                        this.Exception = _ethEstimateGasUnityRequest.Exception;
                        yield break;
                    }
                }
            }
            var transactionSendRequest = new Nethereum.RPC.Eth.Transactions.EthSendTransaction(null);
            var request = transactionSendRequest.BuildRequest(1, transactionInput);

            var metamaskRpcRequest = new MetamaskRpcRequestMessage(request.Id, request.Method, _account,
                request.RawParameters);

            MetamaskInterop.Request(JsonConvert.SerializeObject(metamaskRpcRequest), gameObject, callback, fallback);
        }
    }

