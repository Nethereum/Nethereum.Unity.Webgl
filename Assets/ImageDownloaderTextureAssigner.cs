using Nethereum.Unity.Rpc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Networking;
using UnityEngine;
using UnityEngine.UIElements;

namespace Nethereum.Unity.Utils.Drawing
{
    public class ImageDownloaderTextureAssigner : UnityRequest<bool>
    {
        public IEnumerator DownloadAndSetImageTexture(string url, Image image)
        {
            url = IpfsUrlService.ResolveIpfsUrlGateway(url);
            using (UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(url))
            {
                DownloadHandler handle = webRequest.downloadHandler;
                yield return webRequest.SendWebRequest();


                switch (webRequest.result)
                {
                    case UnityWebRequest.Result.ConnectionError:
                    case UnityWebRequest.Result.DataProcessingError:
                        Exception = new Exception(webRequest.error);
                        yield break;

                    case UnityWebRequest.Result.ProtocolError:
                        Exception = new Exception("Http Error: " + webRequest.error);
                        yield break;

                    case UnityWebRequest.Result.Success:
                        try
                        {
                            Texture2D texture2d = DownloadHandlerTexture.GetContent(webRequest);

                            Sprite sprite = null;
                            sprite = Sprite.Create(texture2d, new Rect(0, 0, texture2d.width, texture2d.height), UnityEngine.Vector2.zero);

                            if (sprite != null)
                            {
                                image.sprite = sprite;
                            }
                        }
                        catch (Exception e)
                        {
                            Exception = e;
                            yield break;
                        }
                        break;
                }
            }
        }
    }
}