using AssemblyToProcess.Model;
using AssemblyToProcess.Util;
using AssemblyToProcess.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssemblyToProcess.Controller
{
    class JukeboxController
    {
        public string Helper { get; private set; }

        public static void TurnOnJukebox(ref JukeboxPlayer player)
        {
            player.On = true;
        }

        public static void SelectMusic(ref JukeboxPlayer player, Song song)
        {
            player.Songs.Add(song);
        }

        public static void PlayMusic(ref JukeboxPlayer player)
        {
            Console.WriteLine(player.Songs[0].Lyric);
        }

        public static string ShowLyrics(Song song)
        {
            return song.Lyric;
        }

        public static string ShowUpperLyrics(Song song)
        {
            return JukeboxHelper.ToUpperLyric(song.Lyric);
        }
    }
}
