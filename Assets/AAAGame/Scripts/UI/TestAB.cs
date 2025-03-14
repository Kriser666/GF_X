using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class TestAB : MonoBehaviour
{
    IEnumerator Start()
    {
        string url = "http://127.0.0.1:8081/StreamingAssets/ScriptableAssets.dat";
        UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle(url);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"加载失败: {request.error} \nURL: {url}");
        }
        else
        {
            AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(request);
            Debug.Log("AssetBundle加载成功！");
            bundle.Unload(false);
        }
    }
}
