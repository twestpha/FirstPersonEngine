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
// Sound Component
// A basic component implementation of a sound, that interacts with the SoundManagerComponent
//##################################################################################################
public class SoundComponent : MonoBehaviour {

    [HeaderAttribute("Sound Component")]
    public SoundAsset sound;

    public bool playOnStartup;
    public bool stopOnDisable;
    public bool stopOnDestroy;

    private int id = SoundManagerComponent.INVALID_SOUND;

    //##############################################################################################
    // If marked to do so, play on start
    //##############################################################################################
    void Start(){
        if(playOnStartup){
            Play();
        }
    }

    //##############################################################################################
    // Play the sound with the given settings, on this gameObject, and cache the resulting id
    //##############################################################################################
    public void Play(){
        id = SoundManagerComponent.PlaySound(
            sound,
            gameObject
        );
    }

    //##############################################################################################
    // If marked to, stop the sound on disable
    //##############################################################################################
    void OnDisable(){
        if(stopOnDisable && id != SoundManagerComponent.INVALID_SOUND){
            SoundManagerComponent.StopSound(id);
        }
    }

    //##############################################################################################
    // If marked to, stop the sound on destroy
    //##############################################################################################
    void OnDestroy(){
        if(stopOnDestroy && id != SoundManagerComponent.INVALID_SOUND){
            SoundManagerComponent.StopSound(id);
        }
    }
}
