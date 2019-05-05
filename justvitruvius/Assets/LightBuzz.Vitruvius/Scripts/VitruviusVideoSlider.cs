using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class VitruviusVideoSlider : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public delegate void ValueChangedHandler(float value);
    public event ValueChangedHandler ValueChanged;

    public delegate void PointerDownHandler();
    public event PointerDownHandler PointerDown;

    public delegate void PointerUpHandler();
    public event PointerUpHandler PointerUp;

    public float value
    {
        get
        {
            return slider.value;
        }
        set
        {
            slider.value = value;
        }
    }

    private Slider slider;

    private void Awake()
    {
        slider = GetComponent<Slider>();
    }

    private void Start()
    {
        slider.onValueChanged.AddListener(Slider_OnValueChanged);
    }

    private void OnApplicationQuit()
    {
        slider.onValueChanged.RemoveAllListeners();
    }

    private void Slider_OnValueChanged(float value)
    {
        if (ValueChanged != null)
        {
            ValueChanged.Invoke(value);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (PointerDown != null)
            {
                PointerDown.Invoke();
            }
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (PointerUp != null)
            {
                PointerUp.Invoke();
            }
        }
    }
}