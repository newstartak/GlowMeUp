using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

public static class SceneWorker
{
    static public string errorMsg;
    static public string lastCharName;


    public static async Task ChangeSceneAsync(string scName, string infoText = null)
    {
        errorMsg = infoText;

        await SceneManager.LoadSceneAsync(scName);
    }
}