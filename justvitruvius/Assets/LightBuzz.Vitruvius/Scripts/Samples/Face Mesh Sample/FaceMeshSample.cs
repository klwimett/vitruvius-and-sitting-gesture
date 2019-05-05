using UnityEngine;
using System.Collections;
using LightBuzz.Vitruvius;
using Windows.Kinect;
using LightBuzz.Vitruvius.Avateering;

public class FaceMeshSample : VitruviusSample
{
    #region Variables

    BodyWrapper body;
    Face face;

    public FaceModel faceModel;

    #endregion

    #region Reserved methods // Awake - OnApplicationQuit - Update

    protected override void Awake()
    {
        base.Awake();

        Avateering.Enable();
        faceModel.Initialize();
    }

    protected override void OnApplicationQuit()
    {
        base.OnApplicationQuit();

        Avateering.Disable();

        if (faceModel != null)
        {
            faceModel.Dispose();
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
            if (faceFrameReader != null)
            {
                if (!faceFrameSource.IsTrackingIdValid)
                {
                    faceFrameSource.TrackingId = body.TrackingId;
                }

                using (var faceFrame = faceFrameReader.AcquireLatestFrame())
                {
                    if (faceFrame != null)
                    {
                        face = faceFrame.Face();
                    }
                }
            }
        }

        if (face != null)
        {
            Avateering.Update(faceModel, face);
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
}