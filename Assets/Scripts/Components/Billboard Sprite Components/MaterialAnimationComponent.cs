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

//##################################################################################################
// Material Animation Component
// This class is responsible for changing a mesh's material every 1 / frameRate seconds.
// Simply, it iterates over a given set of materials and loops if it's set to do that.
// But, it also can be combined with RotatableComponent to drive an animated, rotating sprite.
//##################################################################################################
public class MaterialAnimationComponent : MonoBehaviour {
    public float frameRate;
    public Material[] materials;
    public bool looping;
    public bool randomStartIndex;
    public bool destroyOnFinish = false;

    private float frameTime;
    private Timer frameTimer;

    private int frameIndex = 0;
    private bool finished = false;
    private int previousMaterialCount;

    private Material[] usingMaterials;

    private MeshRenderer meshRenderer;
    private RotatableComponent rotatable;

    //##############################################################################################
    // Get the needed components, and set up the timer. Then, populate the materials from either
    // ourselves or a RotatableComponent. Then, if needed, roll a random start index.
    // Finally, cache the length, set the first-indexed material, and clear the 'finished' flag
    //##############################################################################################
    void Start(){
        meshRenderer = GetComponent<MeshRenderer>();
        rotatable = GetComponent<RotatableComponent>();

        frameTime = 1.0f / frameRate;

        frameTimer = new Timer(frameTime);
        frameTimer.Start();

        if(ShouldUseRotatable()){
            usingMaterials = rotatable.GetAnimation();
        } else {
            usingMaterials = materials;
        }

        if(randomStartIndex){
            frameIndex = Random.Range(0, usingMaterials.Length);
        } else {
            frameIndex = 0;
        }

        previousMaterialCount = usingMaterials.Length;
        meshRenderer.material = usingMaterials[frameIndex];
        finished = false;
    }

    //##############################################################################################
    // This allows the animation to restart when turned off and back on
    //##############################################################################################
    void OnEnable(){
        Start();
    }

    //##############################################################################################
    // Update the materials list if needed, then increment the animation if the frame timer is done
    //##############################################################################################
    void Update(){
        if(ShouldUseRotatable()){
            usingMaterials = rotatable.GetAnimation();
        } else {
            usingMaterials = materials;
        }

        // If the materials changed, reset the animation
        if(usingMaterials.Length != previousMaterialCount){
            frameTimer.Start();

            frameIndex = 0;
            previousMaterialCount = usingMaterials.Length;

            meshRenderer.material = usingMaterials[frameIndex];

            return;
        }

        if(frameTimer.Finished()){
            frameTimer.Start();

            if(looping){
                frameIndex = (frameIndex + 1) % usingMaterials.Length;
            } else {
                frameIndex++;

                if(frameIndex == usingMaterials.Length){
                    finished = true;

                    if(destroyOnFinish){
                        Destroy(gameObject);
                    }
                }

                frameIndex = Mathf.Min(frameIndex, usingMaterials.Length - 1);
            }

            meshRenderer.material = usingMaterials[frameIndex];
        }
    }

    //##############################################################################################
    // Used when changing animations, when the material animation needs to update everything
    // immediately, outside of a frame update
    //##############################################################################################
    public void ForceUpdate(){
        if(ShouldUseRotatable()){
            rotatable.Update();
        }

        previousMaterialCount = -1;
        Update();
    }

    //##############################################################################################
    // Helper function, returns if we have a rotatable ready to use. Using it's enabled value also
    // allows us to disable just the rotatable, and have the material animation function as a simple
    // animation
    //##############################################################################################
    public bool ShouldUseRotatable(){
        return rotatable != null && rotatable.enabled;
    }

    //##############################################################################################
    // Simple getter to return if an animation has finished. This cannot be true if the animation
    // is set to looping.
    //##############################################################################################
    public bool Finished(){
        return finished;
    }
}
