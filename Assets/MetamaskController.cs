using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
//using Nethereum.JsonRpc.UnityClient;
using System;
using Nethereum.Hex.HexTypes;
using System.Numerics;
using ERC721ContractLibrary.Contracts.ERC721PresetMinterPauserAutoId.ContractDefinition;
using Nethereum.Unity.Metamask;
using Nethereum.Unity.Contracts;
using Nethereum.Contracts;
using Nethereum.Unity.Contracts.Standards.ERC721;
using Nethereum.Contracts.Standards.ERC721;
using Nethereum.Unity.Utils.Drawing;
using Nethereum.Unity.Rpc;

//Test external contract 0x345c2fa23160c63218dfaa25d37269f26c85ca47
//0x2002050e7084f5db6ac4e81d54fbb6b35c257592 address
public class MetamaskController : MonoBehaviour
{
    private Button _btnMetamaskConnect;
    private Button _btnDeployNFTContract;
    private Button _btnViewNFTs;
    private Button _btnMintNFT;
    private Label _lblAccountSelected;
    private Label _lblError;
    private ListView _lstViewNFTs;
    private TextField _txtSmartContractAddress;

    private string _selectedAccountAddress; // = "0x12890D2cce102216644c59daE5baed380d84830c";
    private bool _isMetamaskInitialised= false;
    private BigInteger _currentChainId; //444444444500;
    private string _currentContractAddress; // = "0x32eb97b8ad202b072fd9066c03878892426320ed";


    void Start()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        _lblAccountSelected = root.Q<Label>("lbl-account-selected");
        _btnMetamaskConnect = root.Q<Button>("metamask-button");
        _txtSmartContractAddress = root.Q<TextField>("txt-smartContract-Address");
        _lstViewNFTs = root.Q<ListView>("lst-nfts");

        _btnViewNFTs = root.Q<Button>("btn-list-nfts");
        _btnMintNFT = root.Q<Button>("btn-mint-nft");
        
        _lblError = root.Q<Label>("lbl-error");

