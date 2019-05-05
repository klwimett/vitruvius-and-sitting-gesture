using UnityEngine;
using LightBuzz.Vitruvius;
using Windows.Kinect;

public class StudioDrummerSample : VitruviusSample
{
    #region Variables

    BodyWrapper body = null;

    public Stickman stickman;

    byte[] cutoutImagePixels;
    Texture2D cutoutTexture;

    public int minMaxPixelOffset = 10;
    int beginX = 0;
    int endX = 0;

    public Transform leftHand;
    public Transform rightHand;
    public Instrument[] instruments;

    #endregion

    #region Reserved methods // Awake - Start - Update

    protected override void Awake()
    {
        base.Awake();

        stickman.Initialize();

        cutoutTexture = new Texture2D(Constants.DEFAULT_COLOR_WIDTH, Constants.DEFAULT_COLOR_HEIGHT, TextureFormat.RGBA32, false);
    }

    void Start()
    {
        frameView.FrameTexture = cutoutTexture;
    }

    void Update()
    {
        UpdateBodyFrame();

        if (colorFrameReader != null && depthFrameReader != null && bodyIndexFrameReader != null)
        {
            using (ColorFrame colorFrame = colorFrameReader.AcquireLatestFrame())
            using (DepthFrame depthFrame = depthFrameReader.AcquireLatestFrame())
            using (BodyIndexFrame bodyIndexFrame = bodyIndexFrameReader.AcquireLatestFrame())
            {
                if (colorFrame != null && depthFrame != null && bodyIndexFrame != null)
                {
                    colorFrame.GreenScreenHD(depthFrame, bodyIndexFrame, out cutoutImagePixels, beginX, endX);

                    cutoutTexture.LoadRawTextureData(cutoutImagePixels);
                    cutoutTexture.Apply();
                }
            }
        }

        if (body != null)
        {
            stickman.Update(body, null, frameView, CoordinateMapper, visualization);

            leftHand.position = frameView.GetPositionOnFrame(Vector2.Lerp(
                body.Joints[JointType.HandRight].Position.ToPoint(visualization, CoordinateMapper),
                body.Joints[JointType.WristRight].Position.ToPoint(visualization, CoordinateMapper), 0.5f));
            rightHand.position = frameView.GetPositionOnFrame(Vector2.Lerp(
                body.Joints[JointType.HandLeft].Position.ToPoint(visualization, CoordinateMapper),
                body.Joints[JointType.WristLeft].Position.ToPoint(visualization, CoordinateMapper), 0.5f));

            foreach (Instrument percussion in instruments)
            {
                percussion.Validate(leftHand);
                percussion.Validate(rightHand);
            }
        }
    }

    #endregion

    #region OnBodyFrameReceived

    protected override void OnBodyFrameReceived(BodyFrame frame)
    {
        Body body = frame.Bodies().Closest();

        if (body != null)
        {
            if (this.body == null)
            {
                this.body = BodyWrapper.Create(body, CoordinateMapper, visualization);

                OnBodyEnter();
            }
            else
            {
                this.body.Set(body, CoordinateMapper, visualization);
            }
        }
        else if (this.body != null)
        {
            this.body = null;

            OnBodyExit();
        }

        GetMinMaxXFromJoint(out beginX, out endX);
    }

    #endregion

    #region OnBodyEnter

    void OnBodyEnter()
    {
        stickman.gameObject.SetActive(true);
    }

    #endregion

    #region OnBodyExit

    void OnBodyExit()
    {
        stickman.gameObject.SetActive(false);
    }

    #endregion

    #region GetMinMaxXFromJoint

    void GetMinMaxXFromJoint(out int minX, out int maxX)
    {
        minX = int.MaxValue;
        maxX = int.MinValue;

        if (body == null) return;

        for (int i = 0, count = (int)JointType.ThumbLeft; i < count; i++)
        {
            int x = (int)body.Joints[(JointType)i].Position.ToPoint(Visualization.Color, CoordinateMapper).x;

            if (x > maxX)
            {
                maxX = x;
            }

            if (x < minX)
            {
                minX = x;
            }
        }

        minX = Mathf.Clamp(minX - minMaxPixelOffset, 0, Constants.DEFAULT_COLOR_WIDTH);
        maxX = Mathf.Clamp(maxX + minMaxPixelOffset + 1, 0, Constants.DEFAULT_COLOR_WIDTH);
    }

    #endregion
}