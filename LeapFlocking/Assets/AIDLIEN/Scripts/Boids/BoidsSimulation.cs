using UnityEngine;
using System.Runtime.InteropServices;

public class BoidsSimulation : MonoBehaviour
{
    [SerializeField] BoidsSetting bs;
    [SerializeField] private GameObject _OSCobject;
    private ReceiveOSC user;
    // GPU に送るデータなのでレイアウトを固定しておく
    [System.Serializable]
    [StructLayout(LayoutKind.Sequential)]
    struct BoidData
    {
        public Vector3 Velocity;
        public Vector3 Position;
        public Vector4 Color;
        public Vector3 Scale;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct Force
    {
        public Vector3 separation;
        public Vector3 aligment;
        public Vector3 cohesion;
        public Vector3 attraction;
        public Vector3 avoidance;
    }

    const int SIMULATION_BLOCK_SIZE = 256;

    //======================================
    // Private Resources
    //======================================
    ComputeBuffer _boidForceBuffer;
    ComputeBuffer _boidDataBuffer;
    ComputeBuffer _boidForceDataBuffer;

    float   _range;
    Vector3 _userPos;
    bool    _isUserDrag = false;
    bool    _isUserInOut = false;

    int _kernelSteerForce;
    int _kernelMotion;

    //======================================
    // Accessors
    //======================================
    public ComputeBuffer GetBoidDataBuffer()
    {
        return _boidDataBuffer;
    }

    public int GetMaxObjectNum() => bs.m_MaxObjectNum;

    public Vector3 GetSimulationAreaCenter() => bs.m_FrameCenter;

    public Vector3 GetSimulationAreaSize()
        => Vector3.one * (bs.m_FrameRadius * 2f);

     //======================================
    // MonoBehaviour
    //======================================
    void Start()
    {
        user = _OSCobject.GetComponent<ReceiveOSC>();
        CacheKernelIDs();
        InitBuffer();
    }

    void Update()
    {
        if (bs.BoidsCS == null) return;

        UpdateUserInput();
        Simulation();
    }

    void OnDisable()
    {
        ReleaseBuffer();
    }

    void OnDestroy()
    {
        ReleaseBuffer();
    }

    //======================================
    // Initialization
    //======================================
    void CacheKernelIDs()
    {
        if (bs.BoidsCS == null) return;

        _kernelSteerForce = bs.BoidsCS.FindKernel("SteerForceCalculator");
        _kernelMotion     = bs.BoidsCS.FindKernel("MotionCalculator");
    }

    void InitBuffer()
    {
        ReleaseBuffer();

            _boidDataBuffer = new ComputeBuffer(
                bs.m_MaxObjectNum,
                Marshal.SizeOf(typeof(BoidData))
            );
            _boidForceBuffer = new ComputeBuffer(
                bs.m_MaxObjectNum,
                Marshal.SizeOf(typeof(Vector3))
            );
            _boidForceDataBuffer = new ComputeBuffer(
                bs.m_MaxObjectNum,
                Marshal.SizeOf(typeof(Force))
            );

            var forceArr         = new Vector3[bs.m_MaxObjectNum];
            var boidDataArr      = new BoidData[bs.m_MaxObjectNum];
            var boidForceDataArr = new Force[bs.m_MaxObjectNum];

            _range = bs.m_FrameRadius;

            for (int i = 0; i < bs.m_MaxObjectNum; i++)
            {
                Vector4 initColor = new Vector4(0.5f, 0.5f, 0.5f, 1.0f);

                forceArr[i]                 = Vector3.zero;
                Vector3 offset = Random.insideUnitSphere * _range;
                boidDataArr[i].Position     = bs.m_FrameCenter + offset;
                boidDataArr[i].Velocity     = Random.insideUnitSphere * 1.0f;
                boidDataArr[i].Color        = initColor;
                boidDataArr[i].Scale        = Vector3.one * bs.m_Scale;
                boidForceDataArr[i].separation = Vector3.zero;
                boidForceDataArr[i].aligment   = Vector3.zero;
                boidForceDataArr[i].cohesion   = Vector3.zero;
                boidForceDataArr[i].attraction = Vector3.zero;
                boidForceDataArr[i].avoidance  = Vector3.zero;
            }

            _boidForceBuffer.SetData(forceArr);
            _boidDataBuffer.SetData(boidDataArr);
            _boidForceDataBuffer.SetData(boidForceDataArr);
    }

    //======================================
    // Update Input
    //======================================
    void UpdateUserInput()
    {
        if(user.IsUserExist == 0) {_isUserDrag = false; _isUserInOut = true; return;}
        if(user.UserPos.HasValue)
        {
            _userPos = user.UserPos.Value;
            _isUserDrag = true;
            _isUserInOut = false;
        }
    }

    //======================================
    // Simulation
    //======================================
    void Simulation()
    {
        if (_boidDataBuffer == null ||
            _boidForceBuffer == null ||
            _boidForceDataBuffer == null)
        {
            // 何かの理由でバッファが消えていたら復旧
            InitBuffer();
        }

        if (bs.BoidsCS == null) return;

        int threadGroupSize = Mathf.CeilToInt(
            (float)bs.m_MaxObjectNum / SIMULATION_BLOCK_SIZE
        );

        // 端数切り上げ版
        threadGroupSize = (bs.m_MaxObjectNum + SIMULATION_BLOCK_SIZE - 1) / SIMULATION_BLOCK_SIZE;

        DispatchSteerForce(threadGroupSize);
        DispatchMotion(threadGroupSize);
    }

