using System.Collections;
using UnityEngine;

public class SeaOfStars : MonoBehaviour
{
    struct Star
    {
        public Vector3 position;
        public Vector3 direction;
        public float movementSpeed;
        public float rotationSpeed;
        public float scale;
    }

    [Header("Resources")]
    public Mesh mesh;
    public Material material;
    public ComputeShader computeShader;

    [Header("Count")]
    [Min(1)]
    public int count = 500;

    [Header("Boundary")]
    public Vector3 boundarySize = new Vector3(5, 0.35f, 5);

    [Header("Speed")]
    public Vector2 movementSpeed = new Vector2(0.03f, 0.06f);
    public Vector2 rotationSpeed = new Vector2(0.05f, 0.1f);
    public Vector2 regenerationDirectionInterval = new Vector2(2, 5);

    [Header("Scale")]
    public Vector2 scale = new Vector2(0.02f, 0.04f);

    ComputeBuffer m_StarsBuffer;
    ComputeBuffer m_DirectionBuffer;
    ComputeBuffer m_argsBuffer;

    uint[] args = new uint[] { 0, 0, 0, 0, 0 };
    int m_KernelIndex;
    int m_CachedCount;

    Coroutine m_Coroutine;

    void OnEnable()
    {
        Init();
        UpdateBuffer();
        UpdateComputeParma();
        StartUpdateDirection();
    }
    void OnDisable()
    {
        Clear();
    }
    void OnValidate()
    {
        UpdateBuffer();
        UpdateComputeParma();
    }
    void Init()
    {
        m_KernelIndex = computeShader.FindKernel("CSMain");

        m_argsBuffer = new ComputeBuffer(1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);

        m_CachedCount = 1;
    }
    void UpdateBuffer()
    {
        UpdateArgsBuffer();

        UpdateStarBuffer();

        UpdateDirectionBuffer();
    }
    void UpdateArgsBuffer()
    {
        if (m_argsBuffer != null)
        {
            if (mesh != null)
            {
                args[0] = mesh.GetIndexCount(0);
                args[1] = (uint)count;
                m_argsBuffer.SetData(args);
            }
        }
    }
    void UpdateStarBuffer()
    {
        if (count <= 0 || m_CachedCount <= 0) return;
        m_CachedCount = count;

        if (m_StarsBuffer != null)
            m_StarsBuffer.Dispose();
        m_StarsBuffer = new ComputeBuffer(count, sizeof(float) * 3 * 2 + sizeof(float) * 1 * 3);

        var stars = new Star[count];
        for (int i = 0; i < count; i++)
        {
            var x = Random.Range(0, boundarySize.x);
            var y = Random.Range(0, boundarySize.y);
            var z = Random.Range(0, boundarySize.z);
            var localPosition = new Vector3(x, y, z);
            stars[i].position = localPosition + transform.position - boundarySize / 2;

            stars[i].direction = GetRandomDirection();

            stars[i].movementSpeed = Random.Range(movementSpeed.x, movementSpeed.y);

            stars[i].rotationSpeed = Random.Range(rotationSpeed.x, rotationSpeed.y);

            stars[i].scale = Random.Range(scale.x, scale.y);
        }
        m_StarsBuffer.SetData(stars);
        computeShader.SetBuffer(m_KernelIndex, "stars", m_StarsBuffer);

        material.SetBuffer("stars", m_StarsBuffer);
    }
    void UpdateDirectionBuffer()
    {
        if (count <= 0 || m_CachedCount <= 0) return;
        m_CachedCount = count;

        if (m_DirectionBuffer != null)
            m_DirectionBuffer.Dispose();
        m_DirectionBuffer = new ComputeBuffer(count, sizeof(float) * 3 * 1);

        var direction = new Vector3[count];
        for (int i = 0; i < count; i++)
            direction[i] = GetRandomDirection();

        m_DirectionBuffer.SetData(direction);
        computeShader.SetBuffer(m_KernelIndex, "targetDirection", m_DirectionBuffer);
    }
    void UpdateComputeParma()
    {
        computeShader.SetVector("containerPosition", transform.position);
        computeShader.SetVector("boundarySize", boundarySize);
    }
    Vector3 GetRandomDirection()
    {
        var x = Random.value * 2 - 1;
        var y = Random.value * 2 - 1;
        var z = Random.value * 2 - 1;
        return new Vector3(x, y, z).normalized;
    }
    void Update()
    {
        DispatchKernel();

        Graphics.DrawMeshInstancedIndirect(mesh, 0, material, new Bounds(Vector3.zero, 10000 * Vector3.one), m_argsBuffer, 0);
    }
    void DispatchKernel()
    {
        computeShader.SetFloat("deltaTime", Time.deltaTime);

        computeShader.Dispatch(m_KernelIndex, (int)Mathf.Ceil((float)count / 128), 1, 1);
    }
    void StartUpdateDirection()
    {
        if (m_Coroutine != null)
            StopCoroutine(m_Coroutine);
        StartCoroutine(UpdateDirection());
    }
    IEnumerator UpdateDirection()
    {
        while (true)
        {
            UpdateDirectionBuffer();

            yield return new WaitForSeconds(Random.Range(regenerationDirectionInterval.x, regenerationDirectionInterval.y));
        }
    }
    void OnDestroy()
    {
        Clear();
    }
    void Clear()
    {
        if (m_argsBuffer != null)
            m_argsBuffer.Dispose();
        if (m_StarsBuffer != null)
            m_StarsBuffer.Dispose();
        if (m_DirectionBuffer != null)
            m_DirectionBuffer.Dispose();
    }
}