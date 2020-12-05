using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityStandardAssets.ImageEffects;

[RequireComponent(typeof(DamageableComponent))]
public class PlayerComponent : MonoBehaviour {
    public static PlayerComponent player;

    public const float MAX_ANGLE_DELTA = 50.0f;
    public const float MIN_PITCH = 90.0f;
    public const float MAX_PITCH = 270.0f;

    public const float NO_RESPAWN_HEIGHT = 50.0f;

    [Header("Game Objects")]
    public GameObject playerCamera;
    public GameObject cameraFadeLayer;

    [Header("Camera Settings")]
    public float xSensitivity = 1.0f; // TODO make these a setting
    public float ySensitivity = -1.0f;

    [Header("Walking Settings")]
    public float accelTime = 1.0f;
    public float maxWalkSpeed = 1.0f;
    private Vector3 velocity;
    private Vector3 accel;
    private Dictionary<GameObject, float> speedModifiers = new Dictionary<GameObject, float>();

    [Header("Respawn Settings")]
    public float respawnTime;
    public float respawnSpeedTime;

    [Header("Damage Settings")]
    public float damagedInvulnerabilityTime;
    private bool invulnPreviouslyFinished;
    public Image hudOverlay;

    [Header("Player Sounds")]
    public AudioClip[] footStepSounds;
    public float footStepVolume;
    private int prevFootstepSoundIndex;
    private float prevHeight;
    private float prevPrevHeight;

    private Vector3 lastSafePosition;
    private Timer safeTimer;

    private GunComponent gun;
    private CharacterController character;
    private DamageableComponent damage;
    private MotionBlur cameraMotionBlur;
    private CameraLookComponent cameraLook;

    private bool justDied;
    private bool dying;
    private Timer respawnTimer;
    private Timer respawnSpeedTimer;
    private Timer damagedInvulnerabilityTimer;
    private bool damageDisplay;

    private float aimingMultiplier;
    private bool movePending;

    private Image cameraFadeLayerImage;

    private bool movementEnabled;
    private bool lookingEnabled;

	void Start(){
        player = this;

        // We'll use our own cursor
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // Try to target 60
        Application.targetFrameRate = 60;

        movementEnabled = true;
        lookingEnabled = true;

        invulnPreviouslyFinished = true;

        gun = GetComponent<GunComponent>();
        character = GetComponent<CharacterController>();
        damage = GetComponent<DamageableComponent>();

        cameraMotionBlur = playerCamera.GetComponent<MotionBlur>();
        cameraLook = playerCamera.GetComponent<CameraLookComponent>();

        cameraFadeLayerImage = cameraFadeLayer.GetComponent<Image>();

        damage.RegisterOnDamagedDelegate(OnDamaged);
        damage.RegisterOnKilledDelegate(OnKilled);

        respawnTimer = new Timer(respawnTime);
        damagedInvulnerabilityTimer = new Timer(damagedInvulnerabilityTime);
        respawnSpeedTimer = new Timer(respawnSpeedTime);

        safeTimer = new Timer(2.0f);

        prevFootstepSoundIndex = -1;

        aimingMultiplier = 1.0f;
	}

	void Update(){
        UpdateRespawn();
        UpdateDamage();

        if(lookingEnabled){
            UpdateLook();
        }

        if(movementEnabled){
            UpdateMovement();
        }

        UpdateSafePosition();
	}

    void UpdateRespawn(){
        if(damage.CurrentHealth() <= 0.0f){
            if(justDied){
                justDied = false;
                velocity = Vector3.zero;

                // Save on Player Death
                SaveLoadManagerComponent.Instance().Save();

                PlayerRespawnVolumeComponent currentRespawn = PlayerRespawnVolumeComponent.GetCurrentRespawn();
                if(currentRespawn){
                    // LevelManagerComponent.Instance().SetCurrentLevel(currentRespawn.primaryLevel);

                    GameObject respawnPosition = currentRespawn.respawnPosition;
                    transform.position = respawnPosition.transform.position;
                } else {
                    // Oof
                    transform.position = new Vector3(60.0f, 0.0f, 15.0f);
                }

                transform.rotation = Quaternion.identity;

                playerCamera.transform.localRotation = Quaternion.identity;

                playerCamera.transform.localPosition = Vector3.up; // TODO fix this
                playerCamera.transform.localRotation = Quaternion.identity;

                // cameraMotionBlur.blurAmount = 0.0f;

                // I've tried everything else, but since CharacterController requires being grounded (something I can't guarantee on death)
                // I have to destroy and recreate the component to trigger a re-initialize, which zeroes out the velocity
                // Vector3 c_center = character.center;
                // float c_height = character.height;
                // float c_minMoveDistance = character.minMoveDistance;
                // float c_radius = character.radius;
                // float c_skinWidth = character.skinWidth;
                // float c_slopeLimit = character.slopeLimit;
                // float c_stepOffset = character.stepOffset;

                // Unity doesn't allow two of the same component at once
                // DestroyImmediate(character);
                // character = gameObject.AddComponent<CharacterController>();
                //
                // character.center = c_center;
                // character.height = c_height;
                // character.minMoveDistance = c_minMoveDistance;
                // character.radius = c_radius;
                // character.skinWidth = c_skinWidth;
                // character.slopeLimit = c_slopeLimit;
                // character.stepOffset = c_stepOffset;

                character.enabled = true;

                respawnSpeedTimer.Start();

                damage.Respawn();

                dying = false;
            }

            float p = respawnTimer.Parameterized();

            if(p >= 0.1f){
                cameraLook.SetInstant(false);
            }

            p = Mathf.Pow(p, 10.0f);
            cameraFadeLayerImage.color = new Color(0.0f, 0.0f, 0.0f, 1.0f - p);

            if(respawnTimer.Finished()){
                damage.Respawn();
            }
        }
    }

