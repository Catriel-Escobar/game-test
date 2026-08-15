using UnityEngine;

public class TransformFollower : MonoBehaviour
{
    private Transform _target;
    private Vector3 _offset;
    private bool _destroyOnLostTarget;

    public void Init(Transform target, Vector3 offset, bool destroyOnLostTarget = true)
    {
        _target = target;
        _offset = offset;
        _destroyOnLostTarget = destroyOnLostTarget;
    }

    private void LateUpdate()
    {
        if (_target == null)
        {
            if (_destroyOnLostTarget)
                Destroy(gameObject);
            return;
        }

        transform.position = _target.position + _offset;
    }
}
