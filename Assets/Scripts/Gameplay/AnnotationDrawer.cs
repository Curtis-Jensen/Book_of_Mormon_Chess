using UnityEngine;

public class AnnotationDrawer : MonoBehaviour
{
    [SerializeField] private GameObject circlePrefab;
    private GameObject circleInstance;

    void Update()
    {
        if (Input.GetMouseButtonDown(1)) // Right mouse button
        {
            Vector2 mousePos = Input.mousePosition;
            Vector2 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
            
            RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);
            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                ToggleCircle();
            }
        }
    }

    void ToggleCircle()
    {
        if (circleInstance != null)
        {
            Destroy(circleInstance);
            circleInstance = null;
        }
        else if (circlePrefab != null)
        {
            circleInstance = Instantiate(circlePrefab, transform.position, Quaternion.identity, transform);
        }
    }
}
