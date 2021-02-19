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
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR

//##################################################################################################
// Tiled Builder
// This class is a utility level importer for using the *.csv files exported by the program "Tiled"
// (https://www.mapeditor.org/). This assumes the integer values exported refer to prefabs, and
// parses the file and gets the corresponding prefab from the palette and instantiates it.
//
// This is useful when working on "Wolfenstein 3D" style maps, where things fit neatly into a grid.
//
// The typical pipeline for using this tool looks something like this:
// 0) Download, install, run Tiled
// 1) Create a tileset image that's visually representative of the tiles you're going to use. These
//    don't need to be the same resolution as your actuall assets.
// 2) Setup your Tiled *.tmx map and your Tiled *.tsx tileset file.
// 3) Draw the map in Tiled as desired
// 4) Export the map as a *.csv
// 5) Create several prefabs, one for each tile represented in the tileset image
// 6) Create a TiledPaletteData and insert each prefab from the previous step, at the index
//    corresponding with order in the tileset image (Tiled starts upper-left index at 0, then
//    increases left-to-right, as below)
//      0 1 2 3
//      4 5 6 7
// 7) Create a gameobject in your scene and add a TiledBuilder script to it.
// 8) Assign the TiledBuilder attribute mapFile as the *.csv asset created in Step 4
// 9) Assign the TiledBuilder attribute 'palette' as the TiledPaletteData asset created in Step 6
// 10) Click the 'Generate' Button
//
// Note that any valid *.csv file can be used, not necessarily those exported with Tiled. If you
// really want to use excel or sheets as a level editor, not even god can stop you. But we both
// know you really shouldn't.
//
// Note that this script is surrounded by UNITY_EDITOR. Therefore, this script is only for use in
// editor, and does nothing (and has no overhead) during runtime.
//##################################################################################################
#if UNITY_EDITOR
[ExecuteInEditMode]
public class TiledBuilder : MonoBehaviour {
    public float tileUnitScale = 2.0f;

    public TextAsset mapFile;
    public TiledPaletteData palette;

    //##############################################################################################
    // Generate the map using the file and palette provided, parenting all spawned prefabs under
    // this transform.
    // CAUTION! This destroys all children of the gameObject this is on.
    //##############################################################################################
    public void GenerateMap(){
        // Destroy all existing children
        while(transform.childCount > 0){
            DestroyImmediate(transform.GetChild(0).gameObject);
        }

        // Mark this as dirty so the save asterisk appears
        EditorUtility.SetDirty(gameObject);

        string[] lines = mapFile.text.Split('\n');

        // Parse the CSV file
        int verticalCount = lines.Length;
        int horizontalCount = lines[0].Split(',').Length;

        if(verticalCount <= 0 || horizontalCount <= 0){
            Debug.LogError("TiledBuilder.GenerateMap: The parsed vertical tile count is " + verticalCount + " and the horiztonal tile count is " + horizontalCount + " which is not allowed.");
            return;
        }

        for(int i = 0; i < verticalCount; ++i){
            string[] indices = lines[i].Split(',');

            for(int k = 0; k < horizontalCount; ++k){
                int tileIndex = 0;

                try {
                    tileIndex = Int32.Parse(indices[k]);
                } catch {
                    Debug.LogWarning("TiledBuilder.GenerateMap: Tile at position (" + i + ", " + k + ") was not a parseable integer, was " + indices[k] + ", skipping instantiation.");
                    continue;
                }

                GameObject newTilePrefab = palette.tilePrefabs[tileIndex];

                if(newTilePrefab != null){
                    // Spawn as a prefab, so changes to it still reflect in editor
                    GameObject newTile = (GameObject)(PrefabUtility.InstantiatePrefab(newTilePrefab));

                    int nameId = (i * horizontalCount) + k;
                    newTile.name = gameObject.name + "_" + nameId;

                    newTile.transform.parent = transform;
                    newTile.transform.localPosition = new Vector3((float)(k) * tileUnitScale, 0.0f, -(float)(i) * tileUnitScale);
                }
            }
        }
    }
}
#endif // UNITY_EDITOR
