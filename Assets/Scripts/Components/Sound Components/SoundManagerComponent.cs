//##################################################################################################
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
//##################################################################################################

//##################################################################################################
// Author's Note: This is version 0.1 of what is intended to be a comprehensive sound engine, and
// very much a first draft of the functionality. It attempts to simulate simple reverb and sound
// occlusion.
//
// Despite being incomplete, it is superset of the features offered by unity, and I decided to
// include it. It is also used by many of the Shooter Components.
//##################################################################################################

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//##################################################################################################
// Sound Type
// Basic options for the way the sound is simulated\
// TwoDimensional has no positional source and plays as if on the audio listener
// ThreeDimensional has a positional source
// ThreeDimensionalWithReverb has a positional source with simulated reverb and occlusion
//##################################################################################################
public enum SoundType {
    TwoDimensional,
    ThreeDimensional,
    ThreeDimensionalWithReverb,
}

//##################################################################################################
// Sound Count
// Toggle for whether the sound should play once, or should loop
//##################################################################################################
public enum SoundCount {
    Single,
    Looping,
}

//##################################################################################################
// Sound Priority
// Options for how important a given sound is. If all sound sources are being used, a higher
// priority sound will steal the lowest-priorities sound source, if it's priority is greater.
// Essential sounds are always guaranteed to play, even if it means stealing another Essential sound
// source.
//##################################################################################################
public enum SoundPriority {
    None,
    Low,
    Medium,
    High,
    Essential,
}

//##################################################################################################
// Sound Manager Component
// This is the singleton endpoint for managing all the sound sources and playing sounds.
//##################################################################################################
public class SoundManagerComponent : MonoBehaviour {
    //##############################################################################################
    // Singleton Instance, set on start
    //##############################################################################################
    private static SoundManagerComponent instance;
    private static bool destroyed = false;

    //##############################################################################################
    // Basic Sound Constants
    //##############################################################################################
    public const int INVALID_SOUND = -1;

    public const int LISTENER_COLLISION_LAYER = 1 << 8;

    private float MAX_REVERB_DISTANCE = 45.0f;
    private float MAX_OCCLUDE_DISTANCE = 50.0f;

    private float OCCLUDE_FADE_SPEED = 1.0f;

    // This value controls how many sound sources are created on start and added to the pool
    // A larger value means more computation, but stealing sounds occurs less frequently
    public const int SOUND_SOURCE_COUNT = 32;

    public const int DIRECTIONS_COUNT = 6;

    //##############################################################################################
    // Simulated Reverb Constants
    // These are pre-calculated constants for reverb characteristics that the unity engine uses
    // in it's reverb filter.
    //##############################################################################################
    // Reverb Settings
    public const float RVB_DL = 0.0f;
    public const float RVB_RM = -1000.0f;

    // Room High Frequency Curve
    public const float RVB_RM_HF_A = 0.546f;
    public const float RVB_RM_HF_B = -78.4f;
    public const float RVB_RM_HF_C = 11.7f;
    public const float RVB_RM_HF_MIN = -2500.0f;
    public const float RVB_RM_HF_MAX = -300.0f;

    public const float RVB_RM_LF = 0.0f;
    public const float RVB_DT = 1.49f;
    public const float RVB_DH_FR = 0.525f;

    // Reflections Level Curve
    public const float RVB_RF_LV_A = 0.0f;
    public const float RVB_RF_LV_B = 550.0f;
    public const float RVB_RF_LV_C = -2853.0f;
    public const float RVB_RF_LV_MIN = -2780.0f;
    public const float RVB_RF_LV_MAX = -1219.0f;

    public const float RVB_RF_DL = 0.0f;

    // Reverb Level Curve
    public const float RVB_RV_LV_A = 0.756f;
    public const float RVB_RV_LV_B = -91.3f;
    public const float RVB_RV_LV_C = 721.0f;
    public const float RVB_RV_LV_MIN = -1926.0f;
    public const float RVB_RV_LV_MAX = 441.0f;

    // Reverb Delay Curve
    public const float RVB_RV_DL_A = -0.0000469f;
    public const float RVB_RV_DL_B = 0.0047f;
    public const float RVB_RV_DL_C = -0.0135f;
    public const float RVB_RV_DL_MIN = 0.011f;
    public const float RVB_RV_DL_MAX = 0.1f;

