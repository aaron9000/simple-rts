using UnityEngine;
using System.Collections;

public class AudioPlayerBehaviour : MonoBehaviour
{
    public AudioClip ClickSound;
    public AudioClip SplatSound;
    public AudioClip CaptureSound;
    public AudioClip LoseCaptureSound;
    public AudioClip SpawnSound;
    public AudioClip VictorySound;
    public AudioClip DefeatSound;
    public AudioClip GainResourceSound;
    public AudioClip UpgradeSound;
    public AudioClip TurretShootSound;


    private static AudioPlayerBehaviour _instance;

    void Awake()
    {
        _instance = this;
    }

    protected void _playSound(SoundType sound)
    {
        AudioClip clip;
        switch (sound)
        {
            case SoundType.Splat:
                clip = SplatSound;
                break;
            case SoundType.GainResource:
                clip = GainResourceSound;
                break;
            case SoundType.Defeat:
                clip = DefeatSound;
                break;
            case SoundType.Victory:
                clip = VictorySound;
                break;
            case SoundType.Shoot:
                clip = ClickSound;
                break;
            case SoundType.LoseCapture:
                clip = LoseCaptureSound;
                break;
            case SoundType.TurretShoot:
                clip = TurretShootSound;
                break;
            case SoundType.Capture:
                clip = CaptureSound;
                break;
            case SoundType.Spawn:
                clip = SpawnSound;
                break;
            case SoundType.PurchaseUpgrade:
                clip = UpgradeSound;
                break;
            default:
                clip = ClickSound;
                break;
        }
        if (clip == null)
        {
            return;
        }
        AudioSource.PlayClipAtPoint(clip, Vector3.zero);
    }

    public static void PlaySound(SoundType sound)
    {
		if (_instance == null)
		{
			Debug.LogWarning("AudioPlayerBehaviour: PlaySound: make sure an AudioPlayerBehaviour.cs script lives in the scene");
		}
		else
		{
			_instance._playSound(sound);
		}
    }
}