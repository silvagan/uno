using Raylib_CsLo;

namespace Uno;

public static class Program
{
    public static void Main(string[] args)
    {
        Raylib.InitWindow(1280, 720, "Hello, Raylib-CsLo");
        Raylib.SetTargetFPS(60);
        // Main game loop
        while (!Raylib.WindowShouldClose()) // Detect window close button or ESC key
        {
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Raylib.SKYBLUE);
            Raylib.DrawFPS(10, 10);
            Raylib.DrawText("networking is hard D:!!!", 640, 360, 50, Raylib.RED);
            Raylib.EndDrawing();
        }
        Raylib.CloseWindow();
    }
}
