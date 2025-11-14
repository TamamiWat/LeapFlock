using UnityEngine;

[CreateAssetMenu(menuName = "Boids/BoidsSettings")]
public class BoidsSetting : ScriptableObject
{
    //======================================
    // Boids Parameters
    //======================================
    [Header("Boids Count")]
    [Range(256, 32768)]
    public int m_MaxObjectNum = 16384;

    [Header("Boid Scale")]
    public float m_Scale = 0.01f;

    [Header("Neighborhood Radius")]
    [Range(0f, 10f)] public float m_CohesionNeighborRadius   = 0.8f;
    [Range(0f, 10f)] public float m_AlignmentNeighborRadius  = 0.5f;
    [Range(0f, 10f)] public float m_SeparationNeighborRadius = 0.03f;
    [Range(0f, 100f)] public float m_AttractRange            = 10f;
    [Range(0f, 100f)] public float m_AvoidRange              = 8f;

    [Header("Speed / Force")]
    [Range(0f, 10f)] public float m_MaxSpeed       = 5.0f;
    [Range(0f, 10f)] public float m_MaxSteerForce  = 0.5f;
    [Range(0f, 1f)]  public float m_MinSpeed       = 0.5f;

    [Header("Weights")]
    [Range(0f, 100f)] public float m_CohesionWeight   = 0.005f;
    [Range(0f, 100f)] public float m_AlignmentWeight  = 0.01f;
    [Range(0f, 100f)] public float m_SeparationWeight = 0.5f;
    [Range(0f, 100f)] public float m_AttractWeight    = 10.0f;
    [Range(0f, 100f)] public float m_AvoidWeight      = 12.0f;
    [Range(0f, 100f)] public float m_AvoidFrameWeight = 0.2f;

    [Header("Angles (radians-ish)")]
    [Range(0f, 10f)] public float m_CohesionAngle   = 1.5f;
    [Range(0f, 10f)] public float m_AligmentAngle   = 1.5f;
    [Range(0f, 10f)] public float m_SeparationAngle = 1.5f;

    [Header("Simulation Frame")]
    public Vector3 m_FrameCenter = Vector3.zero;
    public Vector3 m_FrameSize   = new Vector3(32.0f, 32.0f, 32.0f);
    [Range(0f, 100f)] public float m_FrameRadius = 0f;

    [Header("Color (HSV Range)")]
    [Range(0f, 1f)] public float m_hueMin = 0.0f;
    [Range(0f, 1f)] public float m_hueMax = 1.0f;
    [Range(0f, 1f)] public float m_satMin = 0.0f;
    [Range(0f, 1f)] public float m_satMax = 1.0f;
    [Range(0f, 1f)] public float m_valMin = 0.0f;
    [Range(0f, 1f)] public float m_valMax = 1.0f;

    [Header("Compute Shader")]
    public ComputeShader BoidsCS;
}
