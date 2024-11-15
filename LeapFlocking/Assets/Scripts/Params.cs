using UnityEngine;
[CreateAssetMenu(menuName = "Boid/Param")]
public class Params : ScriptableObject
{
    //number of boids
    public int N = 1000;
    //setting about Boid flocking
    public float cohesionWeight = 0.008f;
    public float separationWeight = 0.4f;
    public float alignmentWeight = 0.06f;
    public float cohesionDistance = 0.5f;
    public float separationDistance = 0.05f;
    public float alignmentDistance = 0.1f;
    public float cohesionAngle = Mathf.PI / 2;
    public float separationAngle = Mathf.PI / 2;
    public float alignmentAngle = Mathf.PI / 3;
    public float initSpeed = 0.005f;
    public float minVelocity = 0.005f;
    public float maxVelocity = 0.03f;
    public float userWeight = 1f;
    public float userDistance = 3f;

    //setting about boundary (wall)
    public int radialSegments = 24;   // u方向の分割数
    public int tubeSegments = 16;  // v方向の分割数
    public float radius = 1f;         // 大きな円の半径 (R)
    public float tubeRadius = 0.3f;   // チューブの半径 (r)
    public Vector3 wallCenter = Vector3.zero;
    public Vector3 wallScale = new Vector3(5f, 5f, 5f);
    public float wallWeight = 1f;
    public float wallDistance = 3f;
    public float rotationAngleX = 90f;
    public float rotationAngleY = 90f;
    public float rotationAngleZ = 0f;

    public float circulationWeight = 2f;
    public float tubeWeight = 3f;

    public float　maxAcceleration = 2.0f;
    
}