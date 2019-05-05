using UnityEngine;
using System.Collections;

public enum FilterType
{
    Arrange,
    SmoothPoints,
    SmoothClosePoints,
    SmoothFarPoints,
    SimplifyClosePoints,
    SimplifyFarPoints,
    RemoveSteepCorners,
    RemoveWideCorners,
    CutSparePointsAtEnd
}

public class FilterModuleManager : MonoBehaviour
{
    public bool showContour = true;
    public double filterDistance = 15.0;
    public FilterModule[] modules;

    [System.Serializable]
    public class FilterModule
    {
        public FilterType type = FilterType.Arrange;

        public double dValue1;
        public double dValue2;

        public int intValue;
    }
}