using UnityEngine;
using System.Collections;
using Windows.Kinect;
using LightBuzz.Vitruvius;

public enum CursorState { None, Up, Down, Pressing }
public enum HandSide { Auto, Left, Right }

[ExecuteInEditMode]
public class KinectUI : MonoBehaviour
{
    #region Singleton

    static KinectUI instance = null;
    public static KinectUI Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<KinectUI>();
            }

            return instance;
        }
    }

    #endregion

    #region Variables and Properties

    public CursorInfo leftCursorInfo;
    public CursorInfo rightCursorInfo;

    [SerializeField]
    HandSide handSide = HandSide.Auto;
    public HandSide HandSide
    {
        get
        {
            return handSide;
        }
        set
        {
            handSide = value;
        }
    }
    HandSide prevHandSide = HandSide.Left;

    [SerializeField]
    float stillnessThreshold = 0.05f;
    public float StillnessThreshold
    {
        get
        {
            return stillnessThreshold;
        }
    }

    public Vector2 cursorAreaCenter = Vector2.zero;
    public Vector2 cursorAreaSize = Vector2.one;
    Vector2 areaCenter = Vector2.zero;
    Vector2 areaSize = Vector2.zero;
    Vector2 cameraSize = Vector2.one;
#if UNITY_EDITOR
    Color drawAreaColor = new Color(1, 1, 1, 0.5f);
#endif

    bool isVideoPlayback = false;

    FrameView frameView;

    KinectButton[] buttons = null;

    public delegate void OnGaugeEnd();

    #endregion

    #region Reserved methods // Awake - Update - OnDrawGizmos

    void Awake()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying) return;
#endif
        buttons = FindObjectsOfType<KinectButton>();
    }

    void Update()
    {
        float ratio = Screen.width / (float)Screen.height;
        cameraSize.y = Camera.main.orthographicSize * 2f;
        cameraSize.x = cameraSize.y * ratio;
        areaSize.Set(cameraSize.x * cursorAreaSize.x, cameraSize.y * cursorAreaSize.y);
        areaCenter.Set(cameraSize.x * 0.5f * cursorAreaCenter.x, cameraSize.y * 0.5f * cursorAreaCenter.y);
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.color = drawAreaColor;
        Gizmos.DrawCube(areaCenter, areaSize);
    }
#endif

    #endregion

    #region UpdateCursor

    public void UpdateCursor(BodyWrapper body, FrameView frameView, bool isVideoPlayback = false)
    {
        this.frameView = frameView;
        this.isVideoPlayback = isVideoPlayback;

        bool hasBody = body != null;

        HandSide handSide = this.handSide;
        if (handSide == HandSide.Auto && hasBody)
        {
            handSide = body.Joints[JointType.HandLeft].Position.Z > body.Joints[JointType.HandRight].Position.Z ? HandSide.Left : HandSide.Right;
        }

        if (prevHandSide != handSide)
        {
            if (prevHandSide == HandSide.Left)
            {
                leftCursorInfo.ResetStates();
            }
            else
            {
                rightCursorInfo.ResetStates();
            }

            prevHandSide = handSide;

            KinectButton.CancelClick();
        }

        if (handSide == HandSide.Left)
        {
            if (hasBody && body.HandLeftState != HandState.NotTracked)
            {
                leftCursorInfo.Show();
            }
            else
            {
                leftCursorInfo.Hide();
            }

            if (leftCursorInfo.Active)
            {
                leftCursorInfo.UpdateState(body.HandLeftState);
                RefreshCursorPosition(body, HandSide.Left, leftCursorInfo);
            }
        }
        else
        {
            leftCursorInfo.Hide();
        }

        if (handSide == HandSide.Right)
        {
            if (hasBody && body.HandRightState != HandState.NotTracked)
            {
                rightCursorInfo.Show();
            }
            else
            {
                rightCursorInfo.Hide();
            }

            if (rightCursorInfo.Active)
            {
                rightCursorInfo.UpdateState(body.HandRightState);
                RefreshCursorPosition(body, HandSide.Right, rightCursorInfo);
            }
        }
        else
        {
            rightCursorInfo.Hide();
        }
    }

    #endregion

    #region RefreshCursorPosition

    void RefreshCursorPosition(BodyWrapper body, HandSide handSide, CursorInfo cursorInfo)
    {
        JointType handSideType = handSide == HandSide.Right ? JointType.HandLeft : JointType.HandRight;
        JointType handTipSideType = handSide == HandSide.Right ? JointType.HandTipLeft : JointType.HandTipRight;

        cursorInfo.currentPosition = isVideoPlayback ? body.Map2D[handSideType].ToVector2() : body.Joints[handSideType].Position.ToPoint(Visualization.Color);

        Vector2 handTipPos = isVideoPlayback ? body.Map2D[handTipSideType].ToVector2() : body.Joints[handTipSideType].Position.ToPoint(Visualization.Color);
        cursorInfo.transform.up = new Vector3(handTipPos.x - cursorInfo.currentPosition.x, cursorInfo.currentPosition.y - handTipPos.y, 0);

        cursorInfo.currentPosition = frameView.GetPositionOnFrame(cursorInfo.currentPosition);

        float x = (cursorInfo.currentPosition.x + cameraSize.x * 0.5f) / cameraSize.x;
        float y = (cursorInfo.currentPosition.y + cameraSize.y * 0.5f) / cameraSize.y;

        cursorInfo.currentPosition.Set(x * areaSize.x - areaSize.x * 0.5f + areaCenter.x, y * areaSize.y - areaSize.y * 0.5f + areaCenter.y);
        cursorInfo.currentPosition = Vector3.Lerp(cursorInfo.transform.position, cursorInfo.currentPosition, 0.35f);
        cursorInfo.transform.position = cursorInfo.currentPosition;

        UpdateButtons(cursorInfo);

        cursorInfo.previousPosition = cursorInfo.currentPosition;
    }

    #endregion

    #region UpdateButtons

    void UpdateButtons(CursorInfo cursorInfo)
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].ValidateButton(cursorInfo);
        }
    }

    #endregion

    #region ValidateGauge

    public void ValidateGauge(CursorInfo cursor, OnGaugeEnd callback)
    {
        if (cursor.Direction.magnitude < stillnessThreshold)
        {
            if (cursor.gauge.coroutine == null)
            {
                cursor.gauge.Show();
                cursor.gauge.coroutine = StartCoroutine(Coroutine_GaugeLoad(cursor, callback));
            }
        }
        else
        {
            HideGauge(cursor);
        }
    }

    #endregion

    #region HideGauge

    public void HideGauge(CursorInfo cursor)
    {
        if (cursor.gauge.coroutine != null)
        {
            StopCoroutine(cursor.gauge.coroutine);
            cursor.gauge.coroutine = null;
        }

        cursor.gauge.Hide();
    }

    #endregion

    #region Coroutine_GaugeLoad

    IEnumerator Coroutine_GaugeLoad(CursorInfo cursor, OnGaugeEnd callback)
    {
        yield return new WaitUntil(() => cursor.gauge.GaugeTime == 1);

        if (callback != null)
        {
            callback();
        }

        cursor.gauge.Hide();
    }

    #endregion
}