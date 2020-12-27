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

//##################################################################################################
// Sprite Enemy Behavior
// This is an example behavior, implementing a simple sprite-based enemy. It is meant to be
// inherited by a more specialized type of enemy. This class generally manages movement, taking
// damage, and dying.
//##################################################################################################
public class SpriteEnemyBehavior : EnemyBehavior {

    public enum MovementState {
        Idle,
        Moving,
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

    protected CharacterController character;
    protected DamageableComponent damage;
    protected RotatableComponent rotation;
    protected MaterialAnimationComponent materialAnimation;

    //##############################################################################################
    // Get the relevant component, and register for damageable delegates
    //##############################################################################################
    protected virtual void Start(){
        character = GetComponent<CharacterController>();
        damage = GetComponent<DamageableComponent>();
        rotation = GetComponent<RotatableComponent>();
        materialAnimation = GetComponent<MaterialAnimationComponent>();

        if(damage != null){
            damage.RegisterOnDamagedDelegate(Damaged);
            damage.RegisterOnKilledDelegate(Killed);
        }
    }

    //##############################################################################################
    // For now, just update movement. More behavior can be tacked onto here.
    //##############################################################################################
    public override void EnemyUpdate(){
        UpdateMovement();
    }

    //##############################################################################################
    // Very simply move logic, just move towards target with velocity, until we're there.
    //##############################################################################################
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

            if(velocityMag > 0.0f){
                transform.rotation = Quaternion.LookRotation(velocity);
            }

            if(velocityMag > 1.0f){
                velocity.Normalize();
            }

            movePending = true;
            pendingMoveVelocity = velocity * movementSpeed;
        }
    }

    //##############################################################################################
    // Some getters for state
    //##############################################################################################
    protected bool CanMove(){
        return (movementState == MovementState.Idle || movementState == MovementState.Moving);
    }

    protected bool AtGoal(){
        return movementState == MovementState.Idle;
    }

    //##############################################################################################
    // Do the actual move during fixed update so we're not wasting time on physics synching
    //##############################################################################################
    void FixedUpdate(){
        if(movePending){
            movePending = false;

            if(character != null){
                character.SimpleMove(pendingMoveVelocity);
            } else {
                transform.position += pendingMoveVelocity * Time.deltaTime;
            }

            pendingMoveVelocity = Vector3.zero;

            // If it has them, clamp to room bounds
            if(roomBounds != null){
                transform.position = roomBounds.bounds.ClosestPoint(transform.position);
            }
        }
    }

    //##############################################################################################
    // Play the damaged sequence, where more behavior can be tacked onto.
    //##############################################################################################
    public void Damaged(DamageableComponent damaged){
        PlayDamagedSequence();
    }

    //##############################################################################################
    // If the enemy is damaged, register to update it and react
    //##############################################################################################
    public virtual void PlayDamagedSequence(){
        EnemyManagerComponent.RegisterUpdate(this);
    }

    //##############################################################################################
    // Play the death sequence, where more behavior can be tacked onto.
    //##############################################################################################
    public void Killed(DamageableComponent damaged){
        PlayDeathSequence();
    }

    //##############################################################################################
    // For now, just destroy the game object. More behavior can be tacked onto here.
    //##############################################################################################
    public virtual void PlayDeathSequence(){
        Destroy(gameObject);
    }

    //##############################################################################################
    // Commands for where or how to move
    //##############################################################################################
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

    //##############################################################################################
    // When the player enters a trigger around the enemy, register for an update to react.
    //##############################################################################################
    public void OnTriggerEnter(Collider other){
        if(other.tag == "Player"){
            EnemyManagerComponent.RegisterUpdate(this);
        }
    }
}
