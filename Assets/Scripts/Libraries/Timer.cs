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
// Timer
// This is easily the most useful, helpful class I've ever written.
//
// This class is responsible for functioning as a stopwatch timer, where the user provides an amount
// of time (in seconds)
//##################################################################################################
public class Timer {
    private float duration;
    private float startTime;
    private float pausedElapsed;

    //##############################################################################################
    // A parameterless constructor, using default settings, and a time amount of 0.
    //##############################################################################################
    public Timer(){
        pausedElapsed = -1.0f;
        duration = 0.0f;
    }

    //##############################################################################################
    // This constructs with the duration (in Seconds) passed in.
    // Note that constructing a timer starts it in the 'finished' state.
    //##############################################################################################
    public Timer(float duration_){
        pausedElapsed = -1.0f;
        duration = duration_;
        startTime = -duration;
    }

    //##############################################################################################
    // Begin the timer counting down
    //##############################################################################################
    public void Start(){
        startTime = Time.time;
    }

    //##############################################################################################
    // Returns whether or not the timer is paused
    //##############################################################################################
    public bool IsPaused(){
        return pausedElapsed >= 0.0f;
    }

    //##############################################################################################
    // Pause the timer, suspending it's countdown
    //##############################################################################################
    public void Pause(){
        if(pausedElapsed < 0.0f){
            pausedElapsed = Elapsed();
        } else {
            Debug.LogError("Timer is already paused");
        }
    }

    //##############################################################################################
    // Resume the timer, and continuing the remaining countdown
    //##############################################################################################
    public void Unpause(){
        if(IsPaused()){
            startTime = Time.time - pausedElapsed;
            pausedElapsed = -1.0f;
        }
    }

    //##############################################################################################
    // Get the elapsed seconds the timer has been counting down
    // This does not clamp, and can be used as a 'time since started' utility.
    //##############################################################################################
    public float Elapsed(){
        if(IsPaused()){
            return pausedElapsed;
        } else {
            return Time.time - startTime;
        }
    }

    //##############################################################################################
    // This returns the timer's elapsed amount divided by the duration, giving a 0-1 representation
    // of how "done" the timer is.
    // Basically, a value of 0 means the timer hasn't run for long, and a value of 1 means the timer
    // is almost done.
    // This can be used to drive lerps and transitions easily.
    // This value is clamped between 0 and 1.
    //##############################################################################################
    public float Parameterized(){
        return Mathf.Max(Mathf.Min(Elapsed() / duration, 1.0f), 0.0f);
    }

    //##############################################################################################
    // This returns similar functionality as Parameterized, but does not clamp the upper bound,
    // meaning the value is between 0 and positive infinity.
    //##############################################################################################
    public float ParameterizedUnclamped(){
        return Elapsed() / duration;
    }

    //##############################################################################################
    // This returns similar functionality as Parameterized, but loops the Parameterized value.
    // In practice, this means the value will loop from 0, then towards 1, then resetting back
    // at 0 and starting again.
    //##############################################################################################
    public float ParameterizedLooping(){
        return ParameterizedUnclamped() % 1.0f;
    }

    //##############################################################################################
    // Returns whether or not this timer's countdown has finished
    //##############################################################################################
    public bool Finished(){
        return Elapsed() >= duration && !IsPaused();
    }

    //##############################################################################################
    // Set the start time, based on the parameterized amount given
    //##############################################################################################
    public void SetParameterized(float value){
        startTime = Time.time - (value * duration);
    }

    //##############################################################################################
    // Set the duration of the timer
    //##############################################################################################
    public void SetDuration(float duration_){
        duration = duration_;
    }

    //##############################################################################################
    // Returns the duration of the timer
    //##############################################################################################
    public float Duration(){
        return duration;
    }
};
