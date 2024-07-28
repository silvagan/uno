using Raylib_CsLo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Application;

internal class MainMenuScreen
{
    public bool pressedJoin = false;

    static bool editName = false;
    static sbyte[] name = new sbyte[32];

    public MainMenuScreen(string initialName)
    {
        Utils.CopyString(initialName, name);
    }

    public void Tick(float dt)
    {
        pressedJoin = false;

        Raylib.BeginDrawing();
        Raylib.ClearBackground(Raylib.RAYWHITE);

        var windowRect = new Rectangle(0, 0, Raylib.GetScreenWidth(), Raylib.GetScreenHeight());

        var stack = new VerticalStack
        {
            gap = 20,
            position = Utils.GetCenteredPosition(windowRect, new Vector2(200, 300))
        };

        RayGui.GuiLabel(stack.nextRectangle(100, 50), "Uno!");

        unsafe
        {
            fixed (sbyte* namePtr = name)
            {
                if (RayGui.GuiTextBox(stack.nextRectangle(200, 50), namePtr, name.Length - 1, editName))
                {
                    editName = !editName;
                }
            }
        }

        if (GetPlayerName().Length == 0)
        {
            RayGui.GuiDisable();
        }
        if (RayGui.GuiButton(stack.nextRectangle(150, 50), "Join"))
        {
            pressedJoin = true;
        }
        RayGui.GuiEnable();


        Raylib.EndDrawing();
    }

    public string GetPlayerName()
    {
        return Utils.FromBytesToString(name);
    }

}
