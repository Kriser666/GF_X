using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;
using UnityGameFramework.Runtime;
public class URLParamsEntity : MonoBehaviour
{
    // 接收 URL 参数
    [DllImport("__Internal")]
    private static extern string GetUrlParams();

    public static string UrlMsg;

    private void Start()
    {
        UrlMsg = "";
        try
        {
            UrlMsg = GetUrlParams();
        }
        catch (Exception e)
        {
            UrlMsg = "[Exception:]" + e.Message;
        }
        Log.Debug("======>" + UrlMsg);
    }

    // 发送请求到后端
    private IEnumerator FetchDataFromBackend(string userId)
    {
        string url = string.Format(Const.SAVED_GAMES_URL, userId);
        using UnityWebRequest webRequest = UnityWebRequest.Get(url);
        yield return webRequest.SendWebRequest();

        if (webRequest.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Response: " + webRequest.downloadHandler.text);
            // 处理后端返回的数据
        }
        else
        {
            Debug.LogError("Error: " + webRequest.error);
        }
    }
}
