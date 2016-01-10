using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscoBot
{
    class Disco
    {
        //Instance of the bot.
        public static DiscoBot Bot;

        static void Main(string[] args)
        {
            //Initialize our instance.
            Bot = new DiscoBot();
            Bot.Run();
        }
    }
}
