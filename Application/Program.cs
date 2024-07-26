using Raylib_CsLo;

namespace Application;

public static class Program
{
    public static void Main(string[] args)
    {
        Raylib.SetConfigFlags(ConfigFlags.FLAG_WINDOW_RESIZABLE);
        Raylib.InitWindow(1280, 720, "Uno");
        Raylib.SetTargetFPS(144);

        RayGui.GuiLoadStyleDefault();
        RayGui.GuiSetStyle(0, 16, 30); // Default font size
        RayGui.GuiSetStyle(0, 20, 3);  // Default font spacing

        var options = new Options();
        if (!options.Load())
        {
            Console.WriteLine("Failed to load options file");
        }

        var mainMenuScreen = new MainMenuScreen(options.name);
        var matchScreen = new MatchScreen();

        var inMatch = false;

        // Main game loop
        while (!Raylib.WindowShouldClose()) // Detect window close button or ESC key
        {
            float dt = Raylib.GetFrameTime();

            if (inMatch) {
                matchScreen.Tick(dt);
            } else {
                mainMenuScreen.Tick(dt);

                // mainMenuScreen.pressedJoin = true;
                if (mainMenuScreen.pressedJoin)
                {
                    options.name = mainMenuScreen.GetPlayerName();
                    options.Save();
                    
                    Raylib.SetWindowTitle($"Uno [{options.name}]");
                    inMatch = true;
                }
            }
        }
        Raylib.CloseWindow();
    }
}
