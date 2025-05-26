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

    private bool wasTracking = true;

    // No need to initialize anything in Start, we can do it in Update
    void Start()
    {
    }

    void Update()
    {
        var visibleCameras = cameras.Where(c => c.HasValidTracking).ToList();

        if (visibleCameras.Count == 3)
        {
            // Approximate triangulation using angles
            EstimatedPosition = TriangulatePosition(visibleCameras);
            wasTracking = true;
        }
        else
        {
            // Fallback to IMU integration (very basic, drifts over time)
            EstimatedPosition += imu.velocity * Time.deltaTime;

            if (wasTracking && audioSource != null)
            {
                audioSource.Play(); // Tracking lost
                wasTracking = false;
            }
        }

        // Optionally visualize estimation
        hmdTransform.position = EstimatedPosition;
    }


    Vector3 TriangulatePosition(List<OpticalMarkerBasedCamera> cams)
    {

        List<(Vector3 origin, Vector3 direction)> rays = new();

        foreach (var cam in cams)
        {
            if (!cam.AngleToHMD.HasValue)
                continue;

            Vector3 directionToHMD = MathsHelpers.GetVectorAtAngle(cam.Forward, cam.AngleToHMD.Value);
            rays.Add((cam.Position, directionToHMD));
        }

        // Now use all rays to find closest intersection points
        if (rays.Count < 2)
            return hmdTransform.position;

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

        return pairCount > 0 ? totalIntersection / pairCount : hmdTransform.position;
    }

}
