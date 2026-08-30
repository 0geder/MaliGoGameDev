using UnityEngine;

public class MaliGoCameraController : MonoBehaviour
{
    [Header("Target Tracking")]
    public Transform target;
    public Vector3 offset = new Vector3(-8f, 8f, -8f);
    public float smoothSpeed = 6f;
    public bool followTarget = true;

    [Header("Isometric Angle")]
    public Vector3 isometricRotation = new Vector3(30f, 45f, 0f);

    [Header("Zoom Settings")]
    public float zoomSpeed = 5f;
    public float minZoom = 3f;
    public float maxZoom = 12f;

    [Header("Boundaries")]
    public Vector3 minBounds = new Vector3(-35f, 0f, -35f);
    public Vector3 maxBounds = new Vector3(35f, 35f, 35f);

    private Camera mainCamera;
    private Vector3 currentVelocity = Vector3.zero;

    void Awake()
    {
        mainCamera = GetComponent<Camera>();
        transform.rotation = Quaternion.Euler(isometricRotation);
    }

    void Start()
    {
        if (target == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
        }

        if (target != null && followTarget)
        {
            transform.position = target.position + offset;
        }
    }

    void LateUpdate()
    {
        if (followTarget && target != null)
        {
            Vector3 desiredPos = target.position + offset;
            transform.position = Vector3.SmoothDamp(transform.position, desiredPos, ref currentVelocity, 1f / Mathf.Max(0.1f, smoothSpeed));
        }

        // Keep isometric camera angle locked
        transform.rotation = Quaternion.Euler(isometricRotation);
    }

    void Update()
    {
        HandleZooming();
    }

    void HandleZooming()
    {
        if (mainCamera == null) return;

        float scroll = 0f;
#if ENABLE_INPUT_SYSTEM
        if (UnityEngine.InputSystem.Mouse.current != null)
        {
            scroll = UnityEngine.InputSystem.Mouse.current.scroll.ReadValue().y * 0.01f;
        }
#endif
        if (Mathf.Abs(scroll) < 0.001f)
        {
            try
            {
                scroll = Input.GetAxis("Mouse ScrollWheel");
            }
            catch { }
        }

        if (Mathf.Abs(scroll) > 0.001f)
        {
            mainCamera.orthographicSize -= scroll * zoomSpeed;
            mainCamera.orthographicSize = Mathf.Clamp(mainCamera.orthographicSize, minZoom, maxZoom);
        }
    }
}