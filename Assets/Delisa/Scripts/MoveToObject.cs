using UnityEngine;

public class MoveToCoordinates : MonoBehaviour
{
    public Vector3 targetCoordinates; // Target coordinates to move towards
    public float speed = 5f; // Speed of movement
    public float acceleration = 2f; // Acceleration
    public float deceleration = 2f; // Deceleration
    public bool moveOnX = true; // Should move on X axis
    public bool moveOnY = true; // Should move on Y axis
    public bool moveOnZ = true; // Should move on Z axis

    private float currentSpeed = 0f; // Current speed

    private void Update()
    {
        // Calculate the direction to move towards
        Vector3 direction = targetCoordinates - transform.position;

        // Calculate the distance to the target
        float distance = direction.magnitude;

        // Calculate the desired speed based on distance
        float desiredSpeed = Mathf.Clamp(distance, 0f, speed);

        // Smooth start and end of movement
        if (distance > 0.1f)
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, desiredSpeed, acceleration * Time.deltaTime);
        }
        else
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, deceleration * Time.deltaTime);
        }

        // Normalize the direction
        Vector3 normalizedDirection = new Vector3(
            moveOnX ? direction.x : 0f,
            moveOnY ? direction.y : 0f,
            moveOnZ ? direction.z : 0f
        ).normalized;

        // Move the object towards the target
        transform.Translate(normalizedDirection * currentSpeed * Time.deltaTime, Space.World);
    }
}
