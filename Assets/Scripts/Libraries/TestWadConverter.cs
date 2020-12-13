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
using System.Collections.Generic;

//##################################################################################################
//
//##################################################################################################
[ExecuteInEditMode]
public class TestWadConverter : MonoBehaviour {

    public TextAsset wadAsset;

    public string LumpName; // search for this lump

    // List of assets (textures, 'things', etc)

    public bool run;

    public void Update(){
        if(run){
            run = false;

            ParseAndSetupMap();
        }
    }

    private void ParseAndSetupMap(){
        // Open it
        byte[] wadBytes = wadAsset.bytes;

        // Parse it according to https://doomwiki.org/wiki/WAD
        // only parse (1, the first?) map

        // A WAD file always starts with a 12-byte header.
        // 0x08, 4 bytes, infotableofs: An integer holding a pointer to the location of the directory.

        // Their values can never exceed 2^31-1, since Doom reads them as signed ints.
        int lumpCount = Read4(wadBytes, 4);
        int infoTableIndex = Read4(wadBytes, 8);
        Debug.Log("lumpCount: " + lumpCount);
        Debug.Log("infoTableIndex: " + infoTableIndex);

        // The directory associates names of lumps with the data that belong to them. It consists of a number of entries, each with a length of 16 bytes.
        // The length of the directory is determined by the number given in the WAD header. The structure of each entry is as follows:
        int lumpIndex = -1;

        for(int i = 0; i < lumpCount; ++i){
            int offset = i * 16;

            int lumpFileIndex = Read4(wadBytes, infoTableIndex + offset);
            int lumpFileSize = Read4(wadBytes, infoTableIndex + offset + 4);

            string lumpFileName = GetLumpName(wadBytes, infoTableIndex + offset + 8);

            if(lumpFileName.Contains(LumpName.ToUpper())){
                Debug.Log("##########################################");
                Debug.Log("lumpFileName: " + lumpFileName);
                Debug.Log("lumpFileIndex: " + lumpFileIndex);
                Debug.Log("lumpFileSize: " + lumpFileSize);

                lumpIndex = lumpFileIndex;
                break;
            }
        }




        // Build raw geo (save it to file? create at runtime?)
        // apply materials, instantiate things
        // ???
        // Profit
    }

    private int Read4(byte[] bytes, int index){
        // All integers are 4 bytes long in x86-style little-endian order.
        return ((int)(bytes[index + 0] << 0))
             | ((int)(bytes[index + 1] << 8))
             | ((int)(bytes[index + 2] << 16))
             | ((int)(bytes[index + 3] << 24));
    }

    private string GetLumpName(byte[] bytes, int index){
        // An ASCII string defining the lump's name. Only the characters A-Z (uppercase), 0-9, and [ ] - _
        // should be used in lump names (an exception has to be made for some of the Arch-Vile sprites,
        // which use "\"). When a string is less than 8 bytes long, it should be null-padded to the
        // eighth byte. Values exceeding 8 bytes are forbidden.

        return "" + ((char)(bytes[index + 0]))
                  + ((char)(bytes[index + 1]))
                  + ((char)(bytes[index + 2]))
                  + ((char)(bytes[index + 3]))
                  + ((char)(bytes[index + 4]))
                  + ((char)(bytes[index + 5]))
                  + ((char)(bytes[index + 6]))
                  + ((char)(bytes[index + 7]));
    }
}
