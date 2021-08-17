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
    public const float LIGHT_UPDATE_TIME = 0.15f; // seconds

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

    [Space(10)]
    public bool tintWithNearestLight = false;

    [Space(10)]
    public bool useGunBob = false;
    public Vector3 gunImageStartPosition;
    public float gunBobVerticalAmplitude = 25.0f;
    public float gunBobHorizontalAmplitude = 50.0f;
    public float gunBobFrequency = 4.0f;

    private AnimatedGunState state;
    private int currentFrame;

    private Light nearestLight;

    private Timer firingAnimationTimer;
    private Timer reloadingAnimationTimer;
    private Timer lightListUpdateTimer;

    private RectTransform gunImageRectTransform;
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

        if(tintWithNearestLight){
            lightListUpdateTimer = new Timer(LIGHT_UPDATE_TIME);
        }

        if(useGunBob){
            gunImageRectTransform = gunSpriteImage.GetComponent<RectTransform>();
        }

        damage = GetComponent<DamageableComponent>();
    }

    //##############################################################################################
    // Get the player input and trigger the shooting, then play out the appropriate animation
    // based on the state of the gun.
    //##############################################################################################
    protected new void Update(){
        base.Update();

        if(tintWithNearestLight){
            UpdateTintWithNearestLight();
        }

        // Don't update the rest of the animated gun if the player is dead
        if(damage.Dead()){
            return;
        }

        if(useGunBob){
            UpdateGunBob();
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
        if(!reloading && reloadInput && currentGunData.useAmmo && currentGunData.manualReload && remainingMagazineAmmoCount < currentGunData.maxMagazineAmmoCount && remainingBoxAmmoCount != 0){
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

    private void UpdateTintWithNearestLight(){
        // Note that this will only work with point lights

        // Get all lights in scene. From unity for FindObjectsOfType,
        // "This function is very slow. It is not recommended to use this function every frame."
        // Therefore, we only get and iterate the list infrequently, but update the intensity of
        // the tint with the cached light at full framerate
        if(lightListUpdateTimer.Finished()){
            lightListUpdateTimer.Start();

            Light[] lights = FindObjectsOfType<Light>();

            nearestLight = null;
            float minDistanceSquared = float.MaxValue;

            foreach(Light light in lights){
                if(light.type == LightType.Point){
                    float lightDistanceSquared = (light.transform.position - transform.position).sqrMagnitude;

                    if(lightDistanceSquared < minDistanceSquared && lightDistanceSquared < light.range * light.range){
                        minDistanceSquared = lightDistanceSquared;
                        nearestLight = light;
                    }
                }
            }
        }

        if(nearestLight != null){
            // Calculate energy based on 1/distance^2
            float distanceToNearestLightSquared = (nearestLight.transform.position - transform.position).sqrMagnitude;
            float energy = Mathf.Clamp(nearestLight.intensity / distanceToNearestLightSquared, 0.0f, 1.0f);

            // apply lighting based on energy, preserving existing alpha from sprite
            Color lightColor = (energy * nearestLight.color) + RenderSettings.ambientLight;
            gunSpriteImage.color = new Color(lightColor.r, lightColor.g, lightColor.b, gunSpriteImage.color.a);
        } else {
            // Fallback to ambient light
            gunSpriteImage.color = new Color(RenderSettings.ambientLight.r, RenderSettings.ambientLight.g, RenderSettings.ambientLight.b, gunSpriteImage.color.a);
        }
    }

    private void UpdateGunBob(){
        // Get player velocity, paramerize it against max speed
        float playerCurrentVelocity = player.GetVelocity().magnitude;
        float playerVelocityParameterized = playerCurrentVelocity / player.GetMaxWalkSpeed();

        float scaledSine = playerVelocityParameterized * Mathf.Sin(gunBobFrequency * Time.time);
        float scaledCosine = playerVelocityParameterized * Mathf.Cos(gunBobFrequency * Time.time);

        gunImageRectTransform.anchoredPosition = new Vector3(
            gunImageStartPosition.x + gunBobHorizontalAmplitude * scaledSine,
            gunImageStartPosition.y + -Mathf.Abs(gunBobVerticalAmplitude * scaledCosine),
            gunImageStartPosition.z
        );
    }
}
