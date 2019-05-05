using UnityEngine;
using System.Collections;

public class BallSpinning : MonoBehaviour
{
    public float spinningSpeed;

    void Update()
    {
        transform.Rotate(0, spinningSpeed, 0);
    }
}