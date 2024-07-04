using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateFlowersWithinTheRange : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Ԥ����")]
    GameObject[] flowerPrefabs;
    [SerializeField]
    [Tooltip("���ɷ�Χ��x�����ң�y���ϣ�z��ǰ")]
    Vector3 m_GenerationRange = new Vector3(10, 5, 100);
    [SerializeField]
    [Tooltip("��ת��Χ")]
    Vector2 m_RotationRange = new Vector2(5, 10);
    [SerializeField]
    [Tooltip("���ŷ�Χ��0-180")]
    Vector2 m_ScaleRange = new Vector2(1, 2);
    [SerializeField]
    [Tooltip("��ʼ��������")]
    int perCount = 50;
    [SerializeField]
    [Tooltip("��ֵԽ��ʼ��Խ��")]
    [Min(1)]
    int startCountFactor = 2;
    [SerializeField]
    [Tooltip("��ֵԽ�����ʱԽ��")]
    [Min(0.0001f)]
    int endCountFactor = 0;
    [SerializeField]
    [Tooltip("����ʱ��")]
    float duration = 10;

    List<GameObject> m_Objs;
    Coroutine m_Coroutine;
    void OnEnable()
    {
        m_Objs = new List<GameObject>();
        StopCoroutine();
        m_Coroutine = StartCoroutine(Generate());
    }
    void OnDisable()
    {
        StopCoroutine();
        Destroy();
    }
    IEnumerator Generate()
    {
        var generationCount = 0;
        var currentGenCount = 0f;

        var forwardDistance = 0f;
        var forwardStepPerFrame = m_GenerationRange.z / duration * Time.fixedDeltaTime;


        while (forwardDistance < m_GenerationRange.z)
        {
            var generationFactor = -Mathf.Pow(forwardDistance / (m_GenerationRange.z - Mathf.Min(startCountFactor, endCountFactor)), 1f / startCountFactor) + 1;
            currentGenCount += perCount / 100f * generationFactor;

            while (currentGenCount >= 1)
            {
                currentGenCount--;
                var obj = Instantiate(flowerPrefabs[Random.Range(0, flowerPrefabs.Length)]);
                obj.transform.SetParent(transform, false);

                var pX = Random.Range(-m_GenerationRange.x, m_GenerationRange.x) * transform.right;
                var pY = Random.Range(0, m_GenerationRange.y) * transform.up;
                var pZ = forwardDistance * transform.forward;
                obj.transform.position = transform.position + pX + pY + pZ;

                var rX = Random.Range(m_RotationRange.x, m_RotationRange.y);
                var rY = Random.Range(0, 360f);
                var rZ = Random.Range(m_RotationRange.x, m_RotationRange.y);
                obj.transform.eulerAngles = new Vector3(rX, rY, rZ);

                var scale = Random.Range(m_ScaleRange.x, m_ScaleRange.y);
                obj.transform.localScale = Vector3.one * scale;

                m_Objs.Add(obj);

                if (generationCount >= perCount - 1) break;
            }
            forwardDistance += forwardStepPerFrame;
            yield return new WaitForFixedUpdate();
        }
    }
    void StopCoroutine()
    {
        if (m_Coroutine != null)
            StopCoroutine(m_Coroutine);
    }
    void Destroy()
    {
        if (m_Objs != null)
        {
            foreach (var obj in m_Objs)
                Destroy(obj);
            m_Objs.Clear();
            m_Objs = null;
        }
    }
}