using UnityEngine;

public class FootstepAudio : MonoBehaviour
{
    [SerializeField] private AudioClip[] _footstepClips;
    [SerializeField, Range(0f, 1f)] private float _volume = 0.5f;

    public void PlayFootstep()
    {
        if (_footstepClips == null || _footstepClips.Length == 0) return;

        AudioClip clip = _footstepClips[Random.Range(0, _footstepClips.Length)];
        AudioManager.Instance?.PlaySFX(clip, _volume);
    }
}
