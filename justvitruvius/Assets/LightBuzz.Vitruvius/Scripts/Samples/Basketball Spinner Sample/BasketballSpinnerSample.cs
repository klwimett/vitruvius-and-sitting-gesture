using UnityEngine;
using LightBuzz.Vitruvius;
using Windows.Kinect;

public class BasketballSpinnerSample : VitruviusSample
{
    #region Variables

    BodyWrapper body;

    public Transform basketball;
    public float scaleFactor = 1;
    public JointType lockOnJoint = JointType.HandTipLeft;

    #endregion

    #region Reserved methods // Update

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
            Vector2 position2D = frameView.GetPositionOnFrame(body.Joints[lockOnJoint].Position.ToPoint(visualization));

            basketball.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor) / body.Joints[lockOnJoint].Position.Z;
            basketball.position = new Vector3(position2D.x, position2D.y + basketball.localScale.y, 0);
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
        basketball.gameObject.SetActive(true);
    }

    #endregion

    #region OnBodyExit

    void OnBodyExit()
    {
        basketball.gameObject.SetActive(false);
    }

    #endregion
}