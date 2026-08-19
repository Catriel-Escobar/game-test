using UnityEngine;
using UnityEngine.VFX;

public class SpawnShieldRipples : MonoBehaviour
{
    public VisualEffect shieldVFX;
    [Tooltip("Optional: leave empty to accept any collider")]
    public string projectileTag = "Bullet";

    private void Awake()
    {
        if (shieldVFX == null)
            shieldVFX = GetComponent<VisualEffect>();

        if (shieldVFX == null)
            shieldVFX = GetComponentInChildren<VisualEffect>();
    }

    private void OnCollisionEnter(Collision co)
    {
        if (co == null || co.gameObject == null || co.contactCount == 0)
            return;

        if (!string.IsNullOrEmpty(projectileTag) && !co.gameObject.CompareTag(projectileTag))
            return;

        PlayImpact(co.contacts[0].point);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other == null)
            return;

        if (!string.IsNullOrEmpty(projectileTag) && !other.CompareTag(projectileTag))
            return;

        PlayImpact(other.ClosestPoint(transform.position));
    }

    private void PlayImpact(Vector3 worldPoint)
    {
        if (shieldVFX == null)
            return;

        Vector3 localPoint = transform.InverseTransformPoint(worldPoint);

        if (shieldVFX.HasVector3("SphereCenter"))
            shieldVFX.SetVector3("SphereCenter", localPoint);
        else if (shieldVFX.HasVector3("SpeherCenter"))
            shieldVFX.SetVector3("SpeherCenter", localPoint);

        shieldVFX.Play();
    }
}
