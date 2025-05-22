using UnityEngine;

public class HMDMovementSimulator : MonoBehaviour
{
    // SCRIPT USE: testing the IMU class without HMD (can be removed later)

    public float moveSpeed = 2f;       // meters per second
    public float rotateSpeed = 90f;    // degrees per second

    void Update()
    {
        float dt = Time.deltaTime;

        // Movement: WASD or Arrow Keys
        float moveX = Input.GetAxis("Horizontal"); // A/D or Left/Right
        float moveZ = Input.GetAxis("Vertical");   // W/S or Up/Down
        Vector3 move = new Vector3(moveX, 0, moveZ);
        transform.Translate(move * moveSpeed * dt, Space.Self);

        // Up/Down movement: Q = down, E = up
        if (Input.GetKey(KeyCode.Q)) transform.Translate(Vector3.down * moveSpeed * dt, Space.World);
        if (Input.GetKey(KeyCode.E)) transform.Translate(Vector3.up * moveSpeed * dt, Space.World);

        // Rotation: Left/Right arrows or J/K keys
        if (Input.GetKey(KeyCode.J)) transform.Rotate(Vector3.up, -rotateSpeed * dt, Space.World);
        if (Input.GetKey(KeyCode.K)) transform.Rotate(Vector3.up, rotateSpeed * dt, Space.World);
    }
}
