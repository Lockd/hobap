using UnityEngine;

public class ZoneBehaviour : MonoBehaviour
{
    [SerializeField] private float startShrinkingAfter = 30f;
    [Range(0f, 1f), SerializeField] private float shrinkingSpeed = 5f;
    bool shouldStopShrinking = false;

    void Start()
    {
        startShrinkingAfter += Time.time;
    }

    void FixedUpdate()
    {
        if (transform.localScale.x <= 0.01f) shouldStopShrinking = true;

        if (Time.time > startShrinkingAfter && !shouldStopShrinking)
        {
            float shrinkFor = shrinkingSpeed * Time.fixedDeltaTime;
            float newScale = transform.localScale.x - shrinkFor;

            transform.localScale = new Vector3(
                newScale,
                newScale,
                1f
            );
        }
    }
}
