using UnityEngine;
using UnityEngine.UI;
using LightBuzz.Vitruvius;
using Windows.Kinect;

public class GesturesSample : VitruviusSample
{
    #region Variables

    BodyWrapper body;

    GestureController gestureController;
    public Text gestureText;

    #endregion

    #region Reserved methods // Awake - OnApplicationQuit - Update

    protected override void Awake()
    {
        base.Awake();

        gestureController = new GestureController();
        gestureController.GestureRecognized += GestureRecognized;
        gestureController.Start();
    }

    protected override void OnApplicationQuit()
    {
        base.OnApplicationQuit();

        if (gestureController != null)
        {
            gestureController.Stop();
            gestureController.GestureRecognized -= GestureRecognized;
            gestureController = null;
        }
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
            gestureController.Update(body);
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
            }
            else
            {
                this.body.Set(body, CoordinateMapper, visualization);
            }
        }
        else if (this.body != null)
        {
            this.body = null;
        }
    }

    #endregion

    #region GestureRecognized

    void GestureRecognized(object sender, GestureEventArgs e)
    {
        gestureText.text = "Gesture: <b>" + e.GestureType.ToString() + "</b>";
    }

    #endregion
}