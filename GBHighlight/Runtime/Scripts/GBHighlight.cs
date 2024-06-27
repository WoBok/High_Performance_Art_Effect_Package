using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GBHighlight : MonoBehaviour
{
    static GBHighlight s_Instance;
    static GBHighlight Instance
    {
        get
        {
            if (s_Instance == null)

            {
                var obj = new GameObject("GBHighlight");
                s_Instance = obj.AddComponent<GBHighlight>();

                s_Instance.Init();
            }

            return s_Instance;
        }
        set => s_Instance = value;
    }

    GBHighlightRenderPass m_GBHighlightRenderPass;

    List<GameObject> m_CachedGameObjects;
    List<int> m_CachedLayerIndex;

    int m_Layer = -1;

    Coroutine m_Coroutine;

    Camera m_OverlayCamera;

    void Init()
    {
        Instance.m_Layer = LayerMask.NameToLayer("GBHighlight");

        Instance.m_CachedGameObjects = new List<GameObject>();
        Instance.m_CachedLayerIndex = new List<int>();

        Instance.m_GBHighlightRenderPass = new GBHighlightRenderPass();
        Instance.m_GBHighlightRenderPass.renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
    }
    void BeginCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        if (camera.tag != "MainCamera") return;
        if (camera == null) return;
        var data = camera.GetUniversalAdditionalCameraData();
        if (data == null) return;
        if (Instance.m_GBHighlightRenderPass != null)
            data.scriptableRenderer.EnqueuePass(Instance.m_GBHighlightRenderPass);
    }
    /// <summary>
    /// 模式1
    /// </summary>
    /// <param name="gameObjects">需要高亮物体的数组</param>
    /// <param name="duration">持续时间</param>
    public static void Open(GameObject[] gameObjects, float duration = 1)
    {
        Instance.Open(gameObjects);

        Instance.DoPattern1Anim(duration);
    }
    /// <summary>
    /// 模式2
    /// </summary>
    /// <param name="gameObjects">需要高亮物体的数组</param>
    /// <param name="positionWS">扩散效果起始的位置（世界坐标）</param>
    /// <param name="duration">持续时间</param>
    public static void Open(GameObject[] gameObjects, Vector3 positionWS, float duration = 1)
    {
        Instance.Open(gameObjects);

        Instance.DoPattern2Anim(duration, positionWS);
    }
    void Open(GameObject[] gameObjects)
    {
        Close();

        if (Instance.m_Layer == -1)
        {
            Debug.LogError("未找到Layer：GBHighlight，请添加后重试");
            return;
        }

        Instance.m_CachedGameObjects.AddRange(gameObjects);

        Instance.ChangeLayer(gameObjects);

        Instance.AddOverlayCamera();

        RenderPipelineManager.beginCameraRendering += Instance.BeginCameraRendering;
    }
    public static void AddGameObjects(GameObject[] gameObjects)
    {
        foreach (var gameObject in gameObjects)
        {
            if (!Instance.m_CachedGameObjects.Contains(gameObject))
            {
                Instance.m_CachedGameObjects.Add(gameObject);
                Instance.ChangeLayer(gameObject.transform);
            }
        }
    }
    void DoPattern1Anim(float duration)
    {
        Instance.m_GBHighlightRenderPass.SetPattern(1);

        if (Instance.m_Coroutine != null)
            StopCoroutine(Instance.m_Coroutine);
        Instance.m_Coroutine = StartCoroutine(Instance.DoPattern1Param(duration));
    }
    IEnumerator DoPattern1Param(float duration)
    {
        float value = 1;
        while (value >= 0)
        {
            value -= Time.deltaTime / duration;
            Instance.m_GBHighlightRenderPass.SetMaterialParama(value);
            yield return null;
        }
    }
    void DoPattern2Anim(float duration, Vector3 pos)
    {
        Instance.m_GBHighlightRenderPass.SetPattern(2);

        pos = Camera.main.WorldToScreenPoint(pos);
        pos.x /= Screen.width;
        pos.y /= Screen.height;
        Instance.m_GBHighlightRenderPass.SetObjPos(pos);

        if (Instance.m_Coroutine != null)
            StopCoroutine(Instance.m_Coroutine);
        Instance.m_Coroutine = StartCoroutine(Instance.DoPattern2Param(duration));
    }
    IEnumerator DoPattern2Param(float duration)
    {
        float value = 0;
        while (value <= 3)
        {
            value += Time.deltaTime / duration * 3;
            Instance.m_GBHighlightRenderPass.SetMaterialParama(value);
            yield return null;
        }
    }
    public static void Close()
    {
        RenderPipelineManager.beginCameraRendering -= Instance.BeginCameraRendering;

        Instance.RemoveOverlayCamera();

        Instance.RevertLayer();

        Instance.Destroy();
    }
    void Destroy()
    {
        StopAllCoroutines();
        Destroy(Instance.gameObject);
        Instance.m_CachedGameObjects = null;
        Instance.m_CachedLayerIndex = null;
        Instance = null;
    }
    void ChangeLayer(GameObject[] gameObjects)
    {
        if (gameObjects != null)
            foreach (var gameObject in gameObjects)
                Instance.ChangeLayer(gameObject.transform);
    }
    void ChangeLayer(Transform trans)
    {
        Instance.m_CachedLayerIndex.Add(trans.gameObject.layer);
        trans.gameObject.layer = Instance.m_Layer;
        foreach (Transform t in trans)
        {
            Instance.ChangeLayer(t);
        }
    }
    void RevertLayer()
    {
        if (Instance.m_CachedGameObjects != null)
            foreach (var gameObject in Instance.m_CachedGameObjects)
                if (gameObject != null)
                    Instance.RevertLayer(gameObject.transform);
    }
    void RevertLayer(Transform trans)
    {
        if (Instance.m_CachedLayerIndex.Count <= 0) return;
        trans.gameObject.layer = Instance.m_CachedLayerIndex[0];
        Instance.m_CachedLayerIndex.RemoveAt(0);
        foreach (Transform t in trans)
        {
            Instance.RevertLayer(t);
        }
    }
    void AddOverlayCamera()
    {
        Instance.RemoveOverlayCamera();

        var cameraObj = new GameObject("Camera");
        Instance.m_OverlayCamera = cameraObj.AddComponent<Camera>();
        Instance.m_OverlayCamera.GetUniversalAdditionalCameraData().renderType = CameraRenderType.Overlay;
        Instance.m_OverlayCamera.cullingMask = 1 << LayerMask.NameToLayer("GBHighlight");
        var mainCamera = Camera.main;
        cameraObj.transform.SetParent(mainCamera.transform, false);
        mainCamera.GetUniversalAdditionalCameraData().cameraStack.Add(Instance.m_OverlayCamera);
    }
    void RemoveOverlayCamera()
    {
        if (m_OverlayCamera != null)
        {
            Camera.main.GetUniversalAdditionalCameraData().cameraStack.Remove(Instance.m_OverlayCamera);
            Destroy(Instance.m_OverlayCamera.gameObject);
            Instance.m_OverlayCamera = null;
        }
    }
    void OnApplicationQuit()
    {
        Close();
    }
}

