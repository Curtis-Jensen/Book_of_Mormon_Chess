using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float moveSpeed = 5f; // Speed of camera movement
    public float zoomSpeed = 10f; // Speed of camera zooming
    public float minZoom = 1f; // Minimum zoom level

    Camera cam;
    float maxZoom;

    void Start()
    {
        cam = GetComponent<Camera>();
        maxZoom = cam.orthographicSize * 1.5f;
    }

    void Update()
    {
        // Handle camera movement
        float horizontal = Input.GetAxis("Horizontal"); // A/D or Left/Right Arrow
        float vertical = Input.GetAxis("Vertical"); // W/S or Up/Down Arrow

        Vector3 direction = new(horizontal, vertical, 0f);
        transform.position += direction * moveSpeed * Time.deltaTime;

        // Handle camera zooming
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        float newSize = Camera.main.orthographicSize - scroll * zoomSpeed;
        cam.orthographicSize = Mathf.Clamp(newSize, minZoom, maxZoom);
    }
}
