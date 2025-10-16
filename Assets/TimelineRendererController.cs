using UnityEngine;

public class TimelineRendererController : MonoBehaviour
{
    [Tooltip("Parent object whose children are timeline events sorted from oldest (index 0) to newest (last index).")]
    public GameObject timelineParent;

    void Update()
    {
        if (timelineParent == null)
        {
            Debug.LogWarning("Timeline Parent is not assigned!");
            return;
        }

        // Get the normalized timeline progress from this object's X position (assumed between 0 and 1)
        float timelineProgress = transform.localPosition.x;

        int totalChildren = timelineParent.transform.childCount;

        // Calculate the number of events that should be visible (enabled)
        int activeCount = Mathf.FloorToInt(timelineProgress * totalChildren);

        // Ensure the first item is always enabled.
        activeCount = Mathf.Max(activeCount, 1);

        // Loop through each child and enable or disable its renderer based on its index
        for (int i = 0; i < totalChildren; i++)
        {
            Transform child = timelineParent.transform.GetChild(i);
            Renderer childRenderer = child.GetComponentInChildren<Renderer>();

            if (childRenderer != null)
            {
                // Enable if the child's index is less than activeCount; disable otherwise.
                childRenderer.enabled = (i < activeCount);
            }
        }
    }
}
