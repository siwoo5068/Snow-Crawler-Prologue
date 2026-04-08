using UnityEngine;

public class SnowBurial : MonoBehaviour
{
    public float burialTime = 20f;
    public float sinkSpeed = 0.015f;
    public float sinkDelay = 5f;

    private float elapsed;
    private float initialY;

    void Start()
    {
        initialY = transform.position.y;
    }

    void Update()
    {
        elapsed += Time.deltaTime;

        if (elapsed > sinkDelay)
        {
            Vector3 pos = transform.position;
            pos.y -= sinkSpeed * Time.deltaTime;
            transform.position = pos;
        }

        if (elapsed >= burialTime)
            Destroy(gameObject);
    }
}
