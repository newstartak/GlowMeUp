using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScCaptureManager : MonoBehaviour
{
    [Header("카메라 디스플레이")]
    [SerializeField] private RawImage _captureDisplay;
    [SerializeField] private RawImage _confirmDisplay;
    [SerializeField] private RawImage _nameDisplay;

    [Header("Capture 세트")]
    [SerializeField] private GameObject _captureSet;
    [Space]
    [SerializeField] private Button _capture_captureBtn;
    [SerializeField] private TMP_Text _capture_timer_text;

    [Header("Confirm 세트")]
    [SerializeField] private GameObject _confirmSet;
    [Space]
    [SerializeField] private Button _confirm_okBtn;
    [SerializeField] private Button _confirm_retryBtn;
    [SerializeField] private TMP_Text _confirm_timer_text;

    [Header("Name 세트")]
    [SerializeField] private GameObject _nameSet;
    [Space]
    [SerializeField] private Button _name_enterBtn;
    [SerializeField] private Button _name_enterNameBtn;
    [SerializeField] private TMP_InputField _input;
    [Space]
    [SerializeField] private TMP_Text _timer_text;


    /// <summary>
    /// 비동기 실행 중 다른 작업의 의도치 않은 실행 방지용 플래그
    /// </summary>
    private bool _isAsyncing;

    /// <summary>
    /// 스캔/생성 이미지에 대한 고유 아이디
    /// </summary>
    private string _id_image;

    /// <summary>
    /// 캡처 중 미동작 시 메인 화면 이동 관리용 코루틴
    /// </summary>
    private Coroutine _captureCo;

    /// <summary>
    /// 이름 입력 시간 초과 시 랜덤 이름 전송 관리용 코루틴
    /// </summary>
    private Coroutine _nameCo;

    private void Update()
    {
        if(_input.text.Length > 0)
        {
            _name_enterBtn.gameObject.SetActive(false);
            _name_enterNameBtn.gameObject.SetActive(true);
        }
        else
        {
            _name_enterBtn.gameObject.SetActive(true);
            _name_enterNameBtn.gameObject.SetActive(false);
        }
    }

    void Awake()
    {
        InitCamera();

        InitSets();
        InitBtns();

        InitVars();

        InitCoroutine();
    }

    void InitCamera()
    {
        OpenCvWebCamManager.Instance.InitDisplay();
        OpenCvWebCamManager.Instance.StartDisplay(_captureDisplay);
    }

    /// <summary>
    /// 한 씬 내에서 캡처 - 확인 - 이름 입력을 모두 시행,
    /// 해당하는 묶음을 활성화/비활성화 하며 플로우 실행
    /// </summary>
    void InitSets()
    {
        _captureSet.SetActive(true);
        _confirmSet.SetActive(false);
        _nameSet.SetActive(false);
    }

    void InitBtns()
    {
        _capture_captureBtn.onClick.AddListener(() => ClickCaptureBtn());

        _confirm_okBtn.onClick.AddListener(async () => await ClickConfirmOkBtn());
        _confirm_retryBtn.onClick.AddListener(() => ClickCaptureRetryBtn());

        _name_enterNameBtn.onClick.AddListener(async () => await ClickNameEnterBtnAsync());


        // 버튼들 클릭할때마다 메인화면으로 돌아가게 하는 코루틴 초기화 메서드 추가
        _capture_captureBtn.onClick.AddListener(() => InitCoroutine());

        _confirm_retryBtn.onClick.AddListener(() => InitCoroutine());
        // ========================================================================


        // 촬영 확인 후부터는 자동으로 메인화면 돌아가는 코루틴 중지, 이름 입력 시간 제한 및 종료 화면 타임 아웃 등에 의해서 조작
        _confirm_okBtn.onClick.AddListener(() =>
        {
            StopCoroutine(_captureCo);
            _captureCo = null;
        });
    }


    void InitVars()
    {
        _isAsyncing = false;
    }

    void InitCoroutine()
    {
        if(_captureCo != null)
        {
            StopCoroutine(_captureCo);
            _captureCo = null;
        }

        _captureCo = StartCoroutine(CaptureCoroutine());
    }

    IEnumerator CaptureCoroutine()
    {
        int i = XmlManager.Setting.Timer_capture;

        while (i > 0)
        {
            _capture_timer_text.text = i.ToString();
            _confirm_timer_text.text = i.ToString();

            yield return new WaitForSeconds(1);

            i--;
        }

        OpenCvWebCamManager.Instance.isRunning = false;

        NLogManager.Info("Capture Timeout. Return to main screen.");

        Task t = SceneWorker.ChangeSceneAsync("scMain");

        while(t.IsCompleted == false)
        {
            yield return null;
        }
    }

    void ClickCaptureBtn()
    {
        OpenCvWebCamManager.Instance.Capture(_confirmDisplay);
        OpenCvWebCamManager.Instance.Capture(_nameDisplay);

        _captureSet.SetActive(false);
        _confirmSet.SetActive(true);
    }

    async Task ClickConfirmOkBtn()
    {
        if (_isAsyncing)
        {
            return;
        }

        _isAsyncing = true;

        try
        {
            _id_image = DateTime.Now.ToString("HHmmss") + Guid.NewGuid().ToString().Substring(0, 4);

            byte[] imgData = OpenCvWebCamManager.Instance.GetImgBytes();

            WWWForm form = new WWWForm();
            form.AddBinaryData("file", imgData, $"{_id_image}.png", "image/png");

            var endPoints = XmlManager.Http.Endpoint;
            var isHttpSuccess = await WebWorker.HttpPost(endPoints, form);
            if (isHttpSuccess == false)
            {
                NLogManager.Error($"HTTP Server All Dead");

                await SceneWorker.ChangeSceneAsync("scError", "The AI conversion was not successful.\n\nPlease try again shortly, or use the other scanner.");
                return;
            }

            var data = new Dictionary<string, object>
            {
                { "id_device", XmlManager.Device.Id_device },
                { "id_image", _id_image }
            };

            await RedisWorker.PublishRedisAsync("AI_START", data);

            _nameCo = StartCoroutine(NameCoroutine());
        }
        catch (Exception ex)
        {
            NLogManager.Error($"Error occured during send img to AI server: {ex.Message}");

            await SceneWorker.ChangeSceneAsync("scError", "The AI conversion was not successful.\n\nPlease try again shortly, or use the other scanner.");
        }
        finally
        {
            _isAsyncing = false;
        }
        
        _confirmSet.SetActive(false);
        _nameSet.SetActive(true);
    }

    IEnumerator NameCoroutine()
    {
        for (int i = XmlManager.Setting.Timer_name; i >= 0; i--)
        {
            _timer_text.text = i.ToString();

            yield return new WaitForSeconds(1);
        }

        if(_captureCo != null)
        {
            StopCoroutine(_captureCo);
            _captureCo = null;
        }

        NLogManager.Info("Name input Timeout. Random name sent.");

        Task t = SendNameAsync();

        while (t.IsCompleted == false)
        {
            yield return null;
        }
    }

    void ClickCaptureRetryBtn()
    {
        if(_isAsyncing)
        {
            return;
        }

        _confirmSet.SetActive(false);
        _captureSet.SetActive(true);

        OpenCvWebCamManager.Instance.StartDisplay(_captureDisplay);
    }

    public async Task ClickNameEnterBtnAsync()
    {
        if (_isAsyncing)
        {
            return;
        }

        if (string.IsNullOrEmpty(_input.text))
        {
            NLogManager.Warn("character name is empty.");
            return;
        }

        await SendNameAsync();
    }

    async Task SendNameAsync()
    {
        _isAsyncing = true;

        try
        {
            if (string.IsNullOrEmpty(_input.text))
            {
                //var randomNames = XmlManager.Setting.Random_name;
                //int rand = UnityEngine.Random.Range(0, randomNames.Count);

                //SceneWorker.lastCharName = randomNames[rand];

                var randomName = $"Idol {UnityEngine.Random.Range(1, 100)}";

                SceneWorker.lastCharName = randomName;
            }
            else
            {
                SceneWorker.lastCharName = _input.text;
            }

            var data = new Dictionary<string, object>
            {
                { "id_image", _id_image },
                { "char_name", SceneWorker.lastCharName }
            };

            await RedisWorker.PublishRedisAsync("AI_WAITING_LIST", data);

            await SceneWorker.ChangeSceneAsync("scComplete");
        }
        catch (Exception ex)
        {
            NLogManager.Error($"Error occured during send name to AI server: {ex.Message}");

            await SceneWorker.ChangeSceneAsync("scError", "The AI conversion was not successful.\n\nPlease try again shortly, or use the other scanner.");
        }
        finally
        {
            _isAsyncing = false;
        }
    }
}
