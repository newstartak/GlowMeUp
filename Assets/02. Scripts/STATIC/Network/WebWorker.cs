using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public static class WebWorker
{
    public static async Task<bool> HttpPost(List<string> endPoints, WWWForm form)
    {
        foreach (var endPoint in endPoints)
        {
            using UnityWebRequest req = UnityWebRequest.Post(endPoint, form);
            req.timeout = 2;
            await req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                NLogManager.Error($"HTTP Post failed: {req.error}");

                await RedisWorker.InitRedis();
            }
            else
            {
                NLogManager.Info($"HTTP Post Successed: {req.downloadHandler.text}");

                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 안드로이드에서 스트리밍 애셋 사용을 위해 persistentDataPath로 파일 복사
    /// </summary>
    /// <param name="fileName">확장자 포함한 파일명</param>
    /// <returns>복사된 파일의 persistentDataPath</returns>
    public static async Task CopyStreamingToData(string fileName)
    {
#if UNITY_ANDROID
        string dataPath = Application.persistentDataPath + '/' + fileName;
        string strmPath = Application.streamingAssetsPath + '/' + fileName;
#else
        string dataPath = Path.Combine(Application.persistentDataPath, fileName);
        string strmPath = Path.Combine(Application.streamingAssetsPath, fileName);
#endif
        using (UnityWebRequest req = UnityWebRequest.Get(strmPath))
        {
            req.timeout = 2;
            await req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                return;
            }
            
            await File.WriteAllTextAsync(dataPath, req.downloadHandler.text);
        };

        NLogManager.Debug($"{fileName} copy completed from strm to data");
    }
}