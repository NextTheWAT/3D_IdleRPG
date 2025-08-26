using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class CarAI : MonoBehaviour
{
    [Header("Car Wheels (Wheel Collider)")]
    public WheelCollider frontLeft;
    public WheelCollider frontRight;
    public WheelCollider backLeft;
    public WheelCollider backRight;

    [Header("Car Wheels (Transform)")]
    public Transform wheelFL;
    public Transform wheelFR;
    public Transform wheelBL;
    public Transform wheelBR;

    [Header("Car Front (Transform)")]
    public Transform carFront;

    [Header("General Parameters")]
    public List<string> NavMeshLayers;
    public int MaxSteeringAngle = 45;
    public int MaxRPM = 150;

    [Header("Debug")]
    public bool ShowGizmos;
    public bool Debugger;

    // -------- Track Mode (추가) --------
    [Header("Track Mode")]
    public bool useTrackNodes = true;              // 트랙 노드 사용 여부
    public bool loopTrack = true;                  // 마지막→처음 루프
    public float nodeReachDistance = 2f;           // 노드/웨이포인트 도달 판정 거리
    public List<Transform> trackNodes = new();     // 1,2,3... 순서대로 드래그

    private int trackNodeIndex = 0;                // 다음 타깃 노드 인덱스
    // ----------------------------------
    [Header("Track Smoothing")]
    public bool useSplineSmoothing = true;            // 스플라인 사용 여부
    [Range(2, 64)] public int samplesPerSegment = 12; // 세그먼트당 생성 포인트 수
    public bool projectPointsToNavMesh = true;        // 포인트를 NavMesh에 스냅
    public float projectionMaxDistance = 2f;          // 스냅 탐색 반경

    [Header("Debug Gizmo Settings")]
    [SerializeField] private float waypointGizmoRadius;
    [SerializeField] private float trackNodeGizmoRadius;


    [Header("Steering")] 
    private float steerSmoothVel = 0f;
    [SerializeField] float steerResponse = 0.15f;   // 클수록 빠르게 조향
    [SerializeField] float steerSlowStart = 5f;     // 감속 시작 각도
    [SerializeField] float steerSlowEnd = 35f;    // 최대 감속 각도
    [SerializeField] int minCornerRPM = 80;     // 코너 최소 목표RPM

    [HideInInspector] public bool move;

    private Vector3 PostionToFollow = Vector3.zero;
    private int currentWayPoint;
    private float AIFOV = 60;
    private bool allowMovement;
    private int NavMeshLayerBite;
    private List<Vector3> waypoints = new List<Vector3>();
    private float LocalMaxSpeed;
    private int Fails;
    private float MovementTorque = 1;

    void Awake()
    {
        currentWayPoint = 0;
        allowMovement = true;
        move = true;
    }

    void Start()
    {
        GetComponent<Rigidbody>().centerOfMass = Vector3.zero;
        CalculateNavMashLayerBite();
        waypointGizmoRadius = nodeReachDistance;    //실제 크기와 기즈모가 같게 수정
        trackNodeGizmoRadius = nodeReachDistance;   //실제 크기와 기즈모가 같게 수정
    }

    void FixedUpdate()
    {
        UpdateWheels();
        ApplySteering();
        PathProgress();
    }

    private void CalculateNavMashLayerBite()
    {
        if (NavMeshLayers == null || NavMeshLayers.Count == 0 || NavMeshLayers[0] == "AllAreas")
            NavMeshLayerBite = NavMesh.AllAreas;
        else if (NavMeshLayers.Count == 1)
            NavMeshLayerBite = 1 << NavMesh.GetAreaFromName(NavMeshLayers[0]);
        else
        {
            NavMeshLayerBite = 0;
            foreach (string Layer in NavMeshLayers)
            {
                int I = 1 << NavMesh.GetAreaFromName(Layer);
                NavMeshLayerBite |= I;
            }
        }
    }

    private void PathProgress()
    {
        wayPointManager();
        Movement();
        ListOptimizer();

        void wayPointManager()
        {
            if (currentWayPoint >= waypoints.Count)
                allowMovement = false;
            else
            {
                PostionToFollow = waypoints[currentWayPoint];
                allowMovement = true;

                // Y 무시하고 XZ 평면 거리만 사용
                Vector3 a = carFront.position;
                Vector3 b = PostionToFollow;
                a.y = b.y = 0f;

                if (Vector3.Distance(a, b) < nodeReachDistance)
                    currentWayPoint++;
            }

            // 웨이포인트가 거의 소진되면 다음 경로 이어붙이기
            if (currentWayPoint >= waypoints.Count - 3)
                CreatePath();
        }

        void CreatePath()
        {
            // -------- 트랙 노드 우선(추가) --------
            if (useTrackNodes && trackNodes != null && trackNodes.Count > 0)
            {
                TrackPath();
                return;
            }
            // --------------------------------------
        }

        void ListOptimizer()
        {
            if (currentWayPoint > 1 && waypoints.Count > 30)
            {
                waypoints.RemoveAt(0);
                currentWayPoint--;
            }
        }
    }

    // -------- 트랙 경로 생성 (신규) --------
    private void TrackPath()
    {
        if (trackNodes == null || trackNodes.Count < 2) { allowMovement = false; return; }

        int n = trackNodes.Count;

        // 다음 타깃 노드 인덱스 보정
        if (trackNodeIndex >= n)
        {
            if (loopTrack) trackNodeIndex = 0;
            else { useTrackNodes = false; return; }
        }

        // p1=현 타깃, p2=그 다음, p0/p3=양 옆(루프 고려)
        int i1 = trackNodeIndex % n;
        int i2 = (trackNodeIndex + 1) % n;
        int i0 = (trackNodeIndex - 1 + n) % n;
        int i3 = (trackNodeIndex + 2) % n;

        Vector3 p0 = trackNodes[i0].position;
        Vector3 p1 = trackNodes[i1].position;
        Vector3 p2 = trackNodes[i2].position;
        Vector3 p3 = trackNodes[i3].position;

        // p1→p2 구간을 스플라인으로 샘플링해서 waypoints에 이어 붙임
        int steps = Mathf.Max(2, samplesPerSegment);
        for (int s = 0; s <= steps; s++)
        {
            float t = (float)s / steps;
            Vector3 pos = useSplineSmoothing ? CatmullRom(p0, p1, p2, p3, t)
                                             : Vector3.Lerp(p1, p2, t); // 응급: Lerp 대체

            // NavMesh에 스냅하면 길 밖으로 튀는 것 방지
            if (projectPointsToNavMesh &&
                NavMesh.SamplePosition(pos, out var hit, projectionMaxDistance, NavMeshLayerBite))
            {
                pos = hit.position;
            }

            // 시작 중복 포인트 방지: 기존 길이 있고 첫 샘플이면 스킵
            if (s == 0 && waypoints.Count > 0) continue;
            waypoints.Add(pos);
        }

        debug($"Track spline segment {i1}->{i2} appended ({steps + 1} pts)", false);
        trackNodeIndex++;

        if (trackNodeIndex >= n && !loopTrack)
            useTrackNodes = false;
    }

    // --------------------------------------

    public void RandomPath()
    {
        NavMeshPath path = new NavMeshPath();
        Vector3 sourcePostion;

        if (waypoints.Count == 0)
        {
            Vector3 randomDirection = Random.insideUnitSphere * 100;
            randomDirection += transform.position;
            sourcePostion = carFront.position;
            Calculate(randomDirection, sourcePostion, carFront.forward, NavMeshLayerBite);
        }
        else
        {
            sourcePostion = waypoints[waypoints.Count - 1];
            Vector3 randomPostion = Random.insideUnitSphere * 100;
            randomPostion += sourcePostion;
            Vector3 direction = (waypoints[waypoints.Count - 1] - waypoints[waypoints.Count - 2]).normalized;
            Calculate(randomPostion, sourcePostion, direction, NavMeshLayerBite);
        }

        void Calculate(Vector3 destination, Vector3 sourcePostion, Vector3 direction, int areaMask)
        {
            if (NavMesh.SamplePosition(destination, out NavMeshHit hit, 150, 1 << NavMesh.GetAreaFromName(NavMeshLayers[0])) &&
                NavMesh.CalculatePath(sourcePostion, hit.position, areaMask, path) && path.corners.Length > 2)
            {
                if (CheckForAngle(path.corners[1], sourcePostion, direction))
                {
                    waypoints.AddRange(path.corners.ToList());
                    debug("Random Path generated successfully", false);
                }
                else
                {
                    if (CheckForAngle(path.corners[2], sourcePostion, direction))
                    {
                        waypoints.AddRange(path.corners.ToList());
                        debug("Random Path generated successfully", false);
                    }
                    else
                    {
                        debug("Failed to generate a random path. Waypoints are outside the AIFOV. Generating a new one", false);
                        Fails++;
                    }
                }
            }
            else
            {
                debug("Failed to generate a random path. Invalid Path. Generating a new one", false);
                Fails++;
            }
        }
    }

    public void CustomPath(Transform destination)
    {
        NavMeshPath path = new NavMeshPath();
        Vector3 sourcePostion;

        if (waypoints.Count == 0)
        {
            sourcePostion = carFront.position;
            Calculate(destination.position, sourcePostion, carFront.forward, NavMeshLayerBite);
        }
        else
        {
            sourcePostion = waypoints[waypoints.Count - 1];
            Vector3 direction = (waypoints[waypoints.Count - 1] - waypoints[waypoints.Count - 2]).normalized;
            Calculate(destination.position, sourcePostion, direction, NavMeshLayerBite);
        }

        void Calculate(Vector3 destination, Vector3 sourcePostion, Vector3 direction, int areaMask)
        {
            if (NavMesh.SamplePosition(destination, out NavMeshHit hit, 150, areaMask) &&
                NavMesh.CalculatePath(sourcePostion, hit.position, areaMask, path))
            {
                if (path.corners.Length > 1 && CheckForAngle(path.corners[1], sourcePostion, direction))
                {
                    waypoints.AddRange(path.corners.ToList());
                    debug("Custom Path generated successfully", false);
                }
                else
                {
                    if (path.corners.Length > 2 && CheckForAngle(path.corners[2], sourcePostion, direction))
                    {
                        waypoints.AddRange(path.corners.ToList());
                        debug("Custom Path generated successfully", false);
                    }
                    else
                    {
                        debug("Failed to generate a Custom path. Waypoints are outside the AIFOV. Generating a new one", false);
                        Fails++;
                    }
                }
            }
            else
            {
                debug("Failed to generate a Custom path. Invalid Path. Generating a new one", false);
                Fails++;
            }
        }
    }

    private bool CheckForAngle(Vector3 pos, Vector3 source, Vector3 direction)
    {
        Vector3 distance = (pos - source).normalized;
        float CosAngle = Vector3.Dot(distance, direction);
        float Angle = Mathf.Acos(CosAngle) * Mathf.Rad2Deg;
        return Angle < AIFOV;
    }

    private void ApplyBrakes()
    {
        frontLeft.brakeTorque = 5000;
        frontRight.brakeTorque = 5000;
        backLeft.brakeTorque = 5000;
        backRight.brakeTorque = 5000;
    }

    private void UpdateWheels()
    {
        ApplyRotationAndPosition(frontLeft, wheelFL, false);
        ApplyRotationAndPosition(frontRight, wheelFR, true);   // 오른쪽
        ApplyRotationAndPosition(backLeft, wheelBL, false);
        ApplyRotationAndPosition(backRight, wheelBR, true);   // 오른쪽
    }

    private void ApplyRotationAndPosition(WheelCollider wc, Transform wheel, bool isRight)
    {
        wc.ConfigureVehicleSubsteps(5, 12, 15);
        wc.GetWorldPose(out var pos, out var rot);
        var offset = isRight ? Quaternion.Euler(0, 180, 0) : Quaternion.identity;
        wheel.SetPositionAndRotation(pos, rot * offset);
    }

    void ApplySteering()
    {
        Vector3 relativeVector = transform.InverseTransformPoint(PostionToFollow);
        float SteeringAngle = (relativeVector.x / Mathf.Max(relativeVector.magnitude, 0.0001f)) * MaxSteeringAngle;
        LocalMaxSpeed = (SteeringAngle > 15f) ? 100 : MaxRPM;

        frontLeft.steerAngle = SteeringAngle;
        frontRight.steerAngle = SteeringAngle;
    }

    void Movement()
    {
        allowMovement = (move && allowMovement);

        if (!allowMovement) { ApplyBrakes(); return; }

        frontLeft.brakeTorque = frontRight.brakeTorque = 0;
        backLeft.brakeTorque = backRight.brakeTorque = 0;

        int wheelRPM = (int)((frontLeft.rpm + frontRight.rpm + backLeft.rpm + backRight.rpm) / 4);

        // 여유 마진
        int margin = 10;
        float torque = 400 * MovementTorque;

        if (wheelRPM < LocalMaxSpeed - margin)
        {
            // 가속
            backRight.motorTorque = backLeft.motorTorque = frontRight.motorTorque = frontLeft.motorTorque = torque;
        }
        else if (wheelRPM <= LocalMaxSpeed + margin)
        {
            // 코스팅
            backRight.motorTorque = backLeft.motorTorque = frontRight.motorTorque = frontLeft.motorTorque = 0;
        }
        else
        {
            // 과속 → 브레이크 (곡률에 따라 강도 가중)
            float curveT = Mathf.InverseLerp(steerSlowStart, steerSlowEnd, Mathf.Abs(frontLeft.steerAngle));
            float brake = Mathf.Lerp(1500f, 4000f, curveT); // 필요하면 수치 조정
            frontLeft.brakeTorque = frontRight.brakeTorque = backLeft.brakeTorque = backRight.brakeTorque = brake;

            backRight.motorTorque = backLeft.motorTorque = frontRight.motorTorque = frontLeft.motorTorque = 0;
        }
    }


    void debug(string text, bool IsCritical)
    {
        if (!Debugger) return;
        if (IsCritical) Debug.LogError(text);
        else Debug.Log(text);
    }

    private void OnDrawGizmos()
    {
        if (ShowGizmos)
        {
            for (int i = 0; i < waypoints.Count; i++)
            {
                if (i == currentWayPoint) Gizmos.color = Color.blue;
                else Gizmos.color = (i > currentWayPoint) ? Color.red : Color.green;
                Gizmos.DrawWireSphere(waypoints[i], waypointGizmoRadius);
            }
            CalculateFOV();
        }

        // 트랙 노드 시각화(추가)
        if (trackNodes != null && trackNodes.Count > 0)
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < trackNodes.Count; i++)
            {
                var a = trackNodes[i];
                var b = (i == trackNodes.Count - 1) ? (loopTrack ? trackNodes[0] : null) : trackNodes[i + 1];
                if (a != null) Gizmos.DrawWireSphere(a.position, trackNodeGizmoRadius);
                if (a != null && b != null) Gizmos.DrawLine(a.position, b.position);
            }
        }

        void CalculateFOV()
        {
            Gizmos.color = Color.white;
            float totalFOV = AIFOV * 2;
            float rayRange = 10.0f;
            float halfFOV = totalFOV / 2.0f;
            Quaternion leftRayRotation = Quaternion.AngleAxis(-halfFOV, Vector3.up);
            Quaternion rightRayRotation = Quaternion.AngleAxis(halfFOV, Vector3.up);
            Vector3 leftRayDirection = leftRayRotation * transform.forward;
            Vector3 rightRayDirection = rightRayRotation * transform.forward;
            if (carFront != null)
            {
                Gizmos.DrawRay(carFront.position, leftRayDirection * rayRange);
                Gizmos.DrawRay(carFront.position, rightRayDirection * rayRange);
            }
        }
    }

    // p1→p2 구간을 보간할 때 주변 p0,p3까지 필요
    private static Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float t2 = t * t;
        float t3 = t2 * t;
        return 0.5f * (
            (2f * p1) +
            (-p0 + p2) * t +
            (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
            (-p0 + 3f * p1 - 3f * p2 + p3) * t3
        );
    }

}
