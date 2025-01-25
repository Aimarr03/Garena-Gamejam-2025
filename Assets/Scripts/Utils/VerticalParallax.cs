using UnityEngine;

public class VerticalParallax : MonoBehaviour
{
    public Transform cameraTransform; // Assign the camera in the Inspector
    public Vector2 parallaxFactor;    // Control the intensity of the parallax for X and Y
    private Vector3 lastCameraPosition;

    void Start()
    {
        // Store the initial camera position
        lastCameraPosition = cameraTransform.position;
    }

    void Update()
    {
        // Calculate the camera's movement
        Vector3 deltaMovement = cameraTransform.position - lastCameraPosition;

        // Move the background vertically based on the parallax factor
        transform.position += new Vector3(deltaMovement.x * parallaxFactor.x, deltaMovement.y * parallaxFactor.y, 0);

        // Update the last camera position
        lastCameraPosition = cameraTransform.position;
    }
}
