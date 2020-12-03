using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(DamageableComponent))]
public class MovementBehavior : MonoBehaviour {
    public const float AT_GOAL_THRESHOLD = 0.5f;
    public const float STUCK_THRESHOLD = 0.25f;
    public const float LOD_DISTANCE_SQUARED = 10000.0f; // 100 units
    public const float FALL_SPEED = 3.0f;

    public const float STUCK_CHECK_TIME = 0.2f;
    public const int TIME_STUCK_GIVE_UP = 5;
    public const int TIME_STUCK_SIDE_STEP = 3;

    public bool DEBUG;

    public enum MovementState {
        AtGoal,
        DirectRoute,
        SideStepping,
        DeadAndStoppedMoving,
    }

    public float accelTime;
    public float walkSpeed;
    public MovementState state;
    public bool alwaysMove;

    private Vector3 goalPosition;
    private Vector3 interimGoalPosition;
    private Vector3 velocity;
    private Vector3 accel;

    private CharacterController character;
    private DamageableComponent damage;

    private int timesStuck;
    private Vector3 previousPosition;
    private Timer stuckTimer;

    private bool movePending;
    private Vector3 pendingMoveVelocity;

    private bool playerNearby;
    private Timer lodTimer;

    private float deadFallHeight;
    public float heightOffset;

    void Start(){
        character = GetComponent<CharacterController>();
        damage = GetComponent<DamageableComponent>();

        damage.RegisterOnKilledDelegate(OnKilled);

        stuckTimer = new Timer(STUCK_CHECK_TIME);
        stuckTimer.Start();

        // Evaluate lod on first update
        playerNearby = true;
        lodTimer = new Timer(1.0f);
    }

    void Update(){
        if(playerNearby){
            playerNearby = (transform.position - PlayerComponent.player.transform.position).sqrMagnitude <= LOD_DISTANCE_SQUARED;
        } else {
            if(lodTimer.Finished()){
                lodTimer.Start();
                playerNearby = (transform.position - PlayerComponent.player.transform.position).sqrMagnitude <= LOD_DISTANCE_SQUARED;
            }

            if(!playerNearby){
                return;
            }
        }

        // Count how many frames not moving, trigger a side step action
        // pick a random direction and truck it
        if(state != MovementState.AtGoal && stuckTimer.Finished()){
            stuckTimer.Start();

            if((transform.position - previousPosition).magnitude <= STUCK_THRESHOLD) {
                timesStuck++;
            } else {
                timesStuck = 0;
            }

            previousPosition = transform.position;

            if(timesStuck >= TIME_STUCK_GIVE_UP){
                state = MovementState.AtGoal;
                if(DEBUG){ Debug.Log(gameObject.name + ":MovementBehavior giving up because stuck"); }
            }

            // Use stuck timer to drive fall check too
            Vector3 forwardPosition = transform.position + (transform.forward * 0.5f) + (Vector3.up * 0.1f);
            RaycastHit hit;
            if(!Physics.Raycast(forwardPosition, Vector3.up * -1.0f, out hit, 2.5f)){
                if(DEBUG){ Debug.Log(gameObject.name + ":MovementBehavior stopped before falling"); }

                state = MovementState.AtGoal;
                movePending = false;
                pendingMoveVelocity = Vector3.zero;
            }
        }

        if(state == MovementState.DirectRoute){
            Vector3 movementVector = (goalPosition - transform.position).normalized * walkSpeed;

            velocity = Vector3.SmoothDamp(velocity, movementVector, ref accel, accelTime);

            Vector3 flatVelocity = velocity;
            flatVelocity.y = 0.0f;
            if(velocity.sqrMagnitude > 0.0f){
                transform.rotation = Quaternion.LookRotation(flatVelocity);
            }

            movePending = true;
            pendingMoveVelocity = velocity;

            // Check if we're close here (make sure to zero out y?)
            Vector3 delta = goalPosition - transform.position;
            delta.y = 0.0f;

            if(delta.magnitude <= AT_GOAL_THRESHOLD){
                state = MovementState.AtGoal;
                if(DEBUG){ Debug.Log(gameObject.name + ":MovementBehavior reached goal"); }

                movePending = false;
                pendingMoveVelocity = Vector3.zero;
            }

            if(timesStuck >= TIME_STUCK_SIDE_STEP){
                // state = MovementState.SideStepping;
                // interimGoalPosition = something...
            }
        } else if(state == MovementState.SideStepping){
            // pretty much just go towards interim goal, until LOS to direct route?

            // Do the movement and turning here too, and play run animation

            // And the distance check just in case?
        }

        if(damage.CurrentHealth() <= 0 && state != MovementState.DeadAndStoppedMoving){
            Vector3 newPosition = transform.position + ((Vector3.up * -FALL_SPEED) * Time.deltaTime);

            if(newPosition.y <= deadFallHeight){
                newPosition.y = deadFallHeight;

                state = MovementState.DeadAndStoppedMoving;
            }

            transform.position = newPosition;
        }
    }

    void FixedUpdate(){
        if(movePending || alwaysMove){
            character.SimpleMove(pendingMoveVelocity);
            movePending = false;
            pendingMoveVelocity = Vector3.zero;
        }
    }

    public void MoveToPosition(Vector3 goal){
        if(damage.CurrentHealth() > 0){
            if(DEBUG){ Debug.Log(gameObject.name + ":MovementBehavior moving to position " + goal); }

            // Do a quick check to see if we can just go straight for it...? if not, start in sidestep mode?

            goalPosition = goal;
            state = MovementState.DirectRoute;
            timesStuck = 0;
            stuckTimer.Start();
        }
    }

    public void StopMoving(){
        goalPosition = transform.position;
        state = MovementState.AtGoal;

        velocity = Vector3.zero;
        pendingMoveVelocity = Vector3.zero;

        movePending = false;
    }

    public Vector3 Goal(){
        return goalPosition;
    }

    public bool AtGoal(){
        return state == MovementState.AtGoal;
    }

    public bool DeadAndStoppedMoving(){
        return state == MovementState.DeadAndStoppedMoving;
    }

    public void OnKilled(DamageableComponent damage){
        RaycastHit hit;
        deadFallHeight = 0.0f;

        if(Physics.Raycast(transform.position, Vector3.up * -1.0f, out hit, 100.0f)){
            deadFallHeight = hit.point.y;

            if(deadFallHeight < 0.0f){
                deadFallHeight = 0.0f;
            }
        }

        deadFallHeight += heightOffset;

        StopMoving();
    }
}
