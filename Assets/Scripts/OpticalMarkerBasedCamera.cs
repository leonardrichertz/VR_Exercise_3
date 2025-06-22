using TMPro;
using UnityEngine;

public class OpticalMarkerBasedCamera : MonoBehaviour
{
    public Transform hmdTransform;     // The tracked object
    public LayerMask obstructionMask;  // Which layers block the view

    [SerializeField] private float horizontalFOV = 60f;  // Horizontal field of view in degrees
    [SerializeField] private float verticalFOV = 45f;    // Vertical field of view in degrees
    [SerializeField] private float maxTrackingDistance = 10f;  // Maximum distance for reliable tracking

    [SerializeField] private Color trackingColor = Color.green;
    [SerializeField] private Color noTrackingColor = Color.red;
    [SerializeField] private Color outOfRangeColor = Color.yellow;
    [SerializeField] private Color fovBlockedColor = Color.gray;

    public Vector2? AnglestoHMD { get; private set; }  // x: horizontal angle, y: vertical angle
    public bool HasValidTracking => AnglestoHMD.HasValue;
    public Vector3 Position => transform.position;
    public Vector3 Forward => transform.forward;

    private Renderer _renderer;


    void Update()
    {
        _renderer = GetComponent<Renderer>();
        if (_renderer == null)
        {
            Debug.LogWarning("No Renderer found on OpticalMarkerBasedCamera GameObject.");
        }
        UpdateAnglesToHMD();
    }
      private void UpdateAnglesToHMD()
      {
          if (hmdTransform == null || !HasLineOfSight())
          {
              AnglestoHMD = null;
              UpdateVisualColor(noTrackingColor);
              return;
          }

          Vector3 toHMD = hmdTransform.position - transform.position;
          float distance = toHMD.magnitude;

          if (distance > maxTrackingDistance)
          {
              AnglestoHMD = null;
              UpdateVisualColor(outOfRangeColor);
              return;
          }

          // KORREKTUR: Winkel relativ zur Kamera-Forward-Richtung berechnen
          Vector3 cameraForward = transform.forward;
          Vector3 toHMDNormalized = toHMD.normalized;
        
          // Horizontaler Winkel (um Y-Achse)
          Vector3 cameraForwardXZ = new Vector3(cameraForward.x, 0, cameraForward.z).normalized;
          Vector3 toHMDXZ = new Vector3(toHMDNormalized.x, 0, toHMDNormalized.z).normalized;
          float horizontalAngle = Vector3.SignedAngle(cameraForwardXZ, toHMDXZ, Vector3.up);
        
          // Vertikaler Winkel
          float verticalAngle = Mathf.Asin(toHMDNormalized.y) * Mathf.Rad2Deg;

          // FOV-Check
          if (Mathf.Abs(horizontalAngle) > horizontalFOV / 2 || Mathf.Abs(verticalAngle) > verticalFOV / 2)
          {
              AnglestoHMD = null;
              UpdateVisualColor(fovBlockedColor);
              return;
          }

          AnglestoHMD = new Vector2(horizontalAngle, verticalAngle);
          UpdateVisualColor(trackingColor);    }

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

    private void UpdateVisualColor(Color color)
    {
        if (_renderer != null)
        {
            _renderer.material.color = color;
        }
    }
}