    public const float RVB_HF_R = 5000.0f;
    public const float RVB_LF_R = 250.0f;

    // Diffusion Curve
    public const float RVB_DF_A = 0.0355f;
    public const float RVB_DF_B = -3.76f;
    public const float RVB_DF_C = 117.0f;
    public const float RVB_DF_MIN = 21.0f;
    public const float RVB_DF_MAX = 100.0f;

    public const float RVB_DN = 100.0f;

    //##############################################################################################
    // Low Pass Filter Constants
    // These are used for the simulated sound occlusion
    //##############################################################################################
    public const float LPF_OCCLUDED = 2250;
    public const float LPF_CLEAR = 22000;

    //##############################################################################################
    // Sound Manager Component Variables
    //##############################################################################################
    public float globalVolume = 0.5f;
    private int idIndex = 0;

    //##############################################################################################
    // Sound Record
    // A companion class to the sound sources, to track the metadata about that sound source
    // Sound sources are tracked by an Id, and can be referenced in functions using that Id.
    //##############################################################################################
    private class SoundRecord {
        public int id;

        public SoundType type;
        public SoundPriority priority;
        public GameObject parent;

        public float volume;
        public bool shouldOcclude;
        public float occlusion;

        public SoundRecord(){
            id = INVALID_SOUND;
        }

        public bool Valid(){
            return id != INVALID_SOUND;
        }
    }

    private List<AudioSource> usedSources;
    private Queue<AudioSource> unusedSources;
    private List<SoundRecord> records;

    private Vector3[] raycastDirections;

    private GameObject listener;

    //##############################################################################################
    // Setup the instance, sources, records, and raycast directions.
    // Also error check the whole process.
    //##############################################################################################
    void Start(){
        instance = this;

        // Setup records, hinting at the capacity
        if(records == null){
            idIndex = 0;

            records = new List<SoundRecord>();
            records.Capacity = SOUND_SOURCE_COUNT;
        }

        // Setup the sources from scratch, creating a new gameObject and adding the needed components
        // Then, place these newly created sources into the unusedSources
        if(usedSources == null){
            usedSources = new List<AudioSource>();
            usedSources.Capacity = SOUND_SOURCE_COUNT;

            unusedSources = new Queue<AudioSource>();

            for(int i = 0; i < SOUND_SOURCE_COUNT; ++i){
                GameObject newObject = new GameObject();
                newObject.AddComponent<AudioSource>();
                newObject.AddComponent<AudioReverbFilter>();
                newObject.AddComponent<AudioLowPassFilter>();
                newObject.gameObject.transform.parent = gameObject.transform;

                #if UNITY_EDITOR
                    newObject.gameObject.name = ("Sound " + i);
                #endif // UNITY_EDITOR

                AudioSource source = newObject.GetComponent<AudioSource>();
                unusedSources.Enqueue(source);
            }
        }

        // Basically just hard-code the 6 cardinal directions into the array, so we can loop over
        // them later, when calculating reverb for each direction.
        if(raycastDirections == null){
            raycastDirections = new Vector3[DIRECTIONS_COUNT];

            raycastDirections[0] =  Vector3.up;
            raycastDirections[1] = -Vector3.up;
            raycastDirections[2] =  Vector3.right;
            raycastDirections[3] = -Vector3.right;
            raycastDirections[4] =  Vector3.forward;
            raycastDirections[5] = -Vector3.forward;
        }

        // Check that the audio listener in game is set up correctly. This is necessary, so the
        // occlusion raycasts don't collide against the listener itself.
        AudioListener audioListener = FindObjectOfType(typeof(AudioListener)) as AudioListener;
        listener = audioListener.gameObject;

        if(1 << listener.layer != LISTENER_COLLISION_LAYER){
            Logger.Error("Listener " + listener + " has incorrect layer " + (1 << listener.layer) + ", should be " + LISTENER_COLLISION_LAYER);
        }
    }

