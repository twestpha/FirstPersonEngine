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
// Health Volume Component
// This component is for giving the player health when they enter the trigger volume
//##################################################################################################
[RequireComponent(typeof(Collider))]
public class HealthVolumeComponent : MonoBehaviour {

    public bool destroyOnPickup = true;
    public float healAmount;

    public SoundAsset healthPickupSound;

    //##############################################################################################
    // If the colliding object is the player, heal them for the specified amount
    // If marked to, and only if the heal occured, destroy this object on pickup
    //##############################################################################################
    private void OnTriggerEnter(Collider other){
        if(other.tag == "Player"){
            if(other.GetComponent<DamageableComponent>().Heal(healAmount)){
                SoundManagerComponent.PlaySound(healthPickupSound, gameObject);

                if(destroyOnPickup){
                    Destroy(gameObject);
                }
            }
        }
    }
}
