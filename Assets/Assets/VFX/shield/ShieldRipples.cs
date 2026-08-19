using UnityEngine;
using UnityEngine.VFX;

public class ShieldRipples : MonoBehaviour
{
    [SerializeField] private GameObject shieldMaskPrefab;
    [SerializeField] private float rippleLifetime = 2f;

    public void PlayRipple()
    {
        if (shieldMaskPrefab == null)
            return;

        GameObject ripples = Instantiate(shieldMaskPrefab, transform);
        ripples.transform.localPosition = Vector3.zero;
        ripples.transform.localRotation = Quaternion.identity;
        ripples.transform.localScale = Vector3.one;

        VisualEffect shieldRipplesVfx = ripples.GetComponent<VisualEffect>();
        if (shieldRipplesVfx != null)
        {
            SetRippleCenter(shieldRipplesVfx, Vector3.zero);
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

    private void OnCollisionEnter(Collision collision)
    {
        if (collision == null || collision.contactCount == 0)
            return;

        HandleImpact(collision.collider);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other == null)
            return;

        HandleImpact(other);
    }

    private void HandleImpact(Collider other)
    {
        if (other == null)
            return;

        PlayRipple();
    }

    private void SetRippleCenter(VisualEffect visualEffect, Vector3 center)
    {
        if (visualEffect == null)
            return;

        if (visualEffect.HasVector3("SphereCenter"))
        {
            visualEffect.SetVector3("SphereCenter", center);
            return;
        }

        if (visualEffect.HasVector3("SpeherCenter"))
        {
            visualEffect.SetVector3("SpeherCenter", center);
        }
    }
}
