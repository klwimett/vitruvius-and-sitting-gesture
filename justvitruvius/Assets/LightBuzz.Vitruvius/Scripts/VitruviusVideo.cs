using LightBuzz.Vitruvius;
using LightBuzz.Vitruvius.Video;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class VitruviusVideo : MonoBehaviour
{
    [Header("Unity Editor options")]

    [Tooltip("The folder where the video frames will be saved")]
    public string folder;

    [Tooltip("The speed of the video")]
    [Range(0, 10)]
    public float speed = 1f;

    [Tooltip("Loop video")]
    public bool loop = false;

    [Tooltip("Video state")]
    public VitruviusVideoState state = VitruviusVideoState.Record;

    [Header("Recording")]
    public Button buttonRecord;
    public Button buttonLoad;

    [Header("Playback slider")]
    public VitruviusVideoSlider slider;
    public Button buttonPlay;
    public Button buttonPause;
    public Button buttonOptions;
    public Text textTimeElapsed;
    public Text textTimeDuration;

    [Header("Playback options")]
    public GameObject panelOptions;
    public Button buttonBackground;
    public Button buttonClose;
    public Toggle toggleLoop;
    public Toggle toggleSpeed05x;
    public Toggle toggleSpeed1x;
    public Toggle toggleSpeed2x;
    public Toggle toggleSpeed4x;

    public event EventHandler OnRecordingStarted;
    public event EventHandler OnRecordingFinished;
    public event EventHandler OnPlaybackStarted;
    public event EventHandler OnPlaybackFinished;
    public event EventHandler<KinectVideoFrameArrivedEventArgs> OnFrameArrived;

    protected readonly string DEFAULT_FOLDER = "video";

    [HideInInspector]
    public KinectVideoRecorder videoRecorder;

    [HideInInspector]
    public KinectVideoPlayer videoPlayer;

    private void Start()
    {
        if (string.IsNullOrEmpty(folder))
        {
            folder = Path.Combine(Application.persistentDataPath, DEFAULT_FOLDER);
        }

        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        videoRecorder = new KinectVideoRecorder();
        videoRecorder.Path = folder;
        videoRecorder.Started += VideoRecorder_Started;
        videoRecorder.Finished += VideoRecorder_Finished;

        videoPlayer = new KinectVideoPlayer();
        videoPlayer.Path = folder;
        videoPlayer.FrameArrived += VideoPlayer_FrameArrived;
        videoPlayer.Started += VideoPlayer_Started;
        videoPlayer.Finished += VideoPlayer_Finished;

        slider.ValueChanged += Slider_ValueChanged;
        slider.PointerDown += Slider_PointerDown;
        slider.PointerUp += Slider_PointerUp;

        toggleLoop.onValueChanged.AddListener(Loop_ValueChanged);
        toggleSpeed05x.onValueChanged.AddListener(Speed05_ValueChanged);
        toggleSpeed1x.onValueChanged.AddListener(Speed1x_ValueChanged);
        toggleSpeed2x.onValueChanged.AddListener(Speed2x_ValueChanged);
        toggleSpeed4x.onValueChanged.AddListener(Speed4x_ValueChanged);

        buttonRecord.onClick.AddListener(Record_Click);
        buttonPlay.onClick.AddListener(Play_Click);
        buttonPause.onClick.AddListener(Pause_Click);
        buttonOptions.onClick.AddListener(Options_Click);
        buttonClose.onClick.AddListener(CloseOptions_Click);
        buttonBackground.onClick.AddListener(CloseOptions_Click);
#if UNITY_EDITOR
        buttonLoad.onClick.AddListener(Load_Click);
#endif
    }

    private void OnApplicationQuit()
    {
        videoPlayer.FrameArrived -= VideoPlayer_FrameArrived;
        videoPlayer.Finished -= VideoPlayer_Finished;

        slider.ValueChanged -= Slider_ValueChanged;
        slider.PointerDown -= Slider_PointerDown;
        slider.PointerUp -= Slider_PointerUp;

        toggleLoop.onValueChanged.RemoveAllListeners();
        toggleSpeed05x.onValueChanged.RemoveAllListeners();
        toggleSpeed1x.onValueChanged.RemoveAllListeners();
        toggleSpeed2x.onValueChanged.RemoveAllListeners();
        toggleSpeed4x.onValueChanged.RemoveAllListeners();

        buttonRecord.onClick.RemoveAllListeners();
        buttonPlay.onClick.RemoveAllListeners();
        buttonPause.onClick.RemoveAllListeners();
        buttonOptions.onClick.RemoveAllListeners();
        buttonBackground.onClick.RemoveAllListeners();
        buttonClose.onClick.RemoveAllListeners();
#if UNITY_EDITOR
        buttonLoad.onClick.RemoveAllListeners();
#endif
    }

    private void Update()
    {
        slider.gameObject.SetActive(state == VitruviusVideoState.Playback);

        buttonRecord.gameObject.SetActive(state == VitruviusVideoState.Record);
        buttonPlay.gameObject.SetActive(state == VitruviusVideoState.Playback && (videoPlayer.IsPaused || (!videoPlayer.IsPlaying && !videoPlayer.IsPaused)));
        buttonPause.gameObject.SetActive(state == VitruviusVideoState.Playback && (videoPlayer.IsPlaying && !videoPlayer.IsPaused));
#if UNITY_EDITOR
        buttonLoad.gameObject.SetActive(state == VitruviusVideoState.Record);
#endif

        if (videoPlayer.IsPlaying)
        {
            videoPlayer.Update();
            slider.value = videoPlayer.Time / videoPlayer.Duration;
        }
    }

    private void OnValidate()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Speed = speed;
            videoPlayer.Loop = loop;
        }
    }

    public void UpdateRecording(byte[] image, Visualization visualization, ColorFrameResolution resolution, BodyWrapper body, Face face, Floor floor, bool isGreenScreen = false)
    {
        if (videoRecorder.IsRecording)
        {
            videoRecorder.Record(image, visualization, resolution, body, face, floor, isGreenScreen);
        }
    }

    public void UpdatePlayback()
    {
        if (videoPlayer.IsPlaying)
        {
            videoPlayer.UpdateFrame();
        }
    }

    private void VideoRecorder_Started(object sender, EventArgs e)
    {
        if (OnRecordingStarted != null)
        {
            OnRecordingStarted.Invoke(this, null);
        }
    }

    private void VideoRecorder_Finished(object sender, EventArgs e)
    {
        videoPlayer.Play();

        if (videoPlayer.IsPlaying)
        {
            state = VitruviusVideoState.Playback;
        }

        if (OnRecordingFinished != null)
        {
            OnRecordingFinished.Invoke(this, null);
        }
    }

    private void VideoPlayer_Started(object sender, EventArgs e)
    {
        if (OnPlaybackStarted != null)
        {
            OnPlaybackStarted.Invoke(this, null);
        }
    }

    private void VideoPlayer_FrameArrived(object sender, KinectVideoFrameArrivedEventArgs e)
    {
        if (OnFrameArrived != null)
        {
            OnFrameArrived(this, e);
        }
    }

    private void VideoPlayer_Finished(object sender, EventArgs e)
    {
        if (!videoPlayer.Loop)
        {
            buttonPlay.gameObject.SetActive(true);
            buttonPause.gameObject.SetActive(false);
        }

        if (OnPlaybackFinished != null)
        {
            OnPlaybackFinished.Invoke(this, null);
        }
    }

    private void Slider_ValueChanged(float value)
    {
        UpdateTime();

        if (videoPlayer.IsSeeking)
        {
            videoPlayer.Seek = Mathf.Clamp01(value) * videoPlayer.Duration;
        }
    }

    private void Slider_PointerDown()
    {
        if (!videoPlayer.IsPlaying)
        {
            videoPlayer.Play();
            videoPlayer.Pause();
        }

        videoPlayer.IsSeeking = true;
        videoPlayer.Seek = Mathf.Clamp01(slider.value) * videoPlayer.Duration;
    }

    private void Slider_PointerUp()
    {
        videoPlayer.IsSeeking = false;
    }

    private void Play_Click()
    {
        videoPlayer.Play();

        if (videoPlayer.IsPlaying)
        {
            state = VitruviusVideoState.Playback;
        }
    }

    private void Pause_Click()
    {
        videoPlayer.Pause();
    }

    private void CloseOptions_Click()
    {
        panelOptions.SetActive(false);
    }

    private void Options_Click()
    {
        panelOptions.SetActive(!panelOptions.activeSelf);
    }

    private void Record_Click()
    {
        if (videoPlayer.IsPlaying)
        {
            return;
        }

        if (!videoRecorder.IsRecording)
        {
#if UNITY_EDITOR || !UNITY_WINRT

            if (Directory.Exists(videoRecorder.Path))
            {
                Directory.Delete(videoRecorder.Path, true);
                Directory.CreateDirectory(videoRecorder.Path);
            }

#endif
            videoRecorder.Start();

            buttonRecord.GetComponentInChildren<Text>().text = "Stop Recording";
            state = VitruviusVideoState.Record;
        }
        else
        {
            videoRecorder.Stop();

            buttonRecord.GetComponentInChildren<Text>().text = "Start Recording";
        }
    }

