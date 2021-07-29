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

using UnityEngine;
using UnityEngine.UI;

//##################################################################################################
// Model Animated Gun Component
// This class is used with the Unity Animator to play animations on a 3d model of a gun
//
// The player could potentially have multiple ModelAnimatedGunComponents on it, getting enabled and
// disabled based on which weapon is selected.
//
// This class descends from ZoomableGunComponent, which descends from GunComponent
//##################################################################################################
[RequireComponent(typeof(FirstPersonPlayerComponent))]
public class ModelAnimatedGunComponent : ZoomableGunComponent {

    [HeaderAttribute("Model Animated Gun Component")]
    public MeshRenderer gunModel;
    public Animator gunAnimator;
    public string idleAnimationName;
    public string shootAnimationName;
    public string reloadAnimationName;
    public string scopeAnimationName;

    private DamageableComponent damage;

    //##############################################################################################
    // Always enable the model and animator, and trigger idle
    //##############################################################################################
    private void OnEnable(){
        gunModel.enabled = true;
        gunAnimator.enabled = true;

        gunAnimator.SetTrigger(idleAnimationName);
    }

    //##############################################################################################
    // Disable the model and animator on the way out
    //##############################################################################################
    private void OnDisable(){
        gunModel.enabled = false;
        gunAnimator.enabled = false;
    }

    //##############################################################################################
    // Check for required data
    //##############################################################################################
    protected new void Start(){
        base.Start();

        if(gunModel == null){
            Logger.Error("Gun Model on " + gameObject.name + "'s ModelAnimatedGunComponent cannot be null on start");
        }

        if(gunAnimator == null){
            Logger.Error("Gun Animator on " + gameObject.name + "'s ModelAnimatedGunComponent cannot be null on start");
        }

        damage = GetComponent<DamageableComponent>();
    }

    //##############################################################################################
    // Get the player input and trigger the shooting, then play out the appropriate animation
    // based on the state of the gun.
    //##############################################################################################
    protected new void Update(){
        base.Update();

        // Don't update animated gun if the player is dead
        if(damage.Dead()){
            return;
        }

        // Either use getMouseButton if the gun is automatic, or getMouseButtonDown if not
        bool inputTriggerPulled = currentGunData.automaticAction ? Input.GetMouseButton(0) : Input.GetMouseButtonDown(0);

        if(inputTriggerPulled){
            // If we are a manual reload and it's a progressive, interruptible load, do the interrupt
            if(reloading && currentGunData.manualReload && currentGunData.progressiveReloadInterruption){
                reloading = false;

                // If playing a reload sound, try to stop it.
                if(reloadSoundId != SoundManagerComponent.INVALID_SOUND){
                    SoundManagerComponent.StopSound(reloadSoundId);
                }
            }

            if(base.Shoot()){
                player.AddGunRecoil(this);
            }
        }

        bool reloadInput = Input.GetKeyDown(KeyCode.R); // TODO make this a setting
        if(!reloading && reloadInput && currentGunData.useAmmo && currentGunData.manualReload && remainingMagazineAmmoCount < currentGunData.maxMagazineAmmoCount && remainingBoxAmmoCount != 0){
            ReloadGun();
        }

        bool zooming = Zooming();

        gunAnimator.SetBool(idleAnimationName, !shooting && !reloading && !zooming);
        gunAnimator.SetBool(shootAnimationName, shooting && !zooming);
        gunAnimator.SetBool(reloadAnimationName, reloading && !zooming);
        gunAnimator.SetBool(scopeAnimationName, zooming);
    }
}
