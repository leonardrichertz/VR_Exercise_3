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
            
            // KORREKTUR: Richtung korrekt berechnen
            Vector3 direction = cam.Forward; // Start mit Kamera-Forward
            
            // Horizontale Rotation um Y-Achse
            direction = Quaternion.AngleAxis(angles.x, Vector3.up) * direction;
            
            // Vertikale Rotation um die lokale Right-Achse der Kamera
            Vector3 rightAxis = Vector3.Cross(Vector3.up, direction).normalized;
            direction = Quaternion.AngleAxis(angles.y, rightAxis) * direction;
            
            rays.Add((cam.Position, direction));
        }

        
        if (rays.Count < 2)
            return (trackedHMDVisual.position, trackedHMDVisual.rotation);

        Vector3 totalIntersection = Vector3.zero;
        int pairCount = 0;

        for (int i = 0; i < rays.Count; i++)
        {
            for (int j = i + 1; j < rays.Count; j++)
            {
                Vector3 point = MathsHelpers.GetLineIntersection(rays[i].origin, rays[i].direction, rays[j].origin, rays[j].direction);
                totalIntersection += point;
                pairCount++;
            }
        }

        Vector3 estimatedPos = pairCount > 0 ? totalIntersection / pairCount : trackedHMDVisual.position;
        
        // Rotation sollte nicht aus Bewegung abgeleitet werden - das ist falsch
        Quaternion estimatedRot = EstimatedRotation; // Behalten Sie die letzte bekannte Rotation

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
