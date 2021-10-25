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

﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//##################################################################################################
// Ambient Sound Component
// This script is intended to be used with a trigger collider. It fades between ambient sounds
// whenever the player enters the trigger volume.
//##################################################################################################
[RequireComponent(typeof(Collider))]
public class AmbientSoundComponent : MonoBehaviour {
    // static so that multiple instances of this component can fade out the previously playing
    // ambient sound
    static public int currentAmbientSoundId = SoundManagerComponent.INVALID_SOUND;

    // This can be set null, for a volume that simply fades out the ambient sound
    public SoundAsset sound;

    public float fadeTime = 1.0f;

    private bool fading = false;
    private int fadingInSoundId;
    private Timer fadeTimer;

    //##############################################################################################
    // Set up the timer
    //##############################################################################################
    void Start(){
        fadeTimer = new Timer(fadeTime);

        if(!GetComponent<Collider>().isTrigger){
            Logger.Error("Collider on " + gameObject.name + "'s AmbientSoundComponent must be a trigger");
        }
    }

    //##############################################################################################
    // If it's fading, drive the fade from the timer. Fade out the old sound and in the new.
    // When finished, stop the old, then set the shared current sound.
    //##############################################################################################
    void Update(){
        if(fading){
            float p = fadeTimer.Parameterized();

            if(currentAmbientSoundId != SoundManagerComponent.INVALID_SOUND){
                SoundManagerComponent.SetSoundVolume(currentAmbientSoundId, (1.0f - p));
            }

            if(fadingInSoundId != SoundManagerComponent.INVALID_SOUND){
                SoundManagerComponent.SetSoundVolume(fadingInSoundId, p);
            }

            if(fadeTimer.Finished()){
                fading = false;
                SoundManagerComponent.StopSound(currentAmbientSoundId);
                currentAmbientSoundId = fadingInSoundId;
            }
        }
    }

    //##############################################################################################
    // When triggered, kick off playing the new sound (at very small volume) and kick off the fading
    //
    // This system uses TwoDimensional sound, because it's meant to feel omnipresent and ambient
    // without any particular directionality.
    //##############################################################################################
    private void OnTriggerEnter(Collider other){
        // Ignore disabled components
        if(!enabled){
            return;
        }

        if(other.tag == "Player"){
            fading = true;
            fadeTimer.Start();

            fadingInSoundId = SoundManagerComponent.PlaySound(sound, gameObject);
        }
    }
}
