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
// Damageable Piece Component
// This is a small helper class to allow damageables to be composed of multiple small colliders
// that can individually damage the core damageable.
//##################################################################################################
[RequireComponent(typeof(Collider))]
public class DamageablePieceComponent : MonoBehaviour {

    private DamageableComponent parentDamageable;

    void Start(){
        parentDamageable = GetComponentInParent<DamageableComponent>();
    }

    public DamageableComponent GetDamageableComponent(){
        return parentDamageable;
    }
}
