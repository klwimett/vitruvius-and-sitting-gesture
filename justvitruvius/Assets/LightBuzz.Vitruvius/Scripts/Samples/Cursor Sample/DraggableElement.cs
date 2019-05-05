using System;
using UnityEngine;

public class DraggableElement : KinectButton
{
    public event EventHandler Dragging;

    protected override void OnDragging()
    {
        transform.position = cursorInfo.currentPosition;
        
        if (Dragging != null)
        {
            Dragging.Invoke(this, new EventArgs());
        }
    }

    protected override void OnOutsideDragging()
    {
        OnDragging();
    }
}