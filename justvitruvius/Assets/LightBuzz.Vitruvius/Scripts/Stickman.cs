using UnityEngine;
using System.Collections;
using Windows.Kinect;
using LightBuzz.Vitruvius;

public class Stickman : MonoBehaviour
{
    #region Variables

    bool initialized = false;

    public bool Mirrored { get; private set; }

    public bool smoothJoints = false;
    [SerializeField, Range(0f, 1f)]
    float smoothness = 0.75f;
    public float Smoothness
    {
        get
        {
            return smoothness;
        }
        set
        {
            smoothness = Mathf.Clamp01(value);
        }
    }

    public StickmanController controller;

    public bool HasEyes
    {
        get
        {
            return controller.eyeLeft != null && controller.eyeRight != null;
        }
    }

    #endregion

    #region Reserved methods // Update

    void Update() { }

    #endregion

    #region Initialize

    public void Initialize()
    {
        if (initialized) return;

        controller.Initialize();

        initialized = true;
    }

    #endregion

    #region Update

    public void Update(BodyWrapper body, Face face, FrameView frameView, CoordinateMapper coordinateMapper, Visualization visualization)
    {
        if (!initialized || frameView == null) return;

        Mirrored = frameView.MirroredView;

        if (body != null)
        {
            UpdateBody(body, frameView, coordinateMapper, visualization);
        }

        if (controller.eyeLeft != null || controller.eyeRight != null)
        {
            UpdateFace(face, frameView);
        }

        UpdateLines();
    }

    public void Update(Body body, Face face, FrameView frameView, CoordinateMapper coordinateMapper, Visualization visualization)
    {
        if (!initialized || frameView == null) return;

        Mirrored = frameView.MirroredView;

        if (body != null && coordinateMapper != null)
        {
            UpdateBody(body, frameView, coordinateMapper, visualization);
        }

        if (controller.eyeLeft != null || controller.eyeRight != null)
        {
            UpdateFace(face, frameView);
        }

        UpdateLines();
    }

    #endregion

    #region UpdateBody

    void UpdateBody(Body body, FrameView frameView, CoordinateMapper coordinateMapper, Visualization visualization)
    {
        foreach (StickmanJoint joint in controller.Joints)
        {
            Vector2 pos = visualization == Visualization.Color
                ? coordinateMapper.MapCameraPointToColorSpace(body.Joints[joint.JointType].Position).ToPoint()
                : coordinateMapper.MapCameraPointToDepthSpace(body.Joints[joint.JointType].Position).ToPoint();

            if (float.IsInfinity(pos.x))
            {
                pos.x = 0;
            }

            if (float.IsInfinity(pos.y))
            {
                pos.y = 0;
            }

            pos = frameView.GetPositionOnFrame(pos);
            joint.Position = smoothJoints ? Vector3.Lerp(pos, joint.Position, smoothness) : (Vector3)pos;
        }
    }

    void UpdateBody(BodyWrapper body, FrameView frameView, CoordinateMapper coordinateMapper, Visualization visualization)
    {
        foreach (StickmanJoint joint in controller.Joints)
        {
            Vector2 pos = coordinateMapper == null ? body.Map2D[joint.JointType].ToVector2() : (visualization == Visualization.Color
                ? coordinateMapper.MapCameraPointToColorSpace(body.Joints[joint.JointType].Position).ToPoint()
                : coordinateMapper.MapCameraPointToDepthSpace(body.Joints[joint.JointType].Position).ToPoint());

            if (float.IsInfinity(pos.x))
            {
                pos.x = 0;
            }

            if (float.IsInfinity(pos.y))
            {
                pos.y = 0;
            }

            pos = frameView.GetPositionOnFrame(pos);
            joint.Position = smoothJoints ? Vector3.Lerp(pos, joint.Position, smoothness) : (Vector3)pos;
        }
    }

    #endregion

    #region UpdateFace

    void UpdateFace(Face face, FrameView frameView)
    {
        if (face == null)
        {
            if (controller.eyeLeft.gameObject.activeSelf)
            {
                controller.eyeLeft.gameObject.SetActive(false);
                controller.eyeRight.gameObject.SetActive(false);
            }

            return;
        }

        Vector2 eyeLeftPosition = face.EyeLeft2D.ToVector2();
        Vector2 eyeRightPosition = face.EyeRight2D.ToVector2();

        if (!float.IsInfinity(eyeLeftPosition.x) && !float.IsInfinity(eyeLeftPosition.y) &&
               !float.IsInfinity(eyeRightPosition.x) && !float.IsInfinity(eyeRightPosition.y))
        {
            eyeLeftPosition = frameView.GetPositionOnFrame(eyeLeftPosition);
            eyeRightPosition = frameView.GetPositionOnFrame(eyeRightPosition);

            controller.eyeLeft.position = eyeLeftPosition;
            controller.eyeRight.position = eyeRightPosition;

            if (controller.eyesLine != null)
            {
                controller.eyesLine.SetPosition(0, eyeLeftPosition);
                controller.eyesLine.SetPosition(1, eyeRightPosition);
            }

            if (!controller.eyeLeft.gameObject.activeSelf)
            {
                controller.eyeLeft.gameObject.SetActive(true);
                controller.eyeRight.gameObject.SetActive(true);
            }
        }
        else if (controller.eyeLeft.gameObject.activeSelf)
        {
            controller.eyeLeft.gameObject.SetActive(false);
            controller.eyeRight.gameObject.SetActive(false);
        }
    }

    #endregion

    #region UpdateLines

    void UpdateLines()
    {
        for (int i = controller.Joints.Count - 1; i >= 0; i--)
        {
            if (controller.Joints[i].line != null)
            {
                controller.Joints[i].line.SetPosition(0, controller.Joints[i].Position);
                controller.Joints[i].line.SetPosition(1, controller[controller.Joints[i].Link].Position);
            }
        }
    }

    #endregion
}