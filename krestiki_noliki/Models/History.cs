using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace krestiki_noliki.Models
{
    public class History
    {
        public List<Moves> moves = new List<Moves>();
        public Games game = new Games();

        public int[,] matrix = new int[3, 3];
        public int[,] trace = new int[3, 3];

        public bool found = false;
    }
}