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
using UnityEngine.UI;

//##################################################################################################
// First Person Hud Component
// This is responsible for displaying common shooter features to the heads-up display (HUD). This
// includes displaying a flashing layer (usually red) to indicate taking damage, as well as
// displaying the gun and ammo that the player is currently using
//##################################################################################################
[RequireComponent(typeof(FirstPersonPlayerComponent))]
[RequireComponent(typeof(DamageableComponent))]
public class FirstPersonHudComponent : MonoBehaviour {

    [Header("Damage Flash")]
    public Image damageFlashLayer;
    public float damageFlashTime;
    private bool damageFlashing;
    private Timer damageFlashTimer;

    [Header("Health Text")]
    public Text healthText;

    [Header("Ammo Text")]
    public Text remainingMagazineAmmoText;
    public Text remainingBoxAmmoText;

    [Header("Current Gun")]
    public Image currentGunImage;
    public GunComponent[] gunComponents;
    public Sprite[] gunSprites;
    private int currentGunIndex = -1;

    private DamageableComponent playerDamageable;

    //##############################################################################################
    // The gun components and the sprites must be 1-to-1 so that the sprite lookup finds a match.
    // Then, setup the timer and register for the damaged delegate.
    //##############################################################################################
    void Start(){
        if(gunComponents.Length != gunSprites.Length){
            Logger.Error("FirstPersonHudComponent's gunComponents and gunSprites lengths don't match");
        }

        damageFlashTimer = new Timer(damageFlashTime);

        playerDamageable = GetComponent<DamageableComponent>();
        playerDamageable.RegisterOnDamagedDelegate(OnDamaged);
        playerDamageable.RegisterOnHealedDelegate(OnHealed);
        playerDamageable.RegisterOnRespawnedDelegate(OnRespawned);

        healthText.text = playerDamageable.CurrentHealth().ToString();
    }

    //##############################################################################################
    // If the damage is flashing, fade it out over time and disabling when done.
    // Also update the gun sprite and text when relevant.
    //##############################################################################################
	void Update(){
        // Update damage flash
        if(damageFlashing){
            damageFlashLayer.color = new Color(1.0f, 1.0f, 1.0f, 1.0f - damageFlashTimer.Parameterized());

            if(damageFlashTimer.Finished()){
                damageFlashing = false;
                damageFlashLayer.enabled = false;
            }
        }

        // Update current gun index, updating info if gun changed
        int previousGunIndex = currentGunIndex;
        for(int i = 0, count = gunComponents.Length; i < count; ++i){
            if(gunComponents[i].enabled){
                currentGunIndex = i;
                break;
            }
        }

        if(currentGunIndex != previousGunIndex){
            currentGunImage.sprite = gunSprites[currentGunIndex];

            remainingMagazineAmmoText.enabled = gunComponents[currentGunIndex].GetRemainingMagazineAmmoCount() >= 0;
            remainingBoxAmmoText.enabled = gunComponents[currentGunIndex].GetRemainingBoxAmmoCount() >= 0;
        }

        if(remainingMagazineAmmoText != null){
            int remainingMagazineAmmoCount = gunComponents[currentGunIndex].GetRemainingMagazineAmmoCount();
            int remainingBoxAmmoCount = gunComponents[currentGunIndex].GetRemainingBoxAmmoCount();

            // Ammo being negative indicates that its not used
            if(remainingMagazineAmmoCount >= 0){
                remainingMagazineAmmoText.text = remainingMagazineAmmoCount.ToString();
            }

            if(remainingBoxAmmoCount >= 0){
                remainingBoxAmmoText.text = remainingBoxAmmoCount.ToString();
            }
        }
    }

    //##############################################################################################
    // When damage, begin flashing, and update health text
    //##############################################################################################
    public void OnDamaged(DamageableComponent damaged){
        damageFlashing = true;
        damageFlashLayer.enabled = true;
        damageFlashTimer.Start();

        healthText.text = damaged.CurrentHealth().ToString();
        if(damaged.Dead()){
            healthText.enabled = false;
        }
    }

    //##############################################################################################
    // When healed, update health text
    //##############################################################################################
    public void OnHealed(DamageableComponent damaged){
        healthText.text = damaged.CurrentHealth().ToString();
    }

    //##############################################################################################
    // When respawned, update health text
    //##############################################################################################
    public void OnRespawned(DamageableComponent damaged){
        healthText.enabled = true;
        healthText.text = damaged.CurrentHealth().ToString();
    }
}
