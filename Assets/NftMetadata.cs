//using Nethereum.JsonRpc.UnityClient;
using Nethereum.JsonRpc.UnityClient;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class NftMetadata
{
    [JsonProperty("name")]
    public string Name { get; set; }
    [JsonProperty("description")]
    public string Description { get; set; }
    [JsonProperty("external_url")]
    public string ExternalUrl { get; set; }
    [JsonProperty("image")]
    public string Image { get; set; }
}


public class IpfsUrlService
{
    public static string GetIpfsUrl(string url)
    {
        if (url.StartsWith("ipfs:"))
        {
            url = url.Replace("ipfs://", "ipfs.infura.io/ipfs/");
        }
        return url;
    }
}

public class NftMetadataUnityRequest<TNFTMetadata> : UnityRequest<List<TNFTMetadata>> where TNFTMetadata : NftMetadata
{
    public IEnumerator GetAllMetadata(List<string> metadataUrls)
    {
        var returnData = new List<TNFTMetadata>();

        foreach (var metadataUrl in metadataUrls)
        {
            var metadataLocation = IpfsUrlService.GetIpfsUrl(metadataUrl);
            
        
            using (UnityWebRequest webRequest = UnityWebRequest.Get(metadataLocation))
            {
                yield return webRequest.SendWebRequest();


                switch (webRequest.result)
                {
                    case UnityWebRequest.Result.ConnectionError:
                    case UnityWebRequest.Result.DataProcessingError:
                        Exception =  new Exception(webRequest.error);
                        yield break;
                       
                    case UnityWebRequest.Result.ProtocolError:
                        Exception = new Exception("Http Error: " + webRequest.error);
                        yield break;
                       
                    case UnityWebRequest.Result.Success:
                        try
                        {
                            returnData.Add(JsonConvert.DeserializeObject<TNFTMetadata>(webRequest.downloadHandler.text));
                        }catch (Exception e)
                        {
                            Exception = e;
                            yield break;
                        }
                        break;
                }
            }
            Result = returnData;
        }
            
    }
}




