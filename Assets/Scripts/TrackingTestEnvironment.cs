using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TrackingTestEnvironment : MonoBehaviour
{
    [Header("References")]
    public Transform hmdTransform;  // Reference to the XR Rig/HMD
    public OpticalMarkerBasedCamera[] cameras;  // Array of tracking cameras
    public IMU imuDevice;  // Reference to the IMU attached to the HMD
    public GameObject trackingVisualization;  // Sphere or other object showing estimated position
    
    [Header("Visualization")]
    public bool showTrackingRays = true;
    public bool showCameraFOV = true;
    public bool showIMUVectors = true;
    public LineRenderer[] trackingRays;
    
    [Header("UI")]
    public TextMeshProUGUI statusText;
    public Toggle useOpticalToggle;
    public Toggle useIMUToggle;
    public Slider noiseSlider;
    
    private Vector3 estimatedPosition;
    private Quaternion estimatedRotation;
    private bool isInitialized;

    void Start()
    {
        InitializeComponents();
        SetupUI();
    }

    private void InitializeComponents()
    {
        if (trackingVisualization == null)
        {
            // Create a red sphere if no visualization object is assigned
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.localScale = Vector3.one * 0.2f;
            sphere.GetComponent<Renderer>().material.color = Color.red;
            trackingVisualization = sphere;
        }

        // Initialize tracking rays if needed
        if (showTrackingRays && cameras != null)
        {
            trackingRays = new LineRenderer[cameras.Length];
            for (int i = 0; i < cameras.Length; i++)
            {
                GameObject rayObj = new GameObject($"TrackingRay_{i}");
                LineRenderer lr = rayObj.AddComponent<LineRenderer>();
                lr.startWidth = 0.02f;
                lr.endWidth = 0.02f;
                lr.material = new Material(Shader.Find("Sprites/Default"));
                lr.startColor = Color.yellow;
                lr.endColor = Color.yellow;
                trackingRays[i] = lr;
            }
        }

        estimatedPosition = hmdTransform.position;
        estimatedRotation = hmdTransform.rotation;
        isInitialized = true;
    }

    private void SetupUI()
    {
        if (useOpticalToggle != null)
            useOpticalToggle.onValueChanged.AddListener(OnOpticalTrackingToggled);
        
        if (useIMUToggle != null)
            useIMUToggle.onValueChanged.AddListener(OnIMUTrackingToggled);
        
        if (noiseSlider != null)
            noiseSlider.onValueChanged.AddListener(OnNoiseValueChanged);
    }

    void Update()
    {
        if (!isInitialized) return;

        UpdateTracking();
        UpdateVisualization();
        UpdateUI();
    }

    private void UpdateTracking()
    {
        if (useOpticalToggle != null && !useOpticalToggle.isOn && useIMUToggle != null && !useIMUToggle.isOn)
        {
            // If both tracking systems are disabled, just follow the actual position
            estimatedPosition = hmdTransform.position;
            estimatedRotation = hmdTransform.rotation;
            return;
        }

        Vector3 opticalPosition = Vector3.zero;
        Quaternion opticalRotation = Quaternion.identity;
        int validCameras = 0;

        // Update from optical tracking
        if (useOpticalToggle == null || useOpticalToggle.isOn)
        {
            foreach (var camera in cameras)
            {
                if (camera.HasValidTracking)
                {
                    Vector2 angles = camera.AnglestoHMD.Value;
                    Vector3 direction = Quaternion.Euler(-angles.y, angles.x, 0) * Vector3.forward;
                    Ray ray = new Ray(camera.Position, direction);
                    
                    // Simple triangulation - in reality you'd want to use a more sophisticated algorithm
                    float distanceToPlane = Vector3.Dot(hmdTransform.position - camera.Position, camera.Forward);
                    Vector3 estimatedPoint = ray.GetPoint(distanceToPlane);
                    
                    opticalPosition += estimatedPoint;
                    validCameras++;
                }
            }

            if (validCameras > 0)
            {
                opticalPosition /= validCameras;
                // Simple rotation estimation from position difference
                Vector3 toEstimated = opticalPosition - estimatedPosition;
                if (toEstimated.magnitude > 0.01f)
                {
                    opticalRotation = Quaternion.LookRotation(toEstimated);
                }
            }
        }

        // Update from IMU
        if (useIMUToggle == null || useIMUToggle.isOn)
        {
            Vector3 imuPosition = estimatedPosition + imuDevice.Velocity * Time.deltaTime;
            Quaternion imuRotation = imuDevice.Orientation;

            if (validCameras > 0)
            {
                // Fusion between optical and IMU
                float opticalWeight = 0.7f;  // Prefer optical when available
                estimatedPosition = Vector3.Lerp(imuPosition, opticalPosition, opticalWeight);
                estimatedRotation = Quaternion.Slerp(imuRotation, opticalRotation, opticalWeight);
            }
            else
            {
                // IMU only
                estimatedPosition = imuPosition;
                estimatedRotation = imuRotation;
            }
        }
        else if (validCameras > 0)
        {
            // Optical only
            estimatedPosition = opticalPosition;
            estimatedRotation = opticalRotation;
        }
    }

    private void UpdateVisualization()
    {
        if (trackingVisualization != null)
        {
            trackingVisualization.transform.position = estimatedPosition;
            trackingVisualization.transform.rotation = estimatedRotation;
        }

        if (showTrackingRays && trackingRays != null)
        {
            for (int i = 0; i < cameras.Length && i < trackingRays.Length; i++)
            {
                if (cameras[i].HasValidTracking)
                {
                    trackingRays[i].enabled = true;
                    trackingRays[i].SetPosition(0, cameras[i].Position);
                    trackingRays[i].SetPosition(1, estimatedPosition);
                }
                else
                {
                    trackingRays[i].enabled = false;
                }
            }
        }
    }

    private void UpdateUI()
    {
        if (statusText != null)
        {
            string status = "Tracking Status:\n";
            status += $"Position: {estimatedPosition:F2}\n";
            status += $"Rotation: {estimatedRotation.eulerAngles:F1}\n";
            status += $"Error: {Vector3.Distance(estimatedPosition, hmdTransform.position):F3}m\n";
            status += "Active Cameras: ";
            
            foreach (var camera in cameras)
            {
                status += camera.HasValidTracking ? "✓" : "×";
            }

            statusText.text = status;
        }
    }

    private void OnOpticalTrackingToggled(bool enabled)
    {
        foreach (var camera in cameras)
        {
            camera.gameObject.SetActive(enabled);
        }
    }

    private void OnIMUTrackingToggled(bool enabled)
    {
        if (imuDevice != null)
            imuDevice.gameObject.SetActive(enabled);
    }

    private void OnNoiseValueChanged(float value)
    {
        // Adjust noise levels in the simulation
        // This could be implemented by passing the noise value to the tracking components
    }

    void OnDrawGizmos()
    {
        if (!isInitialized) return;

        // Draw estimated position error
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(estimatedPosition, hmdTransform.position);
        
        // Draw coordinate axes at estimated position
        Gizmos.color = Color.red;
        Gizmos.DrawRay(estimatedPosition, estimatedRotation * Vector3.right * 0.2f);
        Gizmos.color = Color.green;
        Gizmos.DrawRay(estimatedPosition, estimatedRotation * Vector3.up * 0.2f);
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(estimatedPosition, estimatedRotation * Vector3.forward * 0.2f);
    }
} 