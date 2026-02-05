using UnityEngine;
using UnityEngine.UI;
using OpenCvSharp;
using System;
using System.Threading.Tasks;

public class OpenCvWebCamManager : Singleton<OpenCvWebCamManager>
{
    private VideoCapture _vidCap;

    private Mat _threadFrame = new Mat();
    private Mat _mainFrame = new Mat();

    private Texture2D _camTx;
    private Texture2D _captureTx;

    public bool isRunning;

    private bool _isRead;

    private int _rotateAngle;

    private object _lock = new object();

    public async Task InitOpenCvAsync()
    {
        await Task.Run(() => InitOpenCv());
    }

    public void InitOpenCv(int[] openParams = null)
    {
        if (openParams == null)
        {
            openParams = new int[]
            {
                (int)VideoCaptureProperties.FrameWidth, 1280,
                (int)VideoCaptureProperties.FrameHeight, 960,
                (int)VideoCaptureProperties.Fps, 30,
                (int)VideoCaptureProperties.FourCC, VideoWriter.FourCC('M', 'J', 'P', 'G')
            };
        }

        try
        {
            if (_vidCap == null)
            {
                _vidCap = new VideoCapture(XmlManager.Device.Index_cam, VideoCaptureAPIs.DSHOW, openParams);
            }

            _vidCap.Set(VideoCaptureProperties.AutoExposure, 0.25);

            if (!_vidCap.IsOpened())
            {
                throw new Exception("Camera open failed");
            }

            NLogManager.Info("OpenCV init completed.");
        }
        catch (Exception ex)
        {
            NLogManager.Error($"Error occured during open camera: {ex.Message}");
            throw;
        }
    }

    public void InitDisplay()
    {
        int w, h;

        _rotateAngle = XmlManager.Setting.Rotate_angle;

        if (_rotateAngle == 90 || _rotateAngle == 270)
        {
            w = _vidCap.FrameHeight;
            h = _vidCap.FrameWidth;
        }
        else
        {
            w = _vidCap.FrameWidth;
            h = _vidCap.FrameHeight;
        }

        if (_camTx == null)
        {
            _camTx = new Texture2D(w, h, TextureFormat.RGB24, false);
        }
    }

    public void StartDisplay(RawImage webCamDisplay)
    {
        _camTx.Reinitialize(_camTx.width, _camTx.height, TextureFormat.RGB24, false);

        isRunning = true;

        Task.Run(() => CaptureThread());

        webCamDisplay.texture = _camTx;
    }

    public void CaptureThread()
    {
        bool isFlip = XmlManager.Setting.Flip;

        while (isRunning && _vidCap.IsOpened())
        {
            try
            {
                if (_vidCap.Read(_threadFrame))
                {
                    Cv2.CvtColor(_threadFrame, _threadFrame, ColorConversionCodes.BGR2RGB);

                    switch (_rotateAngle)
                    {
                        case 90:
                            Cv2.Rotate(_threadFrame, _threadFrame, RotateFlags.Rotate90Clockwise);
                            break;
                        case 180:
                            Cv2.Rotate(_threadFrame, _threadFrame, RotateFlags.Rotate180);
                            break;
                        case 270:
                            Cv2.Rotate(_threadFrame, _threadFrame, RotateFlags.Rotate90Counterclockwise);
                            break;
                    }

                    if (isFlip)
                    {
                        Cv2.Flip(_threadFrame, _threadFrame, FlipMode.Y);
                    }

                    lock (_lock)
                    {
                        _threadFrame.CopyTo(_mainFrame);
                        _isRead = true;
                    }
                }
            }
            catch (Exception ex)
            {
                NLogManager.Error($"Camera thread error: {ex.Message}");
                throw;
            }
        }
    }

    void Update()
    {
        if (_isRead)
        {
            lock (_lock)
            {
                _isRead = false;
            }
        }
        else
        {
            return;
        }

        if (_mainFrame == null || _mainFrame.Empty() || _camTx == null)
        {
            return;
        }

        lock (_lock)
        {
            _camTx.LoadRawTextureData(_mainFrame.Data, _mainFrame.Size().Width * _mainFrame.Size().Height * _mainFrame.Channels());
        }
        _camTx.Apply(false);
    }

    public byte[] GetImgBytes()
    {
        return _captureTx.EncodeToPNG();
    }

    public void Capture(RawImage captureDisplay)
    {
        if (_camTx == null)
        {
            NLogManager.Warn("Webcam is not Ready");
            return;
        }

        if (_captureTx == null)
        {
            _captureTx = new Texture2D(_camTx.width, _camTx.height, TextureFormat.RGB24, false);
        }

        Graphics.CopyTexture(_camTx, _captureTx);

        captureDisplay.texture = _captureTx;

        isRunning = false;
    }

    void OnDestroy()
    {
        isRunning = false;
        _vidCap?.Release();
    }
}
