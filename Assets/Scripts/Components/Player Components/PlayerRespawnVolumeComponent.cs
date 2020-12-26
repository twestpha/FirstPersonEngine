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
// Player Respawn Volume Component
// This component is used to track the player's current respawn, and set that every time enters a
// new respawn volume.
// The gameobject this is on should be relatively global, and should not unload. This is to aid
// loading; the player gets the respawn position and teleports there. This could trigger a
// LevelTriggerComponent, causing the primary level and it's neighbors to load. Thus, the player
// then spawns in with the appropriate levels for that area loaded.
//##################################################################################################
[RequireComponent(typeof(Collider))]
public class PlayerRespawnVolumeComponent : MonoBehaviour {
    protected static PlayerRespawnVolumeComponent currentRespawn = null;

    // Separate, so you can have them trigger the volume away from the point they'll actually
    // be respawning from.
    public Transform respawnPosition;

    // Switching this off means respawn volumes can only be triggered once. Good for linear games
    // where backtracking isn't as necessary, and it's more about progression to the next thing.
    public bool canBeTriggeredMultipleTimes = true;

    public bool defaultGameRespawn = false;

    private bool alreadyTriggered = false;

    //##############################################################################################
    // Do some error checking, and if this is the game's default, set it as such
    //##############################################################################################
    void Start(){
        if(!GetComponent<Collider>().isTrigger){
            Logger.Error("Collider on " + gameObject.name + "'s PlayerRespawnVolumeComponent must be a trigger");
        }

        if(defaultGameRespawn){
            if(currentRespawn != null){
                Logger.Error("Multiple PlayerRespawnVolumeComponents are marked as the default Game Respawn");
            } else {
                SetCurrentRespawn(gameObject);
            }
        }
    }

    //##############################################################################################
    // Static getter for current respawn
    //##############################################################################################
    public static PlayerRespawnVolumeComponent GetCurrentRespawn(){
        return currentRespawn;
    }

    //##############################################################################################
    // External setter for manually setting the game's respawn, either from default, from trigger,
    // or from things like restoring a saved game.
    //##############################################################################################
    public static void SetCurrentRespawn(GameObject respawnObject){
        PlayerRespawnVolumeComponent respawnVolume = respawnObject.GetComponent<PlayerRespawnVolumeComponent>();

        if(respawnVolume){
            Logger.Info("Player respawn set: " + respawnObject.name  + ", position: " + respawnVolume.respawnPosition.position);
            currentRespawn = respawnVolume;
        } else {
            Logger.Info("Invalid actor passed into SetCurrentRespawn: " + respawnObject);
        }
    }

    //##############################################################################################
    // On trigger enter, set the current respawn to this one, and mark it as triggered.
    // Only allow this if multiple triggering is allowed, or if we haven't been triggered already.
    //##############################################################################################
    private void OnTriggerEnter(Collider other){
        if(other.tag == "Player" && (canBeTriggeredMultipleTimes || !alreadyTriggered)){
            alreadyTriggered = true;
            SetCurrentRespawn(gameObject);
        }
    }
}
