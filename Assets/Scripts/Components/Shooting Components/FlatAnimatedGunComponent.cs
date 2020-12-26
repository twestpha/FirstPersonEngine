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
// Flat Animated Gun Component
// This class is a subclass to handle 2D (flat) animated guns, and displaying that ui to the player
//
// The player could potentially have multiple FlatAnimatedGunComponents on it, getting enabled and
// disabled based on which weapon is selected.
//
// This class descends from ZoomableGunComponent, which descends from GunComponent
//##################################################################################################
[RequireComponent(typeof(FirstPersonPlayerComponent))]
public class FlatAnimatedGunComponent : ZoomableGunComponent {

    private enum AnimatedGunState {
        Idle,
        Shooting,
        Reloading,
    }

    [HeaderAttribute("Flat Animated Gun Component")]
    public Image gunSpriteImage;
    public Sprite idleSprite;
    public float firingFramerate = 1.0f;
    public Sprite[] firingSprites;
    public float reloadingFramerate = 1.0f;
    public Sprite[] reloadingSprites;

    private AnimatedGunState state;
    private int currentFrame;

    private Timer firingAnimationTimer;
    private Timer reloadingAnimationTimer;

    private DamageableComponent damage;

    //##############################################################################################
    // Always set the sprite enabled, and to the idle sprite
    //##############################################################################################
    private void OnEnable(){
        gunSpriteImage.enabled = true;
        gunSpriteImage.sprite = idleSprite;
    }

    //##############################################################################################
    // Disable the sprite on the way out
    //##############################################################################################
    private void OnDisable(){
        gunSpriteImage.enabled = false;
    }

    //##############################################################################################
    // Setup the timers and check for required data
    //##############################################################################################
    protected new void Start(){
        base.Start();

        firingAnimationTimer = new Timer(1.0f / firingFramerate);
        reloadingAnimationTimer = new Timer(1.0f / reloadingFramerate);

        if(gunSpriteImage == null){
            Logger.Error("Gun Sprite Image on " + gameObject.name + "'s FlatAnimatedGunComponent cannot be null on start");
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
                currentFrame = 0;

                player.AddGunRecoil(this);

                if(reloading){
                    reloadingAnimationTimer.Start();
                    state = AnimatedGunState.Reloading;
                    gunSpriteImage.sprite = reloadingSprites[0];
                } else {
                    firingAnimationTimer.Start();
                    state = AnimatedGunState.Shooting;
                    gunSpriteImage.sprite = firingSprites[0];
                }
            }
        }

        bool reloadInput = Input.GetKeyDown(KeyCode.R); // TODO make this a setting
        if(!reloading && reloadInput && currentGunData.useAmmo && currentGunData.manualReload && currentAmmoCount < currentGunData.ammoCount){
            ReloadGun();

            reloadingAnimationTimer.Start();
            state = AnimatedGunState.Reloading;
            gunSpriteImage.sprite = reloadingSprites[0];
            currentFrame = 0;
        }

        // Animating the Gun
        if(state == AnimatedGunState.Shooting){
            if(firingAnimationTimer.Finished()){
                firingAnimationTimer.Start();

                currentFrame++;

                if(currentFrame >= firingSprites.Length){
                    state = AnimatedGunState.Idle;
                    gunSpriteImage.sprite = idleSprite;
                } else {
                    gunSpriteImage.sprite = firingSprites[currentFrame];
                }
            }
        } else if(state == AnimatedGunState.Reloading){
            if(reloadingAnimationTimer.Finished()){
                reloadingAnimationTimer.Start();

                currentFrame++;

                if(currentFrame >= reloadingSprites.Length || !reloading){
                    state = AnimatedGunState.Idle;
                    gunSpriteImage.sprite = idleSprite;
                } else {
                    gunSpriteImage.sprite = reloadingSprites[currentFrame];
                }
            }
        }
    }
}
