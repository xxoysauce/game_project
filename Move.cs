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
    public Transform cam;                 
    public bool useCameraYawOnly = true;   

    [Header("Slide & Probe")]
    public float skin = 0.05f;             
    public float probeDistance = 0.6f;     

    Rigidbody rb;
    Animator anim;


    private OpenAIConnector connector;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();

 
        rb.isKinematic = false;
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;


        connector = FindObjectOfType<OpenAIConnector>();
    }

    private void Update()
    {

        if (connector != null && connector.IsDialogueActive)
        {
            if (rb) rb.velocity = Vector3.zero;
            if (anim) anim.SetFloat("Speed", 0f);
            return;
        }
    }

    private void FixedUpdate()
    {

        if (connector != null && connector.IsDialogueActive)
            return;


        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        Vector3 input = new Vector3(x, 0f, z);
        bool hasInput = input.sqrMagnitude > deadZone * deadZone;


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


        bool running = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        float targetSpeed = hasInput ? (running ? runSpeed : walkSpeed) : 0f;
        Vector3 targetVel = hasInput ? wishDir * targetSpeed : Vector3.zero;


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


        rb.velocity = vel;


        if (hasInput && vel.sqrMagnitude > 0.0001f)
        {
            Quaternion look = Quaternion.LookRotation(vel.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, look, rotateLerp * Time.fixedDeltaTime);
        }


        if (anim)
        {
            float norm = (targetSpeed <= 0f) ? 0f : (running ? 1f : 0.5f);
            anim.SetFloat("Speed", norm);
        }
    }
}
