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
// Rotatable Component
// This class is responsible for selecting a sprite (as a material) from a list, based on the
// direction to the camera. This allows a billboarded quad to act as a fake 3d object, changing
// visually as the camera rotates around it.
// This is a companion class used by MaterialAnimationComponent to drive it.
//##################################################################################################
[RequireComponent(typeof(MaterialAnimationComponent))]
public class RotatableComponent : MonoBehaviour {
    public const int DEFAULT_ANIMATION_INDEX = 0;
    public const int DAMAGING_ANIMATION_INDEX = 1;

    public bool mirror = true;

    //##############################################################################################
    // These two classes, Rotatable Animation and Rotatable Animations Array combine to function as
    // "all of the sprites for a complete rotation of an animation". That's kind of confusing, so:
    // RotatableAnimation has an array of mats, representing an animation from a single perspective.
    // RotatableAnimationsArray has an array of those, representing an animation, as seen from every
    // angle that the camera could look.
    // anims then holds all the possible animations for a sprite, as an indexed array.
    // These can be length 1 for a non-animating, rotating sprite.
    //##############################################################################################
    [System.Serializable]
    public class RotatableAnimation {
        public Material[] mats;
    }

    [System.Serializable]
    public class RotatableAnimationsArray {
        public RotatableAnimation[] rots;
    }

    public RotatableAnimationsArray[] anims;

    private Transform targetTransform;

    private int rotationIncrementCount;
    private Vector3 originalScale;
    private float degreesPerSprite;
    private MeshRenderer mesh;

    private int animationIndex;
    private int rotationIndex;

    private bool started = false;

    //##############################################################################################
    // Setup the initial values. Importantly, if the rotatable is mirrored, make sure to calculate
    // the degreesPerSprite correctly with the extra sprite, shown below
    //  Non-mirrored Sprites (8 total)       Mirrored Sprites (5 total)
    //        0                                     0
    //     1  |  7                               1  |   1
    //      \ | /                                 \ | /
    //  2 ----*---- 6                         2 ----*---- 2
    //      / | \                                 / | \
    //     3  |  5                               3  |  3
    //        4                                     4
    // This is just an example with 45* between sprites. This system can handle any number needed.
    //##############################################################################################
    void Start(){
        rotationIncrementCount = anims[0].rots.Length;

        #if UNITY_EDITOR
            for(int i = 1, count = anims.Length; i < count; ++i){
                if(anims[i].rots.Length != rotationIncrementCount){
                    Debug.LogError("Invalid rotation length in RotatableComponent on " + gameObject.name + " for animation at index " + i + ", is length " + anims[i].rots.Length + ", should be " + rotationIncrementCount);
                }
            }
        #endif // UNITY_EDITOR

        targetTransform = Camera.main.transform;

        animationIndex = 0;
        rotationIndex = 0;

        mesh = GetComponent<MeshRenderer>();

        originalScale = transform.localScale;

        if(mirror){
            degreesPerSprite = 180.0f / (float) (rotationIncrementCount - 1);
            //                                   ^ Because of extra sprite at end
        } else {
            degreesPerSprite = 360.0f / (float) (rotationIncrementCount);
        }

        // Because things can force-update the rotation, we need this to check if it's set up
        started = true;
    }

    //##############################################################################################
    // Update the rotationIndex, based on the rotation between this transform's forward vector,
    // and the direction to the targetTransform.
    //##############################################################################################
    public void Update(){
        if(!started){
            return;
        }

        // First, get direction from target to here
        Vector3 fromTarget = transform.position - targetTransform.position;

        fromTarget.y = 0.0f;
        fromTarget.Normalize();

        // Then, get the angle between our forward and that fromTarget, in degrees
        float cross = Vector3.Cross(fromTarget, transform.forward * -1.0f).y;
        float angle = Mathf.Acos(Vector3.Dot(fromTarget, transform.forward * -1.0f));
        angle *= Mathf.Rad2Deg;

        // Use the cross product and mirroring to set x scale to -x (thus flipping the sprite)
        // if we're on the 'mirrored' side
        if(cross > 0.0f && mirror){
            Vector3 newscale = originalScale;
            newscale.x *= -1.0f;
            transform.localScale = newscale;
        } else {
            transform.localScale = originalScale;
        }

        // Catch some bounding issues based on angle wraparound
        if(cross > 0.0f && !mirror){
            // transform into range (0, 360)
            angle = 180.0f + (180.0f - angle);
            angle = Mathf.Max(angle, 0.0f);
        }

        #pragma warning disable 1718
            // Fixes a NaN flickering bug when angle is close to 180*
            // This works because NaN fails all comparisons, even against NaN. But, self comparison
            // also generates a warning.
            if(angle != angle){
                angle = 180.0f;
            }
        #pragma warning restore 1718

        // We want the sprite to be centered in the section of the degreesPerSprite segments
        angle += degreesPerSprite * 0.5f;

        rotationIndex = (int)(angle / degreesPerSprite);

        if(rotationIndex < 0){
            rotationIndex = 0;
        } else if(rotationIndex > rotationIncrementCount - 1){
            rotationIndex = rotationIncrementCount - 1;
        }
    }

    //##############################################################################################
    // Simple getter for which animation's rotation sequence should currently be shown
    //##############################################################################################
    public Material[] GetAnimation(){
        return anims[animationIndex].rots[rotationIndex].mats;
    }

    //##############################################################################################
    // Change the index of the current animation playing.
    // Often, MaterialAnimationComponent.ForceUpdate() is called after this to immediately set the
    // first frame of that animation playing.
    //##############################################################################################
    public void SetAnimationIndex(int index){
        animationIndex = index;
    }
}
