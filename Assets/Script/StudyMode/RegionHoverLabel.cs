using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit;

// Floating label that shows the name of the anatomy region being pointed at,
// just above where the ray hits it, always facing the user.
public class RegionHoverLabel : MonoBehaviour
{
    [Tooltip("The part that is shown and hidden (the label's Canvas).")]
    public GameObject content;
    public TMP_Text label;
    [Tooltip("How far above the hit point, in metres.")]
    public float heightAbovePointer = 0.04f;
    [Tooltip("How far toward the user, so the label doesn't sink into the tooth.")]
    public float towardViewer = 0.03f;
    [Tooltip("Keep roughly the same apparent size at any distance.")]
    public bool constantScreenSize = true;
    [Tooltip("Content scale at 1 m when Constant Screen Size is on.")]
    public float scaleAtOneMetre = 0.0005f;

    private XRRayInteractor[] rays;
    private AnatomyRegion shownRegion;

    private void Awake()
    {
        if (label == null && content != null) label = content.GetComponentInChildren<TMP_Text>(true);
        if (content != null) content.SetActive(false);
    }

    private void LateUpdate()
    {
        AnatomyHotspotManager manager = AnatomyHotspotManager.Active;
        AnatomyRegion region = manager != null ? manager.CurrentRegion : null;
        Camera cam = Camera.main;

        if (region == null || cam == null || content == null)
        {
            Hide();
            return;
        }

        if (region != shownRegion)
        {
            shownRegion = region;
            if (label != null) label.text = region.structureName;
        }

        Vector3 point = FindHitPoint(region);
        Vector3 toViewer = (cam.transform.position - point).normalized;
        Vector3 position = point + Vector3.up * heightAbovePointer + toViewer * towardViewer;

        Transform t = content.transform;
        t.position = position;
        // A world-space canvas reads correctly when it faces away from the camera.
        t.rotation = cam.transform.rotation;

        if (constantScreenSize)
        {
            float distance = Vector3.Distance(cam.transform.position, position);
            t.localScale = Vector3.one * scaleAtOneMetre * distance;
        }

        if (!content.activeSelf) content.SetActive(true);
    }

    private void Hide()
    {
        shownRegion = null;
        if (content != null && content.activeSelf) content.SetActive(false);
    }

    // Where a controller ray touches this region; otherwise the region's middle.
    private Vector3 FindHitPoint(AnatomyRegion region)
    {
        if (rays == null || rays.Length == 0) rays = FindObjectsOfType<XRRayInteractor>();
        foreach (XRRayInteractor ray in rays)
        {
            if (ray == null || !ray.isActiveAndEnabled) continue;
            if (ray.TryGetCurrentUIRaycastResult(out RaycastResult hit) && hit.gameObject != null &&
                hit.gameObject.GetComponentInParent<AnatomyRegion>() == region)
                return hit.worldPosition;
        }

        Renderer[] renderers = region.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return region.transform.position;
        Bounds bounds = renderers[0].bounds;
        foreach (Renderer r in renderers) bounds.Encapsulate(r.bounds);
        return bounds.center;
    }
}