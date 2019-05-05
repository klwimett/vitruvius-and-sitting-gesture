using UnityEngine;
using System.Collections;
using Windows.Kinect;
using LightBuzz.Vitruvius;

public class RealDepthSample : VitruviusSample
{
    #region Variables

    readonly int DEPTH_WIDTH = Constants.DEFAULT_DEPTH_WIDTH;
    readonly int DEPTH_HEIGHT = Constants.DEFAULT_DEPTH_HEIGHT;

    ushort[] depthFrameData = null;
    byte[] textureData = null;
    Texture2D depthTexture = null;

    #endregion

    #region Reserved methods // Awake - Update

    protected override void Awake()
    {
        base.Awake();

        depthFrameData = new ushort[DEPTH_WIDTH * DEPTH_HEIGHT];

        depthTexture = new Texture2D(DEPTH_WIDTH, DEPTH_HEIGHT, TextureFormat.RGBA32, false);
        textureData = new byte[DEPTH_WIDTH * DEPTH_HEIGHT * 4];

        frameView.FrameTexture = depthTexture;
    }

    void Update()
    {
        UpdateDepthFrame(false);
    }

    #endregion

    #region OnDepthFrameReceived

    protected override void OnDepthFrameReceived(DepthFrame frame)
    {
        frame.CopyFrameDataToArray(depthFrameData);

        double maxDepth = double.MinValue;

        // Find the farthest point
        for (int y = 0; y < DEPTH_HEIGHT; y++)
        {
            for (int x = 0; x < DEPTH_WIDTH; x++)
            {
                int index = y * DEPTH_WIDTH + x;

                double depth = depthFrameData[index];

                if (depth > maxDepth)
                {
                    maxDepth = depth;
                }
            }
        }

        // From the farthest point we set the color;
        for (int y = 0; y < DEPTH_HEIGHT; y++)
        {
            for (int x = 0; x < DEPTH_WIDTH; x++)
            {
                int index = y * DEPTH_WIDTH + x;

                byte color = (byte)(depthFrameData[index] / maxDepth * 255);

                textureData[index * 4] = color;
                textureData[index * 4 + 1] = color;
                textureData[index * 4 + 2] = color;
                textureData[index * 4 + 3] = 255;
            }
        }

        depthTexture.LoadRawTextureData(textureData);
        depthTexture.Apply(false);
    }

    #endregion
}