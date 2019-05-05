using LightBuzz.Vitruvius;
using UnityEngine;
using UnityEngine.UI;

public class VitruviusSensorInfo : MonoBehaviour
{
    public MeasurementSystem measurements = MeasurementSystem.Metric;
    public Text textX;
    public Text textY;
    public Text textZ;
    public Text textW;
    public Text textFieldOfView;
    public Text textHeight;
    public Text textMinMaxHeight;
    public Text textTilt;

    public Floor Floor { get; set; }

    private void Update()
    {
        textX.text = "X: " + (Floor != null ? (Floor.X.ToString("N2")) : "N/A");

        textY.text = "Y: " + (Floor != null ? Floor.Y.ToString("N2") : "N/A");

        textZ.text = "Z: " + (Floor != null ? Floor.Z.ToString("N2") : "N/A");

        textW.text = "W: " + (Floor != null ? Floor.W.ToString("N2") : "N/A");

        textFieldOfView.text = Floor != null ? Floor.FieldOfView.ToString() : "N/A";

        textHeight.text = Floor != null ?
            measurements == MeasurementSystem.Metric ?
            Floor.Height.ToString("N2") + "m" :
            Floor.Height.ToInches().ToString("N2") + "in"
            : "N/A";

        textMinMaxHeight.text = Floor != null ?
            measurements == MeasurementSystem.Metric ?
            "(" + Floor.MinHeight.ToString("N2") + "m - " + Floor.MaxHeight.ToString("N2") + "m)" :
            "(" + Floor.MinHeight.ToInches().ToString("N2") + "in - " + Floor.MaxHeight.ToInches().ToString("N2") + "in)"
            : "N/A";

        textTilt.text = Floor != null ? Floor.Tilt.ToString("N0") + "°" : "N/A";
    }
}
