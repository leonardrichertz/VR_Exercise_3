using UnityEngine;
using TMPro;

public class IMU : MonoBehaviour
{
    [Header("IMU Parameters")]
    [SerializeField] private float accelerometerNoise = 0.01f;  // m/s²
    [SerializeField] private float gyroNoise = 0.1f;           // degrees/s
    [SerializeField] private float lowPassFilterFactor = 0.1f;  // 0 to 1, higher = more smoothing
    [SerializeField] private float driftCorrectionStrength = 0.1f;  // 0 to 1, higher = faster correction

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI accelerationText;
    [SerializeField] private TextMeshProUGUI rotationText;

    // Public properties for external access
    public Vector3 LinearAcceleration { get; private set; }   // m/s²
    public Vector3 Velocity { get; private set; }             // m/s
    public Vector3 AngularVelocity { get; private set; }      // deg/s
    public Quaternion Orientation { get; private set; }       // Current orientation

    // Private tracking variables
    private Vector3 lastPosition;
    private Quaternion lastRotation;
    private Vector3 lastVelocity;
    private Vector3 filteredAcceleration;
    private Vector3 filteredAngularVelocity;
    private Vector3 gravityVector = Vector3.zero;
    private bool isInitialized = false;

    void Start()
    {
        InitializeSensors();
    }

    private void InitializeSensors()
    {
        lastPosition = transform.position;
        lastRotation = transform.rotation;
        Orientation = transform.rotation;
        lastVelocity = Vector3.zero;
        filteredAcceleration = Vector3.zero;
        filteredAngularVelocity = Vector3.zero;
        gravityVector = Vector3.up * Physics.gravity.magnitude;
        isInitialized = true;
    }

    void Update()
    {
        if (!isInitialized)
        {
            InitializeSensors();
            return;
        }

        float deltaTime = Time.deltaTime;
        if (deltaTime <= 0f) return;

        UpdateLinearMotion(deltaTime);
        UpdateAngularMotion(deltaTime);
        ApplyDriftCorrection(deltaTime);
        UpdateUIText();
    }

    private void UpdateLinearMotion(float deltaTime)
    {
        // Calculate raw acceleration
        Vector3 currentPosition = transform.position;
        Vector3 currentVelocity = (currentPosition - lastPosition) / deltaTime;
        Vector3 rawAcceleration = (currentVelocity - lastVelocity) / deltaTime;

        // Add simulated sensor noise
        rawAcceleration += Random.insideUnitSphere * accelerometerNoise;

        // Apply low-pass filter
        filteredAcceleration = Vector3.Lerp(filteredAcceleration, rawAcceleration, lowPassFilterFactor);

        // Remove gravity effect
        LinearAcceleration = filteredAcceleration + gravityVector;

        // Update velocity with filtered acceleration
        Velocity = Vector3.Lerp(lastVelocity, currentVelocity, lowPassFilterFactor);

        // Store values for next frame
        lastPosition = currentPosition;
        lastVelocity = Velocity;
    }

    private void UpdateAngularMotion(float deltaTime)
    {
        // Calculate raw angular velocity
        Quaternion deltaRotation = transform.rotation * Quaternion.Inverse(lastRotation);
        deltaRotation.ToAngleAxis(out float angle, out Vector3 axis);

        if (angle > 180f) angle -= 360f; // Handle wrapping
        Vector3 rawAngularVelocity = axis.normalized * angle / deltaTime;

        // Add simulated gyro noise
        rawAngularVelocity += Random.insideUnitSphere * gyroNoise;

        // Apply low-pass filter
        filteredAngularVelocity = Vector3.Lerp(filteredAngularVelocity, rawAngularVelocity, lowPassFilterFactor);
        AngularVelocity = filteredAngularVelocity;

        // Update orientation
        Quaternion deltaOrient = Quaternion.AngleAxis(AngularVelocity.magnitude * deltaTime, AngularVelocity.normalized);
        Orientation *= deltaOrient;

        lastRotation = transform.rotation;
    }

    private void ApplyDriftCorrection(float deltaTime)
    {
        // Correct velocity drift
        if (Velocity.magnitude < 0.01f)
        {
            Velocity = Vector3.Lerp(Velocity, Vector3.zero, driftCorrectionStrength);
        }

        // Correct orientation drift by slightly pulling towards the true rotation
        Orientation = Quaternion.Slerp(Orientation, transform.rotation, driftCorrectionStrength * deltaTime);
    }

    private void UpdateUIText()
    {
        if (accelerationText != null)
            accelerationText.text = $"Acc: {LinearAcceleration:F2} m/s²\nVel: {Velocity:F2} m/s";

        if (rotationText != null)
            rotationText.text = $"Angular Vel: {AngularVelocity:F2}°/s\nOrientation: {Orientation.eulerAngles:F1}°";
    }

    void OnDrawGizmos()
    {
        if (!isInitialized) return;

        // Draw acceleration vector
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + LinearAcceleration);

        // Draw velocity vector
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + Velocity);

        // Draw orientation
        Gizmos.color = Color.green;
        Vector3 forward = transform.position + Orientation * Vector3.forward;
        Gizmos.DrawLine(transform.position, forward);
    }
}
