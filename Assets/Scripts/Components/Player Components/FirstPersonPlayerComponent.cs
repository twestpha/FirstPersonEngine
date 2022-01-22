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
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityStandardAssets.ImageEffects;

//##################################################################################################
// First Person Player Component
// This class is responsible for managing many aspects of a first-person shooter player. It manages
// movement and looking, and taking damage and respawning.
//##################################################################################################
[RequireComponent(typeof(DamageableComponent))]
public class FirstPersonPlayerComponent : MonoBehaviour {
    public static FirstPersonPlayerComponent player;

    public const float MAX_ANGLE_DELTA = 50.0f;

    // 0.1 away from 90* and 270* respectively
    public const float LOWER_PITCH = 89.9f;
    public const float UPPER_PITCH = 270.1f;

    private const float SHAKE_SCALE = 0.06f;
    private const float SHAKE_DECAY_TIME = 0.9f;
    private const float SHAKE_NEAR_DISTANCE = 2.0f; // Distance at which shake is 1x
    private const float SHAKE_FAR_DISTANCE = 20.0f; // Distance at which shake is 0x

    public const float SAFE_TIME = 2.0f;
    public const float PLAYER_RESPAWN_HEIGHT_OFFSET = 50.0f;

    public const float SIDE_JUMP_COOLDOWN_TIME = 0.4f;

    public const float FOOTSTEP_TIME = 0.4f;

    [Header("Camera References")]
    public GameObject playerCamera;
    public Image fade;

    [Header("Camera Settings")]
    public float xSensitivity = 1.0f; // TODO make these a setting
    public float ySensitivity = -1.0f;

    private Dictionary<GameObject, float> lookModifiers = new Dictionary<GameObject, float>();

    private bool lookingEnabled;
    private Quaternion playerInputLookRotation;

    [Header("Movement Settings")]
    public float accelTime = 1.0f;
    public float maxWalkSpeed = 1.0f;

    public bool jumpingEnabled;
    public float jumpVelocity = 1.0f;
    public float jumpTime = 1.0f;

    private bool movementEnabled;
    private Vector3 velocity;
    private Vector3 accel;
    private Dictionary<GameObject, float> speedModifiers = new Dictionary<GameObject, float>();

    [Header("Respawn Settings")]
    public float respawnTime = 1.5f;

    [Header("Player Sounds")]
    public BarkComponent playerHurtBarkComponent;
    public BarkComponent playerFootstepBarkComponent;

    private Vector3 lastSafePosition;
    private Timer safeTimer;

    private GunComponent gun;
    private CharacterController character;
    private DamageableComponent damage;

    public enum RespawnState {
        Alive,
        Dying,
        Respawning,
    }

    private RespawnState respawnState;

    private Timer respawnTimer;
    private bool damageDisplay;

    private Timer shakeDecayTimer;
    private float shakeAmount;

    private Timer recoilDecayTimer;
    private float recoilAmount;

    private Timer jumpTimer;
    private Timer sideJumpCooldownTimer;

    private float footstepTimeRemaining = FOOTSTEP_TIME;

    //##############################################################################################
    // Setup the player, and collect the different components that will get used.
    //##############################################################################################
	void Start(){
        player = this;
        respawnState = RespawnState.Alive;

        // Hide the cursor and lock it to screen
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // Try to target 60
        Application.targetFrameRate = 60;

        movementEnabled = true;
        lookingEnabled = true;

        gun = GetComponent<GunComponent>();
        character = GetComponent<CharacterController>();
        damage = GetComponent<DamageableComponent>();

        damage.RegisterOnDamagedDelegate(OnDamaged);
        damage.RegisterOnKilledDelegate(OnKilled);

        respawnTimer = new Timer(respawnTime);
        shakeDecayTimer = new Timer(SHAKE_DECAY_TIME);
        safeTimer = new Timer(SAFE_TIME);
        recoilDecayTimer = new Timer();
        jumpTimer = new Timer(jumpTime);
        sideJumpCooldownTimer = new Timer(SIDE_JUMP_COOLDOWN_TIME);
	}

    //##############################################################################################
    // The core update look. Basically, update respawn and safe position always.
    // If they're enabled and therefore allowed, update looking and movement.
    //##############################################################################################
	void Update(){
        UpdateRespawn();

        if(lookingEnabled){
            UpdateLook();
        }

        if(movementEnabled){
            UpdateMovement();
        }

        UpdateSafePosition();
	}

