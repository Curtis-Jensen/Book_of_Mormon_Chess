using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    public float moveSpeed = 5f; // Speed of camera movement
    public float zoomSpeed = 10f; // Speed of camera zooming
    public float minZoom = 1f; // Minimum zoom level
    public Vector3 backGroundOffset;
    public GameObject background;

    Camera cam;
    float maxZoom;

    void Start()
    {
        cam = GetComponent<Camera>();

        var boardWidth = PlayerPrefs.GetInt("boardSize");

        cam.orthographicSize = (float)boardWidth / 2;
        maxZoom = cam.orthographicSize * 1.5f;

        CenterBoard(boardWidth);
    }

    void CenterBoard(int boardWidth)
    {
        float centerLength = boardWidth / 2;

        bool evenBoard = boardWidth % 2 == 0;
        if (evenBoard)
        {
            centerLength -= 0.5f;
        }
        var centeredPosition = new Vector3(centerLength, centerLength, -10);
        transform.position = centeredPosition;

        background.transform.position = centeredPosition + (backGroundOffset * boardWidth);
        background.transform.localScale = new Vector3(
            background.transform.localScale.x * boardWidth,  // Width  (x-axis)
            background.transform.localScale.y * boardWidth,  // Height (y-axis)
            background.transform.localScale.z);
    }

    void Update()
    {
        // Handle camera movement
        float horizontal = Input.GetAxis("Horizontal"); // A/D or Left/Right Arrow
        float vertical = Input.GetAxis("Vertical"); // W/S or Up/Down Arrow

        Vector3 direction = new(horizontal, vertical, 0f);
        transform.position += moveSpeed * Time.deltaTime * direction;

        // Handle camera zooming
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        float newSize = Camera.main.orthographicSize - scroll * zoomSpeed;
        cam.orthographicSize = Mathf.Clamp(newSize, minZoom, maxZoom);
    }
}
