using UnityEngine;
using LightBuzz.Vitruvius;

[ExecuteInEditMode]
public class FrameView : MonoBehaviour
{
    #region Constants

    readonly float COLOR_X_RATIO = Constants.DEFAULT_COLOR_HEIGHT / Constants.DEFAULT_COLOR_WIDTH;
    readonly float COLOR_Y_RATIO = Constants.DEFAULT_DEPTH_WIDTH / Constants.DEFAULT_DEPTH_HEIGHT;

    #endregion

    #region Variables and Properties

    [SerializeField]
    Material material = null;
#if UNITY_EDITOR
    [HideInInspector, SerializeField]
    Material prevMaterial = null;
#endif
    public Material Material
    {
        get
        {
            return material;
        }
        set
        {
            if (material != value)
            {
                material = value;
#if UNITY_EDITOR
                prevMaterial = material;
#endif
                Refresh();
            }
        }
    }

    [SerializeField]
    bool mirroredView = true;
#if UNITY_EDITOR
    [HideInInspector, SerializeField]
    bool prevMirroredView = true;
#endif
    public bool MirroredView
    {
        get
        {
            return mirroredView;
        }
        set
        {
            if (mirroredView != value)
            {
                mirroredView = value;
#if UNITY_EDITOR
                prevMirroredView = value;
#endif
                RefreshMirrorView();
                Refresh();
            }
        }
    }

    [SerializeField]
    float viewScale = 1;
#if UNITY_EDITOR
    [HideInInspector, SerializeField]
    float prevViewScale = 1;
#endif
    public float ViewScale
    {
        get
        {
            return viewScale;
        }
        set
        {
            if (viewScale != value)
            {
                viewScale = value;
#if UNITY_EDITOR
                prevViewScale = viewScale;
#endif
                Refresh();
            }
        }
    }

#if UNITY_EDITOR
    [HideInInspector, SerializeField]
    float prevXRatio = 0;
#endif
    public float XRatio { get; private set; }

#if UNITY_EDITOR
    [HideInInspector, SerializeField]
    float prevYRatio = 0;
#endif
    public float YRatio { get; private set; }

    [SerializeField]
    bool keepScreenRatio = true;
#if UNITY_EDITOR
    [HideInInspector, SerializeField]
    bool prevKeepScreenRatio = true;
#endif
    public bool KeepScreenRatio
    {
        get
        {
            return keepScreenRatio;
        }
        set
        {
            if (keepScreenRatio != value)
            {
                keepScreenRatio = value;
#if UNITY_EDITOR
                prevKeepScreenRatio = keepScreenRatio;
#endif
                if (!keepScreenRatio)
                {
                    TextureRatioLessThanScreen = false;
                }

                Refresh();
            }
        }
    }

    public float ScreenRatio { get; private set; }

    public bool TextureRatioLessThanScreen { get; private set; }

    [HideInInspector, SerializeField]
    int currentScreenWidth = 0;
    [HideInInspector, SerializeField]
    int currentScreenHeight = 0;

    public Texture2D FrameTexture
    {
        get
        {
            return material == null ? null : (Texture2D)material.mainTexture;
        }
        set
        {
            if (material == null) return;

            if (value == null && material.mainTexture != null)
            {
                Destroy(material.mainTexture);
            }

            material.mainTexture = value;

            RefreshMirrorView();

            if (value != null)
            {
                float xRatio = value.height / (float)value.width;
                float yRatio = value.width / (float)value.height;

                bool changed = xRatio != XRatio || yRatio != YRatio;

                XRatio = xRatio;
                YRatio = yRatio;

                if (changed)
                {
                    Refresh();
                }
            }
            else if (XRatio != COLOR_X_RATIO)
            {
                XRatio = COLOR_X_RATIO;
                YRatio = COLOR_Y_RATIO;

                Refresh();
            }
        }
    }

    #endregion

    #region Reserved methods // Awake - OnDestroy - Update

    void Awake()
    {
        RefreshMirrorView();
        Refresh();
    }

    void OnDestroy()
    {
        if (material != null)
        {
            FrameTexture = null;
        }
    }

    void Update()
    {
        bool refreshView = false;

        if (Screen.width != currentScreenWidth || Screen.height != currentScreenHeight)
        {
            currentScreenWidth = Screen.width;
            currentScreenHeight = Screen.height;

            refreshView = true;
        }

#if UNITY_EDITOR
        if (prevMaterial != material)
        {
            prevMaterial = material;
            RefreshMirrorView();
            refreshView = true;
        }

        if (prevMirroredView != mirroredView)
        {
            prevMirroredView = mirroredView;
            RefreshMirrorView();
            refreshView = true;
        }

        if (prevViewScale != viewScale)
        {
            prevViewScale = viewScale;
            refreshView = true;
        }

        if (prevXRatio != XRatio)
        {
            prevXRatio = XRatio;
            refreshView = true;
        }

        if (prevYRatio != YRatio)
        {
            prevYRatio = YRatio;
            refreshView = true;
        }

        if (prevKeepScreenRatio != keepScreenRatio)
        {
            prevKeepScreenRatio = keepScreenRatio;
            refreshView = true;
        }
#endif

        if (refreshView)
        {
            Refresh();
        }
    }

    #endregion

    #region Refresh

    public void Refresh()
    {
        if (!float.IsNaN(viewScale) && !float.IsNaN(XRatio) && !float.IsNaN(YRatio))
        {
            if (keepScreenRatio)
            {
                float screenRatio = Screen.width / (float)Screen.height;

                ScreenRatio = screenRatio / YRatio;

                TextureRatioLessThanScreen = screenRatio > YRatio;

                if (TextureRatioLessThanScreen)
                {
                    if (YRatio == 0)
                    {
                        return;
                    }

                    transform.localScale = new Vector3(viewScale * screenRatio, viewScale * (screenRatio / YRatio), 1);
                }
                else
                {
                    transform.localScale = new Vector3(viewScale * YRatio, viewScale, 1);
                }
            }
            else
            {
                transform.localScale = new Vector3(viewScale * YRatio, viewScale, 1);
            }
        }
    }

    #endregion

    #region RefreshMirrorView

    void RefreshMirrorView()
    {
        if (material == null) return;

        material.mainTextureScale = new Vector2(mirroredView ? 1 : -1, -1);
    }

    #endregion

    #region GetPositionOnFrame

    public Vector2 GetPositionOnFrame(Vector2 point2D)
    {
        if (FrameTexture == null) return point2D;

        Vector2 textureSize = new Vector2(FrameTexture.width, FrameTexture.height);

        if (textureSize.x != Constants.DEFAULT_DEPTH_WIDTH)
        {
            textureSize.Set(Constants.DEFAULT_COLOR_WIDTH, Constants.DEFAULT_COLOR_HEIGHT);
        }

        Vector2 framePosition = transform.position;
        Vector2 frameScale = transform.localScale;
        if (mirroredView)
        {
            frameScale.x = -frameScale.x;
        }

        point2D.Set(GetDelta(textureSize.x * 0.5f, textureSize.x * -0.5f, point2D.x) * frameScale.x + framePosition.x,
            GetDelta(textureSize.y * 0.5f, textureSize.y * -0.5f, point2D.y) * frameScale.y + framePosition.y);

        return point2D;
    }

    float GetDelta(float min, float max, float value)
    {
        return (value - min) / (max - min);
    }

    #endregion
}