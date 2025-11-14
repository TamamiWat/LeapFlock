using UnityEngine;

/// <summary>
/// BoidsSimulation が持っている ComputeBuffer(_BoidDataBuffer) を使って、
/// Mesh を GPU インスタンシングで描画するレンダラー。
/// </summary>
[RequireComponent(typeof(BoidsSimulation))]
public class BoidsRender : MonoBehaviour
{
    [Header("Simulation")]
    [SerializeField] private BoidsSimulation _simulation;

    [Header("Render Resources")]
    [SerializeField] private Mesh _mesh;
    [SerializeField] private Material _material; // Shader "Custom/Boids" を指定したマテリアル

    // Indirect Draw 用の引数バッファ
    // args[0] = インデックス数, args[1] = インスタンス数
    // args[2] = start index, args[3] = base vertex, args[4] = start instance
    uint[] _args = new uint[5] { 0, 0, 0, 0, 0 };
    GraphicsBuffer _argsBuffer;

    void Awake()
    {
        // 同じ GameObject に BoidsSimulation が付いている前提
        if (_simulation == null)
        {
            _simulation = GetComponent<BoidsSimulation>();
        }
    }

    void Start()
    {
        _argsBuffer = new GraphicsBuffer(
            GraphicsBuffer.Target.IndirectArguments,
            1,
            _args.Length * sizeof(uint)
        );
    }

    void Update()
    {
        RenderInstancedMesh();
    }

    void OnDisable()
    {
        if (_argsBuffer != null)
        {
            _argsBuffer.Release();
            _argsBuffer = null;
        }
    }

    void RenderInstancedMesh()
    {
        // 必要なリソースが揃っていないなら描画しない
        if (_material == null || _mesh == null || _simulation == null)
            return;

        if (!SystemInfo.supportsInstancing)
            return;

        var boidBuffer = _simulation.GetBoidDataBuffer();
        int boidCount  = _simulation.GetMaxObjectNum();

        if (boidBuffer == null || boidCount <= 0)
            return;

        // メッシュのインデックス数
        uint indexCount = (uint)_mesh.GetIndexCount(0);
        if (indexCount == 0)
            return;

        // 間接描画用引数をセット
        _args[0] = indexCount;
        _args[1] = (uint)boidCount;
        _args[2] = 0;
        _args[3] = 0;
        _args[4] = 0;
        _argsBuffer.SetData(_args);

        // Boid のバッファをマテリアルに渡す
        _material.SetBuffer("_BoidDataBuffer", boidBuffer);

        // 描画範囲（カリング用）
        Bounds bounds = new Bounds(
            _simulation.GetSimulationAreaCenter(),
            _simulation.GetSimulationAreaSize()
        );

        // GPU インスタンシング描画
        Graphics.DrawMeshInstancedIndirect(
            _mesh,
            0,
            _material,
            bounds,
            _argsBuffer
        );
    }
}
