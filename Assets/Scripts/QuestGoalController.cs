using UnityEngine;

public class QuestGoalController : MonoBehaviour
{
    public float requiredTimeInside = 1f; // seconds
    public float requiredOverlap = 0.5f; // 50% overlap

    private float timeInside = 0f;
    private Collider triggerCollider;

    private void Awake()
    {
        triggerCollider = GetComponent<Collider>();
        if (triggerCollider != null)
        {
            triggerCollider.isTrigger = true;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // Calculate approximate overlap
        Bounds playerBounds = other.bounds;
        Bounds triggerBounds = triggerCollider.bounds;

        if (BoundsOverlapPercentage(playerBounds, triggerBounds) >= requiredOverlap)
        {
            timeInside += Time.deltaTime;

            if (timeInside >= requiredTimeInside)
            {
                CompleteQuest();
            }
        }
        else
        {
            timeInside = 0f; // reset if not enough overlap
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            timeInside = 0f; // reset timer when player leaves
        }
    }

    private void CompleteQuest()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.CompleteQuest();
        }

        // Optional: prevent triggering again
        enabled = false;
    }

    // Calculates approximate overlap percentage of player bounds with trigger bounds
    private float BoundsOverlapPercentage(Bounds a, Bounds b)
    {
        float xOverlap = Mathf.Max(0, Mathf.Min(a.max.x, b.max.x) - Mathf.Max(a.min.x, b.min.x));
        float yOverlap = Mathf.Max(0, Mathf.Min(a.max.y, b.max.y) - Mathf.Max(a.min.y, b.min.y));
        float zOverlap = Mathf.Max(0, Mathf.Min(a.max.z, b.max.z) - Mathf.Max(a.min.z, b.min.z));

        float overlapVolume = xOverlap * yOverlap * zOverlap;
        float playerVolume = a.size.x * a.size.y * a.size.z;

        return playerVolume > 0 ? overlapVolume / playerVolume : 0f;
    }
}
