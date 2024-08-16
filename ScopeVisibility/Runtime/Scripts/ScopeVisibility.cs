using UnityEngine;

public class ScopeVisibility : MonoBehaviour
{
    public enum VisibilityShape
    {
        Square,
        Circle,
        None
    }

    [Header("Center: (x,z)")]
    public Vector2 center;
    public float radius;
    [Range(0f, 1f)]
    public float angle;
    public VisibilityShape visibilityShape;

    void OnEnable()
    {
        UpdateKeyWords();
        UpdateParams();
    }
    void OnValidate()
    {
        UpdateKeyWords();
        UpdateParams();
    }
    void UpdateKeyWords()
    {
        switch (visibilityShape)
        {
            case VisibilityShape.Square:
                Shader.EnableKeyword("_VISIBILITY_SQUARE");
                Shader.DisableKeyword("_VISIBILITY_CIRCLE");
                break;
            case VisibilityShape.Circle:
                Shader.EnableKeyword("_VISIBILITY_CIRCLE");
                Shader.DisableKeyword("_VISIBILITY_SQUARE");
                break;
            default:
                Shader.DisableKeyword("_VISIBILITY_SQUARE");
                Shader.DisableKeyword("_VISIBILITY_CIRCLE");
                break;
        }
    }
    void UpdateParams()
    {
        var param = new Vector4(center.x, center.y, radius, angle);
        Shader.SetGlobalVector("_VisibilityParam", param);
    }
}