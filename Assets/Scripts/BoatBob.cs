using UnityEngine;

public class BoatBob : MonoBehaviour
{
    [SerializeField] private float bobHeight = 0.15f;
    [SerializeField] private float bobSpeed = 1f;
    [SerializeField] private float tiltAmount = 3f;

    private Vector3 startPos;

    private void Start()
    {
        startPos = transform.position;
    }

    private void Update()
    {
        float t = Time.time * bobSpeed;
        float yOffset = Mathf.Sin(t) * bobHeight;
        transform.position = startPos + new Vector3(0f, yOffset, 0f);

        float tilt = Mathf.Sin(t) * tiltAmount;
        transform.rotation = Quaternion.Euler(tilt, transform.eulerAngles.y, transform.eulerAngles.z);
    }
}
