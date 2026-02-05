using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using TMPro;

public class ScErrorManager : MonoBehaviour
{
    [SerializeField] private Button _returnBtn;
    [SerializeField] private TMP_Text errorText;

    void Awake()
    {
        _returnBtn.onClick.AddListener(async () => await ClickReturnBtn());

        errorText.text = SceneWorker.errorMsg ?? "Unknown Error, Check the Log File.";
    }

    async Task ClickReturnBtn()
    {
        await SceneWorker.ChangeSceneAsync("scMain");
    }
}