    void DispatchSteerForce(int threadGroupSize)
    {
        bs.BoidsCS.SetInt("_MaxBoidObjectNum", bs.m_MaxObjectNum);
        bs.BoidsCS.SetFloat("_CohesionNeighborhoodRadius",  bs.m_CohesionNeighborRadius);
        bs.BoidsCS.SetFloat("_AlignmentNeighborhoodRadius", bs.m_AlignmentNeighborRadius);
        bs.BoidsCS.SetFloat("_SeparateNeighborhoodRadius",  bs.m_SeparationNeighborRadius);
        bs.BoidsCS.SetFloat("_MaxSpeed",        bs.m_MaxSpeed);
        bs.BoidsCS.SetFloat("_MaxSteerForce",   bs.m_MaxSteerForce);
        bs.BoidsCS.SetFloat("_MinSpeed",        bs.m_MinSpeed);
        bs.BoidsCS.SetFloat("_SeparationWeight", bs.m_SeparationWeight);
        bs.BoidsCS.SetFloat("_CohesionWeight",   bs.m_CohesionWeight);
        bs.BoidsCS.SetFloat("_AlignmentWeight",  bs.m_AlignmentWeight);
        bs.BoidsCS.SetFloat("_AttractForceWeight", bs.m_AttractWeight);
        bs.BoidsCS.SetFloat("_AvoidForceWeight",   bs.m_AvoidWeight);
        bs.BoidsCS.SetVector("_FrameCenter", bs.m_FrameCenter);
        bs.BoidsCS.SetVector("_FrameSize",   bs.m_FrameSize);
        bs.BoidsCS.SetFloat("_FrameRadius",  bs.m_FrameRadius);
        bs.BoidsCS.SetFloat("_AttractRange", bs.m_AttractRange);
        bs.BoidsCS.SetFloat("_AvoidRange",   bs.m_AvoidRange);
        bs.BoidsCS.SetVector("_DragPos", _userPos);
        bs.BoidsCS.SetVector("_TapPos",  _userPos);
        bs.BoidsCS.SetBool("_userInOut", _isUserInOut);
        bs.BoidsCS.SetBool("_userDrag",  _isUserDrag);
        bs.BoidsCS.SetFloat("_AvoidFrameWeight", bs.m_AvoidFrameWeight);
        bs.BoidsCS.SetFloat("_CohesionAngle",   bs.m_CohesionAngle);
        bs.BoidsCS.SetFloat("_AlignmentAngle",  bs.m_AligmentAngle);
        bs.BoidsCS.SetFloat("_SeparationAngle", bs.m_SeparationAngle);
        bs.BoidsCS.SetFloat("_hueMin", bs.m_hueMin);
        bs.BoidsCS.SetFloat("_hueMax", bs.m_hueMax);
        bs.BoidsCS.SetFloat("_satMin", bs.m_satMin);
        bs.BoidsCS.SetFloat("_satMax", bs.m_satMax);
        bs.BoidsCS.SetFloat("_valMin", bs.m_valMin);
        bs.BoidsCS.SetFloat("_valMax", bs.m_valMax);

        bs.BoidsCS.SetBuffer(_kernelSteerForce, "_BoidDataBufferRead",         _boidDataBuffer);
        bs.BoidsCS.SetBuffer(_kernelSteerForce, "_BoidForceBufferWrite",       _boidForceBuffer);
        bs.BoidsCS.SetBuffer(_kernelSteerForce, "_BoidForceDataBufferWrite",   _boidForceDataBuffer);

        bs.BoidsCS.Dispatch(_kernelSteerForce, threadGroupSize, 1, 1);
    }

    void DispatchMotion(int threadGroupSize)
    {
        bs.BoidsCS.SetFloat("_DeltaTime", Time.deltaTime);
        bs.BoidsCS.SetBuffer(_kernelMotion, "_BoidForceBufferRead",      _boidForceBuffer);
        bs.BoidsCS.SetBuffer(_kernelMotion, "_BoidForceDataBufferRead",  _boidForceDataBuffer);
        bs.BoidsCS.SetBuffer(_kernelMotion, "_BoidDataBufferWrite",      _boidDataBuffer);

        bs.BoidsCS.Dispatch(_kernelMotion, threadGroupSize, 1, 1);
    
    }

    //======================================
    // Buffer / Utility
    //======================================
    void ReleaseBuffer()
    {
        if (_boidDataBuffer != null)
        {
            _boidDataBuffer.Release();
            _boidDataBuffer = null;
        }

        if (_boidForceBuffer != null)
        {
            _boidForceBuffer.Release();
            _boidForceBuffer = null;
        }

        if (_boidForceDataBuffer != null)
        {
            _boidForceDataBuffer.Release();
            _boidForceDataBuffer = null;
        }
    }

     Vector3 RandomVector(float min, float max)
    {
        return new Vector3(
            Random.Range(min, max),
            Random.Range(min, max),
            Random.Range(min, max)
        );
    }


}

