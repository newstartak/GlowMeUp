using System.Collections;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScCompleteManager : MonoBehaviour
{
    [SerializeField] private Button _returnBtn;
    [Space]
    [SerializeField] private TMP_Text _timer_text;
    [Space]
    [SerializeField] private TMP_Text _char_name; 

    /// <summary>
    /// 일정 시간 초과 시 메인 화면 이동 관리용 코루틴
    /// </summary>
    private Coroutine _returnCo;

    void Awake()
    {
        _returnBtn.onClick.AddListener(async () => await ClickReturnBtnAsync());

        _returnCo = StartCoroutine(ReturnCoroutine());

        _char_name.text = SceneWorker.lastCharName;
    }

    IEnumerator ReturnCoroutine()
    {
        for (int i = XmlManager.Setting.Timer_complete; i >= 0; i--)
        {
            _timer_text.text = i.ToString();

            yield return new WaitForSeconds(1);
        }

        NLogManager.Info("Complete Idle Timeout. Return to Main Screen.");

        Task t = SceneWorker.ChangeSceneAsync("scMain");

        while (t.IsCompleted == false)
        {
            yield return null;
        }
    }

    Task ClickReturnBtnAsync()
    {
        return SceneWorker.ChangeSceneAsync("scMain");
    }
}