using UnityEngine;

public class OpticalMarkerBasedCamera : MonoBehaviour
{
    public Transform hmdTransform;     // The tracked object
    public LayerMask obstructionMask;  // Which layers block the view

    public float? AngleToHMD { get; private set; }

    void Update()
    {
        UpdateAngleToHMD();
    }

    private void UpdateAngleToHMD()
    {
        if (hmdTransform == null)
        {
            AngleToHMD = null;
            return;
        }

        if (!HasLineOfSight())
        {
            AngleToHMD = null;
            return;
        }

        Vector3 toHMD = hmdTransform.position - transform.position;
        // transform.forward is the forward direction of the camera, aka. the blue z-axis
        float angle = Vector3.Angle(transform.forward, toHMD);
        AngleToHMD = angle;

        Debug.Log($"Angle to HMD: {angle:F2}°");
    }

    private bool HasLineOfSight()
    {
        Vector3 direction = hmdTransform.position - transform.position;
        float distance = direction.magnitude;

        // Perform raycast to check if anything blocks the view
        if (Physics.Raycast(transform.position, direction.normalized, distance, obstructionMask))
        {
            return false; // Obstructed
        }

        return true; // Clear view
    }

    void OnGUI()
    {
        if (AngleToHMD.HasValue)
        {
            GUI.Label(new Rect(10, 10, 300, 20), $"Angle to HMD: {AngleToHMD.Value:F1}°");
        }
        else
        {
            GUI.Label(new Rect(10, 10, 300, 20), "No line of sight to HMD");
        }
    }
}
