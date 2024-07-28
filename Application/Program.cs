using Raylib_CsLo;
using System.Net.Sockets;

namespace Application;

public static class Program
{
    public static string server = "localhost";
    public static int port = 8080;
    public static void Main(string[] args)
    {
        UnoClient unoClient = new UnoClient(server, port);

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
        var matchScreen = new MatchScreen(unoClient);

        var inMatch = false;
        var switchToMatchScreen = false;

        while (!Raylib.WindowShouldClose())
        {
            float dt = Raylib.GetFrameTime();

            if (switchToMatchScreen)
            {
                options.name = mainMenuScreen.GetPlayerName();
                options.Save();
                unoClient.name = options.name;
                Raylib.SetWindowTitle($"Uno [{options.name}]");
                unoClient.Connect();

                matchScreen.OnStart(options.name);

                inMatch = true;
                switchToMatchScreen = false;
            }

            if (inMatch) {
                matchScreen.Tick(dt);
            } else {
                mainMenuScreen.Tick(dt);

                if (mainMenuScreen.pressedJoin)
                {
                    switchToMatchScreen = true;
                }
            }

            if (unoClient.tcp != null)
            {
                unoClient.Update();
            }
        }

        unoClient.Disconnect();
        Raylib.CloseWindow();
    }
}