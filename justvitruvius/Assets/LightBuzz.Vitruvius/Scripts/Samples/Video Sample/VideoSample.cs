using UnityEngine;
using LightBuzz.Vitruvius;
using LightBuzz.Vitruvius.Avateering;
using Microsoft.Kinect.Face;
using Windows.Kinect;

public class VideoSample : VitruviusSample
{
    #region Variables

    public Stickman stickman;
    public FBX model;
    public VitruviusSensorInfo sensorInfo;
    public VitruviusVideo vitruviusVideo;

    BodyWrapper body = null;
    Face face = null;
    Floor floor = null;
    Texture2D texture = null;
    byte[] image = null;

    #endregion

    #region Reserved methods // Awake - OnApplicationQuit - Update

    protected override void Awake()
    {
        base.Awake();

        texture = new Texture2D(1920, 1080, TextureFormat.RGBA32, false);

        vitruviusVideo.OnFrameArrived += VitruviusVideo_OnFrameArrived;

        Avateering.Enable();

        stickman.Initialize();
        model.Initialize();
    }

    protected override void OnApplicationQuit()
    {
        vitruviusVideo.OnFrameArrived -= VitruviusVideo_OnFrameArrived;

        Avateering.Disable();
        model.Dispose();

        base.OnApplicationQuit();
    }

    void Update()
    {
        if (vitruviusVideo.state == VitruviusVideoState.Playback)
        {
            vitruviusVideo.UpdatePlayback();
        }
        else
        {
            image = null;

            UpdateColorFrame(out image);

            UpdateBodyFrame();

            RefreshFrame();

            if (vitruviusVideo.state == VitruviusVideoState.Record && image != null)
            {
                vitruviusVideo.UpdateRecording(image, visualization, resolution, body, face, floor);
            }
        }
    }

    #endregion

    #region OnBodyFrameReceived

    protected override void OnBodyFrameReceived(BodyFrame frame)
    {
        floor = frame.Floor();

        Body body = frame.Bodies().Closest();

        if (body != null && this.body == null)
        {
            this.body = new BodyWrapper();
        }
        else if (body == null && this.body != null)
        {
            this.body = null;
        }

        if (this.body != null)
        {
            this.body.Set(body, CoordinateMapper, visualization);

            if (faceFrameReader != null)
            {
                if (!faceFrameSource.IsTrackingIdValid)
                {
                    faceFrameSource.TrackingId = this.body.TrackingId;
                }

                using (HighDefinitionFaceFrame faceFrame = faceFrameReader.AcquireLatestFrame())
                {
                    if (faceFrame != null)
                    {
                        face = faceFrame.Face();
                    }
                }
            }
        }
    }

    #endregion

    #region VitruviusVideo_OnFrameArrived

    void VitruviusVideo_OnFrameArrived(object sender, KinectVideoFrameArrivedEventArgs e)
    {
        visualization = e.Visualization;
        resolution = e.Resolution;
        texture = e.Image;
        body = e.Body;
        face = e.Face;
        floor = e.Floor;

        frameView.FrameTexture = texture;

        RefreshFrame();
    }

    #endregion

    #region RefreshFrame

    void RefreshFrame()
    {
        if (body != null)
        {
            Avateering.Update(model, body);

            stickman.Update(body, face, frameView, CoordinateMapper, visualization);
        }

        if (floor != null)
        {
            sensorInfo.Floor = floor;
        }
    }

    #endregion
}