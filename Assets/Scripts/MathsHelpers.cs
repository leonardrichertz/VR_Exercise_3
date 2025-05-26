using UnityEngine;

public class MathsHelpers : MonoBehaviour
{
    void Start()
    {
        Debug.Log(GetVectorAtAngle(new Vector3(1, 0, 0), 90));
        Debug.Log(GetLineIntersection(new Vector3(2, 2, 0), new Vector3(-1, -1, 0), new Vector3(0, 9, 3), new Vector3(0, -3, -1)));
    }

    Vector3 GetVectorAtAngle(Vector3 w, float angleInDegrees)
    {
        /*  INPUTS: 
            w: optical marker based camera's transform.forward
            angleInDegrees: angle to the hmd
            
            OUTPUT:
            vector in the direction of the hmd (from the camera), later d1 */

        // Normalize w
        Vector3 wNorm = w.normalized;

        // Pick any vector not parallel to w
        Vector3 arbitrary = (Mathf.Abs(Vector3.Dot(wNorm, Vector3.up)) < 0.99f) ? Vector3.up : Vector3.right;

        // Get a perpendicular vector
        Vector3 perp = Vector3.Cross(wNorm, arbitrary).normalized;

        // Rotate wNorm toward perp by angle
        Quaternion rotation = Quaternion.AngleAxis(angleInDegrees, perp);
        Vector3 v = rotation * wNorm;

        return v;
    }
    
    Vector3 GetLineIntersection(Vector3 p1, Vector3 d1, Vector3 p2, Vector3 d2)
    {
        /*  INPUTS: 
            p1: first optical marker based camera's transform.position
            d1: first camera's direction to the hmd
            p2: second optical marker based camera's transform.position
            d2: second camera's direction to the hmd
            
            OUTPUT:
            position of the hmd (intersection of the lines along p1+d1 and p2+d2) */

        Vector3 r = p2 - p1;
        float a = Vector3.Dot(d1, d1);
        float b = Vector3.Dot(d1, d2);
        float c = Vector3.Dot(d2, d2);
        float d = Vector3.Dot(d1, r);
        float e = Vector3.Dot(d2, r);

        float denominator = a * c - b * b;

        // Avoid division by zero if lines are parallel (shouldn’t happen if you assume intersection)
        if (Mathf.Abs(denominator) < 1e-6f)
            throw new System.Exception("Lines are parallel or nearly parallel");

        float t = (d * c - e * b) / denominator;
        Vector3 intersection = p1 + t * d1;

        return intersection;
    }
}
