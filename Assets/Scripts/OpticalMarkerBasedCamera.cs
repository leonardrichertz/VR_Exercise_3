using TMPro;
using UnityEngine;

public class OpticalMarkerBasedCamera : MonoBehaviour
{
    public Transform hmdTransform;     // The tracked object
    public LayerMask obstructionMask;  // Which layers block the view

    [SerializeField] private float horizontalFOV = 60f;  // Horizontal field of view in degrees
    [SerializeField] private float verticalFOV = 45f;    // Vertical field of view in degrees
    [SerializeField] private float maxTrackingDistance = 10f;  // Maximum distance for reliable tracking
    [SerializeField] private TextMeshProUGUI angleText;

    public Vector2? AnglestoHMD { get; private set; }  // x: horizontal angle, y: vertical angle
    public bool HasValidTracking => AnglestoHMD.HasValue;
    public Vector3 Position => transform.position;
    public Vector3 Forward => transform.forward;

    void Update()
    {
        UpdateAnglesToHMD();
    }

    private void UpdateAnglesToHMD()
    {
        if (hmdTransform == null || !HasLineOfSight())
        {
            AnglestoHMD = null;
            UpdateUIText("No tracking");
            return;
        }

        Vector3 toHMD = hmdTransform.position - transform.position;
        float distance = toHMD.magnitude;

        if (distance > maxTrackingDistance)
        {
            AnglestoHMD = null;
            UpdateUIText("Target too far");
            return;
        }

        // Project the vector onto the camera's local coordinate system
        Vector3 localToHMD = transform.InverseTransformDirection(toHMD);

        // Calculate horizontal and vertical angles
        float horizontalAngle = Mathf.Atan2(localToHMD.x, localToHMD.z) * Mathf.Rad2Deg;
        float verticalAngle = Mathf.Atan2(localToHMD.y, localToHMD.z) * Mathf.Rad2Deg;

        // Check if within FOV
        if (Mathf.Abs(horizontalAngle) > horizontalFOV / 2 || Mathf.Abs(verticalAngle) > verticalFOV / 2)
        {
            AnglestoHMD = null;
            UpdateUIText("Outside FOV");
            return;
        }

        AnglestoHMD = new Vector2(horizontalAngle, verticalAngle);
        UpdateUIText($"H: {horizontalAngle:F1}° V: {verticalAngle:F1}°");
    }

    private bool HasLineOfSight()
    {
        Vector3 direction = hmdTransform.position - transform.position;
        float distance = direction.magnitude;

        // Perform raycast to check if anything blocks the view
        if (Physics.Raycast(transform.position, direction.normalized, out RaycastHit hit, distance, obstructionMask))
        {
            Debug.DrawLine(transform.position, hit.point, Color.red);
            return false; // Obstructed
        }

        Debug.DrawLine(transform.position, transform.position + direction, Color.green);
        return true; // Clear view
    }

    void OnDrawGizmos()
    {
        // Draw FOV visualization
        Gizmos.color = Color.yellow;
        float horizontalRadians = horizontalFOV * Mathf.Deg2Rad * 0.5f;
        float verticalRadians = verticalFOV * Mathf.Deg2Rad * 0.5f;

        Vector3 forward = transform.forward * maxTrackingDistance;
        Vector3 right = transform.right * maxTrackingDistance * Mathf.Tan(horizontalRadians);
        Vector3 up = transform.up * maxTrackingDistance * Mathf.Tan(verticalRadians);

        // Draw FOV lines
        Gizmos.DrawLine(transform.position, transform.position + forward + right);
        Gizmos.DrawLine(transform.position, transform.position + forward - right);
        Gizmos.DrawLine(transform.position, transform.position + forward + up);
        Gizmos.DrawLine(transform.position, transform.position + forward - up);
    }

    private void UpdateUIText(string text)
    {
        if (angleText != null)
        {
            angleText.text = text;
        }
    }
}
