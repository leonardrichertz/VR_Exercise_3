using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class SensorFusionTracker : MonoBehaviour
{
    public IMU imu;
    public Transform hmdTransform;
    public Transform trackedHMDVisual;
    public LayerMask obstructionMask;

    [SerializeField] private OpticalMarkerBasedCamera[] cameras;
    [SerializeField] private AudioSource audioSource;

    public Vector3 EstimatedPosition { get; private set; }
    public Quaternion EstimatedRotation { get; private set; }

    private bool wasTracking = true;

    void Start()
    {
        EstimatedPosition = hmdTransform.position;
        EstimatedRotation = hmdTransform.rotation;
    }

    void Update()
    {
        var visibleCameras = cameras.Where(c => c.HasValidTracking).ToList();

        if (visibleCameras.Count >= 2)
        {
            // Approximate triangulation using angles
            (EstimatedPosition, EstimatedRotation) = TriangulatePosition(visibleCameras);
            wasTracking = true;
        }
        else
        {
            // Fallback to IMU integration
            EstimatedPosition += imu.Velocity * Time.deltaTime;
            EstimatedRotation = imu.Orientation;

            if (wasTracking && audioSource != null)
            {
                audioSource.Play(); // Tracking lost
                wasTracking = false;
            }
        }

        // Visualize estimation
        trackedHMDVisual.position = EstimatedPosition;
        trackedHMDVisual.rotation = EstimatedRotation;
    }

    (Vector3 position, Quaternion rotation) TriangulatePosition(List<OpticalMarkerBasedCamera> cams)
    {
        List<(Vector3 origin, Vector3 direction)> rays = new();

        foreach (var cam in cams)
        {
            if (!cam.AnglestoHMD.HasValue)
                continue;

            Vector2 angles = cam.AnglestoHMD.Value;
            Vector3 direction = Quaternion.Euler(-angles.y, angles.x, 0) * Vector3.forward;
            rays.Add((cam.Position, direction));
        }

        // Now use all rays to find closest intersection points
        if (rays.Count < 2)
            return (trackedHMDVisual.position, trackedHMDVisual.rotation);

        Vector3 totalIntersection = Vector3.zero;
        int pairCount = 0;

        for (int i = 0; i < rays.Count; i++)
        {
            for (int j = i + 1; j < rays.Count; j++)
            {
                Vector3 point = GetLineIntersection(rays[i].origin, rays[i].direction, rays[j].origin, rays[j].direction);
                totalIntersection += point;
                pairCount++;
            }
        }

        Vector3 estimatedPos = pairCount > 0 ? totalIntersection / pairCount : trackedHMDVisual.position;

        // Calculate rotation based on movement direction
        Vector3 movement = estimatedPos - EstimatedPosition;
        Quaternion estimatedRot = movement.magnitude > 0.01f ?
            Quaternion.Lerp(EstimatedRotation, Quaternion.LookRotation(movement), 0.5f) :
            EstimatedRotation;

        return (estimatedPos, estimatedRot);
    }

    private Vector3 GetLineIntersection(Vector3 point1, Vector3 direction1, Vector3 point2, Vector3 direction2)
    {
        Vector3 cross = Vector3.Cross(direction1, direction2);
        float denominator = cross.sqrMagnitude;

        // If lines are parallel, return midpoint
        if (denominator < 0.0001f)
        {
            return (point1 + point2) * 0.5f;
        }

        Vector3 c1 = Vector3.Cross(direction1, cross);
        Vector3 c2 = Vector3.Cross(direction2, cross);

        float s = Vector3.Dot(c2, point2 - point1) / denominator;
        float t = Vector3.Dot(c1, point2 - point1) / denominator;

        Vector3 intersection1 = point1 + direction1 * s;
        Vector3 intersection2 = point2 + direction2 * t;

        // Return average of the two intersection points
        return (intersection1 + intersection2) * 0.5f;
    }
}
