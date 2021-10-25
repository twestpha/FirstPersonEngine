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
// Custom Math
// A utility class with several static functions for math operations that unity's math doesn't
// implement.
//##################################################################################################
class CustomMath {

    //##############################################################################################
    // Given a current value and a target value (either above or below the current value),
    // return the current value + step change, in the direction towards target
    // Comes in both float and int flavors
    //##############################################################################################
    public static float StepToTarget(float current, float target, float stepChange){
        float currentToTarget = target - current;
        float sign = currentToTarget > 0.0f ? 1.0f : -1.0f;

        if(Mathf.Abs(currentToTarget) <= stepChange){
            return target;
        } else {
            return current + (sign * stepChange);
        }
    }

    public static int StepToTarget(int current, int target, int stepChange){
        int currentToTarget = target - current;
        int sign = currentToTarget > 0 ? 1 : -1;

        if(Mathf.Abs(currentToTarget) <= stepChange){
            return target;
        } else {
            return current + (sign * stepChange);
        }
    }

    //##############################################################################################
    // The default version of an ease-in-out curve
    //##############################################################################################
    public static float EaseInOut(float t){
        return EaseInOut(t, 0.5f);
    }

    //##############################################################################################
    // This function takes in two values; the t value along the curve (x value in desmos) and the
    // cutoff value (0 < cutoff < 1) that helps define the curve.
    //
    // The curve is graphically available here:
    // https://www.desmos.com/calculator/qynqwqg5xr
    //
    // Suffice to say, the cutoff being low makes the function more like an ease-in (sqrt) curve,
    // and the cutoff being high make the function more like an ease-out (squared) curve.
    // A cutoff value of 0.5 blends these, both easing in and out.
    //##############################################################################################
    public static float EaseInOut(float t, float cutoff){
        if(cutoff > 1.0f){ cutoff = 1.0f; }
        if(cutoff < 0.0f){ cutoff = 0.0f; }

        if(t <= cutoff){
            return t * t / cutoff;
        } else {
            t = (1.0f - t);
            t *= t;
            t /= (1.0f - cutoff);
            return 1.0f - t;
        }
    }
}
