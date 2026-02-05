using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.Experimental.Rendering;

public class WebCamManager : MonoBehaviour
{
    private WebCamDevice[] _cams;

    private WebCamTexture _camTx;

    private RawImage _display;

    private Texture2D _captureTx;

    void Awake()
    {
        InitDisplay();

        InitCam();
    }



    void InitDisplay()
    {
        _display = GetComponent<RawImage>();
    }


    void InitCam()
    {
        try
        {
            _cams = WebCamTexture.devices;

            int index = XmlManager.Device.Index_cam;

            _camTx = new WebCamTexture(_cams[index].name, 1, 1, 30);

            Debug.Log($"camTx format: {GraphicsFormatUtility.GetTextureFormat(_camTx.graphicsFormat)}");
            NLogManager.Info($"WebCam connected: {_cams[index].name}");

            StartCam();
        }
        catch (Exception ex)
        {
            NLogManager.Error($"Webcam doesn't exists: {ex.Message}");
        }
    }

    public void StartCam()
    {
        if (_camTx == null)
        {
            NLogManager.Warn("Webcam is not Ready");
            return;
        }


        _display.texture = _camTx;

        Debug.Log($"디스플레이 포맷: {GraphicsFormatUtility.GetTextureFormat(_display.texture.graphicsFormat)}");

        _camTx.Play();

        Debug.Log("Webcam size: " + _camTx.width + " x " + _camTx.height);
        Debug.Log("isPlaying: " + _camTx.isPlaying);
    }

    public void Capture()
    {
        if (_camTx == null || _camTx.isPlaying == false)
        {
            NLogManager.Warn("Webcam is not Ready");
            return;
        }

        _captureTx = new Texture2D(_camTx.width, _camTx.height, TextureFormat.RGB24, false);

        _captureTx.SetPixels(_camTx.GetPixels());
        _captureTx.Apply();

        _camTx.Pause();
        _display.texture = _captureTx;
    }

    public byte[] GetImgBytes()
    {
        return _captureTx.EncodeToPNG();
    }

    void OnDestroy()
    {
        if (_camTx != null)
        {
            _camTx.Stop();
            Destroy(_camTx);
            _camTx = null;
        }
    }

    void OnApplicationQuit()
    {
        if (_camTx != null)
        {
            _camTx.Stop();
            Destroy(_camTx);
            _camTx = null;
        }
    }
}