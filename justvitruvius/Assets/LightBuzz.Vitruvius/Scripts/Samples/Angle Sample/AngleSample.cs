using UnityEngine;
using LightBuzz.Vitruvius;
using LightBuzz.Vitruvius.Avateering;
using Joint = Windows.Kinect.Joint;
using Windows.Kinect;

public class AngleSample : VitruviusSample
{
    #region Variables

    BodyWrapper body;

    public Stickman stickman;
    public JointPeak[] jointPeaks;

    public SelectiveAngleHuman model;

    public AngleArc frameViewArc;
    public AngleArc modelArc;

    GUIStyle guiStyle;

    #endregion

    #region Reserved methods // Awake - OnApplicationQuit - Update - OnGUI

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
            if (model.updateModel)
            {
                Avateering.Update(model, body);
            }

            stickman.Update(body, null, frameView, CoordinateMapper, visualization);

            Joint startJoint;
            Joint centerJoint;
            Joint endJoint;
            Vector2 centerJointPosition;
            Vector2 startJointDir;
            Vector2 endJointDir;

            #region JointPeaks

            for (int i = 0; i < jointPeaks.Length; i++)
            {
                startJoint = body.Joints[jointPeaks[i].start];
                centerJoint = body.Joints[jointPeaks[i].center];
                endJoint = body.Joints[jointPeaks[i].end];

                centerJointPosition = stickman.controller[jointPeaks[i].center].Position;
                startJointDir = ((Vector2)stickman.controller[jointPeaks[i].start].Position - centerJointPosition).normalized;
                endJointDir = ((Vector2)stickman.controller[jointPeaks[i].end].Position - centerJointPosition).normalized;

                jointPeaks[i].arc.Angle = Vector2.Angle(startJointDir, endJointDir);

                jointPeaks[i].arc.transform.position = centerJointPosition;
                jointPeaks[i].arc.transform.up = Quaternion.Euler(0, 0, jointPeaks[i].arc.Angle) *
                    (Vector2.Dot(Quaternion.Euler(0, 0, 90) * startJointDir, endJointDir) > 0 ? startJointDir : endJointDir);

                jointPeaks[i].jointAngle = (float)centerJoint.Angle(startJoint, endJoint);
            }

            #endregion

            startJoint = body.Joints[model.start];
            centerJoint = body.Joints[model.center];
            endJoint = body.Joints[model.end];

            #region FrameView Arc

            startJointDir = frameView.GetPositionOnFrame(startJoint.Position.ToPoint(visualization, CoordinateMapper));
            centerJointPosition = frameView.GetPositionOnFrame(centerJoint.Position.ToPoint(visualization, CoordinateMapper));
            endJointDir = frameView.GetPositionOnFrame(endJoint.Position.ToPoint(visualization, CoordinateMapper));

            startJointDir = (startJointDir - centerJointPosition).normalized;
            endJointDir = (endJointDir - centerJointPosition).normalized;

            frameViewArc.Angle = Vector2.Angle(startJointDir, endJointDir);

            frameViewArc.transform.position = centerJointPosition;
            frameViewArc.transform.up = Quaternion.Euler(0, 0, frameViewArc.Angle) *
                (Vector2.Dot(Quaternion.Euler(0, 0, 90) * startJointDir, endJointDir) > 0 ? startJointDir : endJointDir);

            #endregion

            #region Model Arc

            Vector3 arcPosition = model.GetBone(model.center).Transform.position;
            arcPosition.z -= 2;

            centerJointPosition = centerJoint.Position.ToVector3();
            startJointDir = ((Vector2)startJoint.Position.ToVector3() - centerJointPosition).normalized;
            endJointDir = ((Vector2)endJoint.Position.ToVector3() - centerJointPosition).normalized;

            modelArc.Angle = Vector2.Angle(startJointDir, endJointDir);

            modelArc.transform.position = arcPosition;
            modelArc.transform.up = Quaternion.Euler(0, 0, modelArc.Angle) *
                (Vector2.Dot(Quaternion.Euler(0, 0, 90) * startJointDir, endJointDir) > 0 ? startJointDir : endJointDir);

            #endregion
        }
    }

    void OnGUI()
    {
        if (!stickman.isActiveAndEnabled) return;

        if (guiStyle == null)
        {
            guiStyle = new GUIStyle(GUI.skin.textArea);
            guiStyle.alignment = TextAnchor.MiddleCenter;
        }

        for (int i = 0; i < jointPeaks.Length; i++)
        {
            Vector2 jointPosition = Camera.main.WorldToScreenPoint(jointPeaks[i].arc.transform.position);

            GUI.Label(new Rect(jointPosition.x, Screen.height - jointPosition.y, 50, 25), jointPeaks[i].jointAngle.ToString("N0") + "°", guiStyle);
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
        frameViewArc.gameObject.SetActive(true);
        modelArc.gameObject.SetActive(true);
    }

    #endregion

    #region OnBodyExit

    void OnBodyExit()
    {
        stickman.gameObject.SetActive(false);
        frameViewArc.gameObject.SetActive(false);
        modelArc.gameObject.SetActive(false);
    }

    #endregion
}