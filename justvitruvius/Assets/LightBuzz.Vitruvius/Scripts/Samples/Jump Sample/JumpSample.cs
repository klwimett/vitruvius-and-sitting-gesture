using UnityEngine;
using UnityEngine.UI;
using LightBuzz.Vitruvius;
using LightBuzz.Vitruvius.Avateering;
using Windows.Kinect;

public class JumpSample : VitruviusSample
{
    #region Variables

    BodyWrapper body;

    public Stickman stickman;

    public JumpFBX model;

    public Text playerStateText;

    #endregion

    #region Reserved methods // Awake - OnApplicationQuit - Update

    protected override void Awake()
    {
        base.Awake();

        Avateering.Enable();
        stickman.Initialize();
        model.Initialize();
    }

    protected override void OnApplicationQuit()
    {
        base.OnApplicationQuit();

        Avateering.Disable();
        model.Dispose();
    }

    protected void Update()
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
            if (model.updateModel)
            {
                Avateering.Update(model, body);
            }

            stickman.Update(body, null, frameView, CoordinateMapper, visualization);

            playerStateText.text = "Player State: " + (model.JumpHeight > 0 ? "Jumping: " + model.JumpHeight.ToString("N2") : "Floored");
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