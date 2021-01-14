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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//##################################################################################################
//
//##################################################################################################
public class DestructableComponent : MonoBehaviour {
    public const float FORCE_DISABLE_DESTRUCTABLES_TIME = 10.0f; // seconds

    public bool destructOnStart = true;
    public bool disablePhysicsOnFinished = true;
    public Transform origin;
    public float explosionForce = 500.0f;
    public float explosionRotationForce = 100.0f;

    public GameObject[] destructables;

    private bool destructed = false;
    private int[] finishedFrameCount;
    private Timer forceDistableDestructablesTimer = new Timer(FORCE_DISABLE_DESTRUCTABLES_TIME);

    //##############################################################################################
    //
    //##############################################################################################
    void Start(){
        if(origin == null){
            origin = transform;
        }

        if(destructOnStart){
            Destruct();
        }

        if(disablePhysicsOnFinished){
            finishedFrameCount = new int[destructables.Length];
        } else {
            // Disable this component to prevent updating, since it's not needed
            enabled = false;
        }
    }

    void Update(){
        if(destructed && disablePhysicsOnFinished){
            for(int i = 0, count = destructables.Length; i < count; ++i){
                GameObject destructable = destructables[i];
                if(destructable != null){
                    Rigidbody body = destructable.GetComponent<Rigidbody>();

                    if(body.IsSleeping() || forceDistableDestructablesTimer.Finished()){
                        body.isKinematic = true;
                        destructable.GetComponent<Collider>().enabled = false;

                        destructables[i] = null;
                    }
                }
            }
        }
    }

    //##############################################################################################
    //
    //##############################################################################################
    public void Destruct(){
        destructed = true;
        forceDistableDestructablesTimer.Start();

        for(int i = 0, count = destructables.Length; i < count; ++i){
            GameObject destructable = destructables[i];

            Vector3 fromOrigin = (destructable.transform.position - origin.position).normalized;
            fromOrigin += new Vector3(
                Random.Range(-0.1f, 0.1f),
                Random.Range(-0.1f, 0.1f),
                Random.Range(-0.1f, 0.1f)
            );

            float randomForce = Random.Range(0.8f * explosionForce, 1.2f * explosionForce);

            Vector3 randomTorque = new Vector3(
                Random.Range(-1.0f, 1.0f),
                Random.Range(-1.0f, 1.0f),
                Random.Range(-1.0f, 1.0f)
            );
            randomTorque.Normalize();

            Rigidbody body = destructable.GetComponent<Rigidbody>();
            body.AddForce(fromOrigin * randomForce);
            body.AddRelativeTorque(randomTorque * explosionRotationForce);
        }
    }
}
