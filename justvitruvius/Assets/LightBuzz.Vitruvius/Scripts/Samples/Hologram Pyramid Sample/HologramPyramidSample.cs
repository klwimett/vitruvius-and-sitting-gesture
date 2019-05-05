using UnityEngine;
using LightBuzz.Vitruvius;
using Windows.Kinect;

public class HologramPyramidSample : VitruviusSample
{
    #region Variables
    
    public Material frameMaterial;

    #endregion

    #region Reserved methods // Update

    void Update()
    {
        if (colorFrameReader != null && depthFrameReader != null && bodyIndexFrameReader != null)
        {
            using (ColorFrame colorFrame = colorFrameReader.AcquireLatestFrame())
            using (DepthFrame depthFrame = depthFrameReader.AcquireLatestFrame())
            using (BodyIndexFrame bodyIndexFrame = bodyIndexFrameReader.AcquireLatestFrame())
            {
                if (colorFrame != null && depthFrame != null && bodyIndexFrame != null)
                {
                    frameMaterial.mainTexture = colorFrame.GreenScreen(depthFrame, bodyIndexFrame);
                }
            }
        }
    }

    #endregion
}