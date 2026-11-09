using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

// Rotates this object by "dragging" it while grabbed, so the side
// facing the user follows the ray, like spinning a globe.
[RequireComponent(typeof(XRGrabInteractable))]
public class DragToRotate : MonoBehaviour
{
    [Tooltip("Degrees the object turns per degree the controller turns.")]
    public float sensitivity = 2f;
    [Tooltip("Allow tilting up and down as well as spinning left and right.")]
    public bool allowPitch = true;
    [Tooltip("Rotate around the middle of the visible model instead of its pivot.")]
    public bool rotateAroundVisualCenter = true;

    private XRGrabInteractable grab;
    private Transform controller;
    private Vector3 lastDirection;
    private Vector3 pivot;

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
        pivot = rotateAroundVisualCenter ? GetVisualCenter() : transform.position;
    }

    private void OnRelease(SelectExitEventArgs args) => controller = null;

    private void Update()
    {
        if (controller == null) return;

        Vector3 direction = controller.forward;
        Vector3 up = Vector3.up;
        Vector3 right = Camera.main != null ? Camera.main.transform.right : Vector3.right;

        float yaw = Vector3.SignedAngle(
            Vector3.ProjectOnPlane(lastDirection, up),
            Vector3.ProjectOnPlane(direction, up), up);
        float pitch = allowPitch
            ? Vector3.SignedAngle(
                Vector3.ProjectOnPlane(lastDirection, right),
                Vector3.ProjectOnPlane(direction, right), right)
            : 0f;

        // Rotate opposite to the hand, so the near surface follows the ray.
        transform.RotateAround(pivot, up, -yaw * sensitivity);
        transform.RotateAround(pivot, right, -pitch * sensitivity);

        lastDirection = direction;
    }

    private Vector3 GetVisualCenter()
    {
        bool found = false;
        Bounds bounds = default;
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
        {
            if (!r.enabled) continue;
            if (!found) { bounds = r.bounds; found = true; }
            else bounds.Encapsulate(r.bounds);
        }
        return found ? bounds.center : transform.position;
    }
}