    //##############################################################################################
    // Iterate over the sounds, cleaning up finished sounds, and applying occlusion where necessary
    // This is done in late update, in the hopes that most objects are done moving around by then.
    //##############################################################################################
    void LateUpdate(){
        // Iterate the playing sources, moving the sound source to the parent's world position.
        // If the sound should use occlusion, cast a ray between the listener and the sound.
        // Fade the occlusion amount in or out based on that result.

        for(int i = 0, c = usedSources.Count; i < c; ++i){
            if(usedSources[i].isPlaying){
                if(records[i].parent){
                    usedSources[i].transform.position = records[i].parent.transform.position;

                    if(records[i].shouldOcclude){
                        float target = Occluded(usedSources[i].transform.position, listener.transform.position) ? 1.0f : 0.0f;
                        records[i].occlusion = CustomMath.StepToTarget(records[i].occlusion, target, OCCLUDE_FADE_SPEED * Time.deltaTime);

                        // Square Root Lerp sounds subjectively a little better
                        float t = Mathf.Sqrt(records[i].occlusion);
                        usedSources[i].GetComponent<AudioLowPassFilter>().cutoffFrequency = Mathf.Lerp(LPF_CLEAR, LPF_OCCLUDED, t);
                    }
                }
            } else {
                // If the source is done playing, clean it up and release it to the pool
                usedSources[i].Stop();
                unusedSources.Enqueue(usedSources[i]);

                usedSources.RemoveAt(i);
                records.RemoveAt(i);

                // Make sure the loop considers the new size when iterating
                c = usedSources.Count;
            }
        }
    }

    // #############################################################################################
    // This prevents spamming the VerifySingleton method after this component has been destroyed
    // #############################################################################################
    private void OnDestroy(){
        destroyed = true;
    }

    // #############################################################################################
    // Static Methods
    // First, always verify the singleton has been set up. This is mostly a heads-up for developer
    // setting up their guns and wondering why something's not working.
    // The rest of the functions are common endpoints for making calls into the sound manager.
    //##############################################################################################
    public static bool VerifySingleton(){
        if(instance == null){
            if(!destroyed){
                Logger.Error("No SoundManagerComponent was found in the game. Consider adding a GameObject to your scene with a SoundManagerComponent on it.");
            }

            return false;
        }

        return true;
    }

    public static int PlaySound(AudioClip clip, SoundCount count, SoundType type, float volume, float pitchBend, SoundPriority priority){
        if(!VerifySingleton()){
            return INVALID_SOUND;
        }

        return SoundManagerComponent.instance.PlaySoundInternal(clip, count, type, priority, volume, pitchBend, null);
    }

    public static int PlaySound(AudioClip clip, SoundCount count, SoundType type, SoundPriority priority, float volume, float pitchBend, GameObject transformObject){
        if(!VerifySingleton()){
            return INVALID_SOUND;
        }

        return SoundManagerComponent.instance.PlaySoundInternal(clip, count, type, priority, volume, pitchBend, transformObject);
    }

    public static void StopSound(int id){
        if(!VerifySingleton()){
            return;
        }

        SoundManagerComponent.instance.StopSoundInteral(id);
    }

    public static void StopAllSounds(){
        if(!VerifySingleton()){
            return;
        }

        SoundManagerComponent.instance.StopAllSoundsInternal();
    }

    public static bool Playing(int id){
        if(!VerifySingleton()){
            return false;
        }

        return SoundManagerComponent.instance.PlayingInternal(id);
    }

    public static void SetGlobalVolume(float volume){
        if(!VerifySingleton()){
            return;
        }

        SoundManagerComponent.instance.SetGlobalVolumeInternal(volume);
    }

    public static void SetSoundVolume(int id, float volume){
        if(!VerifySingleton()){
            return;
        }

        SoundManagerComponent.instance.SetSoundVolumeInternal(id, volume);
    }

