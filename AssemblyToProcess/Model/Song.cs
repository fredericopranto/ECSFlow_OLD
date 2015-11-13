using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssemblyToProcess.Model
{
    class Song
    {
        public string Lyric { get; set; }

        public Song(string lyric)
        {
            this.Lyric = lyric;
        }
    }
}
