using UnityEngine;
using System.Collections;

public class ButtonGauge : MonoBehaviour
{
    public Animator animator;
    public AngleArc arc;
    public Coroutine coroutine = null;

    public float GaugeTime
    {
        get
        {
            return Mathf.Clamp01(gameObject.activeSelf ? animator.GetCurrentAnimatorStateInfo(0).normalizedTime : 1);
        }
    }

    public void Show()
    {
        if (!gameObject.activeSelf)
        {
            arc.Angle = 0;
            gameObject.SetActive(true);
        }
    }

    public void Hide()
    {
        if (gameObject.activeSelf)
        {
            gameObject.SetActive(false);
        }
    }
}