    //##############################################################################################
    // This is the primary function for how a sound is set up and played. It takes the arguments:
    // Clip, the sound clip unity asset to actually play,
    // Count, Type, and Priority, the behavior of the sound,
    // Volume and PitcBend, the audible sound characteristics, and
    // An optional transform that the sound should 'attach' to and play from
    //
    // This function returns the sound id of the record, for reference later. For example, a loop
    // can be started and the id cached, then stopped at a later time, using that id.
    //##############################################################################################
    public int PlaySoundInternal(AudioClip clip, SoundCount count, SoundType type, SoundPriority priority, float volume, float pitchBend, GameObject transformObject){
        // Don't play if there's no reason to
        if(clip == null || volume <= 0.0f){
            return INVALID_SOUND;
        }

        int index = INVALID_SOUND;

        // If possible, just use an audio source that's not playing
        if(unusedSources.Count > 0){
            AudioSource source = unusedSources.Dequeue();

            usedSources.Add(source);
            records.Add(new SoundRecord());

            index = usedSources.Count - 1;
        }

        // otherwise, stomp the lowest-priority playing sound
        if(index == INVALID_SOUND){
            int lowestIndex = INVALID_SOUND;

            SoundPriority lowestPriority = SoundPriority.Essential;

            for(int i = 0, c = usedSources.Count; i < c; ++i){
                if(records[i].priority <= lowestPriority){
                    lowestPriority = records[i].priority;
                    lowestIndex = i;
                }
            }

            if(priority < records[lowestIndex].priority){
                index = INVALID_SOUND;
            } else {
                index = lowestIndex;
            }
        }

        // If we've found a valid index
        if(index != INVALID_SOUND){
            // Cache some useful components
            AudioSource source = usedSources[index];
            AudioReverbFilter reverb = source.GetComponent<AudioReverbFilter>();
            AudioLowPassFilter lpFilter = source.GetComponent<AudioLowPassFilter>();

            // Setup source
            source.Stop();
            source.clip = clip;
            source.loop = (count == SoundCount.Looping);

            // Spatialize sound effects correctly
            if(type == SoundType.ThreeDimensional || type == SoundType.ThreeDimensionalWithReverb){
                source.spatialBlend = transformObject == null ? 0.0f : 1.0f;
            } else {
                source.spatialBlend = 0.0f;
            }

            // Randomize the pitch if needed
            if(pitchBend > 0.0f){
                source.pitch = 1.0f + (Random.value * pitchBend) - (pitchBend * 0.5f);
            } else {
                source.pitch = 1.0f;
            }

            // Apply volume
            source.volume = volume * globalVolume;

            // Position immediately, before next update
            source.gameObject.transform.position = transformObject == null ? Vector3.zero : transformObject.transform.position;

            // setup records
            SoundRecord record = records[index];
            record.volume = volume; // Specifically, don't save globalVolume here
            record.type = type;
            record.priority = priority;
            record.id = idIndex;
            record.parent = transformObject;
            record.shouldOcclude = false;

            // Increment the id tracking
            idIndex++;

            // Calculate the reverb if specified
            if(transformObject != null && type == SoundType.ThreeDimensionalWithReverb){
                // Occlusion
                record.shouldOcclude = true;
                record.occlusion = Occluded(transformObject.transform.position, listener.transform.position) ? 1.0f : 0.0f;
                lpFilter.cutoffFrequency = Mathf.Lerp(LPF_CLEAR, LPF_OCCLUDED, record.occlusion);

                // Reverb
                reverb.reverbPreset = AudioReverbPreset.User;

                float totalDistance = 0.0f;

                for(int j = 0; j < DIRECTIONS_COUNT; ++j){
                    RaycastHit hit;
                    if(Physics.Raycast(transformObject.transform.position, raycastDirections[j], out hit, MAX_REVERB_DISTANCE, ~LISTENER_COLLISION_LAYER, QueryTriggerInteraction.Ignore)){
                        totalDistance += hit.distance;
                        // Debug.DrawLine(transformObject.transform.position, transformObject.transform.position + (raycastDirections[j] * hit.distance), Color.red, 1.0f);
                    } else {
                        totalDistance += MAX_REVERB_DISTANCE;
                        // Debug.DrawLine(transformObject.transform.position, transformObject.transform.position + (raycastDirections[j] * MAX_REVERB_DISTANCE), Color.green, 1.0f);
                    }
                }

                float avg = totalDistance / (float)(DIRECTIONS_COUNT);

                reverb.dryLevel = RVB_DL;
                reverb.room = RVB_RM;

                reverb.roomHF = Mathf.Clamp(EvalPoly(avg, RVB_RM_HF_A, RVB_RM_HF_B, RVB_RM_HF_C), RVB_RM_HF_MIN, RVB_RM_HF_MAX);

                reverb.roomLF = RVB_RM_LF;
                reverb.decayTime = RVB_DT;
                reverb.decayHFRatio = RVB_DH_FR;

                reverb.reflectionsLevel = Mathf.Clamp(EvalPoly(avg, RVB_RF_LV_A, RVB_RF_LV_B, RVB_RF_LV_C), RVB_RF_LV_MIN, RVB_RF_LV_MAX);

                reverb.reflectionsDelay = RVB_RF_DL;

                reverb.reverbLevel = Mathf.Clamp(EvalPoly(avg, RVB_RV_LV_A, RVB_RV_LV_B, RVB_RV_LV_C), RVB_RV_LV_MIN, RVB_RV_LV_MAX);
                reverb.reverbDelay = Mathf.Clamp(EvalPoly(avg, RVB_RV_DL_A, RVB_RV_DL_B, RVB_RV_DL_C), RVB_RV_DL_MIN, RVB_RV_DL_MAX);

                reverb.hfReference = RVB_HF_R;
                reverb.lfReference = RVB_LF_R;

                reverb.diffusion = Mathf.Clamp(EvalPoly(avg, RVB_DF_A, RVB_DF_B, RVB_DF_C), RVB_DF_MIN, RVB_DF_MAX);

                reverb.density = RVB_DN;
            } else {
                reverb.reverbPreset = AudioReverbPreset.Off;

                lpFilter.cutoffFrequency = LPF_CLEAR;
            }

            // play the sound
            source.Play();

            return records[index].id;
        } else {
            Logger.Warning("Failed to play " + type + " " + clip + " with priority " + priority);
            return INVALID_SOUND;
        }
    }

