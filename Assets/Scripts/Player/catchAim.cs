using UnityEngine;

public class catchAim : MonoBehaviour
{
    [SerializeField] private Transform aimArrow;
    [SerializeField] private Camera mainCamera;

    void Update()
    {
        // get mouse position in screen space
        Vector3 mouseScreenPosition = Input.mousePosition;

        // convert to world space
        Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(new Vector3(mouseScreenPosition.x, mouseScreenPosition.y, mainCamera.nearClipPlane));

        // get direction
        Vector3 direction = (mouseWorldPosition - aimArrow.position).normalized;

        // calculate rotation to face the direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        aimArrow.rotation = Quaternion.Euler(0, 0, angle);
    }
}
