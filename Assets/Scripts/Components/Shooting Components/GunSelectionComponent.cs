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

using System;
using UnityEngine;

//##################################################################################################
// Gun Selection Component
// A minimal script for correlating a button press with enabling a gun, disabling the previous one
//##################################################################################################
public class GunSelectionComponent : MonoBehaviour {
    public const int INVALID_SELECTION = -1;

    [Serializable]
    public class GunSelection {
        public GunComponent gun;
        public KeyCode selectionKey;
    }

    public GunSelection[] selections;

    private int previouslySelectedIndex = INVALID_SELECTION;

    //##############################################################################################
    // If any gun is set to start enabled, count that as the previously selected gun
    //##############################################################################################
    void Start(){
        for(int i = 0, count = selections.Length; i < count; ++i){
            if(selections[i].gun.enabled){
                previouslySelectedIndex = i;
                break;
            }
        }
    }

    //##############################################################################################
    // If any of the inputs are pressed, enable that gun and disable the previous
    //##############################################################################################
    void Update(){
        for(int i = 0, count = selections.Length; i < count; ++i){
            GunSelection gunSelection = selections[i];

            if(Input.GetKeyDown(gunSelection.selectionKey) && i != previouslySelectedIndex){
                // Disable the previous gun
                if(previouslySelectedIndex != INVALID_SELECTION){
                    selections[previouslySelectedIndex].gun.enabled = false;
                }

                // Enable the current gun
                gunSelection.gun.enabled = true;

                previouslySelectedIndex = i;

                return;
            }
        }
    }
}
