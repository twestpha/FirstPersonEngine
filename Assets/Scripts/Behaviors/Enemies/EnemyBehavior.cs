using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehavior : MonoBehaviour {
    public const float STUNNED_ROTATION_SPEED = 720.0f;
    public const float DAMAGE_FLASH_DURATION = 0.25f;
    public const float MAX_KNOCKBACK_DURATION = 1.5f;

    public enum MovementState {
        Idle,
        Moving,
        Knockback,
    }

    [Header("Movement")]
    public float movementSpeed;
    public float accelerationTime;
    public MovementState movementState;
    public float atGoalThreshold = 0.05f;

    private Vector3 velocity;
    private Vector3 acceleration;
    private Vector3 targetPosition;
    private bool movePending;
    private Vector3 pendingMoveVelocity;

    public BoxCollider roomBounds;

    [Header("Damaging")]
    public bool useDamageKnockback;
    public float knockbackDistance = 0.5f;
    public float knockbackSpeed = 1.0f;
    public bool useDamageFlash;
    private bool damageFlashing;
    public GameObject deathEffectsPrefab;
    public Vector3 deathEffectsOffset;
    public float hitSoundEffectsVolume;
    public AudioClip hitSoundEffects;
    public bool stunnable = false;
    private bool currentlyStunned = false;
    public float stunDuration = 3.0f;

    protected Timer damageFlashTimer;
    private Timer maxKnockbackTimer;
    private Timer stunTimer;

    protected CharacterController character;
    protected DamageableComponent damage;
    protected RotatableComponent rotation;
    protected MaterialAnimationComponent materialAnimation;

    protected virtual void Start(){
        character = GetComponent<CharacterController>();
        damage = GetComponent<DamageableComponent>();
        rotation = GetComponent<RotatableComponent>();
        materialAnimation = GetComponent<MaterialAnimationComponent>();

        if(damage != null){
            if(useDamageFlash){
                damageFlashTimer = new Timer(DAMAGE_FLASH_DURATION);
            }

            if(useDamageKnockback){
                maxKnockbackTimer = new Timer(MAX_KNOCKBACK_DURATION);
            }

            damage.RegisterOnDamagedDelegate(Damaged);
            damage.RegisterOnKilledDelegate(Killed);
        }

        if(stunnable){
            stunTimer = new Timer(stunDuration);
        }
    }

    public virtual void EnemyUpdate(){
        UpdateMovement();
        UpdateDamaged();
        UpdateStun();
    }

    public void UpdateMovement(){
        if(movementState == MovementState.Moving){
            // Check if we're close
            Vector3 toTarget = targetPosition - transform.position;
            toTarget.y = 0.0f;
            if(toTarget.magnitude <= atGoalThreshold){
                movementState = MovementState.Idle;
                return;
            }

            toTarget.Normalize();

            velocity = Vector3.SmoothDamp(velocity, toTarget, ref acceleration, accelerationTime);

            float velocityMag = velocity.sqrMagnitude;

            if(velocityMag > 0.0f && !currentlyStunned){
                transform.rotation = Quaternion.LookRotation(velocity);
            }

            if(velocityMag > 1.0f){
                velocity.Normalize();
            }

            movePending = true;
            pendingMoveVelocity = velocity * movementSpeed;
        } else if(movementState == MovementState.Knockback){
            Vector3 toTarget = targetPosition - transform.position;
            toTarget.y = 0.0f;

            if(toTarget.magnitude <= atGoalThreshold || maxKnockbackTimer.Finished()){
                movementState = MovementState.Idle;
                return;
            }

            toTarget.Normalize();

            movePending = true;
            pendingMoveVelocity = toTarget * knockbackSpeed;
        }
    }

    public void UpdateDamaged(){
        if(useDamageFlash && damageFlashing && damageFlashTimer.Finished()){
            damageFlashing = false;

            rotation.SetAnimationIndex(RotatableComponent.DEFAULT_ANIMATION_INDEX);
            materialAnimation.ForceUpdate();

            if(damage.Dead()){
                PlayDeathSequence();
            }
        }
    }

    public void UpdateStun(){
        if(stunnable && currentlyStunned){
            movePending = false;
            pendingMoveVelocity = Vector3.zero;

            Quaternion rotationMovement = Quaternion.Euler(0.0f, STUNNED_ROTATION_SPEED * Time.deltaTime, 0.0f);
            transform.rotation *= rotationMovement;

            materialAnimation.ForceUpdate();

            if(stunTimer.Finished()){
                currentlyStunned = false;
            }
        }
    }

    protected bool CanMove(){
        return (movementState == MovementState.Idle || movementState == MovementState.Moving) && !currentlyStunned;
    }

    protected bool AtGoal(){
        return movementState == MovementState.Idle;
    }

    void FixedUpdate(){
        if(movePending){
            movePending = false;

            if(character != null){
                character.SimpleMove(pendingMoveVelocity);
            } else {
                transform.position += pendingMoveVelocity * Time.deltaTime;
            }

            pendingMoveVelocity = Vector3.zero;

            if(roomBounds != null){
                transform.position = roomBounds.bounds.ClosestPoint(transform.position);
            }
        }
    }

    public void Damaged(DamageableComponent damaged){
        if(useDamageKnockback){
            maxKnockbackTimer.Start();

            movementState = MovementState.Knockback;

            Vector3 damageDirection = transform.position - damage.GetDamagerOrigin();
            damageDirection.y = 0.0f;
            damageDirection.Normalize();

            targetPosition = transform.position + (damageDirection * knockbackDistance);
        }

        if(useDamageFlash){
            damageFlashTimer.Start();
            damageFlashing = true;

            rotation.SetAnimationIndex(RotatableComponent.DAMAGING_ANIMATION_INDEX);
            materialAnimation.ForceUpdate();
        }

        if(hitSoundEffects != null){
            SoundManagerComponent.PlaySound(
                hitSoundEffects,
                SoundCount.Single,
                SoundType.ThreeDimensional,
                hitSoundEffectsVolume,
                0.0f,
                SoundPriority.Medium
            );
        }

        EnemyManagerComponent.RegisterUpdate(this);
    }

    public void Killed(DamageableComponent damaged){
        if(useDamageFlash){
            // Wait until after damage flash to destroy for better feel
            damageFlashTimer.Start();
            damageFlashing = true;

            rotation.SetAnimationIndex(RotatableComponent.DAMAGING_ANIMATION_INDEX);
            materialAnimation.ForceUpdate();
        } else {
            PlayDeathSequence();
        }
    }

    public void PlayDeathSequence(){
        if(deathEffectsPrefab != null){
            GameObject deathEffects = Object.Instantiate(deathEffectsPrefab);

            Vector3 fxPosition = transform.position + deathEffectsOffset;

            deathEffects.transform.position = fxPosition;
        }

        Destroy(gameObject);
    }

    public void MoveToPosition(Vector3 target){
        if(CanMove()){
            movementState = MovementState.Moving;
            targetPosition = target;
        }
    }

    public void StopMoving(){
        movementState = MovementState.Idle;
        targetPosition = transform.position;
    }

    public void Stun(){
        if(stunnable){
            currentlyStunned = true;
            stunTimer.Start();
        }

        EnemyManagerComponent.RegisterUpdate(this);
    }

    public void OnTriggerEnter(Collider other){
        if(other.tag == "Player"){
            // Debug.Log("Registering Update for " + gameObject);
            EnemyManagerComponent.RegisterUpdate(this);
        }
    }
}
