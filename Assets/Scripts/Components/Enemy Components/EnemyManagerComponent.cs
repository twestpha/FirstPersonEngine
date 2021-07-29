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
using System;

//##################################################################################################
// Enemy Manager Component
// Basically: Prevents 'enemies' from updating themself. A trigger on the enemy behaviour calls the
// RegisterUpdate to get an update next frame, and only for one frame. This allows for much better
// performance, having only nearby enemy AI getting updated.
// At the end of the frame, the update lists are purged. Therefore, if the enemy still has work to
// do, make sure to re-register it at the end of it's update.
// To accomplish that, we must use a double-buffer, so that registering during EnemyUpdate() doesn't
// register for the currently updating list.
//##################################################################################################
public class EnemyManagerComponent : MonoBehaviour {
    private static EnemyManagerComponent instance;

    #if UNITY_EDITOR
    // For viewing in unity editor, to see how many enemies are being updated
    public int debugUpdateCount;
    #endif // UNITY_EDITOR

    private bool updatingA;

    private List<EnemyBehavior> updateListA;
    private List<EnemyBehavior> updateListB;

    //##############################################################################################
    // Set this as the global instance, and setup the double buffers
    //##############################################################################################
    void Start(){
        instance = this;

        updateListA = new List<EnemyBehavior>();
        updateListB = new List<EnemyBehavior>();
    }

    //##############################################################################################
    // First, we must pick which list is currently updating. Do some null checking to prevent
    // destroyed gameobjects, and then call EnemyUpdate on all the live ones. Finally, purge that
    // list of updated objects for reuse later.
    //##############################################################################################
    void Update(){
        #if UNITY_EDITOR
        debugUpdateCount = Mathf.Max(updateListA.Count, updateListB.Count);
        #endif // UNITY_EDITOR

        updatingA = !updatingA;
        List<EnemyBehavior> updateList = updatingA ? updateListA : updateListB;

        for(int i = 0, count = updateList.Count; i < count; ++i){
            // Skip destroyed enemies
            if(updateList[i] != null){
                // Prevents excepting enemies from taking down all enemy updates
                try {
                    updateList[i].EnemyUpdate();
                } catch (Exception e){
                    Debug.LogError("Error while updating " + updateList[i] + " with exception " + e);
                }
            }
        }

        updateList.Clear();
    }

    //##############################################################################################
    // Wrapper for registering an enemy's update
    //##############################################################################################
    public static void RegisterUpdate(EnemyBehavior enemy){
        instance.RegisterUpdateInternal(enemy);
    }

    //##############################################################################################
    // Basically, register the EnemyBehavior with the list that's not currently updating.
    // Also, an enemy can only register once for an update.
    //##############################################################################################
    public void RegisterUpdateInternal(EnemyBehavior enemy){
        // Register with the opposite list that's updating
        if(updatingA){
            if(!updateListB.Contains(enemy)){
                updateListB.Add(enemy);
            }
        } else {
            if(!updateListA.Contains(enemy)){
                updateListA.Add(enemy);
            }
        }
    }
}