    //##############################################################################################
    // Stop playing a specific given sound, and return it to the pool
    // This is a somewhat expensive operation, having to loop over all currently playing sounds.
    //##############################################################################################
    public void StopSoundInteral(int id){
        for(int i = 0, c = usedSources.Count; i < c; ++i){
            if(records[i].id == id && usedSources[i].isPlaying){
                records[i].id = INVALID_SOUND;
                usedSources[i].Stop();

                unusedSources.Enqueue(usedSources[i]);

                usedSources.RemoveAt(i);
                records.RemoveAt(i);

                return;
            }
        }
    }

    //##############################################################################################
    // This function stops all currently playing sounds
    // Used for things like returning to menu
    // This is a somewhat expensive operation, having to loop over all currently playing sounds.
    //##############################################################################################
    public void StopAllSoundsInternal(){
        for(int i = 0, c = usedSources.Count; i < c; ++i){
            if(usedSources[i].isPlaying){
                usedSources[i].Stop();
                unusedSources.Enqueue(usedSources[i]);
            }
        }

        usedSources.Clear();
        records.Clear();
    }

    //##############################################################################################
    // This function returns whether a given sound id is currently playing
    // This is a somewhat expensive operation, having to loop over all currently playing sounds.
    //##############################################################################################
    public bool PlayingInternal(int id){
        for(int i = 0, c = usedSources.Count; i < c; ++i){
            if(records[i].id == id){
                return usedSources[i].isPlaying;
            }
        }

        return false;
    }

    //##############################################################################################
    // Set the global volume, and update all the currently playing sounds of this as well
    // This is a somewhat expensive operation, having to loop over all currently playing sounds.
    //##############################################################################################
    public void SetGlobalVolumeInternal(float volume){
        globalVolume = volume;

        for(int i = 0, c = usedSources.Count; i < c; ++i){
            usedSources[i].volume = records[i].volume * globalVolume;
        }
    }

    //##############################################################################################
    // Set a specific sound's volume (taking into account global volume)
    // This is a somewhat expensive operation, having to loop over all currently playing sounds.
    //##############################################################################################
    public void SetSoundVolumeInternal(int id, float volume){
        for(int i = 0, c = usedSources.Count; i < c; ++i){
            if(records[i].id == id){
                records[i].volume = volume;
                usedSources[i].volume = globalVolume * volume;
            }
        }
    }

    //##############################################################################################
    // Utility Methods
    // EvalPoly is called to evaluate a simple polynomial function based on the a, b, and c inputs.
    // y = ax^2 + bx + c
    // Occluded returns whether the line between two given positions is occluded
    //##############################################################################################
    private float EvalPoly(float x, float a, float b, float c){
        return (x * x * a) + (x * b) + c;
    }

    private bool Occluded(Vector3 soundPosition, Vector3 listenerPosition){
        RaycastHit hit;
        Vector3 toSound = soundPosition - listenerPosition;
        float toSoundMag = toSound.magnitude;

        return Physics.Raycast(listenerPosition, toSound, out hit, Mathf.Min(MAX_OCCLUDE_DISTANCE, toSoundMag), ~LISTENER_COLLISION_LAYER);

    }
}
