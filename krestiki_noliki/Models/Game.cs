using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace krestiki_noliki.Models
{
    public class Game
    {

        public int[,] matrix;
        public int firstMove;
        public int winX, winY;
        public int winner;
        public int countMoves;
        public bool status;
        public bool saved;
        public DateTime date;

        public Game()
        {
            matrix = new int[3, 3];
            firstMove = -1;
            winX = -1;
            winY = -1;
            winner = 0;
            countMoves = 0;
            status = true; // true - игра продолжается; false - игра закончена;
            saved = false;
            date = DateTime.Now;
        }

    }
}