#if UNITY_EDITOR
    private void Load_Click()
    {
        var path = UnityEditor.EditorUtility.OpenFolderPanel("Load Vitruvius Kinect Video", videoPlayer.Path, "");

        if (string.IsNullOrEmpty(path))
        {
            return;
        }

        folder = path;

        videoRecorder.Path = path;
        videoPlayer.Path = path;
        videoPlayer.Play();

        if (videoPlayer.IsPlaying)
        {
            state = VitruviusVideoState.Playback;
        }
    }
#endif

    private void Loop_ValueChanged(bool value)
    {
        videoPlayer.Loop = value;
    }

    private void Speed05_ValueChanged(bool value)
    {
        if (toggleSpeed05x.isOn)
        {
            videoPlayer.Speed = 0.5f;

            toggleSpeed1x.isOn = false;
            toggleSpeed2x.isOn = false;
            toggleSpeed4x.isOn = false;
        }
    }

    private void Speed1x_ValueChanged(bool value)
    {
        if (toggleSpeed1x.isOn)
        {
            videoPlayer.Speed = 1.0f;

            toggleSpeed05x.isOn = false;
            toggleSpeed2x.isOn = false;
            toggleSpeed4x.isOn = false;
        }
    }

    private void Speed2x_ValueChanged(bool value)
    {
        if (toggleSpeed2x.isOn)
        {
            videoPlayer.Speed = 2.0f;

            toggleSpeed05x.isOn = false;
            toggleSpeed1x.isOn = false;
            toggleSpeed4x.isOn = false;
        }
    }

    private void Speed4x_ValueChanged(bool value)
    {
        if (toggleSpeed4x.isOn)
        {
            videoPlayer.Speed = 4.0f;

            toggleSpeed05x.isOn = false;
            toggleSpeed1x.isOn = false;
            toggleSpeed2x.isOn = false;
        }
    }

    private void UpdateTime()
    {
        if (float.IsNaN(slider.value))
        {
            slider.value = 1f;
        }

        textTimeDuration.text = TimeFormatter.Format(videoPlayer.Duration).ToTimeString();
        textTimeElapsed.text = TimeFormatter.Format(slider.value * videoPlayer.Duration).ToTimeString();
    }
}

public class Tuple<T1, T2>
{
    public T1 Item1 { get; set; }

    public T2 Item2 { get; set; }

    public Tuple()
    {
    }

    public Tuple(T1 item1, T2 item2)
    {
        Item1 = item1;
        Item2 = item2;
    }
}

public static class TimeFormatter
{
    public static Tuple<int, int> Format(float totalSeconds)
    {
        int minutes = (int)(totalSeconds / 60);
        int seconds = (int)(totalSeconds - minutes * 60);

        return new Tuple<int, int>(minutes, seconds);
    }

    public static string ToTimeString(this Tuple<int, int> tuple)
    {
        string minutes = tuple.Item1.ToString("D2");
        string seconds = tuple.Item2.ToString("D2");
        string result = minutes + ":" + seconds;

        return result;
    }
}