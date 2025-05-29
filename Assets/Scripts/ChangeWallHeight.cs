using UnityEngine;

public class ChangeWallHeight : MonoBehaviour
{
    public GameObject wall;
    private float scaleStep = 0.2f;

    private float minHeight = 0f;
    private float maxHeight = 8f;
    private float newY;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Controller"))
        {
            Vector3 scale = wall.transform.localScale;

            if (gameObject.name == "Pluscube" && scale.y < maxHeight)
            {
                newY = scale.y + scaleStep;
            }

            if (gameObject.name == "Minuscube" && scale.y > minHeight)
            {
                newY = scale.y - scaleStep;
            }

            wall.transform.localScale = new Vector3(scale.x, newY, scale.z);

        }
    }
}
