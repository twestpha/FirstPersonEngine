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
// Bark Component
// When called to, play a semi-random sound bark. Guaranteed to never play the same two sounds in a
// row (unless there's only one sound to play).
// This is useful for things like player or enemy damaged sounds, which in game audio, are called
// "Barks".
//##################################################################################################
public class BarkComponent : MonoBehaviour {

    public SoundAsset[] barks;

    private int previousPickedBark;

    //##############################################################################################
    // Error check and setup
    //##############################################################################################
    void Start(){
        previousPickedBark = -1;

        if(barks.Length < 1){
            Logger.Error("Barks on " + gameObject.name + "'s BarkComponent cannot have less than one entry");
        }
    }

    //##############################################################################################
    // Do a simple random selection of the next bark. If it's the same one, decrement to get a
    // different one. Only do this if we have enough barks. Then play the sound, and record the
    // result.
    //##############################################################################################
    public void Bark(){
        if(barks.Length < 1){
            return;
        }

        int newPickedBark = previousPickedBark;

        if(barks.Length != 1){
            newPickedBark = Random.Range(0, barks.Length);

            if(newPickedBark == previousPickedBark){
                newPickedBark = (newPickedBark + 1) % barks.Length;
            }
        } else {
            newPickedBark = 0;
        }

        SoundManagerComponent.PlaySound(barks[newPickedBark], gameObject);
        previousPickedBark = newPickedBark;
    }
}
