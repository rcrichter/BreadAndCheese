using System;

namespace BreadandCheese
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (BreadandCheese game = new BreadandCheese())
            {
                game.Run();
            }
        }
    }
#endif
}

