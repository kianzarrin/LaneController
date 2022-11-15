namespace LaneController.UI.Gizmos; 
using System;
using System.Linq;
using UnityEngine;

public class KeyTyping {
    private Vector2 screenPos = Vector2.zero;
    public string registeredString = "";
    public float registeredFloat;
    public void Register() {
        if (registeredString.Length == 0) {
            if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus)) {
                registeredString = "-";
                screenPos = GUIUtils.MousePos;
            }
        } else {
            if (screenPos != Vector2.zero) {
                if (screenPos != GUIUtils.MousePos) {
                    registeredString = "";
                    registeredFloat = 0;
                    screenPos = Vector2.zero;
                }
            }

            if (Input.GetKeyDown(KeyCode.Backspace)) {
                registeredString = registeredString.Remove(registeredString.Length - 1);
                ParseRegisteredFloat();
            }
        }

        if (!registeredString.Contains('.') && registeredString.Length >= (registeredString.Contains('-') ? 2 : 1)) {
            if (Input.GetKeyDown(KeyCode.Comma) || Input.GetKeyDown(KeyCode.Period) || Input.GetKeyDown(KeyCode.KeypadPeriod))
                registeredString += '.';
        }
        for (int i = 0; i < 10; i++) {
            if (Input.GetKeyDown(i.ToString()) || Input.GetKeyDown((KeyCode)Enum.Parse(typeof(KeyCode), "Keypad" + i.ToString()))) {
                if (i == 0) {
                    if (registeredString == "0" || registeredString == "-0")
                        return;
                }
                if (registeredString == "")
                    screenPos = GUIUtils.MousePos;
                registeredString += i.ToString();
                ParseRegisteredFloat();
            }
        }
    }
    private void ParseRegisteredFloat() {
        if (registeredString == "") {
            registeredFloat = 0;
            screenPos = Vector2.zero;
            return;
        }
        var last = registeredString.Last();
        if (last == '.') {
            registeredFloat = float.Parse(registeredString.Remove(registeredString.Length - 1));
            return;
        } else if (last == '-') {
            registeredFloat = 0;
            return;
        } else
            registeredFloat = float.Parse(registeredString);
    }
}
