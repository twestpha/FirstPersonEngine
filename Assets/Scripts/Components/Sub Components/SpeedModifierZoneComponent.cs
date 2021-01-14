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
// Sub Components are a toolbox of simple, common building block behaviours, meant to do a single
// thing within the constraints of the other existing components.
//
// Speed Modifier Zone Component
// This component is used to add or remove a speed modifier to the player. We use our gameobject
// as the id object.
//##################################################################################################
public class SpeedModifierZoneComponent : MonoBehaviour {

    public float speedMultiplier = 0.5f;

    private bool currentlyModifying = false;

    void Start(){
        DamageableComponent playerDamageable = FirstPersonPlayerComponent.player.GetComponent<DamageableComponent>();
        playerDamageable.RegisterOnKilledDelegate(PlayerKilled);
    }

    void OnTriggerEnter(Collider other){
        if(other.tag == "Player"){
            currentlyModifying = true;
            FirstPersonPlayerComponent.player.AddSpeedModifier(gameObject, speedMultiplier);
        }
    }

    void OnTriggerExit(Collider other){
        if(other.tag == "Player"){
            currentlyModifying = false;
            FirstPersonPlayerComponent.player.RemoveSpeedModifier(gameObject);
        }
    }

    void PlayerKilled(DamageableComponent damaged){
        if(currentlyModifying){
            FirstPersonPlayerComponent.player.RemoveSpeedModifier(gameObject);
        }
    }
}
