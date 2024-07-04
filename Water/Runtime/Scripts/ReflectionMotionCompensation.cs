using UnityEngine;

public class ReflectionMotionCompensation : MonoBehaviour
{
    public float speed = 3f;
    public float range = 0.1f;
    public float interval = 0.3f;
    public float speedRange = 0.2f;

    Material m_Material;

    float m_LastFrameSpeed;
    float m_CurrentSpeed;
    float m_Acceleration;
    float m_FrameDistance;
    float m_Distance;
    float m_CurrentInterval;

    Vector3 m_LastFramePosition;
    Vector3 m_LastPosition;
    Vector3 m_WaterCameraPosition;

    Camera m_MainCamera;
    Camera MainCamera { get { if (m_MainCamera == null) m_MainCamera = Camera.main; return m_MainCamera; } }
    void Start()
    {
        m_Material = GetComponent<MeshRenderer>().material;
        m_LastFrameSpeed = 0;
        m_LastFramePosition = MainCamera.transform.position;
        m_LastPosition = MainCamera.transform.position;
        m_WaterCameraPosition = MainCamera.transform.position;
    }
    void FixedUpdate()
    {
        m_Distance = Vector3.Distance(MainCamera.transform.position, m_LastPosition);
        if (m_Distance > range)
        {
            m_WaterCameraPosition = Vector3.Lerp(m_WaterCameraPosition, MainCamera.transform.position, Time.fixedDeltaTime * speed);
            m_Material.SetVector("_CameraPosition", m_WaterCameraPosition);
        }

        m_FrameDistance = Vector3.Distance(MainCamera.transform.position, m_LastFramePosition);
        m_CurrentSpeed = m_FrameDistance / Time.fixedDeltaTime;
        m_Acceleration = (m_CurrentSpeed - m_LastFrameSpeed) / Time.fixedDeltaTime;

        if (m_Acceleration < speedRange)
        {
            if (m_Distance < range)
            {
                m_CurrentInterval += Time.fixedDeltaTime;
                if (m_CurrentInterval >= interval)
                {
                    m_CurrentInterval = 0;
                    m_LastPosition = MainCamera.transform.position;
                }
            }
            else
            {
                m_LastPosition = m_WaterCameraPosition;
            }
        }

        m_LastFramePosition = MainCamera.transform.position;
        m_LastFrameSpeed = m_CurrentSpeed;
    }
}