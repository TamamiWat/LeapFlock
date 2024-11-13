using UnityEngine;
using System.Collections.Generic;
using System.Collections.ObjectModel;

public class Simulation : MonoBehaviour
{
    [SerializeField]GameObject boidObj;
    [SerializeField]Params param;

    List<Boid> boids_ = new List<Boid>();

    public ReadOnlyCollection<Boid> boids
    {
        get { return boids_.AsReadOnly(); }
    }

    void AddBoid()
    {
        var go = Instantiate(boidObj, Random.insideUnitSphere, Random.rotation);
        go.transform.SetParent(transform);
        var boid = go.GetComponent<Boid>();
        boid.simulation = this;
        boid.param = param;
        boids_.Add(boid);
    }

    void AddBoidOnTorus()
    {
        Vector3 torusCenter = param.wallCenter; // トーラスの中心位置
        float radius = param.radius;   // トーラスの大きな半径
        float tubeRadius = param.tubeRadius;     // チューブの半径
        Vector3 scale = param.wallScale;        // トーラスのスケール

        // トーラス内のランダムな位置を生成
        Vector3 randomPosition = GetRandomPositionInTorus(torusCenter, radius, tubeRadius, scale);
        Quaternion randomRotation = Random.rotation;

        // ボイドを生成してトーラス内に配置
        var go = Instantiate(boidObj, randomPosition, randomRotation);
        go.transform.SetParent(transform);

        var boid = go.GetComponent<Boid>();
        boid.simulation = this;
        boid.param = param;
        boids_.Add(boid);
    }

    Vector3 GetRandomPositionInTorus(Vector3 center, float radius, float tubeRadius, Vector3 scale)
    {
        // ランダムな角度でリング上の位置を計算
        float theta = Random.Range(0, 2 * Mathf.PI);
        Vector3 ringPosition = new Vector3(
            Mathf.Cos(theta) * radius,
            0,
            Mathf.Sin(theta) * radius
        );

        // ランダムな方向でチューブ内の位置を計算
        float phi = Random.Range(0, 2 * Mathf.PI);
        Vector3 tubeOffset = new Vector3(
            Mathf.Cos(phi) * tubeRadius,
            Mathf.Sin(phi) * tubeRadius,
            0
        );

        // チューブ内の位置をリングの法線に沿って回転
        Quaternion rotation = Quaternion.LookRotation(ringPosition.normalized, Vector3.up);
        tubeOffset = rotation * tubeOffset;

        // スケールとトーラス中心位置を考慮して位置を返す
        return center + Vector3.Scale(ringPosition + tubeOffset, scale);
    }

    void RemoveBoid()
    {
        if (boids_.Count == 0) return;

        var lastIndex = boids_.Count - 1;
        var boid = boids_[lastIndex];
        Destroy(boid.gameObject);
        boids_.RemoveAt(lastIndex);
    }

    void Update()
    {
        while (boids_.Count < param.N)
        {
            AddBoid();
            //AddBoidOnTorus();
        }
        while (boids_.Count > param.N)
        {
            RemoveBoid();
        }
    }

    void OnDrawGizmos()
    {
        if (!param) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(param.wallCenter, Vector3.Scale(Vector3.one, param.wallScale));
        DrawTorus(Vector3.zero, param.radius, param.tubeRadius, 
                    param.radialSegments, param.tubeSegments, param.wallScale);
    }

    void DrawTorus(Vector3 center, float radius, float tubeRadius, int segments, int tubeSegments, Vector3 scale)
    {
        Quaternion rotation = Quaternion.Euler(param.rotationAngleX, param.rotationAngleY, param.rotationAngleZ);

        for (int i = 0; i < segments; i++)
        {
            float theta = i * 2.0f * Mathf.PI / segments;
            float nextTheta = (i + 1) * 2.0f * Mathf.PI / segments;

            for (int j = 0; j < tubeSegments; j++)
            {
                float phi = j * 2.0f * Mathf.PI / tubeSegments;
                float nextPhi = (j + 1) * 2.0f * Mathf.PI / tubeSegments;

                // 各点の位置を計算し、スケールを適用
                Vector3 p1 = center + rotation * Vector3.Scale(TorusPoint(Vector3.zero, radius, tubeRadius, theta, phi), scale);
                Vector3 p2 = center + rotation * Vector3.Scale(TorusPoint(Vector3.zero, radius, tubeRadius, nextTheta, phi), scale);
                Vector3 p3 = center + rotation * Vector3.Scale(TorusPoint(Vector3.zero, radius, tubeRadius, theta, nextPhi), scale);
                Vector3 p4 = center + rotation * Vector3.Scale(TorusPoint(Vector3.zero, radius, tubeRadius, nextTheta, nextPhi), scale);

                // トーラスのワイヤーフレームを描画
                Gizmos.DrawLine(p1, p2);
                Gizmos.DrawLine(p1, p3);
                Gizmos.DrawLine(p2, p4);
                Gizmos.DrawLine(p3, p4);
            }
        }
    }

    Vector3 TorusPoint(Vector3 center, float radius, float tubeRadius, float theta, float phi)
    {
        // トーラス上の位置を計算
        float x = (radius + tubeRadius * Mathf.Cos(phi)) * Mathf.Cos(theta);
        float z = (radius + tubeRadius * Mathf.Cos(phi)) * Mathf.Sin(theta);
        float y = tubeRadius * Mathf.Sin(phi);
        return new Vector3(x, y, z);
    }
}
