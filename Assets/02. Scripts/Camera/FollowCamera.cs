using UnityEngine;

[RequireComponent(typeof(Camera))]
public class FollowOrthoCamera : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Isometric Setup")]
    [SerializeField] private Vector3 worldOffset = new Vector3(0.1f, 20f, -15f); // ���� ī�޶� ��ġ ����
    [SerializeField] private Vector3 fixedEuler = new Vector3(45f, 0f, 0f);      // ���� ȸ��(X=45)
    [SerializeField] private float orthoSize = 17f;                               // ���� Size

    [Header("Smoothing")]
    [SerializeField] private float moveSmoothTime = 0.15f; // 0.1~0.25 ��õ
    private Vector3 velocity; // SmoothDamp��

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

        // ��ǥ ��ġ(�÷��̾� ���� ��ġ + ���� ������)
        Vector3 desiredPos = target.position + worldOffset;

        // �ε巴�� ���� (���� ��� ���� ������Ʈ + SmoothDamp�� ��鸲 �ּ�ȭ)
        transform.position = Vector3.SmoothDamp(
            transform.position, desiredPos, ref velocity, moveSmoothTime);

        // ȸ��/������ ���� ����(������ �ٸ� �ڵ尡 �ٲ㵵 ����)
        if (!cam.orthographic) cam.orthographic = true;
        if (Mathf.Abs(cam.orthographicSize - orthoSize) > 0.001f)
            cam.orthographicSize = orthoSize;

        var rot = Quaternion.Euler(fixedEuler);
        if (transform.rotation != rot) transform.rotation = rot;
    }

    // ��Ÿ�� �� Ÿ�� ������
    public void SetTarget(Transform t) => target = t;
    public void SetSize(float size) => orthoSize = size; // �ʿ� �� �ܺο��� �� ����
}