        _btnDeployNFTContract = root.Q<Button>("btn-deploy-nft"); 
        _btnMetamaskConnect.clicked += MetamaskConnectButton_Clicked;
        _btnDeployNFTContract.clicked += _btnDeployNFTContract_clicked;
        _btnMintNFT.clicked += _btnMintNFT_clicked;
        _btnViewNFTs.clicked += _btnViewNFTs_clicked;
  
        
    }

    private void _btnViewNFTs_clicked()
    {
        StartCoroutine(GetAllNFTImages());
    }

    private void _btnMintNFT_clicked()
    {
        StartCoroutine(MintNFT());
    }

    public IEnumerator MintNFT()
    {
        var contractTransactionUnityRequest = GetContractTransactionUnityRequest();

        if (contractTransactionUnityRequest != null)
        {
            var mintFunction = new MintFunction() { To = _selectedAccountAddress };
           
            yield return contractTransactionUnityRequest.SignAndSendTransaction<MintFunction>(mintFunction, _currentContractAddress);
            if(contractTransactionUnityRequest.Exception == null)
            {
                print(contractTransactionUnityRequest.Result);
            }
            else
            {
                DisplayError(contractTransactionUnityRequest.Exception.Message);
            }
            
        }
    }

    public IEnumerator GetAllNFTImages()
    {
        try
        {
            _lstViewNFTs.hierarchy.Clear();
        }
        catch { }
        var nftsOfUser = new NFTsOfUserUnityRequest(_selectedAccountAddress, GetUnityRpcRequestClientFactory());
        yield return nftsOfUser.GetAllMetadataUrls(_currentContractAddress, _selectedAccountAddress);

        if (nftsOfUser.Exception != null)
        {
            DisplayError(nftsOfUser.Exception.Message);
            yield break;
        }

        Debug.LogWarning(nftsOfUser.Result.Count);

        if (nftsOfUser.Result != null)
        {
            var metadataUnityRequest = new NftMetadataUnityRequest<NftMetadata>();
            yield return metadataUnityRequest.GetAllMetadata(nftsOfUser.Result);

            if (metadataUnityRequest.Exception != null)
            {
                DisplayError(metadataUnityRequest.Exception.Message);
                yield break;
            }
            if (metadataUnityRequest.Result != null)
            {
                foreach (var item in metadataUnityRequest.Result)
                {
                    var image = new Image();
                    _lstViewNFTs.hierarchy.Add(image);
                    Debug.LogWarning(item.Image);
                    StartCoroutine(new ImageDownloaderTextureAssigner().DownloadAndSetImageTexture(item.Image, image));
                }
            }
        }
    }
   

    private void _btnDeployNFTContract_clicked()
    {
        StartCoroutine(DeploySmartContract());
    }

    private IEnumerator DeploySmartContract()
    {
        var contractTransactionUnityRequest = GetContractTransactionUnityRequest();

        if (contractTransactionUnityRequest != null)
        {
            var erc721PresetMinter = new ERC721PresetMinterPauserAutoIdDeployment()
            {
                BaseURI = "https://my-json-server.typicode.com/juanfranblanco/samplenftdb/tokens/", //This is a simple example using a centralised server.. use ipfs etc for a proper decentralised inmutable
                Name = "NFTArt",
                Symbol = "NFA"
            };

            yield return contractTransactionUnityRequest.SignAndSendDeploymentContractTransaction<ERC721PresetMinterPauserAutoIdDeployment>(erc721PresetMinter);

            if (contractTransactionUnityRequest.Exception == null)
            {
                yield return GetDeploymentSmartContractAddressFromReceipt(contractTransactionUnityRequest.Result);
            }
            else
            {
                DisplayError(contractTransactionUnityRequest.Exception.Message);
            }

        }
    
    }


    private IEnumerator GetDeploymentSmartContractAddressFromReceipt(string transactionHash)
    {
		print(transactionHash);
		//create a poll to get the receipt when mined
		var transactionReceiptPolling = new TransactionReceiptPollingRequest(GetUnityRpcRequestClientFactory());

        //checking every 2 seconds for the receipt
        yield return transactionReceiptPolling.PollForReceipt(transactionHash, 2);

        var deploymentReceipt = transactionReceiptPolling.Result;
        _currentContractAddress = deploymentReceipt.ContractAddress;
        _txtSmartContractAddress.value = deploymentReceipt.ContractAddress;
		print(_currentContractAddress);
       
    }

    private void MetamaskConnectButton_Clicked()
    {
        _lblError.visible = false;
#if !DEBUG
        if (MetamaskInterop.IsMetamaskAvailable())
        {
            MetamaskInterop.EnableEthereum(gameObject.name, nameof(EthereumEnabled), nameof(DisplayError));
        }
        else
        {
            DisplayError("Metamask is not available, please install it");
        }
#endif

    }

    public void EthereumEnabled(string addressSelected)
    {
#if !DEBUG
        if (!_isMetamaskInitialised)
        {
            MetamaskInterop.EthereumInit(gameObject.name, nameof(NewAccountSelected), nameof(ChainChanged));
            MetamaskInterop.GetChainId(gameObject.name, nameof(ChainChanged), nameof(DisplayError));
            _isMetamaskInitialised = true;
        }
        NewAccountSelected(addressSelected);
#endif
    }

    public void ChainChanged(string chainId)
    {
        print(chainId);
        _currentChainId = new HexBigInteger(chainId).Value;
        try
        {
            //simple workaround to show suported configured chains
            print(_currentChainId.ToString());
            StartCoroutine(GetBlockNumber());
        }
        catch(Exception ex)
        {
            DisplayError(ex.Message);
        }
    }

    public void NewAccountSelected(string accountAddress)
    {
     
        _selectedAccountAddress = accountAddress;
        _lblAccountSelected.text = accountAddress;
        _lblAccountSelected.visible = true;
    }


    public void DisplayError(string errorMessage)
    {
        _lblError.text = errorMessage;
        _lblError.visible = true;

    }


    private IEnumerator GetBlockNumber()
    {
        var blockNumberRequest = new EthBlockNumberUnityRequest(GetUnityRpcRequestClientFactory());
        yield return blockNumberRequest.SendRequest();
        print(blockNumberRequest.Result.Value);
    }

    public IUnityRpcRequestClientFactory GetUnityRpcRequestClientFactory()
    {
#if !DEBUG
        if (MetamaskInterop.IsMetamaskAvailable()) 
        {
            return new MetamaskRequestRpcClientFactory(_selectedAccountAddress, null, 1000);
        }
        else
        {
            DisplayError("Metamask is not available, please install it");
            return null;
        }
#endif
        _selectedAccountAddress = "0x12890D2cce102216644c59daE5baed380d84830c";
        return new UnityWebRequestRpcClientFactory("http://localhost:8545");
    }


    public IContractTransactionUnityRequest GetContractTransactionUnityRequest()
    {
#if !DEBUG
        if (MetamaskInterop.IsMetamaskAvailable())
        {
            return new MetamaskTransactionUnityRequest(_selectedAccountAddress, GetUnityRpcRequestClientFactory());
        }
        else
        {
            DisplayError("Metamask is not available, please install it");
            return null;
        }
#endif
        _selectedAccountAddress = "0x12890D2cce102216644c59daE5baed380d84830c";
        return new TransactionSignedUnityRequest("http://localhost:8545", "0xb5b1870957d373ef0eeffecc6e4812c0fd08f554b37b233526acc331bf1544f7", 444444444500);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}




