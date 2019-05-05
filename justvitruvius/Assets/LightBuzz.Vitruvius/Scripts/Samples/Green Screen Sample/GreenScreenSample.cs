using LightBuzz.Vitruvius;
using Windows.Kinect;

public class GreenScreenSample : VitruviusSample
{
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
                    frameView.FrameTexture = colorFrame.GreenScreen(depthFrame, bodyIndexFrame);
                }
            }
        }
    }

    #endregion
}