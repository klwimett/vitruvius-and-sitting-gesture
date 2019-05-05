using UnityEngine;
using LightBuzz.Vitruvius;
using LightBuzz.Vitruvius.Avateering;
using Windows.Kinect;

public class FacePointsSample : VitruviusSample
{
    #region Variables

    BodyWrapper body;
    Face face;

    public DetailedFace detailedFace;

    #endregion

    #region Reserved methods // Awake - Update

    protected override void Awake()
    {
        base.Awake();

        Avateering.Enable();
        detailedFace.Initialize();
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

                        if (face != null)
                        {
                            if (!detailedFace.gameObject.activeSelf)
                            {
                                detailedFace.gameObject.SetActive(true);
                            }

                            detailedFace.UpdateFace(face, frameView, visualization, CoordinateMapper);
                        }
                        else if (detailedFace.gameObject.activeSelf)
                        {
                            detailedFace.gameObject.SetActive(false);
                        }
                    }
                }
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