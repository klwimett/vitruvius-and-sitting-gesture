using UnityEngine;
using System.Collections;

public class HologramAdjustment : MonoBehaviour
{
    public Transform top;
    public Transform bottom;
    public Transform left;
    public Transform right;

    public float position = 3.1f;
    public float scale = 7.9f;

    void Update()
    {
        if (Input.GetKey(KeyCode.KeypadPlus))
        {
            scale += Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.KeypadMinus))
        {
            scale -= Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.UpArrow))
        {
            position += Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.DownArrow))
        {
            position -= Time.deltaTime;
        }
        
        top.position = new Vector3(0, position, 0);
        bottom.position = new Vector3(0, -position, 0);
        left.position = new Vector3(-position, 0, 0);
        right.position = new Vector3(position, 0, 0);

        Vector3 scaleVector = new Vector3(scale, scale, 1);
        top.localScale = scaleVector;
        bottom.localScale = scaleVector;
        left.localScale = scaleVector;
        right.localScale = scaleVector;
    }
}