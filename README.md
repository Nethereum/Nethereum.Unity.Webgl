# Nethereum.Unity.Webgl
Nethereum Unity Webgl sample starter 

Example on how to integrate a WebGl unity project with Metamask, Deploy an ERC721 smartcontract (NFT) and interact with the smart contract minting a token and retrieving the asset (image) associated with it.



![MMUnity2021 gif](https://user-images.githubusercontent.com/562371/148795418-d09438d0-5857-4dfc-92af-3a3b025f8c22.gif)


## Notes
+ Entry point is the Metamask Controller: https://github.com/Nethereum/Nethereum.Unity.Webgl/blob/main/Assets/MetamaskController.cs
+ Only supported in 2021 and above due to emscripten
+ If creating a custom index.html file, the script needs to instantiate ```nethereumUnityInstance``` as per the example here:
https://github.com/Nethereum/Nethereum.Unity.Webgl/blob/main/WebGl/index.html#L111
+ This example uses coroutines only, if you want to use Web3 / Tasks check the main template https://github.com/Nethereum/Unity3dSampleTemplate, that uses the WebGLThreadingPatcher https://github.com/VolodymyrBS/WebGLThreadingPatcher, or any other way to enable wasm with Task threading support. 


## Future thoughts, next steps:
+ Include in the sample WalletConnect, probably using a submodule to provide the integration guidelines.
+ Same example using Web3 / Tasks


