using UnityEngine;
using System.Collections;
using Windows.Kinect;

public class CursorInfo : MonoBehaviour
{
    #region Variables and Properties

    public bool Active
    {
        get
        {
            return gameObject.activeSelf;
        }
    }

#if UNITY_EDITOR
    new
#endif
        public SpriteRenderer renderer;
    public Sprite openState;
    public Sprite closedState;

    public ButtonGauge gauge;

    [HideInInspector]
    public Vector2 previousPosition;
    [HideInInspector]
    public Vector2 currentPosition;

    public Vector2 Direction
    {
        get
        {
            return currentPosition - previousPosition;
        }
    }

    [HideInInspector]
    public HandState prevHandState = HandState.NotTracked;
    [HideInInspector]
    public CursorState cursorState = CursorState.None;

    #endregion

    #region Show

    public void Show()
    {
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }
    }

    #endregion

    #region Hide

    public void Hide()
    {
        if (gameObject.activeSelf)
        {
            gameObject.SetActive(false);
            gauge.Hide();
        }
    }

    #endregion

    #region UpdateState

    public void UpdateState(HandState state)
    {
        if (state != prevHandState)
        {
            if (state == HandState.Open)
            {
                renderer.sprite = openState;
            }
            else if (state == HandState.Closed)
            {
                renderer.sprite = closedState;
            }

            prevHandState = state;
        }

        if (state == HandState.Open)
        {
            if (cursorState == CursorState.Pressing)
            {
                cursorState = CursorState.Up;
            }
            else if (cursorState == CursorState.Up)
            {
                cursorState = CursorState.None;
            }
        }
        else if (state == HandState.Closed)
        {
            if (cursorState == CursorState.None)
            {
                cursorState = CursorState.Down;
            }
            else if (cursorState == CursorState.Down)
            {
                cursorState = CursorState.Pressing;
            }
        }
    }

    #endregion

    #region ResetStates

    public void ResetStates()
    {
        prevHandState = HandState.NotTracked;
        cursorState = CursorState.None;
    }

    #endregion
}