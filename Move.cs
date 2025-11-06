using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 2.5f;
    public float runSpeed = 5f;
    public float rotateLerp = 12f;
    public float deadZone = 0.02f;

    [Header("Camera")]
    public Transform cam;                  // Main Camera Transform 할당 권장
    public bool useCameraYawOnly = true;   // 카메라 Yaw만 적용

    [Header("Slide & Probe")]
    public float skin = 0.05f;             // 벽에 너무 붙지 않도록
    public float probeDistance = 0.6f;     // 전방 탐지 거리(속도에 따라 자동 스케일)

    Rigidbody rb;
    Animator anim;

    // 🟡 추가: OpenAIConnector 참조
    private OpenAIConnector connector;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();

        // 동적 리지드바디 세팅
        rb.isKinematic = false;
        rb.useGravity = false; // 평면 이동이면 꺼두기
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        // 씬에서 자동으로 OpenAIConnector 찾기
        connector = FindObjectOfType<OpenAIConnector>();
    }

    private void Update()
    {
        // 🔹 대화 중에는 플레이어 이동 완전히 정지
        if (connector != null && connector.IsDialogueActive)
        {
            if (rb) rb.velocity = Vector3.zero;
            if (anim) anim.SetFloat("Speed", 0f);
            return;
        }
    }

    private void FixedUpdate()
    {
        // 대화 중에는 이동 불가
        if (connector != null && connector.IsDialogueActive)
            return;

        // --- 입력 처리 ---
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        Vector3 input = new Vector3(x, 0f, z);
        bool hasInput = input.sqrMagnitude > deadZone * deadZone;

        // --- 이동 방향 (카메라 기준) ---
        Vector3 wishDir = Vector3.zero;
        if (hasInput)
        {
            wishDir = input.normalized;
            if (cam)
            {
                float yaw = cam.eulerAngles.y;
                if (useCameraYawOnly)
                    wishDir = Quaternion.Euler(0f, yaw, 0f) * wishDir;
                else
                    wishDir = Quaternion.Euler(cam.eulerAngles.x, yaw, cam.eulerAngles.z) * wishDir;
            }
        }

        // --- 속도 계산 ---
        bool running = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        float targetSpeed = hasInput ? (running ? runSpeed : walkSpeed) : 0f;
        Vector3 targetVel = hasInput ? wishDir * targetSpeed : Vector3.zero;

        // --- 전방 충돌 감지 및 벽 따라가기 ---
        Vector3 vel = targetVel;
        if (vel.sqrMagnitude > 0f)
        {
            Vector3 dir = vel.normalized;
            float dist = Mathf.Max(vel.magnitude * Time.fixedDeltaTime + skin, probeDistance);

            if (rb.SweepTest(dir, out RaycastHit hit, dist))
            {
                Vector3 slide = Vector3.ProjectOnPlane(dir, hit.normal).normalized;
                vel = slide * targetSpeed;
                rb.position = rb.position + hit.normal * skin * 0.5f;
            }
        }

        // --- 리지드바디 이동 ---
        rb.velocity = vel;

        // --- 회전 ---
        if (hasInput && vel.sqrMagnitude > 0.0001f)
        {
            Quaternion look = Quaternion.LookRotation(vel.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, look, rotateLerp * Time.fixedDeltaTime);
        }

        // --- 애니메이션 업데이트 ---
        if (anim)
        {
            float norm = (targetSpeed <= 0f) ? 0f : (running ? 1f : 0.5f);
            anim.SetFloat("Speed", norm);
        }
    }
}
