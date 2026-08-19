using UnityEngine;

public class ShieldFollow : MonoBehaviour
{
    private readonly Vector3 _offset = new Vector3(0f, 1f, 0f);
    private Transform _target;

    public void Init(Transform target)
    {
        _target = target;
    }

    private void LateUpdate()
    {
        if (_target == null)
        {
            Destroy(gameObject);
            return;
        }

        transform.position = _target.position + _offset;
    }
}
