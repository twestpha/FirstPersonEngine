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

﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//##################################################################################################
// Localizer
// Responsible for returning localized strings when requested, and setting up the localiztion
// lookup information.
//
// In practice, this is combined with a spreadsheet application, where the information is exported
// as a *.txt file with tab-separated values.
//##################################################################################################
public class Localizer : MonoBehaviour {
    // static singleton instance
    private static Localizer instance;

    private const string MISSING_LOCALIZATION = "Missing Localization for key: ";

    // The order of this enum must match the order of the entries in the text asset file.
    // The 0th index is reserved for the key, so ordering starts at the 1st index.
    // A typical spreadsheet layout might look like this:
    // ---------+--------------+------------+------------------+----
    // test_key | Hello, world | Hola Mundo | Bonjour le monde | ...
    // ---------+--------------+------------+------------------+----
    public enum Language {
        English,
        Spanish,
        French,
        Italian,
        German,
        Russian,
        Japanese,

        Count,
    }

    // This asset is expected to be a *.txt or *.csv file (per unity), but the values should also
    // be tab-separated for parsing.
    public TextAsset localization;

    public Language currentLanguage;

    private Dictionary<Language, Dictionary<string, string>> localizationLookup;

    //##############################################################################################
    // Set ourselves as the singleton, then kick off the setup
    //##############################################################################################
    void Start(){
        instance = this;
        SetupLookup();
    }

    //##############################################################################################
    // Set up the language dictionaries, then parse and add localization keys and values to those.
    //##############################################################################################
    private void SetupLookup(){
        if(localization == null){
            return;
        }

        localizationLookup = new Dictionary<Language, Dictionary<string, string>>();

        for(int i = 0, languageCount = (int)(Language.Count); i < languageCount; ++i){
            localizationLookup.Add((Language)(i), new Dictionary<string, string>());
        }

        // Parse localization file and populate lookup dictionary. First, split based on newlines.
        // Each line is considered one 'entry', with a key, and several different values for each
        // language.
        // Localization entries that need to use a newline should use '<br>', and will get parsed
        // and converted later
        string[] entries = localization.text.Split('\n');
        foreach(string entry in entries){

            // Only parse lines with the delimiter
            // Tabs were chosen as the delimiter, because comma separation prohibited the use of the
            // comma in the values, which is a common feature in the english language.
            if(entry.Contains("\t")){
                string[] tokens = entry.Split('\t');
                string key = tokens[0];

                for(int i = 0, languageCount = (int)(Language.Count); i < languageCount; ++i){
                    // Add one, because 0th index is the key
                    int languageTokenIndex = i + 1;

                    string token = tokens[languageTokenIndex];

                    // Replace <br> with \n at this point.
                    token = token.Replace("<br>", "\n");

                    // Save in lookup
                    if(!localizationLookup[(Language)(i)].ContainsKey(key)){
                        localizationLookup[(Language)(i)].Add(key, token);
                    } else {
                        Logger.Error("Error adding loc key '" + key + "'");
                    }
                }
            }
        }
    }

    //##############################################################################################
    // Return a localized string for a given key
    //##############################################################################################
    public static string Localize(string key){
        return instance.LocalizeInternal(key);
    }

    //##############################################################################################
    // If the key has a value, return that, otherwise return some information to help diagnose
    // the issue
    //##############################################################################################
    public string LocalizeInternal(string key){
        if(localization != null && localizationLookup[currentLanguage].TryGetValue(key, out string value)){
            return value;
        } else {
            return (MISSING_LOCALIZATION + key);
        }
    }

    //##############################################################################################
    // Return a localized string for a given key, with arguments passed in, using the format '(0)',
    // '(1)', etc.
    // Note that this does NOT localize the arguments, only the key.
    //##############################################################################################
    public static string LocalizeWithArgs(string key, string[] args){
        string rawString = instance.LocalizeInternal(key);

        for(int i = 0, count = args.Length; i < count; ++i){
            string argToken = "(" + i.ToString() + ")";

            if(rawString.Contains(argToken)){
                rawString = rawString.Replace(argToken, args[i]);
            }
        }

        return rawString;
    }

    //##############################################################################################
    // Getter for current language
    //##############################################################################################
    public static Language GetCurrentLanguage(){
        return instance.GetCurrentLanguageInternal();
    }

    public Language GetCurrentLanguageInternal(){
        return currentLanguage;
    }
}
