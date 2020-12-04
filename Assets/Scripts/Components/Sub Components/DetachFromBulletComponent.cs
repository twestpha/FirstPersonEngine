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
// Detach From Bullet Component
// Once the bullet this is on is destroyed, unattach ourselves. This is commonly used for trails or
// effects, that shouldn't get destroyed along with a bullet.
//##################################################################################################
public class DetachFromBulletComponent : MonoBehaviour {

    public BulletComponent bulletParent;

    void Start(){
        bulletParent.RegisterOnBulletDestroyedDelegate(BulletDestroyed);
    }

    public void BulletDestroyed(){
        transform.parent = null;
    }
}
