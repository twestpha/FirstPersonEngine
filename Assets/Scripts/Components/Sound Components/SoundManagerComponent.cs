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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//##################################################################################################
// Sound Features
// These flags represent a what given features a sound may have.
// The 'dynamic' modifier indicates the feature will continue during the duration of the sound, as
// opposed to only being evaluated when the sound is played. The latter saves performance, but
// obviously results in a more realistic simulation, especially if the sound's environment changes.
//
// Slapbacks are not yet implemented
//##################################################################################################
public enum SoundFeatures {
    None = 0,
    ThreeDimensional       = 1 << 0,
    SimulatedReverb        = 1 << 1,
    DynamicSimulatedReverb = 1 << 2,
    Occlusion              = 1 << 3,
    DynamicOcclusion       = 1 << 4,
    DistanceDelayed        = 1 << 5,
    EmitsSlapbacks         = 1 << 6,
    Doppler                = 1 << 7,
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
    private static bool destroyed;

    //##############################################################################################
    // Basic Sound Constants
    //##############################################################################################

    // This value controls how many sound sources are created on start and added to the pool
    // A larger value means more computation, but stealing sounds occurs less frequently
    public const int SOUND_SOURCE_COUNT = 32;

    // Returned when sound is unable to be played
    public const int INVALID_SOUND = -1;

    // Allows occlusion and reverb to ignore certain objects (importantly, the listener)
    public const int IGNORE_SOUND_RAYCAST_LAYER = 1 << 8; // In this case, the player

    // Indices in the raycastDirections table for the direction
    public const int DIRECTIONS_UP       = 0;
    public const int DIRECTIONS_DOWN     = 1;
    public const int DIRECTIONS_RIGHT    = 2;
    public const int DIRECTIONS_LEFT     = 3;
    public const int DIRECTIONS_FORWARD  = 4;
    public const int DIRECTIONS_BACK     = 5;
    public const int DIRECTIONS_COUNT    = 6;

    // How long between updates for certain dynamic features
    public const float REVERB_UPDATE_TIME = 0.7f;
    public const float OCCLUSION_UPDATE_TIME = 0.1f;

    //##############################################################################################
    // Sound Record
    // A companion class to the sound sources, to track the metadata about that sound source
    // Sound sources are tracked by a soundId, and can be referenced in functions using that soundId
    //##############################################################################################
    private class SoundRecord {
        public int soundId;

        public SoundFeatures features;
        public AudioReverbFilter reverb;
        public float previousVolumeNormalized;
        public float physicalVolumeNormalized;
        public float timeUntilReverbUpdate;

        public AudioLowPassFilter lowPassFilter;
        public float previousOcclusion;
        public float currentOcclusion;
        public float timeUntilOcclusionUpdate;

        public SoundPriority priority;
        public GameObject parent;
        public float volume;

        public SoundRecord(){
            soundId = INVALID_SOUND;
        }

        public bool Valid(){
            return soundId != INVALID_SOUND;
        }
    }

    //##############################################################################################
    // Sound Manager Component Variables
    //##############################################################################################
    private Vector3[] raycastDirections;

    private List<AudioSource> usedSources;
    private Queue<AudioSource> unusedSources;
    private List<SoundRecord> records;

    private GameObject listener;

    public float globalVolume = 0.5f;
    private int soundIdIndex = 0;

