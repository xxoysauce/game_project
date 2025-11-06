using UnityEngine;

/// <summary>

/// </summary>
public class FollowCam : MonoBehaviour
{
    [Header("Target")]
    public Transform target;                           
    [Tooltip("장면이 x배로 커졌다면 여기에도 같은 배수를 넣으세요 (예: 100)")]
    public float worldScale = 1f;

    [Header("Framing")]
    [Tooltip("위/아래 각도")]
    public float pitch = 35f;
    [Tooltip("좌/우 각도")]
    public float yaw = 45f;
    [Tooltip("우클릭으로 카메라를 드래그 회전할 수 있게 할지")]
    public bool allowOrbit = false;
    public float orbitSpeed = 120f;

    [Header("Distance / Zoom")]
    [Tooltip("기본 거리")]
    public float distance = 8f;
    public float minDistance = 6.5f;
    public float maxDistance = 12f;
    [Tooltip("마우스 휠 줌 사용 여부")]
    public bool allowZoom = false;
    public float zoomSpeed = 8f;

    [Header("Smoothing")]
    [Tooltip("카메라 추적/회전 보간 속도")]
    public float followLerp = 12f;
    [Tooltip("타겟 기준 시선 오프셋 (머리 위로 살짝)")]
    public Vector3 lookOffset = new Vector3(0f, 1.2f, 0f);

    [Header("Heading Align (옵션)")]
    [Tooltip("우클릭을 안 할 때, 캐릭터 진행 방향으로 서서히 정렬")]
    public bool alignToTargetHeading = true;
    public float headingLerp = 6f;

    [Header("Collision")]
    public LayerMask collisionMask;            
    [Tooltip("카메라 충돌 캡슐 반경(스케일과 함께 보정됩니다)")]
    public float collisionRadius = 0.2f;
    [Tooltip("충돌 표면에서 얼마나 더 띄울지(안 파고들게)")]
    public float collisionSurfacePadding = 0.1f;

    // 내부
    float _curDistance;
    float _minPitch = 15f, _maxPitch = 70f;

    void Awake()
    {
        // 타겟 자동 할당
        if (!target)
        {
            var t = GameObject.FindWithTag("Player");
            if (t) target = t.transform;
        }

 
        if (Mathf.Abs(worldScale - 1f) > 0.001f)
        {
            distance *= worldScale;
            minDistance *= worldScale;
            maxDistance *= worldScale;
            lookOffset *= worldScale;
            collisionRadius *= worldScale;
            collisionSurfacePadding *= worldScale;
        }

        _curDistance = Mathf.Clamp(distance, minDistance, maxDistance);

        if (!target)
            Debug.LogWarning("[FollowCam] target이 비어 있어요! (Player 태그를 확인하세요)");
    }

    void Update()
    {
        if (!target) return;

        // 휠/핀치 줌
        if (allowZoom)
        {
            float scroll = Input.mouseScrollDelta.y;
            if (Mathf.Abs(scroll) > 0.01f)
                distance -= scroll * zoomSpeed * Time.deltaTime;

            if (Input.touchCount == 2)
            {
                Touch a = Input.GetTouch(0);
                Touch b = Input.GetTouch(1);
                var prevMag = (a.position - a.deltaPosition - (b.position - b.deltaPosition)).magnitude;
                var curMag = (a.position - b.position).magnitude;
                distance -= (curMag - prevMag) * 0.01f;
            }

            distance = Mathf.Clamp(distance, minDistance, maxDistance);
        }


        if (allowOrbit && Input.GetMouseButton(1))
        {
            yaw += Input.GetAxis("Mouse X") * orbitSpeed * Time.deltaTime;
            pitch -= Input.GetAxis("Mouse Y") * (orbitSpeed * 0.6f) * Time.deltaTime;
            pitch = Mathf.Clamp(pitch, _minPitch, _maxPitch);
        }

        else if (alignToTargetHeading)
        {
            Vector3 fwd = target.forward; 
            fwd.y = 0f;
            if (fwd.sqrMagnitude > 0.0001f)
            {
                float targetYaw = Mathf.Atan2(fwd.x, fwd.z) * Mathf.Rad2Deg;
                yaw = Mathf.LerpAngle(yaw, targetYaw, headingLerp * Time.deltaTime);
            }
        }
    }

    void LateUpdate()
    {
        if (!target) return;

        _curDistance = Mathf.Lerp(_curDistance, distance, Time.deltaTime * 8f);


        Vector3 from = target.position + lookOffset;

   
        Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 desiredPos = from + rot * (Vector3.back * _curDistance);


        Vector3 dir = desiredPos - from;
        float dist = dir.magnitude;
        if (dist > 0.0001f)
        {
            dir.Normalize();

            float safe = Mathf.Min(_curDistance, dist);
            int mask = (collisionMask.value == 0) ? ~0 : collisionMask.value;

            if (Physics.SphereCast(from, collisionRadius, dir, out RaycastHit hit, dist, mask))
            {

                float hitDist = Mathf.Max(hit.distance - collisionSurfacePadding, minDistance);
                safe = Mathf.Min(safe, hitDist);
            }

            desiredPos = from + dir * safe;
        }


        transform.position = Vector3.Lerp(transform.position, desiredPos, Time.deltaTime * followLerp);

        Vector3 lookDir = (from - transform.position);
        if (lookDir.sqrMagnitude > 0.0001f)
        {
            Quaternion lookRot = Quaternion.LookRotation(lookDir.normalized, Vector3.up);
            transform.rotation = Quaternion.Lerp(transform.rotation, lookRot, Time.deltaTime * followLerp);
        }
    }
}
