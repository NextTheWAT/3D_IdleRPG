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

    // -------- Track Mode (�߰�) --------
    [Header("Track Mode")]
    public bool useTrackNodes = true;              // Ʈ�� ��� ��� ����
    public bool loopTrack = true;                  // ��������ó�� ����
    public float nodeReachDistance = 2f;           // ���/��������Ʈ ���� ���� �Ÿ�
    public List<Transform> trackNodes = new();     // 1,2,3... ������� �巡��

    private int trackNodeIndex = 0;                // ���� Ÿ�� ��� �ε���
    // ----------------------------------
    [Header("Track Smoothing")]
    public bool useSplineSmoothing = true;            // ���ö��� ��� ����
    [Range(2, 64)] public int samplesPerSegment = 12; // ���׸�Ʈ�� ���� ����Ʈ ��
    public bool projectPointsToNavMesh = true;        // ����Ʈ�� NavMesh�� ����
    public float projectionMaxDistance = 2f;          // ���� Ž�� �ݰ�

    [Header("Debug Gizmo Settings")]
    [SerializeField] private float waypointGizmoRadius;
    [SerializeField] private float trackNodeGizmoRadius;


    [Header("Steering")] 
    private float steerSmoothVel = 0f;
    [SerializeField] float steerResponse = 0.15f;   // Ŭ���� ������ ����
    [SerializeField] float steerSlowStart = 5f;     // ���� ���� ����
    [SerializeField] float steerSlowEnd = 35f;    // �ִ� ���� ����
    [SerializeField] int minCornerRPM = 80;     // �ڳ� �ּ� ��ǥRPM

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
        waypointGizmoRadius = nodeReachDistance;    //���� ũ��� ����� ���� ����
        trackNodeGizmoRadius = nodeReachDistance;   //���� ũ��� ����� ���� ����
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

                // Y �����ϰ� XZ ��� �Ÿ��� ���
                Vector3 a = carFront.position;
                Vector3 b = PostionToFollow;
                a.y = b.y = 0f;

                if (Vector3.Distance(a, b) < nodeReachDistance)
                    currentWayPoint++;
            }

            // ��������Ʈ�� ���� �����Ǹ� ���� ��� �̾���̱�
            if (currentWayPoint >= waypoints.Count - 3)
                CreatePath();
        }

        void CreatePath()
        {
            // -------- Ʈ�� ��� �켱(�߰�) --------
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

    // -------- Ʈ�� ��� ���� (�ű�) --------
    private void TrackPath()
    {
        if (trackNodes == null || trackNodes.Count < 2) { allowMovement = false; return; }

        int n = trackNodes.Count;

        // ���� Ÿ�� ��� �ε��� ����
        if (trackNodeIndex >= n)
        {
            if (loopTrack) trackNodeIndex = 0;
            else { useTrackNodes = false; return; }
        }

        // p1=�� Ÿ��, p2=�� ����, p0/p3=�� ��(���� ���)
        int i1 = trackNodeIndex % n;
        int i2 = (trackNodeIndex + 1) % n;
        int i0 = (trackNodeIndex - 1 + n) % n;
        int i3 = (trackNodeIndex + 2) % n;

        Vector3 p0 = trackNodes[i0].position;
        Vector3 p1 = trackNodes[i1].position;
        Vector3 p2 = trackNodes[i2].position;
        Vector3 p3 = trackNodes[i3].position;

        // p1��p2 ������ ���ö������� ���ø��ؼ� waypoints�� �̾� ����
        int steps = Mathf.Max(2, samplesPerSegment);
        for (int s = 0; s <= steps; s++)
        {
            float t = (float)s / steps;
            Vector3 pos = useSplineSmoothing ? CatmullRom(p0, p1, p2, p3, t)
                                             : Vector3.Lerp(p1, p2, t); // ����: Lerp ��ü

            // NavMesh�� �����ϸ� �� ������ Ƣ�� �� ����
            if (projectPointsToNavMesh &&
                NavMesh.SamplePosition(pos, out var hit, projectionMaxDistance, NavMeshLayerBite))
            {
                pos = hit.position;
            }

            // ���� �ߺ� ����Ʈ ����: ���� ���� �ְ� ù �����̸� ��ŵ
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
        ApplyRotationAndPosition(frontRight, wheelFR, true);   // ������
        ApplyRotationAndPosition(backLeft, wheelBL, false);
        ApplyRotationAndPosition(backRight, wheelBR, true);   // ������
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

        // ���� ����
        int margin = 10;
        float torque = 400 * MovementTorque;

        if (wheelRPM < LocalMaxSpeed - margin)
        {
            // ����
            backRight.motorTorque = backLeft.motorTorque = frontRight.motorTorque = frontLeft.motorTorque = torque;
        }
        else if (wheelRPM <= LocalMaxSpeed + margin)
        {
            // �ڽ���
            backRight.motorTorque = backLeft.motorTorque = frontRight.motorTorque = frontLeft.motorTorque = 0;
        }
        else
        {
            // ���� �� �극��ũ (����� ���� ���� ����)
            float curveT = Mathf.InverseLerp(steerSlowStart, steerSlowEnd, Mathf.Abs(frontLeft.steerAngle));
            float brake = Mathf.Lerp(1500f, 4000f, curveT); // �ʿ��ϸ� ��ġ ����
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

        // Ʈ�� ��� �ð�ȭ(�߰�)
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

    // p1��p2 ������ ������ �� �ֺ� p0,p3���� �ʿ�
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