    //##############################################################################################
    // Setup the instance, sources, records, and raycast directions.
    // Also error check the whole process.
    //##############################################################################################
    public void Awake(){
        instance = this;

        // Setup records, hinting at the capacity
        if(records == null){
            soundIdIndex = 0;

            records = new List<SoundRecord>();
            records.Capacity = SOUND_SOURCE_COUNT;
        }

        // Setup the sources from scratch, creating a new gameObject and adding the needed components
        // Then, place these newly created sources into the unusedSources
        if(usedSources == null){
            usedSources = new List<AudioSource>();
            usedSources.Capacity = SOUND_SOURCE_COUNT;

            transform.position = Vector3.zero;

            unusedSources = new Queue<AudioSource>();

            for(int i = 0; i < SOUND_SOURCE_COUNT; ++i){
                GameObject newObject = new GameObject();
                newObject.AddComponent<AudioSource>();

                newObject.AddComponent<AudioReverbFilter>();
                newObject.GetComponent<AudioReverbFilter>().reverbPreset = AudioReverbPreset.Off;

                newObject.AddComponent<AudioLowPassFilter>();
                newObject.GetComponent<AudioLowPassFilter>().cutoffFrequency = SoundConstants.LPF_CLEAR;

                newObject.gameObject.transform.parent = transform;

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

        if(1 << listener.layer != IGNORE_SOUND_RAYCAST_LAYER){
            Logger.Error("Listener " + listener + " has incorrect layer " + (1 << listener.layer) + ", should be " + IGNORE_SOUND_RAYCAST_LAYER);
        }
    }

    //##############################################################################################
    // Iterate over the sounds, cleaning up finished sounds, and applying occlusion where necessary
    // This is done in late update, in the hopes that most objects are done moving around by then.
    //##############################################################################################
    void LateUpdate(){
        float deltaTime = Time.deltaTime;

        for(int i = 0, c = usedSources.Count; i < c; ++i){
            if(usedSources[i].isPlaying){
                SoundRecord record = records[i];

                // Place the sound in the correct position
                if(record.parent){
                    usedSources[i].transform.position = record.parent.transform.position;
                }

                if((record.features & SoundFeatures.ThreeDimensional) != 0){

                    // Simualate the dynamic reverb if update time elapsed
                    if((record.features & SoundFeatures.DynamicSimulatedReverb) != 0){
                        record.timeUntilReverbUpdate -= deltaTime;

                        if(record.timeUntilReverbUpdate <= 0.0f){
                            record.previousVolumeNormalized = record.physicalVolumeNormalized;

                            float physicalVolumeNormalized = CalculatePhysicalVolumeNormalized(record.parent);
                            record.physicalVolumeNormalized = physicalVolumeNormalized;

                            record.timeUntilReverbUpdate = REVERB_UPDATE_TIME;
                        }

                        float physicalVolume = CustomMath.StepToTarget(
                            record.previousVolumeNormalized,
                            record.physicalVolumeNormalized,
                            (1 / REVERB_UPDATE_TIME) * deltaTime
                        );

                        ApplyReverbFromPhysicalVolume(record.reverb, physicalVolume);
                    }

                    // Simualate the dynamic occlusion if update time elapsed
                    if((record.features & SoundFeatures.DynamicOcclusion) != 0){
                        record.timeUntilOcclusionUpdate -= deltaTime;

                        if(record.timeUntilOcclusionUpdate <= 0.0f){
                            record.previousOcclusion = record.currentOcclusion;
                            record.currentOcclusion = Occluded(record.parent);

                            record.timeUntilOcclusionUpdate = OCCLUSION_UPDATE_TIME;
                        }

                        float occlusion = CustomMath.StepToTarget(
                            record.previousOcclusion,
                            record.currentOcclusion,
                            (1 / OCCLUSION_UPDATE_TIME) * deltaTime
                        );

                        record.previousOcclusion = occlusion;

                        ApplyOcclusion(record.lowPassFilter, occlusion);
                    }
                }
            }  else {
                // If source is not playing, return to the unused sources
                usedSources[i].Stop();
                unusedSources.Enqueue(usedSources[i]);

                usedSources.RemoveAt(i);
                records.RemoveAt(i);

                c = usedSources.Count;
            }
        }

        #if UNITY_EDITOR
            if(usedSources.Count != records.Count){
                Logger.Error("Mismatched used sources and records count!");
            }
        #endif // UNITY_EDITOR
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

    public static int PlaySound(SoundAsset asset){
        if(!VerifySingleton()){
            return INVALID_SOUND;
        }

        return SoundManagerComponent.instance.PlaySoundInternal(asset, null);
    }

    public static int PlaySound(SoundAsset asset, GameObject transformObject){
        if(!VerifySingleton()){
            return INVALID_SOUND;
        }

        return SoundManagerComponent.instance.PlaySoundInternal(asset, transformObject);
    }

    public static void StopSound(int targetId){
        if(!VerifySingleton()){
            return;
        }

        instance.StopSoundInternal(targetId);
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
    // This is the primary function for how a sound is set up and played TODO TODO
    //
    // This function returns the sound id of the record, for reference later. For example, a loop
    // can be started and the id cached, then stopped at a later time, using that id.
    //##############################################################################################
    public int PlaySoundInternal(SoundAsset asset, GameObject transformObject){
        if(asset == null || asset.clip == null || asset.volume <= 0.0f){
            return INVALID_SOUND;
        }

        int index = GetValidSoundIndex(asset.priority);

        if(index != INVALID_SOUND){
            AudioSource source = usedSources[index];
            AudioReverbFilter reverb = source.GetComponent<AudioReverbFilter>();
            AudioLowPassFilter lowPassFilter = source.GetComponent<AudioLowPassFilter>();

            // Setup source
            source.Stop();
            source.clip = asset.clip;
            source.loop = (asset.count == SoundCount.Looping);

            // Randomize the pitch if needed
            if(asset.pitchBend > 0.0f){
                source.pitch = 1.0f + (Random.value * asset.pitchBend) - (asset.pitchBend * 0.5f);
            } else {
                source.pitch = 1.0f;
            }

            // Apply volume
            source.volume = asset.volume * globalVolume;

            // Position immediately before next update
            source.gameObject.transform.position = transformObject == null ? Vector3.zero : transformObject.transform.position;

            // setup records
            SoundRecord record = records[index];
            record.volume = asset.volume;
            record.features = asset.features;
            record.priority = asset.priority;
            record.soundId = soundIdIndex;
            record.parent = transformObject;

            record.reverb = reverb;
            record.lowPassFilter = lowPassFilter;

            soundIdIndex++;

            // Set defaults for advanced features
            bool delayed = false;
            reverb.reverbPreset = AudioReverbPreset.Off;
            lowPassFilter.cutoffFrequency = SoundConstants.LPF_CLEAR;
            source.spatialBlend = SoundConstants.SPATIAL_BLEND_TWO_D;

            // Advanced features require three-dimensional spatialization and a valid transform
            if((asset.features & SoundFeatures.ThreeDimensional) != 0){
                if(transformObject != null){
                    source.spatialBlend = SoundConstants.SPATIAL_BLEND_THREE_D;

                    if((asset.features & SoundFeatures.SimulatedReverb) != 0){
                        float physicalVolumeNormalized = CalculatePhysicalVolumeNormalized(record.parent);
                        ApplyReverbFromPhysicalVolume(reverb, physicalVolumeNormalized);

                        record.timeUntilReverbUpdate = REVERB_UPDATE_TIME;
                        record.previousVolumeNormalized = physicalVolumeNormalized;
                        record.physicalVolumeNormalized = physicalVolumeNormalized;
                    }

                    if((asset.features & SoundFeatures.Occlusion) != 0){
                        // Mark previous occlusion as invalid to immediately apply
                        record.currentOcclusion = Occluded(transformObject);
                        record.previousOcclusion = record.currentOcclusion;

                        record.timeUntilOcclusionUpdate = OCCLUSION_UPDATE_TIME;

                        ApplyOcclusion(lowPassFilter, record.currentOcclusion);
                    }

                    if((asset.features & SoundFeatures.DistanceDelayed) != 0){
                        delayed = true;
                    }

                    if((asset.features & SoundFeatures.EmitsSlapbacks) != 0){
                        // TODO Shoot ray away from listener, and create delayed slapback at that point?
                    }

                    if((asset.features & SoundFeatures.Doppler) != 0){
                        source.dopplerLevel = SoundConstants.DOPPLER_ON;
                    } else {
                        source.dopplerLevel = SoundConstants.DOPPLER_OFF;
                    }
                }

                if(transformObject == null && asset.features != 0){
                    Logger.Error("All Sound Assets with Sound Features require a transform object as an argument.");
                    record.features = 0;
                }
            }

            // play the sound unless delayed
            if(!delayed){
                source.Play();
            }

            return records[index].soundId;
        } else {
            Logger.Warning("Failed to play " + asset.clip + " with priority " + asset.priority);
            return INVALID_SOUND;
        }
    }

    //##############################################################################################
    // Stop playing a specific given sound, and return it to the pool
    // This is a somewhat expensive operation, having to loop over all currently playing sounds.
    //##############################################################################################
    public void StopSoundInternal(int targetId){
        for(int i = 0, c = usedSources.Count; i < c; ++i){
            if(records[i].soundId == targetId && usedSources[i].isPlaying){
                records[i].soundId = INVALID_SOUND;
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
    public bool PlayingInternal(int targetId){
        for(int i = 0, c = usedSources.Count; i < c; ++i){
            if(records[i].soundId == targetId){
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
    public void SetSoundVolumeInternal(int targetId, float volume){
        for(int i = 0, c = usedSources.Count; i < c; ++i){
            if(records[i].soundId == targetId){
                records[i].volume = volume;
                usedSources[i].volume = globalVolume * volume;
            }
        }
    }

    //##############################################################################################
    // Utility Methods
    //##############################################################################################

    // Returns a valid index if possible, stomping priority if necessary
    private int GetValidSoundIndex(SoundPriority priority){
        // If possible, just use an audio source that's not playing
        if(unusedSources.Count > 0){
            AudioSource source = unusedSources.Dequeue();

            usedSources.Add(source);
            records.Add(new SoundRecord());

            return usedSources.Count - 1;
        }

        // otherwise, stomp the lowest-priority playing sound
        int lowestIndex = INVALID_SOUND;
        SoundPriority lowestPriority = SoundPriority.Essential;

        for(int i = 0, c = usedSources.Count; i < c; ++i){
            if(records[i].priority <= lowestPriority){
                lowestPriority = records[i].priority;
                lowestIndex = i;
            }
        }

        if(priority < records[lowestIndex].priority){
            return INVALID_SOUND;
        } else {
            return lowestIndex;
        }
    }

    // Casts raycasts in cardinal directions to very roughly approximate the volume of a space
    private float CalculatePhysicalVolumeNormalized(GameObject transformObject){
        float[] distances = new float[DIRECTIONS_COUNT];

        // Shoot 6 raycasts out in the cardinal directions, get distances
        for(int i = 0; i < DIRECTIONS_COUNT; ++i){
            RaycastHit hit;
            if(Physics.Raycast(transformObject.transform.position, raycastDirections[i], out hit, SoundConstants.MAX_REVERB_DISTANCE, ~IGNORE_SOUND_RAYCAST_LAYER, QueryTriggerInteraction.Ignore)){
                distances[i] = hit.distance;
                // Debug.DrawLine(transformObject.transform.position, transformObject.transform.position + (raycastDirections[i] * distances[i]), Color.red, 1.0f);
            } else {
                distances[i] += SoundConstants.MAX_REVERB_DISTANCE;
                // Debug.DrawLine(transformObject.transform.position, transformObject.transform.position + (raycastDirections[i] * SoundConstants.MAX_REVERB_DISTANCE), Color.green, 1.0f);
            }
        }

        // Calculate volume of that space based on distances
        float physicalVolume = 0.0f;

        physicalVolume += PyramidVolume(distances[DIRECTIONS_UP], distances[DIRECTIONS_RIGHT], distances[DIRECTIONS_FORWARD]);
        physicalVolume += PyramidVolume(distances[DIRECTIONS_UP], distances[DIRECTIONS_RIGHT], distances[DIRECTIONS_BACK]);
        physicalVolume += PyramidVolume(distances[DIRECTIONS_UP], distances[DIRECTIONS_LEFT], distances[DIRECTIONS_FORWARD]);
        physicalVolume += PyramidVolume(distances[DIRECTIONS_UP], distances[DIRECTIONS_LEFT], distances[DIRECTIONS_BACK]);

        physicalVolume += PyramidVolume(distances[DIRECTIONS_DOWN], distances[DIRECTIONS_RIGHT], distances[DIRECTIONS_FORWARD]);
        physicalVolume += PyramidVolume(distances[DIRECTIONS_DOWN], distances[DIRECTIONS_RIGHT], distances[DIRECTIONS_BACK]);
        physicalVolume += PyramidVolume(distances[DIRECTIONS_DOWN], distances[DIRECTIONS_LEFT], distances[DIRECTIONS_FORWARD]);
        physicalVolume += PyramidVolume(distances[DIRECTIONS_DOWN], distances[DIRECTIONS_LEFT], distances[DIRECTIONS_BACK]);

        // Normalize against a known maximum volume
        float physicalVolumeNormalized = Mathf.Clamp(physicalVolume / SoundConstants.MAX_REVERB_PHYSICAL_VOLUME, 0.0f, 1.0f);

        return physicalVolumeNormalized;
    }

    // Applies reverb settings to a filter based on a physical volume passed in
    private void ApplyReverbFromPhysicalVolume(AudioReverbFilter reverb, float physicalVolumeNormalized){
        reverb.reverbPreset = AudioReverbPreset.User;

        reverb.dryLevel = SoundConstants.RVB_DL;
        reverb.room = SoundConstants.RVB_RM;

        // Room High Frequency Curve
        reverb.roomHF = EvalPoly(physicalVolumeNormalized, SoundConstants.RVB_RM_HF_A, SoundConstants.RVB_RM_HF_B, SoundConstants.RVB_RM_HF_C);

        // Room Low Frequency Constant
        reverb.roomLF = SoundConstants.RVB_RM_LF;

        // Decay Time Constant
        reverb.decayTime = SoundConstants.RVB_DT;

        // Decay HF Ratio Curve
        reverb.decayHFRatio = EvalPoly(physicalVolumeNormalized, SoundConstants.RVB_DHFR_A, SoundConstants.RVB_DHFR_B, SoundConstants.RVB_DHFR_C);

        // Reflections Level Curve
        reverb.reflectionsLevel = EvalPoly(physicalVolumeNormalized, SoundConstants.RVB_RF_LV_A, SoundConstants.RVB_RF_LV_B, SoundConstants.RVB_RF_LV_C);

        // Reflections Delay Constant
        reverb.reflectionsDelay = SoundConstants.RVB_RF_DL;

        // Reverb Level Curve
        reverb.reverbLevel = EvalPoly(physicalVolumeNormalized, SoundConstants.RVB_RV_LV_A, SoundConstants.RVB_RV_LV_B, SoundConstants.RVB_RV_LV_C);

        // Reverb Delay Curve
        float reverbDelayMax = Mathf.Clamp(physicalVolumeNormalized, 0.0f, SoundConstants.RVB_RV_DL_MAX);
        reverb.reverbDelay = EvalPoly(reverbDelayMax, SoundConstants.RVB_RV_DL_A, SoundConstants.RVB_RV_DL_B, SoundConstants.RVB_RV_DL_C);

        // Frequency Reference Constants
        reverb.hfReference = SoundConstants.RVB_HF_R;
        reverb.lfReference = SoundConstants.RVB_LF_R;

        // Diffusion Curve
        reverb.diffusion = EvalPoly(physicalVolumeNormalized, SoundConstants.RVB_DF_A, SoundConstants.RVB_DF_B, SoundConstants.RVB_DF_C);

        // Density Constant
        reverb.density = SoundConstants.RVB_DN;
    }

    // Evaluates a simple ax^2 + bx + c polynomial equation
    private float EvalPoly(float x, float a, float b, float c){
        return (x * x * a) + (x * b) + c;
    }

    // Returns the volume of a pyramid with a given length, width, and height
    private float PyramidVolume(float length, float width, float height){
        return (length * width * height) / 3.0f;
    }

    // Determines based on raycasts whether a sound is occluded from the listeners perspective
    private float Occluded(GameObject transformObject){
        Vector3 listenerPosition = listener.transform.position;
        Vector3 soundPosition = transformObject.transform.position;

        Vector3 toListener = listenerPosition - soundPosition;
        float toListenerMag = toListener.magnitude;

        RaycastHit hit;
        if(Physics.Raycast(soundPosition, toListener, out hit, Mathf.Min(SoundConstants.MAX_OCCLUDE_DISTANCE, toListenerMag), ~IGNORE_SOUND_RAYCAST_LAYER)){
            // Debug.DrawLine(soundPosition, hit.point, Color.red, 1.0f);
            return SoundConstants.OCCLUSION_ON;
        } else {
            // Debug.DrawLine(soundPosition, listenerPosition, Color.green, 1.0f);
            return SoundConstants.OCCLUSION_OFF;
        }
    }

    // Apply the occlusion strength to a audio filter
    private void ApplyOcclusion(AudioLowPassFilter lowPassFilter, float occlusionStrength){
        lowPassFilter.cutoffFrequency = Mathf.Lerp(SoundConstants.LPF_CLEAR, SoundConstants.LPF_OCCLUDED, occlusionStrength);
    }
}