    //##############################################################################################
    // Either fade out if dead, or teleport then fade in if respawning.
    //##############################################################################################
    void UpdateRespawn(){
        if(respawnState == RespawnState.Dying || respawnState == RespawnState.Respawning){

            float t = respawnTimer.Parameterized();

            if(fade != null){
                if(respawnState == RespawnState.Dying){
                    fade.color = new Color(0.0f, 0.0f, 0.0f, t);
                } else {
                    fade.color = new Color(0.0f, 0.0f, 0.0f, 1.0f - t);
                }
            }

            if(respawnTimer.Finished()){
                if(respawnState == RespawnState.Dying){
                    respawnTimer.Start();
                    respawnState = RespawnState.Respawning;

                    velocity = Vector3.zero;

                    // Uncomment this to save on player respawn
                    // SaveLoadManagerComponent.Instance().Save();

                    PlayerRespawnVolumeComponent currentRespawn = PlayerRespawnVolumeComponent.GetCurrentRespawn();
                    if(currentRespawn){
                        transform.position = currentRespawn.respawnPosition.transform.position;

                        playerCamera.transform.localRotation = currentRespawn.transform.rotation;
                        playerInputLookRotation = currentRespawn.transform.rotation;
                    } else {
                        // In case there's no respawn point, teleport the player up a bunch.
                        transform.position = new Vector3(0.0f, transform.position.y + PLAYER_RESPAWN_HEIGHT_OFFSET, 0.0f);

                        playerCamera.transform.localRotation = Quaternion.identity;
                        playerInputLookRotation = Quaternion.identity;
                    }

                    transform.rotation = Quaternion.identity;

                    // This is not ideal, but since CharacterController requires being grounded to set
                    // velocity - something that can't be guaranteed on death - We have to destroy and
                    // recreate the component to trigger a re-initialize, which zeroes out the velocity.
                    Vector3 c_center = character.center;
                    float c_height = character.height;
                    float c_minMoveDistance = character.minMoveDistance;
                    float c_radius = character.radius;
                    float c_skinWidth = character.skinWidth;
                    float c_slopeLimit = character.slopeLimit;
                    float c_stepOffset = character.stepOffset;

                    // Unity doesn't allow two of this component at once
                    DestroyImmediate(character);
                    character = gameObject.AddComponent<CharacterController>();

                    character.center = c_center;
                    character.height = c_height;
                    character.minMoveDistance = c_minMoveDistance;
                    character.radius = c_radius;
                    character.skinWidth = c_skinWidth;
                    character.slopeLimit = c_slopeLimit;
                    character.stepOffset = c_stepOffset;

                    character.enabled = true;

                    movementEnabled = true;
                    lookingEnabled = true;

                    damage.Respawn();
                } else {
                    respawnState = RespawnState.Alive;
                }
            }
        }
    }

    //##############################################################################################
    // Update the look direction based on the mouse input, camera shake amount, and recoil
    //##############################################################################################
    void UpdateLook(){
        float totalModifier = 1.0f;

        // Make sure to purge null modifiers to cleanup up destroyed, dangling references
        foreach(var idAndModifier in lookModifiers){
            if(idAndModifier.Key != null){
                totalModifier *= idAndModifier.Value;
            } else {
                speedModifiers.Remove(idAndModifier.Key);
            }
        }

        // zoomLookModifier
        float xInput = Input.GetAxis("Mouse X") * xSensitivity * totalModifier;
        float yInput = Input.GetAxis("Mouse Y") * ySensitivity * totalModifier;

        xInput = Mathf.Clamp(xInput, -MAX_ANGLE_DELTA, MAX_ANGLE_DELTA);
        yInput = Mathf.Clamp(yInput, -MAX_ANGLE_DELTA, MAX_ANGLE_DELTA);

        float lookYawPrevious = playerInputLookRotation.eulerAngles.y;
        float lookPitchPrevious = playerInputLookRotation.eulerAngles.x;

        // Remapping  from Mouse X movement (right and left) to rotation about the y axis
        float lookYaw = lookYawPrevious + xInput;
        // Remapping  from Mouse Y movement (up and down) to rotation about the x axis
        float lookPitch = lookPitchPrevious + yInput;

        // Apply recoil
        if(recoilAmount > 0.0f){
            float recoilT = 1.0f - recoilDecayTimer.Parameterized();

            lookPitch -= recoilAmount * recoilT * Time.deltaTime;

            if(recoilDecayTimer.Finished()){
                recoilAmount = 0.0f;
            }
        }

        // We can't simply clamp because of the way the rotation goes around the circle
        //       270*
        //       ^
        //     --+-- 270* to 360* is ok
        //    /  |  \
        // <-+---+---+-> 360*/0* Forward
        //    \  |  /
        //     --+-- 0* to 90* is ok
        //      90*

        bool inUpperQuadrant = lookPitchPrevious > 180.0f;

        if(lookPitch < UPPER_PITCH && inUpperQuadrant){
            lookPitch = UPPER_PITCH;
        } else if(lookPitch > LOWER_PITCH && !inUpperQuadrant){
            lookPitch = LOWER_PITCH;
        }

        Quaternion yawRotation = Quaternion.Euler(0, lookYaw, 0);
        Quaternion pitchRotation = Quaternion.Euler(lookPitch, 0, 0);

        // The order in which these are combined matters. We want to apply yaw first, then pitch
        playerInputLookRotation = yawRotation * pitchRotation;

        // Calculate and apply the camera shake
        Quaternion shakeOffsetRotation = Quaternion.identity;

        if(shakeAmount > 0.0f){
            float t = 1.0f - shakeDecayTimer.Parameterized();
            t = Mathf.Sin(t * Mathf.PI);

            Vector3 shakeOffset = new Vector3(
                Mathf.Sin(Time.time * 31.0f) * t * shakeAmount * SHAKE_SCALE,
                Mathf.Sin(Time.time * 19.0f) * t * shakeAmount * SHAKE_SCALE,
                1.0f
            );

            shakeOffsetRotation = Quaternion.LookRotation(shakeOffset);

            if(shakeDecayTimer.Finished()){
                shakeAmount = 0.0f;
            }
        }

        // Apply the offset to the playerInputLookRotation so shake is only applied on top of the
        // input
        playerCamera.transform.localRotation = playerInputLookRotation * shakeOffsetRotation;
    }

