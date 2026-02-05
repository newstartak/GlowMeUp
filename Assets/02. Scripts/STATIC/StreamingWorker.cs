using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public static class StreamingWorker
{
    public static Task<string> GetFile(string fileName)
    {
#if UNITY_ANDROID
        return GetFile_Android(fileName);
#else
        return Task.FromResult( Path.Combine(Application.streamingAssetsPath, fileName) );
#endif
    }

    private static async Task<string> GetFile_Android(string fileName)
    {
        string dataPath = Application.persistentDataPath + '/' + fileName;

        if (File.Exists(dataPath) == false)
        {
            await WebWorker.CopyStreamingToData(fileName);
        }

        return dataPath;
    }
}
