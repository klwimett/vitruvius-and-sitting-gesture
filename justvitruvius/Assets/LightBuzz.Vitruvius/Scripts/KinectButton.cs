using UnityEngine;

public class KinectButton : MonoBehaviour
{
    #region Variables and Properties

    static KinectButton active = null;

    [SerializeField]
#if UNITY_EDITOR
    new
#endif
    Collider2D collider;
    public Collider2D Collider
    {
        get
        {
            if (collider == null)
            {
                collider = GetComponent<Collider2D>();
            }

            return collider;
        }
    }

    public bool isDraggable = false;
    float dragStartTime = 0;
    public float dragTime = 0.2f;
    bool isHovering = false;
    bool isDragging = false;
    bool wasDragging = false;
    bool isPressing = false;
    bool wasHovering = false;
    bool canceled = false;

    CursorState cursorState = CursorState.None;

    protected CursorInfo cursorInfo;

    #endregion

    #region Validate and Trigger methods

    public void ValidateButton(CursorInfo cursorInfo)
    {
        canceled = false;

        if ((this.cursorInfo != null && this.cursorInfo != cursorInfo) &&
            (isHovering || isDragging || isPressing))
        {
            return;
        }

        this.cursorInfo = cursorInfo;
        cursorState = cursorInfo.cursorState;

        if (Collider == null || (active != null && active != this)) return;

        isDragging = false;

        if (isDraggable)
        {
            if (cursorState == CursorState.Down)
            {
                dragStartTime = Time.timeSinceLevelLoad + dragTime;
            }
            else if (cursorState == CursorState.Pressing)
            {
                if (dragStartTime < Time.timeSinceLevelLoad)
                {
                    isDragging = true;
                }
            }
        }

        isHovering = IsContained(ref cursorInfo.currentPosition);

        ValidateEvents();
    }

    void ValidateEvents()
    {
        if (isHovering)
        {
            OnPersistentHovering();

            if (canceled)
            {
                return;
            }

            if (cursorState != CursorState.Up)
            {
                if (!isDragging)
                {
                    if (!wasHovering)
                    {
                        OnHoverEnter();

                        if (canceled)
                        {
                            return;
                        }
                    }
                    else
                    {
                        OnHoverStay();

                        if (canceled)
                        {
                            return;
                        }
                    }
                }

                if (cursorState == CursorState.Down)
                {
                    active = this;
                    isPressing = true;
                }
            }
        }
        else if (wasHovering)
        {
            if (cursorState == CursorState.None)
            {
                OnNormal();

                if (canceled)
                {
                    return;
                }
            }

            OnHoverExit();

            if (canceled)
            {
                return;
            }
        }

        if (isPressing)
        {
            if (isDragging)
            {
                if (!wasDragging)
                {
                    OnDraggingStarted();

                    if (canceled)
                    {
                        return;
                    }
                }
                else if (!isHovering)
                {
                    OnOutsideDragging();

                    if (canceled)
                    {
                        return;
                    }
                }
                else
                {
                    OnDragging();

                    if (canceled)
                    {
                        return;
                    }
                }
            }

            if (isHovering)
            {
                if (cursorState == CursorState.Down)
                {
                    OnPreClick();

                    if (canceled)
                    {
                        return;
                    }
                }
                else if (cursorState == CursorState.Up)
                {
                    OnClick();

                    if (canceled)
                    {
                        return;
                    }
                }
            }
        }

        if (cursorState == CursorState.Up)
        {
            if (!isHovering && isPressing)
            {
                OnNormal();

                if (canceled)
                {
                    return;
                }
            }

            active = null;
            isPressing = false;
        }

        wasHovering = isHovering;
        wasDragging = isDragging;
    }

    public void TriggerHover()
    {
        isHovering = true;
        isDragging = false;
        wasHovering = false;

        ValidateEvents();
    }

    public void TriggerClick()
    {
        CancelClick();

        isHovering = true;
        isDragging = isDraggable;
        cursorState = CursorState.Down;

        ValidateEvents();
    }

    public void TriggerNormal()
    {
        isHovering = false;
        isPressing = true;
        cursorState = CursorState.Up;

        ValidateEvents();
    }

    #endregion

    #region Overridable events

    protected virtual void OnPersistentHovering()
    {

    }

    protected virtual void OnHoverEnter()
    {

    }

    protected virtual void OnHoverStay()
    {

    }

    protected virtual void OnHoverExit()
    {

    }

    protected virtual void OnNormal()
    {

    }

    protected virtual void OnPreClick()
    {

    }

    protected virtual void OnClick()
    {

    }

    protected virtual void OnDraggingStarted()
    {

    }

    protected virtual void OnDragging()
    {

    }

    protected virtual void OnOutsideDragging()
    {

    }

    #endregion

    #region IsContained

    public bool IsContained(ref Vector2 inputPoint)
    {
        if (Collider == null)
        {
            return false;
        }

        return Collider.OverlapPoint(inputPoint);
    }

    #endregion

    #region Static methods

    public static void CancelClick()
    {
        if (active != null)
        {
            active.canceled = true;
            active.isPressing = false;
            active = null;
        }
    }

    #endregion
}