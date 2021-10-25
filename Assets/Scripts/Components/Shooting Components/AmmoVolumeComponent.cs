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
// Ammo Volume Component
// This component is for giving the player ammo when they enter the trigger volume, based on type.
//##################################################################################################
[RequireComponent(typeof(Collider))]
public class AmmoVolumeComponent : MonoBehaviour {

    // Either give ammo to all guns of this type, or just the first one found
    public bool giveAmmoToAllGuns = false;

    public bool destroyOnPickup = true;

    public AmmoType ammoType;
    public int pickupAmount;

    public SoundAsset ammoPickupSound;

    //##############################################################################################
    // If the colliding object is the player, find matching ammo types and give the gun ammo.
    // If marked to, destroy this object on pickup
    //##############################################################################################
    private void OnTriggerEnter(Collider other){
        if(other.tag == "Player"){
            GunComponent[] playerGuns = other.gameObject.GetComponents<GunComponent>();

            bool gaveAmmo = false;

            foreach(var gun in playerGuns){
                // Only pickup if the gun isn't maxed out already
                if(gun.currentGunData.ammoType == ammoType && gun.GetRemainingBoxAmmoCount() < gun.currentGunData.maxBoxAmmoCount){
                    gaveAmmo = true;
                    gun.GiveAmmo(ammoType, pickupAmount);

                    if(!giveAmmoToAllGuns){
                        break;
                    }
                }
            }

            if(gaveAmmo){
                SoundManagerComponent.PlaySound(ammoPickupSound, gameObject);

                if(destroyOnPickup){
                    Destroy(gameObject);
                }
            }
        }
    }
}
