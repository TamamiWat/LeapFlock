using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour
{
    public Simulation simulation { get; set; }
    public Params param { get; set; }
    public Vector3 pos { get; private set; }
    public Vector3 velocity { get; private set; }
    private GameObject leapmotion;
    private HandDataGetter user;
    Vector3 accel = Vector3.zero;
    List<Boid> cohesionList = new List<Boid>();
    List<Boid> separationList = new List<Boid>();
    List<Boid> alignmentList = new List<Boid>();


    void Start()
    {
        pos = param.wallCenter;
        velocity = transform.forward * param.initSpeed;
        leapmotion = GameObject.Find ("LeapMotionManager");
        user = leapmotion.GetComponent<HandDataGetter>();
    }

    void Update()
    {
        //UpdateTorus();
        UpdateNeighbors();
        UpdateWall();
        UpdateCohesion();
        UpdateSeparation();
        UpdateAlignment();
        UpdateUser();
        UpdateMove();
    }

    void UpdateWall()
    {
        if (!simulation) return;

        Vector3 scale = param.wallScale * 0.5f;
        Vector3 center = param.wallCenter;
        accel +=
            CalcAccelAgainstWall(center.x - scale.x - pos.x, Vector3.right) +
            CalcAccelAgainstWall(center.y - scale.y - pos.y, Vector3.up) +
            CalcAccelAgainstWall(center.z - scale.z - pos.z, Vector3.forward) +
            CalcAccelAgainstWall(center.x + scale.x - pos.x, Vector3.left) +
            CalcAccelAgainstWall(center.y + scale.y - pos.y, Vector3.down) +
            CalcAccelAgainstWall(center.z + scale.z - pos.z, Vector3.back);
    }

    Vector3 CalcAccelAgainstWall(float distance, Vector3 dir)
    {
        if (distance < param.wallDistance)
        {
            return dir * (param.wallWeight / Mathf.Abs(distance / param.wallDistance));
        }
        return Vector3.zero;
    }

    void UpdateMove()
    {
        var dt = Time.deltaTime;

        velocity += accel * dt;
        var dir = velocity.normalized;
        var speed = velocity.magnitude;
        velocity = Mathf.Clamp(speed, param.minVelocity, param.maxVelocity) * dir;
        pos += velocity * dt;

        var rot = Quaternion.LookRotation(velocity);
        transform.SetPositionAndRotation(pos, rot);

        accel = Vector3.zero;
    }

    void UpdateTorus()
    {
        if(!simulation) return;

        Vector3 torusCenter = param.wallCenter;
        float radius = param.radius; 
        float tubeRadius = param.tubeRadius;  
        Vector3 scale = param.wallScale;

        accel += CalcAccelWithinTorus(pos, torusCenter, radius, tubeRadius, scale);
    }

    Vector3 CalcAccelWithinTorus(Vector3 pos, Vector3 torusCenter, float radius, float tubeRadius, Vector3 scale)
    {
        // ローカル空間での位置を取得しスケールを適用
        Vector3 localPos = Vector3.Scale(pos - torusCenter, scale);

        // トーラスのリング中心への距離を計算
        float distanceFromCenterToRing = new Vector2(localPos.x, localPos.z).magnitude;
        float distanceToTubeCenter = Mathf.Abs(distanceFromCenterToRing - radius);

        // トーラスのチューブ外に出た場合、内側へ戻す力を計算
        if (distanceToTubeCenter > tubeRadius)
        {
            // チューブ中心方向のベクトル
            Vector3 toRingCenter = new Vector3(localPos.x, 0, localPos.z).normalized * (radius - distanceFromCenterToRing);
            Vector3 toTubeCenter = (localPos - toRingCenter).normalized * (tubeRadius - distanceToTubeCenter);

            // チューブ中心に戻す力を適用
            Vector3 force = (toRingCenter + toTubeCenter).normalized * param.wallWeight;
            return force / Mathf.Max(1f, distanceToTubeCenter); // 安全に距離で割る
        }

        return Vector3.zero; // チューブ内に収まっている場合は加速なし
    }

    void UpdateUser()
    {
        if(user.UserPos.HasValue)
        {
            Vector3 position = user.UserPos.Value;
            Debug.Log("User Position: " + position);
            accel += CalcAttractionUser(position, pos);
        }
        else
        {
            Debug.Log("No hand detected");
        }
        
    }

    Vector3 CalcAttractionUser(Vector3 user, Vector3 pos)
    {
        float distance = Vector3.Distance(user, pos);
        if(distance < param.userDistance)
        {
            Vector3 directionToUser = (user - pos).normalized;
            return directionToUser * (param.userWeight / Mathf.Abs(Mathf.Max(distance, 0,001f) / param.userDistance));
        }
        return Vector3.zero;
    }

    void UpdateNeighbors()
    {
        cohesionList.Clear();
        separationList.Clear();
        alignmentList.Clear();

        if (!simulation) return;

        var cohesionDistanceThreshold = param.cohesionDistance;
        var separationDistanceThreshold = param.separationDistance;
        var alignmentDistanceThreshold = param.alignmentDistance;
        var cohesionAngleThreshold = Mathf.Cos(param.cohesionAngle);
        var separationAngleThreshold = Mathf.Cos(param.separationAngle);
        var alignmentAngleThreshold = Mathf.Cos(param.alignmentAngle);

        foreach (var other in simulation.boids)
        {
            if (other == this) continue;

            var displacement = other.pos - pos;
            var distance = displacement.magnitude;

            var direction =  displacement.normalized;
            var forward = velocity.normalized;

            var product = Vector3.Dot(forward, direction);

            if(distance < cohesionDistanceThreshold && product > cohesionAngleThreshold)
            {
                cohesionList.Add(other);
            }

            if(distance < separationDistanceThreshold && product > separationAngleThreshold)
            {
                separationList.Add(other);
            }

            if(distance < alignmentDistanceThreshold && product > alignmentAngleThreshold)
            {
                alignmentList.Add(other);
            }
        }


    }
    
    void UpdateCohesion()
    {
        if(cohesionList.Count == 0) return;

        var averagePos = Vector3.zero;
        foreach(var coh in cohesionList)
        {
            averagePos += coh.pos;
        }
        averagePos /= cohesionList.Count;

        accel += (averagePos - pos) * param.cohesionWeight;
    }

    void UpdateSeparation()
    {
        if (separationList.Count == 0) return;

        Vector3 force = Vector3.zero;

        foreach (var sep in separationList)
        {
            force += (pos - sep.pos).normalized;
        }
        force /= separationList.Count;

        accel += force * param.separationWeight;
    }

    void UpdateAlignment()
    {
        if (alignmentList.Count == 0) return;

        var averageVelocity = Vector3.zero;

        foreach(var ali in alignmentList)
        {
            averageVelocity += ali.velocity;
        }
        averageVelocity /= alignmentList.Count;

        accel += (averageVelocity - velocity) * param.alignmentWeight;
        
    }
}