    //##############################################################################################
    // Gather inputs, get intended movement direction, and try to move there.
    //##############################################################################################
    void UpdateMovement(){
        if(!character.enabled){
            return;
        }

        // Zero-ing out the y components keeps the player at the same height
        Vector3 forward = playerCamera.transform.forward;
        forward.y = 0.0f;
        forward.Normalize();

        Vector3 right = playerCamera.transform.right;
        right.y = 0.0f;
        right.Normalize();

        // Multiple if's so that the inputs can be combined, so there's not a single input
        Vector3 movementVector = Vector3.zero;

        if(Input.GetKey(KeyCode.W)){
            movementVector += forward;
        }

        if(Input.GetKey(KeyCode.A)){
            movementVector -= right;
        }

        if(Input.GetKey(KeyCode.S)){
            movementVector -= forward;
        }

        if(Input.GetKey(KeyCode.D)){
            movementVector += right;
        }

        movementVector.Normalize();

        float maxSpeed = maxWalkSpeed;
        movementVector *= maxSpeed;

        // Smoothdamp towards max walk speed without changing vertical velocities (ignore damping gravity and jumping)
        float previousYVelocity = velocity.y;
        velocity = Vector3.SmoothDamp(velocity, movementVector, ref accel, accelTime);
        velocity.y = previousYVelocity;

        // Add jump velocity when space is pressed and grounded
        // Apply jump until space is released or timer expires
        if(jumpingEnabled && Input.GetKey(KeyCode.Space)){
            if(character.isGrounded /*|| (character.collisionFlags == CollisionFlags.Sides && sideJumpCooldownTimer.Finished())*/){ // This allows wall-jumping after a short time after the previous jump. Might be too intense for this game?
                jumpTimer.Start();

                if(character.collisionFlags == CollisionFlags.Sides){
                    sideJumpCooldownTimer.Start();
                }
            }

            if(!jumpTimer.Finished()){
                velocity.y = jumpVelocity;
            }
        }

        if(character.isGrounded){
            sideJumpCooldownTimer.SetParameterized(1.0f);
        }

        float totalModifier = 1.0f;

        // Make sure to purge null modifiers to cleanup up destroyed, dangling references
        foreach(var idAndModifier in speedModifiers){
            if(idAndModifier.Key != null){
                totalModifier *= idAndModifier.Value;
            } else {
                speedModifiers.Remove(idAndModifier.Key);
            }
        }

        if(!Dead()){
            character.Move(velocity * totalModifier * Time.deltaTime);

            if(character.isGrounded && playerFootstepBarkComponent != null){
                float footstepMultiplier = velocity.magnitude / maxWalkSpeed;
                footstepTimeRemaining -= (footstepMultiplier * Time.deltaTime);

                if(footstepTimeRemaining < 0.0f){
                    playerFootstepBarkComponent.Bark();
                    footstepTimeRemaining = FOOTSTEP_TIME;
                }
            }
        }

        if(character.isGrounded){
            velocity.y = -1.0f;
        } else {
            velocity.y += Physics.gravity.y * Time.deltaTime;
        }
    }

    //##############################################################################################
    // Keep track of the not-dead, grounded position with an infrequent timer. This is helpful for
    // when a player drops items on death, and we don't want to spawn that when they fall down a
    // pit.
    //##############################################################################################
    void UpdateSafePosition(){
        if(!Dead() && safeTimer.Finished() && character.isGrounded){
            lastSafePosition = transform.position;
            safeTimer.Start();
        }
    }

