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
// Gun Data
// Contains all the basic characteristics of guns
// Specialized behavior is expected to be scripted into child classes of GunComponent
//##################################################################################################
[CreateAssetMenu(fileName = "GunData", menuName = "Shooter/Gun Data", order = 0)]
public class GunData : ScriptableObject {
    [Header("Gun Characteristics")]
    public float damage = 1.0f;
    public DamageType damageType;
    public float coolDown = 1.0f;
    public float spread = 0.0f;
    public int shots = 1;

    // This is specially used by the player input to decide whether to use
    // getInput or getInputDown (i.e. requiring a re-press of the input)
    public bool automaticAction = true;

    [Header("Ammo Characteristics")]
    public bool useAmmo = false;
    public int ammoCount = 1;
    public float reloadTime = 1.0f;

    // Allows player to manually reload using input
    // useAmmo must be set true for this to have an effect
    public bool manualReload;

    // First, makes reloading progressive (one bullet at a time) and uses reloadTime as that time
    // Then, it allows shooting to interrupt the reloading (possible resulting in a non-full reload)
    // useAmmo and manualReload must be set true for this to have an effect
    public bool progressiveReloadInterruption;

    [Header("Zoom Characteristics")]
    public bool useZoom;
    public float zoomTime = 0.15f;
    public float zoomedFieldOfView = 15.0f;
    public float zoomMovementModifier = 1.0f;

    [Header("Recoil")]
    public float momentumRecoil = 0.0f;
    public float aimRecoil = 0.0f;

    [Header("Bullet Characteristics")]
    public float muzzleVelocity = 100.0f;
    public GameObject bulletPrefab;
    public Vector3 muzzleOffset;

    [Header("Effects")]
    public GameObject firingEffectsPrefab;
    public Vector3 firingEffectsOffset;

    [Header("Sounds")]
    public AudioClip fireSound;
    public float fireSoundVolume = 1.0f;
    public float fireSoundPitchBend = 0.0f;

    public AudioClip reloadSound;
    public float reloadSoundVolume = 1.0f;
    public float reloadSoundPitchBend = 0.0f;
}
