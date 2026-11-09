using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

// Rotates this object by "dragging" it while grabbed, so the side
// facing the user follows the ray, like spinning a globe.
// Built-in position and rotation tracking are turned off.
[RequireComponent(typeof(XRGrabInteractable))]
public class DragToRotate : MonoBehaviour
{
    [Tooltip("Degrees the object turns per degree the controller turns.")]
    public float sensitivity = 2f;
    [Tooltip("Allow tilting up and down as well as spinning left and right.")]
    public bool allowPitch = true;

    private XRGrabInteractable grab;
    private Transform controller;
    private Vector3 lastDirection;

    private void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();
        grab.trackPosition = false;
        grab.trackRotation = false;
        grab.throwOnDetach = false;
    }

    private void OnEnable()
    {
        grab.selectEntered.AddListener(OnGrab);
        grab.selectExited.AddListener(OnRelease);
    }

    private void OnDisable()
    {
        grab.selectEntered.RemoveListener(OnGrab);
        grab.selectExited.RemoveListener(OnRelease);
        controller = null;
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        controller = args.interactorObject.transform;
        lastDirection = controller.forward;
    }

    private void OnRelease(SelectExitEventArgs args) => controller = null;

    private void Update()
    {
        if (controller == null) return;

        Vector3 direction = controller.forward;
        Vector3 up = Vector3.up;
        Vector3 right = Camera.main != null ? Camera.main.transform.right : Vector3.right;

        // How far the ray moved left/right and up/down since last frame.
        float yaw = Vector3.SignedAngle(
            Vector3.ProjectOnPlane(lastDirection, up),
            Vector3.ProjectOnPlane(direction, up), up);
        float pitch = allowPitch
            ? Vector3.SignedAngle(
                Vector3.ProjectOnPlane(lastDirection, right),
                Vector3.ProjectOnPlane(direction, right), right)
            : 0f;

        // Rotate opposite to the hand, so the near surface follows the ray.
        transform.Rotate(up, -yaw * sensitivity, Space.World);
        transform.Rotate(right, -pitch * sensitivity, Space.World);

        lastDirection = direction;
    }
}