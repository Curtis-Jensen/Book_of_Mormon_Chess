using UnityEngine;

public class AnnotationDrawer : MonoBehaviour
{
    [SerializeField] private GameObject circlePrefab;
    private GameObject circleInstance;

    void OnMouseDown(int button)
    {
        Debug.Log($"working!");
        if (button == 1) // Right mouse button
        {
            ToggleCircle();
        }
    }

    public void ToggleCircle()
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
