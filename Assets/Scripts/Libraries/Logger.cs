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
using UnityEngine.UI;

//##################################################################################################
// Logger
// A utility class for logging information directly to the screen (shown and hidden using the tilde
// key) so that logs can be seen during gameplay.
//##################################################################################################
public class Logger : MonoBehaviour {
    public const float CONSOLE_MOVE_SPEED = 600.0f;

    private static Logger instance;

    public RectTransform consoleRectTransform;
    public Text[] consoleTextLines;

    public float consoleUpHeight;
    public float consoleDownHeight;

    public enum LogLevel {
        Info,
        Warning,
        Error,
    }

    private bool targetDown = false;
    private bool moving = false;

    //##############################################################################################
    // Assign the instance, and set the console rect up
    //##############################################################################################
    void Start(){
        instance = this;

        consoleRectTransform.anchoredPosition = new Vector2(consoleRectTransform.anchoredPosition.x, consoleUpHeight);
    }

    //##############################################################################################
    // If tilde (backquote) is pressed, toggle the target position, and begin moving if needed
    // Only continue moving if we haven't reached the destination
    //##############################################################################################
    void Update(){
        if(Input.GetKeyDown(KeyCode.BackQuote)){
            targetDown = !targetDown;
            moving = true;
        }

        if(moving){
            float previousHeight = consoleRectTransform.anchoredPosition.y;
            float consoleHeight = CustomMath.StepToTarget(previousHeight, targetDown ? consoleDownHeight : consoleUpHeight, CONSOLE_MOVE_SPEED * Time.deltaTime);

            consoleRectTransform.anchoredPosition = new Vector2(consoleRectTransform.anchoredPosition.x, consoleHeight);

            moving = Mathf.Abs(previousHeight - consoleHeight) > 0.001f;
        }
    }

    //##############################################################################################
    // Static functions to print to both the unity console log and the screen console
    //##############################################################################################
    public static void Info(string message, UnityEngine.Object args = null){
        string formattedMessage = string.Format(message, args);

        Debug.Log(formattedMessage);
        instance.PrintToConsole(formattedMessage, LogLevel.Info);
    }

    public static void Warning(string message, UnityEngine.Object args = null){
        string formattedMessage = string.Format(message, args);

        Debug.LogWarning(formattedMessage);
        instance.PrintToConsole(formattedMessage, LogLevel.Warning);
    }

    public static void Error(string message, UnityEngine.Object args = null){
        string formattedMessage = string.Format(message, args);

        Debug.LogError(formattedMessage);
        instance.PrintToConsole(formattedMessage, LogLevel.Error);
    }

    //##############################################################################################
    // Move each entry up, and then print the newest message at the bottom
    //##############################################################################################
    public void PrintToConsole(string message, LogLevel logLevel){
        int lastConsoleTextIndex = consoleTextLines.Length - 1;

        // Stop at second-to-last console string
        for(int i = 0, count = consoleTextLines.Length; i < count - 1; ++i){
            consoleTextLines[i].text = consoleTextLines[i + 1].text;
            consoleTextLines[i].color = consoleTextLines[i + 1].color;
        }

        consoleTextLines[lastConsoleTextIndex].text = message;

        if(logLevel == LogLevel.Info){
            consoleTextLines[lastConsoleTextIndex].color = Color.white;
        } else if(logLevel == LogLevel.Warning){
            consoleTextLines[lastConsoleTextIndex].color = Color.yellow;
        } else if(logLevel == LogLevel.Error){
            consoleTextLines[lastConsoleTextIndex].color = Color.red;
        }
    }
}
