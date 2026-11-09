using TMPro;
using UnityEngine;

// Shows which surface of the current tooth faces the user, such as "Occlusal view".
// Uses the tooth prefab's ViewMarkers children: the marker pointing most
// directly at the camera gives the name. Updates live, including while rotating.
public class ToothViewLabel : MonoBehaviour
{
    public StudyModeManager manager;
    public TMP_Text label;
    [Tooltip("{0} is replaced with the marker's name.")]
    public string format = "Viewing: {0}";
    [Tooltip("Show nothing unless a marker faces the user at least this directly (1 = straight on).")]
    [Range(0f, 1f)] public float minFacing = 0.5f;
    [Tooltip("How much more directly another surface must face the user before the label switches. Prevents flicker between two surfaces.")]
    [Range(0f, 0.3f)] public float switchMargin = 0.05f;

    private string currentMarker = "";

    private void Awake()
    {
        if (label == null) label = GetComponent<TMP_Text>();
        if (manager == null) manager = FindObjectOfType<StudyModeManager>();
        SetText("");
    }

    private void LateUpdate()
    {
        GameObject tooth = manager != null ? manager.CurrentTooth : null;
        if (tooth == null)
        {
            currentMarker = "";
            SetText("");
            return;
        }

        currentMarker = FacingMarker(tooth.transform, currentMarker);
        SetText(currentMarker.Length > 0 ? string.Format(format, currentMarker) : "");
    }

    // Returns the name of the marker facing the user, keeping the current one
    // unless another faces the user more directly by at least switchMargin.
    private string FacingMarker(Transform tooth, string current)
    {
        Transform markers = tooth.Find("ViewMarkers");
        Camera cam = Camera.main;
        if (markers == null || cam == null) return "";

        Vector3 origin = markers.position;
        Vector3 toViewer = (cam.transform.position - origin).normalized;

        string best = "";
        float bestFacing = float.NegativeInfinity;
        float currentFacing = float.NegativeInfinity;
        foreach (Transform marker in markers)
        {
            Vector3 direction = marker.position - origin;
            if (direction.sqrMagnitude < 1e-6f) continue;
            float facing = Vector3.Dot(direction.normalized, toViewer);
            if (marker.name == current) currentFacing = facing;
            if (facing > bestFacing) { bestFacing = facing; best = marker.name; }
        }

        // Keep the current label while it still faces the user and nothing is clearly better.
        if (current.Length > 0 && currentFacing >= minFacing && bestFacing < currentFacing + switchMargin)
            return current;

        return bestFacing >= minFacing ? best : "";
    }

    private void SetText(string text)
    {
        if (label != null && label.text != text) label.text = text;
    }
}