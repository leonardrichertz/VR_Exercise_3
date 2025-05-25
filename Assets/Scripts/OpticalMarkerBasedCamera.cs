using TMPro;
using UnityEngine;

public class OpticalMarkerBasedCamera : MonoBehaviour
{
    public Transform hmdTransform;     // The tracked object
    public LayerMask obstructionMask;  // Which layers block the view

    [SerializeField] private TextMeshProUGUI angleText; // World-space UI reference


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

        UpdateUIText($"Angle to HMD: {angle:F1}°");
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
            Debug.Log($"Angle to HMD: {AngleToHMD.Value:F1}°");
            GUI.Label(new Rect(10, 10, 300, 20), $"Angle to HMD: {AngleToHMD.Value:F1}°");
        }
        else
        {
            Debug.Log("No line of sight to HMD");
            GUI.Label(new Rect(10, 10, 300, 20), "No line of sight to HMD");
        }
    }

    private void UpdateUIText(string text)
    {
        if (angleText != null)
        {
            angleText.text = text;
        }
    }
}
