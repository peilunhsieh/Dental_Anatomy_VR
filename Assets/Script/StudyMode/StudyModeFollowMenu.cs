using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class StudyModeFollowMenu : MonoBehaviour
{
    [Header("Target")]
    public Transform head;

    [Header("Placement")]
    public float distanceFromHead = 1.6f;
    public float heightOffset = -0.05f;

    [Header("Follow")]
    [Tooltip("Off: place the menu in front of the user once, then keep it fixed in the world.")]
    public bool followHead = false;
    public float positionSmooth = 8f;
    public float rotationSmooth = 8f;

    [Header("Recenter Input")]
    [Tooltip("Button that brings the menu back in front of the user.")]
    public InputActionReference recenterAction;

    private void OnEnable()
    {
        if (recenterAction != null && recenterAction.action != null)
        {
            recenterAction.action.performed += OnRecenter;
            recenterAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (recenterAction != null && recenterAction.action != null)
            recenterAction.action.performed -= OnRecenter;
    }

    private void OnRecenter(InputAction.CallbackContext context) => SnapToHead();

    private IEnumerator Start()
    {
        if (head == null && Camera.main != null)
        {
            head = Camera.main.transform;
        }

        // Wait one frame so headset tracking has a real head position.
        yield return null;
        SnapToHead();
    }

    private void LateUpdate()
    {
        if (!followHead || head == null) return;

        Vector3 forward = head.forward;
        forward.y = 0f;
        forward.Normalize();

        Vector3 targetPosition =
            head.position +
            forward * distanceFromHead +
            Vector3.up * heightOffset;

        Quaternion targetRotation =
            Quaternion.LookRotation(forward, Vector3.up);

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            Time.deltaTime * positionSmooth
        );

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            Time.deltaTime * rotationSmooth
        );
    }

    public void SnapToHead()
    {
        if (head == null) return;

        Vector3 forward = head.forward;
        forward.y = 0f;
        forward.Normalize();

        transform.position =
            head.position +
            forward * distanceFromHead +
            Vector3.up * heightOffset;

        transform.rotation =
            Quaternion.LookRotation(forward, Vector3.up);
    }
}