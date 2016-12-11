using UnityEngine;
using System.Runtime.InteropServices;
public struct myPoint
{
    public int x;
    public int y;
    public myPoint(int _x, int _y)
    {
        x = _x;
        y = _y;
    }
    public static implicit operator Vector2(myPoint p)
    {
        return new Vector2(p.x, p.y);
    }
    public static implicit operator string(myPoint p)
    {
        return p.x + "" + p.y;
    }
}
public static class SetCursor {



    [DllImport("user32.dll")]
    public static extern bool SetCursorPos(int X, int Y);
    //need to be scaling this apparently, https://msdn.microsoft.com/en-us/library/aa970067(v=vs.110).aspx
    //http://stackoverflow.com/questions/8739523/directing-mouse-events-dllimportuser32-dll-click-double-click

    //these are called attributes --https://msdn.microsoft.com/en-us/library/aa288454(v=vs.71).aspx
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GetCursorPos(out myPoint pos);

    private static void MoveCursorToNearbyObject(GameObject objToFocus)
    {
        float targetWidth = 1920f;
        float targetHeight = 1080f;

        Vector2 inputCursor = Input.mousePosition;
        inputCursor.y = Screen.height - 1 - inputCursor.y;
        myPoint p;
        GetCursorPos(out p);
        var renderRegionOffset = p - inputCursor;
        var renderRegionScale = new Vector2(targetWidth / Screen.width, targetHeight / Screen.height);

        var objPos = objToFocus.transform.position;

        var newXPos = (int)(Camera.main.WorldToScreenPoint(objPos).x + renderRegionOffset.x);
        var newYPos = (int)(Screen.height - (Camera.main.WorldToScreenPoint(objPos).y) + renderRegionOffset.y);

        SetCursorPos(newXPos, newYPos);
    }
}
