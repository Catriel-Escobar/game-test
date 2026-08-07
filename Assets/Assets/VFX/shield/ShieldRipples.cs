using UnityEngine;
using UnityEngine.VFX;

public class ShieldRipples : MonoBehaviour
{
    [SerializeField] private GameObject shieldMaskPrefab;
    [SerializeField] private float rippleLifetime = 2f;

    public void PlayRipple(Vector3 hitPoint)
    {
        if (shieldMaskPrefab == null)
            return;

        GameObject ripples = Instantiate(shieldMaskPrefab, transform);
        Vector3 localHitPoint = transform.InverseTransformPoint(hitPoint);
        ripples.transform.localPosition = localHitPoint;
        ripples.transform.localRotation = Quaternion.identity;
        ripples.transform.localScale = Vector3.one;

        VisualEffect shieldRipplesVfx = ripples.GetComponent<VisualEffect>();
        if (shieldRipplesVfx != null)
        {
            SetRippleCenter(shieldRipplesVfx, localHitPoint);
            shieldRipplesVfx.Play();
        }
        else
        {
            ParticleSystem particleSystem = ripples.GetComponent<ParticleSystem>();
            if (particleSystem != null)
                particleSystem.Play();
        }

        Destroy(ripples, rippleLifetime);
    }

    public void PlayRipple()
    {
        PlayRipple(transform.position);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision == null || collision.contactCount == 0)
            return;

        HandleImpact(collision.collider, collision.contacts[0].point);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other == null)
            return;

        HandleImpact(other, other.ClosestPoint(transform.position));
    }

    private void HandleImpact(Collider other, Vector3 hitPoint)
    {
        if (other == null)
            return;

        PlayRipple(hitPoint);
    }

    private void SetRippleCenter(VisualEffect visualEffect, Vector3 hitPoint)
    {
        if (visualEffect == null)
            return;

        if (visualEffect.HasVector3("SphereCenter"))
        {
            visualEffect.SetVector3("SphereCenter", hitPoint);
            return;
        }

        if (visualEffect.HasVector3("SpeherCenter"))
        {
            visualEffect.SetVector3("SpeherCenter", hitPoint);
        }
    }
}
