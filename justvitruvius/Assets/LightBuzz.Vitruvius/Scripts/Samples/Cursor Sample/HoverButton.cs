using UnityEngine;
using System;

public class HoverButton : KinectButton
{
    public event EventHandler Click;

    protected override void OnHoverExit()
    {
        KinectUI.Instance.HideGauge(cursorInfo);
    }

    protected override void OnHoverStay()
    {
        KinectUI.Instance.ValidateGauge(cursorInfo, OnGaugeEnd);
    }

    void OnGaugeEnd()
    {
        Debug.Log("Clicked on " + name);

        if (Click != null)
        {
            Click.Invoke(this, new EventArgs());
        }
    }
}