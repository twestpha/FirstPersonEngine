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
// Level Trigger Component
// This class simply notifies the level manager component when entered, with the information of
// which level should be set as the 'primary'.
// The gameobject that this script is on, is expected to be a child of the gameobject with the
// LevelManagerComponent.
//##################################################################################################
[RequireComponent(typeof(Collider))]
public class LevelTriggerComponent : MonoBehaviour {

    public LevelManagerComponent.Level primaryLevel;

    //##############################################################################################
    // Do a quick error check to catch non-trigger collider
    //##############################################################################################
    void Start(){
        if(!GetComponent<Collider>().isTrigger){
            Logger.Error("Collider on " + gameObject.name + "'s LevelTriggerComponent must be a trigger");
        }
    }

    //##############################################################################################
    // On enter, find the level manager component in our parent hierarchy, then notify it that the
    // primary level should change to this one.
    //##############################################################################################
    private void OnTriggerEnter(Collider other){
        if(other.tag == "Player"){
            LevelManagerComponent manager = GetComponentInParent<LevelManagerComponent>();

            if(!manager){
                Logger.Error("LevelTriggerComponent " + name + " not parented under LevelManagerComponent correctly");
                return;
            }

            manager.SetPrimaryLevel(primaryLevel);
        }
    }
}
