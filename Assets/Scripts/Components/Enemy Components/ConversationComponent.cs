
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//##################################################################################################
//
//##################################################################################################
[RequireComponent(typeof(DamageableComponent))]
public class ConversationComponent : MonoBehaviour {
    public AudioClip[] barks;
    public float[] volumes;

    private int previousPickedBark;

    void Start(){
        previousPickedBark = -1;

        if(barks.Length != volumes.Length || barks.Length == 0 || volumes.Length == 0){
            Debug.LogError("Conversation component barks and volumes aren't set up correctly on: " + gameObject);
        }
    }

    public void Bark(){
        int newPickedBark = previousPickedBark;
        while(newPickedBark == previousPickedBark){
            newPickedBark = (int)(Random.value * (barks.Length - 1));
        }

        // Play the sound
        SoundManagerComponent.PlaySound(
            barks[newPickedBark],
            SoundCount.Single,
            SoundType.ThreeDimensional,
            SoundPriority.Medium,
            volumes[newPickedBark],
            0.0f,
            gameObject
        );

        previousPickedBark = newPickedBark;
    }
}
