using UnityEngine;

//##################################################################################################
// Sound Asset
// A wrapper data for the 'audioclip' unity asset. This allows the user to set the volume, features,
// and other settings for the sound for use in the sound manager component.
//##################################################################################################
[CreateAssetMenu(fileName = "SoundAsset", menuName = "Sound/SoundAsset", order = 1)]
public class SoundAsset : ScriptableObject {
#pragma warning disable 0649
    [SerializeField]
    private AudioClip _clip = null;
    public AudioClip clip {
        get {return _clip; }
    }

    [SerializeField, Range(0.0f, 1.0f)]
    private float _volume = 1.0f;
    public float volume {
        get {return _volume; }
    }

    [SerializeField, Range(0.0f, 1.0f)]
    private float _pitchBend = 0.0f;
    public float pitchBend {
        get {return _pitchBend; }
    }

    [SerializeField, EnumFlag("Features")]
    private SoundFeatures _features;
    public SoundFeatures features {
        get {return _features; }
    }

    [SerializeField]
    private SoundCount _count;
    public SoundCount count {
        get {return _count; }
    }

    [SerializeField]
    private SoundPriority _priority;
    public SoundPriority priority {
        get {return _priority; }
    }
#pragma warning restore 0649
}