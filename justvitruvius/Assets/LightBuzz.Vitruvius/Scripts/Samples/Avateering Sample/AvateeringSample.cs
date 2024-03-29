﻿using LightBuzz.Vitruvius;
using LightBuzz.Vitruvius.Avateering;
using Windows.Kinect;
using Microsoft.Kinect.Face;
using System.Linq;

public class AvateeringSample : VitruviusSample
{
    #region Variables

    public bool showStickmen = true;
    public Stickman[] stickmen;

    public bool controlOneAvatarEach;
    public FBX[] models = new FBX[5];

    public DetailedFace detailedFace;

    public string saveFramePath = @"C:\Users\Public\Documents\SampleFrame.png";
    public bool toggleSaveFrame = false;

    #endregion

    #region Reserved methods // Awake - OnApplicationQuit - Update

    protected override void Awake()
    {
        base.Awake();

        Avateering.Enable();

        for (int i = 0; i < stickmen.Length; i++)
        {
            stickmen[i].Initialize();
        }

        for (int i = 0; i < models.Length; i++)
        {
            models[i].Initialize();
        }

        detailedFace.Initialize();
    }

    protected override void OnApplicationQuit()
    {
        base.OnApplicationQuit();

        Avateering.Disable();

        for (int i = 0; i < models.Length; i++)
        {
            models[i].Dispose();
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
    }

    #endregion

    #region OnColorFrameReceived

    protected override void OnColorFrameReceived(ColorFrame frame)
    {
        CaptureFrame();
    }

    #endregion

    #region OnDepthFrameReceived

    protected override void OnDepthFrameReceived(DepthFrame frame)
    {
        CaptureFrame();
    }

    #endregion

    #region OnInfraredFrameReceived

    protected override void OnInfraredFrameReceived(InfraredFrame frame)
    {
        CaptureFrame();
    }

    #endregion

    #region OnBodyFrameReceived

    protected override void OnBodyFrameReceived(BodyFrame frame)
    {
        if (controlOneAvatarEach)
        {
            // Each player controls one avatar.

            Body[] users = frame.Bodies().Where(b => b.IsTracked).ToArray();
            int userCount = users.Length;

            for (int i = 0; i < userCount; i++)
            {
                if (models[i].updateModel)
                {
                    Avateering.Update(models[i], users[i]);
                }

                if (showStickmen)
                {
                    if (!stickmen[i].gameObject.activeSelf)
                    {
                        stickmen[i].gameObject.SetActive(true);
                    }
                }
                else if (stickmen[i].gameObject.activeSelf)
                {
                    stickmen[i].gameObject.SetActive(false);
                }

                stickmen[i].Update(users[i], null, frameView, CoordinateMapper, visualization);
            }

            if (faceFrameReader != null && users.Length > 0)
            {
                if (!faceFrameSource.IsTrackingIdValid)
                {
                    faceFrameSource.TrackingId = users[0].TrackingId;
                }

                using (var faceFrame = faceFrameReader.AcquireLatestFrame())
                {
                    if (faceFrame != null)
                    {
                        Face face = faceFrame.Face();

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

            for (int i = userCount; i < models.Length; i++)
            {
                if (stickmen[i].gameObject.activeSelf)
                {
                    stickmen[i].gameObject.SetActive(false);
                }
            }
        }
        else
        {
            // One player controls all the avatars.

            Body user = frame.Bodies().Closest();

            if (user != null)
            {
                for (int i = 0; i < models.Length; i++)
                {
                    if (models[i].updateModel)
                    {
                        Avateering.Update(models[i], user);
                    }
                }

                if (showStickmen)
                {
                    if (!stickmen[0].gameObject.activeSelf)
                    {
                        stickmen[0].gameObject.SetActive(true);
                    }
                }
                else if (stickmen[0].gameObject.activeSelf)
                {
                    stickmen[0].gameObject.SetActive(false);
                }

                stickmen[0].Update(user, null, frameView, CoordinateMapper, visualization);

                if (faceFrameReader != null)
                {
                    if (!faceFrameSource.IsTrackingIdValid)
                    {
                        faceFrameSource.TrackingId = user.TrackingId;
                    }

                    using (HighDefinitionFaceFrame faceFrame = faceFrameReader.AcquireLatestFrame())
                    {
                        if (faceFrame != null)
                        {
                            Face face = faceFrame.Face();

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
    }

    #endregion

    #region CaptureFrame

    void CaptureFrame()
    {
        if (toggleSaveFrame)
        {
            toggleSaveFrame = false;
            frameView.FrameTexture.Save(saveFramePath);
        }
    }

    #endregion
}