    void UpdateDamage(){
        // TODO refactor this so it doesn't flicker when you just died
        bool finished = damagedInvulnerabilityTimer.Finished();

        if(!finished && damage.CurrentHealth() > 0.0f){
            hudOverlay.enabled = damageDisplay;
            damageDisplay = !damageDisplay;
        } else if(finished && !invulnPreviouslyFinished){
            damage.SetInvincible(false);
            hudOverlay.enabled = true;
        }

        invulnPreviouslyFinished = finished;
    }

    void UpdateLook(){
        float xInput = Input.GetAxis("Mouse X") * xSensitivity;
        float yInput = Input.GetAxis("Mouse Y") * ySensitivity;

        xInput = Mathf.Clamp(xInput, -MAX_ANGLE_DELTA, MAX_ANGLE_DELTA);
        yInput = Mathf.Clamp(yInput, -MAX_ANGLE_DELTA, MAX_ANGLE_DELTA);

        float yawPrevious = playerCamera.transform.rotation.eulerAngles.y;
        float pitchPrevious = playerCamera.transform.rotation.eulerAngles.x;

        // Remapping  from Mouse X movement (right and left) to rotation about the y axis
        float yaw = yawPrevious + xInput;
        // Remapping  from Mouse Y movement (up and down) to rotation about the x axis
        float pitch = pitchPrevious + yInput;

        // We can't simply clamp because of the way the rotation goes around the circle
        //      90*
        //      ^
        //    --+-- 0* to 90* is ok
        //   /  |  \
        // <-+--+--+-> 0*
        //   \  |  /
        //    --+-- 360* to 270* is ok
        //      270*

        // If the pitch was in our bounds but is no longer, clamp to that bound
        if(pitch < MAX_PITCH && pitchPrevious >= MAX_PITCH){
            pitch = MAX_PITCH;
        } else if(pitch > MIN_PITCH && pitchPrevious <= MIN_PITCH){
            pitch = MIN_PITCH;
        }

        Quaternion yawRotation = Quaternion.Euler(0, yaw, 0);
        Quaternion pitchRotation = Quaternion.Euler(pitch, 0, 0);

        // The order in which these are combined matters. We want to apply yaw first, then pitch
        // Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * smooth);
        playerCamera.transform.localRotation = yawRotation * pitchRotation;
    }

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

        velocity = Vector3.SmoothDamp(velocity, movementVector, ref accel, accelTime);

        float totalModifier = 1.0f;
        foreach(float modifier in speedModifiers.Values){
            totalModifier *= modifier;
        }

        if(!Dead()){
            character.SimpleMove(velocity * aimingMultiplier * totalModifier);
        }
    }

    void UpdateSafePosition(){
        if(!Dead() && safeTimer.Finished() && character.isGrounded){
            lastSafePosition = transform.position;
            safeTimer.Start();
        }
    }

    public void AddGunRecoil(){
        // Momentum Recoil
        Vector3 forward = playerCamera.transform.forward;
        forward.y = 0.0f;
        forward.Normalize();

        velocity += gun.GetMomentumRecoil() * -forward;

        // Aim Recoil
        float yawPrevious = playerCamera.transform.rotation.eulerAngles.y;
        float pitchPrevious = playerCamera.transform.rotation.eulerAngles.x;

        float newPitch = pitchPrevious - gun.GetAimRecoil();

        if(newPitch < MAX_PITCH && pitchPrevious >= MAX_PITCH){
            newPitch = MAX_PITCH;
        } else if(newPitch > MIN_PITCH && pitchPrevious <= MIN_PITCH){
            newPitch = MIN_PITCH;
        }

        Quaternion yawRotation = Quaternion.Euler(0, yawPrevious, 0);
        Quaternion pitchRotation = Quaternion.Euler(newPitch, 0, 0);

        playerCamera.transform.localRotation = yawRotation * pitchRotation;
    }

    public void OnDamaged(DamageableComponent damage){
        damagedInvulnerabilityTimer.Start();
        damage.SetInvincible(true);
        GetComponent<BarkComponent>().Bark();
    }

    public void OnKilled(DamageableComponent damage){
        if(!dying){
            dying = true;
            justDied = true;
            damage.SetInvincible(false);

            character.enabled = false;

            cameraLook.SetInstant(true);

            cameraFadeLayerImage.color = new Color(0.0f, 0.0f, 0.0f, 1.0f);

            respawnTimer.Start();

            Debug.Log("=== PLAYER DEATH === at position " + transform.position + " from " + damage.GetDamager());
        }
    }

    public void SetAimingMultiplier(float value){
        aimingMultiplier = value;
    }

    public void SetVelocity(Vector3 newVelocity){
        velocity = newVelocity;
    }

    public Vector3 GetVelocity(){
        return velocity;
    }

    public bool Dead(){
        return damage.Dead();
    }

    public void AbleMovement(bool abled){
        movementEnabled = abled;

        if(abled){
            velocity = Vector3.zero;
        }
    }

    public void AbleLooking(bool abled){
        lookingEnabled = abled;
    }

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
            Debug.LogError("Trying to remove modifier " + id + ", but does not exist");
        }
    }
}
