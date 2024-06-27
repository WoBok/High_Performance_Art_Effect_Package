using UnityEngine;

public class DissolutionController : MonoBehaviour
{
    [Range(0f, 1f)]
    public float dissValue;
    public Material material;
    public Texture2D texture;
    Color m_Color;
#if UNITY_EDITOR
    bool m_IsSetTexture;
#endif
    void OnEnable()
    {
        m_Color = material.GetColor("_OutlineColor");
        material.SetTexture("_DissolutionMap", texture);
        material.EnableKeyword("_ALPHACLIPING_ON");
#if UNITY_EDITOR
        m_IsSetTexture = false;
#endif
    }
    void Update()
    {
        UpdateMaterial();
    }
#if UNITY_EDITOR
    void OnValidate()
    {
        if (!m_IsSetTexture)
        {
            m_IsSetTexture = true;
            material.SetTexture("_DissolutionMap", texture);
        }
        UpdateMaterial();
    }
#endif
    void UpdateMaterial()
    {
        material.SetFloat("_Cutoff", dissValue);
        m_Color.a = 1 - dissValue;
        material.SetColor("_OutlineColor", m_Color);
    }
    void OnDisable()
    {
        material.DisableKeyword("_ALPHACLIPING_ON");
        material.SetFloat("_Cutoff", 0);
        m_Color.a = 1;
        material.SetColor("_OutlineColor", m_Color);
        material.SetTexture("_DissolutionMap", null);
#if UNITY_EDITOR
        m_IsSetTexture = false;
#endif
    }
}