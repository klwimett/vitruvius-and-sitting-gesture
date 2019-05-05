using UnityEngine;
using System.Collections;
using Windows.Kinect;
using System;
using System.Collections.Generic;

[System.Serializable]
public class StickmanController
{
    #region Variables and Properties

    bool initialized = false;

    [SerializeField]
    List<StickmanJoint> joints = new List<StickmanJoint>();
    public List<StickmanJoint> Joints
    {
        get
        {
            return joints;
        }
    }

    public Transform eyeLeft;
    public Transform eyeRight;
    public LineRenderer eyesLine;

    public StickmanJoint this[JointType index]
    {
        get
        {
            return joints.Find(x => x.JointType == index);
        }
    }

    public StickmanJoint this[int index]
    {
        get
        {
            return joints[index];
        }
    }

    #endregion

    #region Initialize

    public void Initialize()
    {
        if (initialized) return;

        for (int i = 0; i < joints.Count; i++)
        {
            joints[i].SetJointType();
        }

        initialized = true;
    }

    #endregion
}

[System.Serializable]
public class StickmanJoint
{
    #region Variables and Properties

    public Transform transform;
    public LineRenderer line;

    JointType jointType;
    public JointType JointType
    {
        get
        {
            return jointType;
        }
    }

    JointType link;
    public JointType Link
    {
        get
        {
            return link;
        }
    }

    public Vector3 Position
    {
        get
        {
            return transform.position;
        }
        set
        {
            transform.position = value;
        }
    }

    #endregion

    #region SetJointType

    public void SetJointType()
    {
        TryParse(transform.name, out jointType);
        TryParse(transform.parent.name, out link);
    }

    #endregion

    #region TryParse

    static bool TryParse<TEnum>(string value, out TEnum result) where TEnum : struct
    {
        try
        {
            result = (TEnum)Enum.Parse(typeof(TEnum), value);
        }
        catch
        {
            result = default(TEnum);
            return false;
        }

        return true;
    }

    #endregion
}