using UnityEngine;

public class WakeFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float verticalOffset = -0.02f;

    private float baseHeight;
    private bool initialized;

    private void LateUpdate()
    {
        if (target == null) return;

        if (!initialized)
        {
            baseHeight = target.position.y;
            initialized = true;
        }

        var pos = target.position;
        transform.position = new Vector3(pos.x, baseHeight + verticalOffset, pos.z);
        transform.rotation = Quaternion.identity;
    }
}
