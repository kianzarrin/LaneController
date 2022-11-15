namespace LaneController.UI.Gizmos;
using UnityEngine;
public static class GUIUtils {
    public static Rect ClampRectToScreen(Rect source) {
        var rect = new Rect(source);
        if (rect.width > Screen.width || rect.height > Screen.height)
            return rect;
        if (rect.x < 0)
            rect.x = 0;
        if (rect.y < 0)
            rect.y = 0;
        if (rect.x + rect.width > Screen.width)
            rect.x = Screen.width - rect.width;
        if (rect.y + rect.height > Screen.height)
            rect.y = Screen.height - rect.height;
        return rect;
    }
    public static Rect RectFromCorners(Vector2 topLeftCorner, Vector2 bottomRightCorner, bool fixInversed) {
        if (fixInversed) {
            if (bottomRightCorner.x >= topLeftCorner.x && bottomRightCorner.y >= topLeftCorner.y)
                return new Rect(topLeftCorner, new Vector2(bottomRightCorner.x - topLeftCorner.x, bottomRightCorner.y - topLeftCorner.y));

            else if (bottomRightCorner.x < topLeftCorner.x && bottomRightCorner.y > topLeftCorner.y)
                return new Rect(bottomRightCorner.x, topLeftCorner.y, topLeftCorner.x - bottomRightCorner.x, bottomRightCorner.y - topLeftCorner.y);

            else if (bottomRightCorner.x < topLeftCorner.x && bottomRightCorner.y < topLeftCorner.y)
                return new Rect(bottomRightCorner.x, bottomRightCorner.y, topLeftCorner.x - bottomRightCorner.x, topLeftCorner.y - bottomRightCorner.y);

            else if (bottomRightCorner.x > topLeftCorner.x && bottomRightCorner.y < topLeftCorner.y)
                return new Rect(topLeftCorner.x, bottomRightCorner.y, bottomRightCorner.x - topLeftCorner.x, topLeftCorner.y - bottomRightCorner.y);
        }
        return new Rect(topLeftCorner, new Vector2(bottomRightCorner.x - topLeftCorner.x, bottomRightCorner.y - topLeftCorner.y));
    }

    public static bool IsMouseInside(this Rect rect) {
        return rect.Contains(new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y));
    }
    public static Vector2 MousePos {
        get {
            return new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
        }
    }
    public static Vector2 RightMostPosition(params Vector2[] positions) {
        Vector2 rightMost = positions[0];
        for (int i = 1; i < positions.Length; i++) {
            if (rightMost.x < positions[i].x)
                rightMost = positions[i];
        }
        return rightMost;
    }
    public static Vector2 WorldToGuiPoint(this Vector3 position) {
        var guiPosition = Camera.main.WorldToScreenPoint(position);
        guiPosition.y = Screen.height - guiPosition.y;
        return new Vector2(guiPosition.x, guiPosition.y);
    }
    public static Vector2 WorldToGuiPoint(this Vector3 position, Camera cam) {
        var guiPosition = cam.WorldToScreenPoint(position);
        guiPosition.y = Screen.height - guiPosition.y;
        return new Vector2(guiPosition.x, guiPosition.y);
    }
}
