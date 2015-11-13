using AssemblyToProcess.Controller;
using AssemblyToProcess.Model;
using System;
using System.Collections.Generic;

namespace AssemblyToProcess.View
{
    public class JukeboxPlayer
    {
        private List<Song> songs;
        public bool On;

        internal List<Song> Songs
        {
            get
            {
                return songs;
            }

            set
            {
                songs = value;
            }
        }

        public JukeboxPlayer()
        {
            this.Songs = new List<Song>();
        }

        static void Main(string[] args)
        {
            JukeboxPlayer bluesPlayer = new JukeboxPlayer();
            JukeboxPlayer eletronicPlayer = new JukeboxPlayer();

            JukeboxController.TurnOnJukebox(ref eletronicPlayer);
            JukeboxController.SelectMusic(ref eletronicPlayer, new Song("Let's Get Started"));
            JukeboxController.PlayMusic(ref eletronicPlayer);

            Console.WriteLine("Fim");

        }
    }
}
