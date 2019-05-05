using UnityEngine;
using UnityEngine.UI;
using LightBuzz.Vitruvius;
using Windows.Kinect;

public class PositioningAssistantSample : VitruviusSample
{
    #region Variables

    BodyWrapper body;

    public Stickman stickman;
    public SpriteRenderer positionIndicator;
    public Text positionText;

    #endregion

    #region Reserved methods // Awake - Update

    protected override void Awake()
    {
        base.Awake();

        stickman.Initialize();
    }

    void Update()
    {
        switch (visualization)
        {
            case Visualization.Color:
                UpdateColorFrame();
                break;
            case Visualization.Depth:
                UpdateDepthFrame();
                break;
            default:
                UpdateInfraredFrame();
                break;
        }

        UpdateBodyFrame();

        if (body != null)
        {
            stickman.Update(body, null, frameView, CoordinateMapper, visualization);

            float headY = body.Joints[JointType.Head].Position.ToPoint(Visualization.Color, CoordinateMapper).y;
            float ankleLeftY = body.Joints[JointType.AnkleLeft].Position.ToPoint(Visualization.Color, CoordinateMapper).y;
            float ankleRightY = body.Joints[JointType.AnkleRight].Position.ToPoint(Visualization.Color, CoordinateMapper).y;
            float spineBaseX = body.Joints[JointType.SpineBase].Position.ToPoint(Visualization.Color, CoordinateMapper).x;

            bool isInCorrectPosition = headY > 0f && ankleLeftY < 1080f && ankleRightY < 1080f && spineBaseX > 950f && spineBaseX < 970f;

            positionIndicator.color = isInCorrectPosition ? Color.green : Color.red;
            positionText.text = isInCorrectPosition ? "Good!" : "Move to the marked spot";
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
}