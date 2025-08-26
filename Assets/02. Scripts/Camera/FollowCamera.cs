using UnityEngine;

[RequireComponent(typeof(Camera))]
public class FollowOrthoCamera : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Isometric Setup")]
    [SerializeField] private Vector3 worldOffset = new Vector3(0.1f, 20f, -15f); // 현재 카메라 위치 기준
    [SerializeField] private Vector3 fixedEuler = new Vector3(45f, 0f, 0f);      // 현재 회전(X=45)
    [SerializeField] private float orthoSize = 17f;                               // 현재 Size

    [Header("Smoothing")]
    [SerializeField] private float moveSmoothTime = 0.15f; // 0.1~0.25 추천
    private Vector3 velocity; // SmoothDamp용

    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = orthoSize;
        transform.rotation = Quaternion.Euler(fixedEuler);
    }

    private void LateUpdate()
    {
        if (!target) return;

        // 목표 위치(플레이어 월드 위치 + 고정 오프셋)
        Vector3 desiredPos = target.position + worldOffset;

        // 부드럽게 따라감 (부착 대신 별도 오브젝트 + SmoothDamp로 흔들림 최소화)
        transform.position = Vector3.SmoothDamp(
            transform.position, desiredPos, ref velocity, moveSmoothTime);

        // 회전/사이즈 강제 유지(씬에서 다른 코드가 바꿔도 안전)
        if (!cam.orthographic) cam.orthographic = true;
        if (Mathf.Abs(cam.orthographicSize - orthoSize) > 0.001f)
            cam.orthographicSize = orthoSize;

        var rot = Quaternion.Euler(fixedEuler);
        if (transform.rotation != rot) transform.rotation = rot;
    }

    // 런타임 중 타겟 지정용
    public void SetTarget(Transform t) => target = t;
    public void SetSize(float size) => orthoSize = size; // 필요 시 외부에서 줌 제어
}
