using UnityEngine;

public class UnitController : MonoBehaviour
{
    [System.Serializable]
    public class AudioSettings
    {
        [SerializeField] private AudioClip landingAudioClip = null;
        [SerializeField] private AudioClip[] footstepAudioClips = new AudioClip[] { };
        [SerializeField, Range(0f, 1f)] private float FootstepAudioVolume = 0.5f;

        public void OnFootstep(AnimationEvent _event, Transform _socket)
        {
            if (_event.animatorClipInfo.weight <= 0.5f
                || footstepAudioClips.Length == 0
                || _socket == null)
            {
                return;
            }

            int _id = Random.Range(0, footstepAudioClips.Length);

            AudioSource.PlayClipAtPoint(footstepAudioClips[_id], _socket.position, FootstepAudioVolume);
        }

        public void OnLand(AnimationEvent _event, Transform _socket)
        {
            if (_event.animatorClipInfo.weight <= 0.5f || _socket == null)
            {
                return;
            }

            AudioSource.PlayClipAtPoint(landingAudioClip, _socket.position, FootstepAudioVolume);
        }
    }

    [Header("Audio")]
    [SerializeField] private Transform audioSocket = null;
    [SerializeField] private AudioSettings audioSettings = new AudioSettings();

    private void OnFootstep(AnimationEvent animationEvent)
    {
        audioSettings.OnFootstep(animationEvent, getAudioSocket());
    }

    private void OnLand(AnimationEvent animationEvent)
    {
        audioSettings.OnLand(animationEvent, getAudioSocket());
    }

    private Transform getAudioSocket()
    {
        return audioSocket != null 
            ? audioSocket 
            : transform;
    }
}
