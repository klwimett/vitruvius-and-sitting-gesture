using UnityEngine;
using LightBuzz.Vitruvius;
using LightBuzz.Vitruvius.FingerTracking;
using Windows.Kinect;

public class FingerTrackingSample : VitruviusSample
{
    #region Variables

    Body body;
    ushort[] depthData;
    HandsController handsController = new HandsController();

    public FrameView handsFrameView;
    Color32[] handsColors;
    Texture2D handsTexture;

    [Space(5)]

    public Transform wristLeft;
    public Transform handLeft;
    public Transform elbowLeft;
    public LineRenderer handThumbLeftLine;
    public FingerHelper[] fingersHandLeft;

    [Space(5)]

    public Transform wristRight;
    public Transform handRight;
    public Transform elbowRight;
    public LineRenderer handThumbRightLine;
    public FingerHelper[] fingersHandRight;

    DepthSpacePoint[] leftHandContour = null;
    DepthSpacePoint[] rightHandContour = null;

    public FilterModuleManager moduleManager;

    #endregion

    #region Reserved methods // Awake - Start - OnDrawGizmos - OnApplicationQuit - Update

    protected override void Awake()
    {
        base.Awake();

        handsController.moduleManager = moduleManager;
    }

    void Start()
    {
        int width = Constants.DEFAULT_DEPTH_WIDTH;
        int height = Constants.DEFAULT_DEPTH_HEIGHT;

        depthData = new ushort[width * height];
        handsColors = new Color32[width * height];
        handsTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);

        handsController.HandsDetected += HandsController_HandsDetected;
    }

    void OnDrawGizmos()
    {
        if (leftHandContour == null || rightHandContour == null) return;

        Gizmos.color = Color.red;

        Vector3 from = new Vector3();
        Vector3 to = new Vector3();

        for (int i = 1; i < leftHandContour.Length; i++)
        {
            from.Set(leftHandContour[i].X, leftHandContour[i].Y, 0);
            to.Set(leftHandContour[i - 1].X, leftHandContour[i - 1].Y, 0);
            from = frameView.GetPositionOnFrame(from);
            to = frameView.GetPositionOnFrame(to);

            Gizmos.DrawLine(from, to);
        }

        for (int i = 1; i < rightHandContour.Length; i++)
        {
            from.Set(rightHandContour[i].X, rightHandContour[i].Y, 0);
            to.Set(rightHandContour[i - 1].X, rightHandContour[i - 1].Y, 0);
            from = frameView.GetPositionOnFrame(from);
            to = frameView.GetPositionOnFrame(to);

            Gizmos.DrawLine(from, to);
        }
    }

    protected override void OnApplicationQuit()
    {
        if (handsController != null)
        {
            handsController.HandsDetected -= HandsController_HandsDetected;
            handsController = null;
        }

        base.OnApplicationQuit();
    }

    void Update()
    {
        UpdateBodyFrame();
        UpdateDepthFrame(false);
        UpdateInfraredFrame();
    }

    #endregion

    #region OnBodyFrameReceived

    protected override void OnBodyFrameReceived(BodyFrame frame)
    {
        body = frame.Bodies().Closest();
    }

    #endregion

    #region OnDepthFrameReceived

    protected override void OnDepthFrameReceived(DepthFrame frame)
    {
        frame.CopyFrameDataToArray(depthData);

        handsController.Update(depthData, body);
    }

    #endregion

    #region HandsController_HandsDetected

    void HandsController_HandsDetected(object sender, HandCollection e)
    {
        if (e.HandLeft != null)
        {
            leftHandContour = (DepthSpacePoint[])e.HandLeft.ContourDepth;

            wristLeft.position = frameView.GetPositionOnFrame(e.HandLeft.WristPosition.ToVector3());
            handLeft.position = frameView.GetPositionOnFrame(e.HandLeft.HandPosition.ToVector3());
            elbowLeft.position = frameView.GetPositionOnFrame(body.Joints[JointType.ElbowLeft].Position.ToPoint(Visualization.Depth, CoordinateMapper));
            handThumbLeftLine.SetPosition(0, elbowLeft.position);
            handThumbLeftLine.SetPosition(1, wristLeft.position);
            handThumbLeftLine.SetPosition(2, handLeft.position);

            for (int i = 0; i < e.HandLeft.Fingers.Count; i++)
            {
                fingersHandLeft[i].finger.position = frameView.GetPositionOnFrame(e.HandLeft.Fingers[i].DepthPoint.ToPoint());

                FingerJoint[] joints = e.HandLeft.Fingers[i].joints;
                for (int j = 0; j < joints.Length; j++)
                {
                    fingersHandLeft[i].joints[j].position = frameView.GetPositionOnFrame(joints[j].DepthPoint.ToPoint());
                }
            }
        }

        if (e.HandRight != null)
        {
            rightHandContour = (DepthSpacePoint[])e.HandRight.ContourDepth;

            wristRight.position = frameView.GetPositionOnFrame(e.HandRight.WristPosition.ToVector3());
            handRight.position = frameView.GetPositionOnFrame(e.HandRight.HandPosition.ToVector3());
            elbowRight.position = frameView.GetPositionOnFrame(body.Joints[JointType.ElbowRight].Position.ToPoint(Visualization.Depth, CoordinateMapper));
            handThumbRightLine.SetPosition(0, elbowRight.position);
            handThumbRightLine.SetPosition(1, wristRight.position);
            handThumbRightLine.SetPosition(2, handRight.position);

            for (int i = 0; i < e.HandRight.Fingers.Count; i++)
            {
                fingersHandRight[i].finger.position = frameView.GetPositionOnFrame(e.HandRight.Fingers[i].DepthPoint.ToPoint());

                FingerJoint[] joints = e.HandRight.Fingers[i].joints;
                for (int j = 0; j < joints.Length; j++)
                {
                    fingersHandRight[i].joints[j].position = frameView.GetPositionOnFrame(joints[j].DepthPoint.ToPoint());
                }
            }
        }

        Color32 innerColorLeft = Color.blue;
        Color32 innerColorRight = Color.green;
        Color32 contourColor = Color.red;
        Color32 transparent = Color.clear;

        for (int i = 0, dataCount = depthData.Length; i < dataCount; i++)
        {
            switch (handsController.HandArray[i])
            {
                case 0:
                    handsColors[i] = innerColorLeft;
                    break;
                case 2:
                    handsColors[i] = innerColorRight;
                    break;
                case 1:
                case 3:
                    handsColors[i] = contourColor;
                    break;
                default:
                    handsColors[i] = transparent;
                    break;
            }
        }

        handsTexture.SetPixels32(handsColors);
        handsTexture.Apply(false);

        handsFrameView.FrameTexture = handsTexture;
    }

    #endregion
}