public class GBHighlightRenderPass : ScriptableRenderPass
{
    Material m_Material;
    string m_ParamName = "_Saturation";

    public GBHighlightRenderPass()
    {
        m_Material = new Material(Shader.Find("GBHighlight/Pattern 1"));
    }

    public void SetPattern(int pattern)
    {
        switch (pattern)
        {
            case 1:
                m_ParamName = "_Saturation";
                m_Material = new Material(Shader.Find("GBHighlight/Pattern 1"));
                break;
            case 2:
                m_ParamName = "_Radius";
                m_Material = new Material(Shader.Find("GBHighlight/Pattern 2"));
                break;
        }
    }

    public void SetObjPos(Vector2 pos)
    {
        m_Material.SetVector("_ObjScreenPos", pos);
    }

    public void SetMaterialParama(float value)
    {
        m_Material.SetFloat(m_ParamName, value);
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        var cmd = CommandBufferPool.Get("GBHightlight");

        var source = renderingData.cameraData.renderer.cameraColorTargetHandle;

        cmd.Blit(source, source, m_Material);

        context.ExecuteCommandBuffer(cmd);

        cmd.Clear();

        CommandBufferPool.Release(cmd);

        #region Error in renderer...
        //当进行Blit操作之后，渲染会出现问题，表现为新渲染出的物体ZTest一直为Always
        //如果将Pass的RenderPassEvent设置为AfterRenderingPostProcessing，即便是不进行Blit也会出现相同情况
        //猜测两者进行了某项相同的操作，如关闭了全局的ZTest，此现象使用URP自带的Render Objects RenderFeature可复现

        //m_SortingCriteria = SortingCriteria.CommonOpaque;
        //m_DrawingSettings = CreateDrawingSettings(new ShaderTagId("UniversalForward"), ref renderingData, m_SortingCriteria);
        //m_FilteringSettings = new FilteringSettings(RenderQueueRange.opaque, m_LayerMask);
        //context.DrawRenderers(renderingData.cullResults, ref m_DrawingSettings, ref m_FilteringSettings);

        //m_SortingCriteria = SortingCriteria.CommonTransparent;
        //m_DrawingSettings = CreateDrawingSettings(new ShaderTagId("UniversalForward"), ref renderingData, m_SortingCriteria);
        //m_FilteringSettings = new FilteringSettings(RenderQueueRange.transparent, m_LayerMask);
        //context.DrawRenderers(renderingData.cullResults, ref m_DrawingSettings, ref m_FilteringSettings); 
        #endregion
    }
}