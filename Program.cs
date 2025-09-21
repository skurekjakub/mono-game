using System;

try
{
    using var game = new game_mono.Game1();
    game.Run();
}
catch (Exception ex)
{
    Console.WriteLine("=== MONOGAME CRASH REPORT ===");
    Console.WriteLine($"Error: {ex.Message}");
    Console.WriteLine($"Type: {ex.GetType().Name}");
    Console.WriteLine($"Stack Trace:\n{ex.StackTrace}");
    
    if (ex.InnerException != null)
    {
        Console.WriteLine($"\nInner Exception: {ex.InnerException.Message}");
        Console.WriteLine($"Inner Type: {ex.InnerException.GetType().Name}");
        Console.WriteLine($"Inner Stack Trace:\n{ex.InnerException.StackTrace}");
    }
    
    // Only try to read key if console input is available
    try
    {
        if (!Console.IsInputRedirected)
        {
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
    catch (Exception)
    {
        // Ignore if we can't read input
    }
    
    throw; // Re-throw for proper exit code
}
