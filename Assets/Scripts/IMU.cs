using UnityEngine;

public class IMU : MonoBehaviour
{
    // Beschleunigung = Änderung der Geschwindigkeit pro Zeit
    public Vector3 linearAcceleration { get; private set; }   // m/s²

    // Geschwindigkeit (velocity) in Weltkoordinaten
    public Vector3 velocity { get; private set; }             // m/s

    // Winkelgeschwindigkeit = Änderung der Orientierung pro Zeit
    public Vector3 angularVelocity { get; private set; }      // deg/s

    private Vector3 lastPosition;
    private Quaternion lastRotation;
    private Vector3 lastVelocity;

    void Start()
    {
        lastPosition = transform.position;
        lastRotation = transform.rotation;
        lastVelocity = Vector3.zero;
    }

    void Update()
    {
        float deltaTime = Time.deltaTime;
        if (deltaTime <= 0f) return;

        // Linear velocity and acceleration
        Vector3 currentPosition = transform.position;
        velocity = (currentPosition - lastPosition) / deltaTime;
        linearAcceleration = (velocity - lastVelocity) / deltaTime;

        lastPosition = currentPosition;
        lastVelocity = velocity;

        // Angular velocity (degrees per second)
        Quaternion deltaRotation = transform.rotation * Quaternion.Inverse(lastRotation);
        deltaRotation.ToAngleAxis(out float angle, out Vector3 axis);
        if (angle > 180f) angle -= 360f; // Handle wrapping
        angularVelocity = axis.normalized * angle / deltaTime;

        lastRotation = transform.rotation;
    }

    void OnGUI()
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = 10;
        style.normal.textColor = Color.black;

        // Define some spacing
        float padding = 400f;
        float labelHeight = 20f;
        float labelWidth = Mathf.Min(10f, Screen.width - 2 * padding);  // Max 500 or whatever fits

        float x = Screen.width - labelWidth - padding;
        float y = padding/2;

        GUI.Label(new Rect(x, y, labelWidth, labelHeight), $"Beschleunigung: {linearAcceleration:F2}", style);
        y += labelHeight;

        GUI.Label(new Rect(x, y, labelWidth, labelHeight), $"Lage: {transform.rotation:F2}", style);
    }
}
