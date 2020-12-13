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
// Sub Components are a toolbox of simple, common building block behaviours, meant to do a single
// thing within the constraints of the other existing components.
//
// Destroy After Time Component
// After a given amount of time, destroy this gameObject
//##################################################################################################
public class DestroyAfterTimeComponent : MonoBehaviour {
    public float time;

    private Timer destroyTimer;

    void Start(){
        destroyTimer = new Timer(time);
        destroyTimer.Start();
    }

    void Update(){
        if(destroyTimer.Finished()){
            Destroy(gameObject);
        }
    }
}
