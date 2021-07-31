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
// Simple Example Enemy
// This is an example enemy behavior to demonstrate how these are set up and the difference in
// scripting. Behaviors are typically less like an API and more like defining the behavior and
// characteristics of an interactable chunk of 'gameplay'. This allows us to build complicated
// relationships between existing components, to get novel and interesting behavior.
//##################################################################################################
public class SimpleExampleEnemy : SpriteEnemyBehavior {
    // Update radii are usually significantly larger than the trigger radius, so that player can
    // try to run away and still have the enemies update.
    public const float UDPATE_RADIUS = 45.0f;

    public const float SHOOT_RADIUS = 12.0f;

    public const int IDLE_ANIMATION_INDEX = 0;
    public const int WALK_ANIMATION_INDEX = 1;
    public const int SHOOT_ANIMATION_INDEX = 2;

    public enum ExampleState {
        Idle,
        Patrolling,
        Shooting,
    }

    public ExampleState exampleState;

    public float patrolIdleTime;
    public float shootCooldown;

    public Transform patrolPointA;
    public Transform patrolPointB;

    public Transform muzzleTransform;

    public BarkComponent bark;

    public GameObject bodyToSpawn;

    private bool patrollingAToB;

    private Timer patrolIdleTimer;
    private Timer shootTimer;

    private GunComponent gun;

    //##############################################################################################
    // Make sure to call base start, then setup some simple state
    //##############################################################################################
    protected override void Start(){
        base.Start();

        patrollingAToB = true;

        patrolIdleTimer = new Timer(patrolIdleTime);
        shootTimer = new Timer(shootCooldown);

        gun = GetComponent<GunComponent>();

        patrolIdleTimer.Start();
    }

    //##############################################################################################
    // If we ever encounter the player, shoot at them. Otherwise, patrol from A to B, wait, then
    // patrol back.
    //##############################################################################################
    public override void EnemyUpdate(){
        base.EnemyUpdate();

        float playerDistance = (transform.position - FirstPersonPlayerComponent.player.transform.position).magnitude;

        // any detection of the player during the patrol stops and shoot
        if(exampleState != ExampleState.Shooting && playerDistance < SHOOT_RADIUS){
            exampleState = ExampleState.Shooting;
            StopMoving();
            rotation.SetAnimationIndex(SHOOT_ANIMATION_INDEX);
        }

        // Pretty simple finite state machine
        if(exampleState == ExampleState.Idle){
            if(patrolIdleTimer.Finished()){
                exampleState = ExampleState.Patrolling;
                MoveToPosition(patrollingAToB ? patrolPointB.position : patrolPointA.position);
                rotation.SetAnimationIndex(WALK_ANIMATION_INDEX);
                patrollingAToB = !patrollingAToB;
            }
        } else if(exampleState == ExampleState.Patrolling){
            if(AtGoal()){
                patrolIdleTimer.Start();
                exampleState = ExampleState.Idle;
                rotation.SetAnimationIndex(IDLE_ANIMATION_INDEX);
            }
        } else if(exampleState == ExampleState.Shooting){
            if(playerDistance > SHOOT_RADIUS){
                exampleState = ExampleState.Idle;
                patrolIdleTimer.Start();
            }

            bool hasShootToken = AttackTokenComponent.RequestToken(gameObject);

            if(shootTimer.Finished() && hasShootToken){
                Vector3 toPlayer = FirstPersonPlayerComponent.player.transform.position - transform.position;
                toPlayer.y = 0.0f;
                transform.rotation = Quaternion.LookRotation(toPlayer);

                gun.Shoot();
                shootTimer.Start();
                rotation.SetAnimationIndex(IDLE_ANIMATION_INDEX);
            }
        }

        // Make sure to re-register for next frames update if we're still near enough!
        if(playerDistance < UDPATE_RADIUS){
            EnemyManagerComponent.RegisterUpdate(this);
        }
    }

    //##############################################################################################
    // Play a grunt bark when damaged
    //##############################################################################################
    public override void PlayDamagedSequence(){
        bark.Bark();

        base.PlayDamagedSequence();
    }

    //##############################################################################################
    // Spawn a body prefab when killed
    //##############################################################################################
    public override void PlayDeathSequence(){
        GameObject spawnedBody = GameObject.Instantiate(bodyToSpawn);
        spawnedBody.transform.position = transform.position;

        base.PlayDeathSequence();
    }
}