    //##############################################################################################
    // Add both movement and camera look recoil to the player
    //##############################################################################################
    public void AddGunRecoil(GunComponent gun){
        // Momentum Recoil
        Vector3 forward = playerCamera.transform.forward;
        forward.y = 0.0f;
        forward.Normalize();

        velocity += gun.GetMomentumRecoil() * -forward;

        // Aim Recoil
        // Note that this stomps any existing recoil.
        float recoilTime = gun.GetCooldown();

        recoilDecayTimer.SetDuration(recoilTime);
        recoilDecayTimer.Start();

        // The gun returns the total amount of degrees the recoil is going to do. So, we divide this
        // by time to get Degrees Per Second to apply over time. But, we're going to start a 1x, but
        // over the span of the timer, reduce that to 0x, giving it a falloff. So, to make sure we
        // hit the total amount of degrees, we integrate over that curve, so we multiply by 2 to
        // get that value correctly.
        recoilAmount = gun.GetAimRecoil() * 2.0f / recoilTime;
    }

    //##############################################################################################
    // Add a Camera shake with an amount, scaled by distance, and start the decay timer.
    //##############################################################################################
    #if UNITY_EDITOR
    [ContextMenu("Test Small Camera Shake")]
    void TestSmallCameraShake(){
        AddCameraShake(0.5f, transform.position);
    }

    [ContextMenu("Test Medium Camera Shake")]
    void TestMediumCameraShake(){
        AddCameraShake(1.5f, transform.position);
    }

    [ContextMenu("Test Large Camera Shake")]
    void TestLargeCameraShake(){
        AddCameraShake(5.0f, transform.position);
    }
    #endif // UNITY_EDITOR

    public void AddCameraShake(float amount, Vector3 origin){
        float distance = (transform.position - origin).magnitude;
        float t = Mathf.Abs(SHAKE_FAR_DISTANCE - distance) / (SHAKE_FAR_DISTANCE - SHAKE_NEAR_DISTANCE);
        t = Mathf.Clamp(t, 0.0f, 1.0f);

        if(t > 0.0f){
            shakeAmount += (t * amount);
            shakeDecayTimer.Start();
        }
    }

    //##############################################################################################
    // When damaged, play a pseudo-random grunt or big oof
    //##############################################################################################
    public void OnDamaged(DamageableComponent damage){
        if(playerHurtBarkComponent != null){
            playerHurtBarkComponent.Bark();
        }
    }

    //##############################################################################################
    // On killed, disable features and set state.
    //##############################################################################################
    public void OnKilled(DamageableComponent damage){
        respawnState = RespawnState.Dying;
        respawnTimer.Start();

        // Stop movement
        velocity = Vector3.zero;
        character.enabled = false;

        movementEnabled = false;
        lookingEnabled = false;

        // Log this for debugging and data
        Logger.Info("=== PLAYER DEATH === at position " + transform.position + " from " + damage.GetDamager());
    }

    //##############################################################################################
    // Simple setters and getters
    //##############################################################################################
    public void SetVelocity(Vector3 newVelocity){
        velocity = newVelocity;
    }

    public Vector3 GetVelocity(){
        return velocity;
    }

    public float GetMaxWalkSpeed(){
        return maxWalkSpeed;
    }

    public bool Dead(){
        return damage.Dead();
    }

    public void AbleMovement(bool abled){
        movementEnabled = abled;

        if(!abled){
            // Make sure to clear out velocity
            velocity = Vector3.zero;
        }
    }

    public void AbleLooking(bool abled){
        lookingEnabled = abled;
    }

    //##############################################################################################
    // Add or remove a speed modifier. These are identified by the game object
    //##############################################################################################
    public void AddSpeedModifier(GameObject id, float modifier){
        if(!speedModifiers.ContainsKey(id)){
            speedModifiers.Add(id, modifier);
        } else {
            // Edit the speed if it does exist
            speedModifiers[id] = modifier;
        }
    }

    public void RemoveSpeedModifier(GameObject id){
        if(speedModifiers.ContainsKey(id)){
            speedModifiers.Remove(id);
        } else {
            Logger.Error("Trying to remove speed modifier " + id + ", but does not exist");
        }
    }

    //##############################################################################################
    // Add or remove a look modifier. These are identified by the game object
    //##############################################################################################
    public void AddLookModifier(GameObject id, float modifier){
        if(!lookModifiers.ContainsKey(id)){
            lookModifiers.Add(id, modifier);
        } else {
            // Edit the look if it does exist
            lookModifiers[id] = modifier;
        }
    }

    public void RemoveLookModifier(GameObject id){
        if(lookModifiers.ContainsKey(id)){
            lookModifiers.Remove(id);
        } else {
            Logger.Error("Trying to remove look modifier " + id + ", but does not exist");
        }
    }
}
