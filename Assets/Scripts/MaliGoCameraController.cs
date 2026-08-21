using UnityEngine;

public class MaliGoCameraController : MonoBehaviour
{
    [Header("Camera Settings")]
    public float panSpeed = 10f;
    public float zoomSpeed = 5f;
    public float minZoom = 8f;
    public float maxZoom = 20f;

    [Header("Boundaries")]
    public Vector3 minBounds = new Vector3(-20f, 18f, -20f);
    public Vector3 maxBounds = new Vector3(20f, 18f, 20f);

    private Camera mainCamera;

    void Start()
    {
        mainCamera = GetComponent<Camera>();
    }

    void Update()
    {
        HandlePanning();
        HandleZooming();
    }

    void HandlePanning()
    {
        if (Input.GetMouseButton(2) || (Input.GetMouseButton(0) && Input.GetKey(KeyCode.Space)))
        {
            float moveX = Input.GetAxis("Mouse X") * panSpeed * Time.deltaTime;
            float moveZ = Input.GetAxis("Mouse Y") * panSpeed * Time.deltaTime;

            Vector3 move = transform.right * -moveX + transform.forward * -moveZ;
            move.y = 0;

            transform.position += move;
            transform.position = new Vector3(
                Mathf.Clamp(transform.position.x, minBounds.x, maxBounds.x),
                transform.position.y,
                Mathf.Clamp(transform.position.z, minBounds.z, maxBounds.z)
            );
        }
    }

    void HandleZooming()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            mainCamera.orthographicSize -= scroll * zoomSpeed;
            mainCamera.orthographicSize = Mathf.Clamp(mainCamera.orthographicSize, minZoom, maxZoom);
        }
    }
}