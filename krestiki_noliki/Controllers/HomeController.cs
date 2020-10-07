using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using krestiki_noliki.Models;

namespace krestiki_noliki.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/

        public static void ParseCookie(HttpCookie myCookie, Dictionary<string, List<int>> result)
        {
            string[] elem = new string[2] { "x", "o" };

            for (int i = 0; i < elem.Length; i++)
            {
                string[] splitCookie = myCookie[elem[i]].Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                
                for (int j = 0; j < splitCookie.Length; j++)
                {
                    int val = Convert.ToInt32(splitCookie[j]);
                    result[elem[i]].Add(val);
                }
            }
        }

        public static void CreateMatrix(HttpCookie myCookie, int[,] matrix)
        {
            for (int i = 0; i < matrix.Rank; i++)
            {
                string[] splitCookie = myCookie.Values[i].Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

                for (int j = 0; j < splitCookie.Length; j++)
                {
                    int x = Convert.ToInt32(splitCookie[j][0].ToString());
                    int y = Convert.ToInt32(splitCookie[j][1].ToString());

                    matrix[x, y] = 2 * i - 1; // x == -1; o == 1; (с учетом позиции в куки-файле)
                }
            }
        }

        public static int NumberOfTypeElements(int[,] matrix, int type)
        {
            int n = 0;

            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(0); j++)
                {
                    if (matrix[i, j] == type)
                        n++;
                }
            }

            return n;
        }

        public static void ZeroesFirstMoveLogic(int[,] matrix, HttpCookie myCookie, HttpResponseBase response, ref bool outcome)
        {
            int x = -1;
            int y = -1;

            int i = -1;
            int j = -1;

            bool result = false;
            Random rnd = new Random();

            for (int k = 0; k < matrix.GetLength(0); k++)
            {
                for (int l = 0; l < matrix.GetLength(0); l++)
                {
                    if (matrix[k, l] == -1)
                    {
                        x = k;
                        y = l;
                        break;
                    }
                }
                if (x != -1)
                    break;
            }

            if ((x != 1) && ((x == y) || (x + y) == 2)) // Corner opening
                i = j = 1;

            if ((x == y) && (x == 1)) // Center opening
            {
                while (!result)
                {
                    i = rnd.Next(3);
                    j = rnd.Next(3);

                    if ((i != 1) && ((i == j) || (i + j) == 2))
                        result = true;
                }
            }

            if ((x + y) % 2 == 1) // Edge opening
            {
                int subStrategy = rnd.Next(0, 3);
                int noise = 0;

                while (!result)
                {
                    noise = rnd.Next(-1, 2);
                    if (noise != 0)
                        result = true;
                }

                switch (subStrategy)
                {
                    case 0:
                        { // Center mark
                            i = j = 1;
                            break;
                        }
                    case 1: // Corner mark next to X
                        {
                            if (x == 1)
                            {
                                i = x + noise;
                                j = y;
                            }
                            else
                            {
                                i = x;
                                j = y + noise;
                            }
                            break;
                        }
                    case 2:
                        { // Edge mark opposite the X
                            if ((x + y) == 1)
                            {
                                if (x < y)
                                {
                                    i = x + 2;
                                    j = y;
                                }
                                else
                                {
                                    i = x;
                                    j = y + 2;
                                }
                            }
                            else
                            {
                                if (x > y)
                                {
                                    i = x - 2;
                                    j = y;
                                }
                                else
                                {
                                    i = x;
                                    j = y - 2;
                                }
                            }
                            break;
                        }
                }
            }

            CommitMove(matrix, myCookie, response, ref outcome, i, j);
        }

        public static void CommitMove(int[,] matrix, HttpCookie myCookie, HttpResponseBase response, ref bool outcome, int i, int j)
        {
            myCookie["o"] = string.Concat(myCookie["o"], i, j, "|");
            response.AppendCookie(myCookie);
            
            matrix[i, j] = 1;

            outcome = true;
        }

        public static void CheckGameStatus(Game game)
        {
            // true - игра продолжается; false - игра закончена;

            int emptyCells = 0; // счётчик пустых клеток
            int sum_h, sum_v;
            int sum_d1 = 0; // главная диагональ
            int sum_d2 = 0;

            for (int i = 0; i < game.matrix.GetLength(0); i++)
            {
                sum_h = 0;
                sum_v = 0;

                sum_d1 += game.matrix[i, i];
                sum_d2 += game.matrix[i, 2 - i];

                for (int j = 0; j < game.matrix.GetLength(0); j++)
                {
                    sum_h += game.matrix[i, j];
                    sum_v += game.matrix[j, i];

                    if (game.matrix[i, j] == 0)
                        emptyCells++;
                }

                if (Math.Abs(sum_h) == 3)
                {
                    game.winX = i;
                    game.winner = Math.Sign(sum_h);
                    
                    game.status = false;
                    return;
                }
                else if (Math.Abs(sum_v) == 3)
                {
                    game.winY = i;
                    game.winner = Math.Sign(sum_v);
                    
                    game.status = false;
                    return;
                }
            }

            if (Math.Abs(sum_d1) == 3)
            {
                game.winX = 0;
                game.winY = 0;

                game.winner = Math.Sign(sum_d1);
                game.status = false;

                return;
            }
            else if (Math.Abs(sum_d2) == 3)
            {
                game.winX = 0;
                game.winY = 2;

                game.winner = Math.Sign(sum_d2);
                game.status = false;

                return;
            }

            if (game.status && emptyCells == 0)
                game.status = false;
        }

        public static void StrategyWinBlock(int p, int[,] matrix, HttpCookie myCookie, HttpResponseBase response, ref bool outcome)
        {
            int x = -1;
            int y = -1;

            int sum_h, sum_v;
            int sum_d1 = 0; // главная диагональ
            int sum_d2 = 0;

            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                sum_h = 0;
                sum_v = 0;

                sum_d1 += matrix[i, i];
                sum_d2 += matrix[i, 2 - i];

                for (int j = 0; j < matrix.GetLength(0); j++)
                {
                    sum_h += matrix[i, j];
                    sum_v += matrix[j, i];
                }

                if (sum_h == 2*p)
                {
                    for (int k = 0; k < matrix.GetLength(0); k++)
                    {
                        if (matrix[i, k] == 0)
                        {
                            x = i;
                            y = k;
                        }
                    }
                    break;
                }

                if (sum_v == 2*p)
                {
                    for (int k = 0; k < matrix.GetLength(0); k++)
                    {
                        if (matrix[k, i] == 0)
                        {
                            x = k;
                            y = i;
                        }
                    }
                    break;
                }
            }

            if (x == -1 && y == -1)
            {
                if (sum_d1 == 2 * p)
                {
                    for (int k = 0; k < matrix.GetLength(0); k++)
                    {
                        if (matrix[k, k] == 0)
                        {
                            x = k;
                            y = k;
                        }
                    }
                }

                if (sum_d2 == 2 * p)
                {
                    for (int k = 0; k < matrix.GetLength(0); k++)
                    {
                        if (matrix[k, 2 - k] == 0)
                        {
                            x = k;
                            y = 2 - k;
                        }
                    }
                }
            }

            if (x != -1 && y != -1)
                CommitMove(matrix, myCookie, response, ref outcome, x, y);
        }

        public static bool CheckIfForkIsPossible(int[,] matrix, int x, int y, int p)
        {
            int sum_h = 0, sum_v = 0;
            int sum_d1 = 0, sum_d2 = 0;

            matrix[x, y] = p;

            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                sum_h += matrix[x, i];
                sum_v += matrix[i, y];

                if (x == y)
                    sum_d1 += matrix[i, i];

                if (x + y == 2)
                    sum_d2 += matrix[i, 2 - i];
            }

            int [] sum = new int[4] { sum_h, sum_v, sum_d1, sum_d2 };

            if (sum.Count((s) => { return (s == (2 * p)); }) >= 2)
            {
                matrix[x, y] = 0;
                return true;
            }

            matrix[x, y] = 0;
            return false;
        }

        public static int NumOfPotentialForks(int p, int[,] matrix)
        {
            int count = 0;

            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(0); j++)
                {
                    if (matrix[i, j] == 0 && CheckIfForkIsPossible(matrix, i, j, p))
                        count++;
                }
            }

            return count;
        }

        public static void StrategyFork(int p, int[,] matrix, HttpCookie myCookie, HttpResponseBase response, ref bool outcome)
        {
            int n = NumberOfTypeElements(matrix, p);

            if (n < 2)
                return;

            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(0); j++)
                {
                    if (matrix[i, j] == 0 && CheckIfForkIsPossible(matrix, i, j, p))
                    {
                        CommitMove(matrix, myCookie, response, ref outcome, i, j);
                        return;
                    }
                }
            }
        }

        public static void StrategyBlockOpponentsFork(int[,] matrix, HttpCookie myCookie, HttpResponseBase response, ref bool outcome)
        {
            if (NumberOfTypeElements(matrix, -1) < 2)
                return;

            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(0); j++)
                {
                    if (matrix[i, j] == 0)
                    {
                        if (CheckIfForkIsPossible(matrix, i, j, -1))
                        {
                            matrix[i, j] = 1;

                            if (NumOfPotentialForks(-1, matrix) == 0)
                            {
                                CommitMove(matrix, myCookie, response, ref outcome, i, j);
                                return;

                            }
                            else
                            {
                                matrix[i, j] = 0;
                            }
                        }
                    }
                }
            }

            if (!outcome && NumOfPotentialForks(-1, matrix) > 1)
            {
                SubStrategyForceDefending(matrix, myCookie, response, ref outcome);
            }

        }

        public static void SubStrategyForceDefending(int[,] matrix, HttpCookie myCookie, HttpResponseBase response, ref bool outcome)
        {
            int hSumAbs, hSum;
            int vSumAbs, vSum;
            int d1SumAbs = 0, d1Sum = 0;
            int d2SumAbs = 0, d2Sum = 0;

            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                hSumAbs = 0;
                hSum = 0;

                vSumAbs = 0;
                vSum = 0;

                d1SumAbs += Math.Abs(matrix[i, i]);
                d1Sum += matrix[i, i];

                d2SumAbs += Math.Abs(matrix[i, 2 - i]);
                d2Sum += matrix[i, 2 - i];

                for (int j = 0; j < matrix.GetLength(0); j++)
                {
                    hSumAbs += Math.Abs(matrix[i, j]);
                    hSum += matrix[i, j];

                    vSumAbs += Math.Abs(matrix[j, i]);
                    vSum += matrix[j, i];
                }

                if (vSum == 1 && vSumAbs == vSum)
                {
                    for (int k = 0; k < matrix.GetLength(0); k++)
                    {
                        if (matrix[k, i] == 0)
                        {
                            matrix[k, i] = 1;

                            for (int l = 0; l < matrix.GetLength(0); l++)
                            {
                                if (matrix[l, i] == 0 && !CheckIfForkIsPossible(matrix, l, i, -1))
                                {
                                    CommitMove(matrix, myCookie, response, ref outcome, k, i);
                                    return;
                                }
                                else
                                {
                                    matrix[k, i] = 0;
                                }
                            }
                        }
                    }
                }

                if (hSum == 1 && hSumAbs == hSum)
                {
                    for (int k = 0; k < matrix.GetLength(0); k++)
                    {
                        if (matrix[i, k] == 0)
                        {
                            matrix[i, k] = 1;

                            for (int l = 0; l < matrix.GetLength(0); l++)
                            {
                                if (matrix[i, l] == 0 && !CheckIfForkIsPossible(matrix, i, l, -1))
                                {
                                    CommitMove(matrix, myCookie, response, ref outcome, i, k);
                                    return;
                                }
                                else
                                {
                                    matrix[i, k] = 0;
                                }
                            }
                        }
                    }
                }
            }

            if (d1Sum == 1 && d1SumAbs == d1Sum)
            {
                for (int k = 0; k < matrix.GetLength(0); k++)
                {
                    if (matrix[k, k] == 0)
                    {
                        matrix[k, k] = 1;

                        for (int l = 0; l < matrix.GetLength(0); l++)
                        {
                            if (matrix[l, l] == 0 && !CheckIfForkIsPossible(matrix, l, l, -1))
                            {
                                CommitMove(matrix, myCookie, response, ref outcome, k, k);
                                return;
                            }
                            else
                            {
                                matrix[k, k] = 0;
                            }
                        }
                    }
                }
            }

            if (d2Sum == 1 && d2SumAbs == d2Sum)
            {
                for (int k = 0; k < matrix.GetLength(0); k++)
                {
                    if (matrix[k, 2 - k] == 0)
                    {
                        matrix[k, 2 - k] = 1;

                        for (int l = 0; l < matrix.GetLength(0); l++)
                        {
                            if (matrix[l, 2 - l] == 0 && !CheckIfForkIsPossible(matrix, l, 2 - l, -1))
                            {
                                CommitMove(matrix, myCookie, response, ref outcome, k, 2 - k);
                                return;
                            }
                            else
                            {
                                matrix[k, 2 - k] = 0;
                            }
                        }
                    }
                }
            }

        }

        public static void StrategyPrepareToPlayFork(int[,] matrix, HttpCookie myCookie, HttpResponseBase response, ref bool outcome)
        {
            int n = 0, m = 0;
            int x = 0, y = 0;
            bool s = false;

            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    if (matrix[i, j] == 0)
                    {
                        matrix[i, j] = 1;

                        n = NumOfPotentialForks(1, matrix);

                        if (n >= 2)
                        {
                            int c = 0;

                            for (int k = 0; k < matrix.GetLength(0); k++)
                            {
                                for (int l = 0; l < matrix.GetLength(0); l++)
                                {
                                    if (matrix[k, l] == 0 && CheckIfForkIsPossible(matrix, k, l, 1))
                                    {
                                        matrix[k, l] = -1;

                                        if (NumOfPotentialForks(1, matrix) != 0)
                                            c++;

                                        matrix[k, l] = 0;
                                    }
                                }
                            }

                            if (c == n) // Супер-позиция найдена.
                                s = true;
                        }

                        matrix[i, j] = 0;

                        if (s)
                        {
                            CommitMove(matrix, myCookie, response, ref outcome, i, j);
                            return;
                        }

                        if (n > m)
                        {
                            m = n;
                            x = i;
                            y = j;
                        }
                    }
                }
            }

            if (m > 0)
                CommitMove(matrix, myCookie, response, ref outcome, x, y);
        }

        public static void StrategyMarkCenter(int[,] matrix, HttpCookie myCookie, HttpResponseBase response, ref bool outcome)
        {
            if (matrix[1, 1] == 0)
                CommitMove(matrix, myCookie, response, ref outcome, 1, 1);
        }

        public static void StrategyMarkOppositeCorner(int[,] matrix, HttpCookie myCookie, HttpResponseBase response, ref bool outcome)
        {
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(0); j++)
                {   
                    // 02, 20
                    if ((i + j) == 2 && i != 1)
                    {
                        if (matrix[i, j] == -1 && matrix[j, i] == 0)
                        {
                            CommitMove(matrix, myCookie, response, ref outcome, j, i);
                            return;
                        }
                    }

                    // 00, 22
                    if (i == j && i != 1)
                    {
                        if (matrix[i, j] == -1 && matrix[2 - i, 2 - j] == 0)
                        {
                            CommitMove(matrix, myCookie, response, ref outcome, 2 - i, 2 - j);
                            return;
                        }
                    }
                }
            }
        }

        public static void StrategyMarkEmptyCorner(int[,] matrix, HttpCookie myCookie, HttpResponseBase response, ref bool outcome)
        {
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(0); j++)
                {
                    if ((i == j || (i + j) == 2) && (i != 1))
                    {
                        if (matrix[i, j] == 0)
                        {
                            CommitMove(matrix, myCookie, response, ref outcome, i, j);
                            return;
                        }
                    }
                }
            }
        }

        public static void StrategyMarkEmptySide(int[,] matrix, HttpCookie myCookie, HttpResponseBase response, ref bool outcome)
        {
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    if ((i + j) % 2 == 1)
                    {
                        if (matrix[i, j] == 0)
                        {
                            CommitMove(matrix, myCookie, response, ref outcome, i, j);
                            return;
                        }
                    }
                }
            }
        }

        public ActionResult Index(int? row, int? col)
        {
            DateTime date = DateTime.UtcNow;
            string label = string.Empty;

            HttpCookie myCookie, myParams;

            Game game = new Game();
            
            /*
            ViewBag.Row = row;
            ViewBag.Col = col;

            ViewBag.SessionId = Session.SessionID;
            ViewBag.IsNewSession = Session.IsNewSession;
            */

            if (!Session.IsNewSession)
            {
                myCookie = Request.Cookies["game_" + Session["label"]];
                myCookie.SameSite = SameSiteMode.Strict;

                CreateMatrix(myCookie, game.matrix);
                CheckGameStatus(game);

                if (game.status && (row != null && col != null) && (row >= 0 && row < 3) && (col >= 0 && col < 3))
                {
                    // Ход крестиков
                    if (!(myCookie["x"].Contains(string.Concat(row, col)) || myCookie["o"].Contains(string.Concat(row, col))))
                    {
                        myCookie["x"] = string.Concat(myCookie["x"], row, col, "|");
                        Response.AppendCookie(myCookie); // Сохраняем ход крестиков в куки файл

                        game.matrix[(int)row, (int)col] = -1; // Обновляем матрицу игры

                        // Ответный ход ноликов
                        // 1. Анализируем закончена ли игра после хода крестиков
                        CheckGameStatus(game);

                        // 2. Если игра продолжается, делаем ответный ход ноликами по стратегии Ньюэлла и Саймона (1972 г)
                        if (game.status)
                        {
                            bool outcome = false;

                            // Логика первого ответного хода для ноликов
                            int n = NumberOfTypeElements(game.matrix, 0);
                            if (n == (game.matrix.Length - 1))
                            {
                                ZeroesFirstMoveLogic(game.matrix, myCookie, Response, ref outcome);
                                ViewBag.Strategy = "First move";
                            }

// Newel and Simon's 1972 strategy: https://en.wikipedia.org/wiki/Tic-tac-toe
// 1. Win: If the player has two in a row, they can place a third to get three in a row.
// 2. Block: If the opponent has two in a row, the player must play the third themselves to block the opponent.
// 3. Fork: Create an opportunity where the player has two threats to win (two non-blocked lines of 2).
// 4. Blocking an opponent's fork:
// Option 1: 
// The player should create two in a row to force the opponent into defending, as long as it doesn't result in them creating a fork.
// Option 2: 
// If there is a configuration where the opponent can fork, the player should block that fork.
// 4a. Prepare for a fork.
// 5. Center: A player marks the center.
// 6. Opposite corner: If the opponent is in the corner, the player plays the opposite corner.
// 7. Empty corner: The player plays in a corner square.
// 8. Empty side: The player plays in a middle square on any of the 4 sides.

                            if (!outcome)
                            {
                                StrategyWinBlock(1, game.matrix, myCookie, Response, ref outcome); // 1. Win
                                ViewBag.Strategy = "Win";
                            }

                            if (!outcome)
                            {
                                StrategyWinBlock(-1, game.matrix, myCookie, Response, ref outcome); // 2. Block
                                ViewBag.Strategy = "Block";
                            }

                            if (!outcome)
                            {
                                StrategyFork(1, game.matrix, myCookie, Response, ref outcome); // 3. Fork
                                ViewBag.Strategy = "Fork";
                            }

                            if (!outcome)
                            {
                                StrategyBlockOpponentsFork(game.matrix, myCookie, Response, ref outcome); // 4. Block opponent's Fork
                                ViewBag.Strategy = "Block opponent's Fork";
                            }

                            if (!outcome)
                            {
                                StrategyPrepareToPlayFork(game.matrix, myCookie, Response, ref outcome); // 4a. Prepare for a Fork
                                ViewBag.Strategy = "Prepare for a Fork";
                            }

                            if (!outcome)
                            {
                                StrategyMarkCenter(game.matrix, myCookie, Response, ref outcome); // 5. Mark center
                                ViewBag.Strategy = "Mark center";
                            }

                            if (!outcome)
                            {
                                StrategyMarkOppositeCorner(game.matrix, myCookie, Response, ref outcome); // 6. Mark opposite corner
                                ViewBag.Strategy = "Mark opposite corner";
                            }

                            if (!outcome)
                            {
                                StrategyMarkEmptyCorner(game.matrix, myCookie, Response, ref outcome); // 7. Mark empty corner
                                ViewBag.Strategy = "Mark empty corner";
                            }

                            if (!outcome)
                            {
                                StrategyMarkEmptySide(game.matrix, myCookie, Response, ref outcome); // 8. Mark empty side
                                ViewBag.Strategy = "Mark empty side";
                            }

                            // ***

                            while (!outcome)
                            {
                                Random rnd = new Random(); // Произвольная стратегия
                                int x = rnd.Next(3);
                                int y = rnd.Next(3);

                                if (game.matrix[x, y] == 0)
                                {
                                    CommitMove(game.matrix, myCookie, Response, ref outcome, x, y);
                                    ViewBag.Strategy = "Random";
                                }
                            }

                            // 3. Анализируем, закончена ли игра после ответного хода ноликов
                            CheckGameStatus(game);
                        }

                    }
                }

                if (!game.status) // Если игра закончена, сохраняем результат в базе данных
                {
                    myParams = Request.Cookies["params"];
                    int saved = Convert.ToInt32(myParams["saved"]);

                    game.saved = true;
                    myParams["saved"] = "1";
                    Response.AppendCookie(myParams);

                    if (1 == 2 && saved == 0)
                    {
                        GameDBEntities dbContext = new GameDBEntities();

                        Guid id = Guid.NewGuid();
                        
                        game.firstMove = Convert.ToInt32(myParams["firstMove"]);
                        game.countMoves = game.matrix.Length - NumberOfTypeElements(game.matrix, 0);

                        dbContext.Games.Add(new Games
                        {
                            Id = id,
                            FirstMove = game.firstMove,
                            CountMoves = game.countMoves,
                            Winner = game.winner,
                            WinX = game.winX,
                            WinY = game.winY,
                            Date = game.date.ToUniversalTime(),
                        });

                        string firstKey = "x", secondKey = "o";

                        if (game.firstMove == 1)
                        {
                            firstKey = "o";
                            secondKey = "x";
                        }

                        string[] first = myCookie.Values[firstKey].Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                        string[] second = myCookie.Values[secondKey].Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

                        int j = 0;
                        int x = 0, y = 0;

                        for (int i = 0; i < first.Length; i++)
                        {
                            j++;

                            x = Convert.ToInt32(first[i][0].ToString());
                            y = Convert.ToInt32(first[i][1].ToString());

                            dbContext.Moves.Add(new Moves { Id = id, MoveId = j, X = x, Y = y });

                            j++;

                            if (i < second.Length)
                            {
                                x = Convert.ToInt32(second[i][0].ToString());
                                y = Convert.ToInt32(second[i][1].ToString());

                                dbContext.Moves.Add(new Moves { Id = id, MoveId = j, X = x, Y = y });
                            }

                        }

                        try
                        {
                            dbContext.SaveChanges();

                            ViewBag.RecordId = dbContext.Games.Find(new object[] { id }).Num;

                            game.saved = true;
                            myParams["saved"] = "1";
                            Response.AppendCookie(myParams);
                        }
                        catch (Exception e)
                        { }

                        dbContext.Dispose();

                    }
                }

            }
            else
            {
                // Удаляем предыдущие куки-файлы (game_%)
                string[] removeCookies = Request.Cookies.AllKeys;

                foreach (var item in removeCookies)
                {
                    if (item.Contains("game_"))
                    {
                        HttpCookie removeCookie = new HttpCookie(item);
                        removeCookie.Expires = DateTime.Now.AddSeconds(-1);
                        Response.AppendCookie(removeCookie);
                    }
                }
                
                // Создаём куки-файл для текущей игры (game_timestamp): в нём хранится история игры
                label = string.Concat(new int[] { date.Year, date.DayOfYear, date.Hour, date.Minute, date.Second, date.Millisecond });

                myCookie = new HttpCookie("game_" + label);
                myCookie.SameSite = SameSiteMode.Strict;
                myCookie.Expires = DateTime.MaxValue;

                myCookie["x"] = string.Empty;

                myParams = Request.Cookies["params"];

                if (myParams == null)
                    myParams = new HttpCookie("params") { SameSite = SameSiteMode.Strict };
                else
                    game.firstMove = -1 * Convert.ToInt32(myParams["firstMove"]);

                myParams["firstMove"] = Convert.ToString(game.firstMove);
                myParams["saved"] = "0";

                if (game.firstMove == 1)
                {
                    Random rnd = new Random();
                    int x = 2 * rnd.Next(0, 2);
                    int y = 2 * rnd.Next(0, 2);

                    game.matrix[x, y] = 1;
                    myCookie["o"] = string.Concat(x, y, "|");
                }
                else
                    myCookie["o"] = string.Empty;

                Response.AppendCookie(myCookie);
                Response.AppendCookie(myParams);

                Session["label"] = label;
                // Session.Timeout = 10;
            }

            // Считываем историю игры из куки файла и подготавливаем модель для отображения
            // ParseCookie(myCookie, game.passData);

            return View(game);
        }

        public ActionResult Restart()
        {
            Session.Abandon();
            Response.Redirect("/Home/Index");

            return View();
        }

        public ActionResult Statistics()
        {
            GameDBEntities dbContext = new GameDBEntities();

            Summary sum = new Summary();

            // public static IEnumerable<TResult> GroupBy<TSource, TKey, TResult>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TKey, IEnumerable<TSource>, TResult> resultSelector);
            // IEnumerable<int> my = dbContext.Games.GroupBy<Games, int, int>(p => p.Winner, (k, s) => s.Count());

            // public static IQueryable<IGrouping<TKey, TSource>> GroupBy<TSource, TKey>(this IQueryable<TSource> source, Expression<Func<TSource, TKey>> keySelector);
            var groups = dbContext.Games.GroupBy(p => p.Winner);

            foreach (var g in groups)
                sum.gameSummary[g.Key] = g.Count();

            sum.gameData = dbContext.Games.OrderBy(p => p.Num).ToList();

            dbContext.Dispose();

            return View(sum);
        }

        public ActionResult CleanDB()
        {
            GameDBEntities dbContext = new GameDBEntities();

            dbContext.Database.ExecuteSqlCommand("EXEC dbo.CleanDB");
            dbContext.Dispose();

            Response.Redirect("/Home/Statistics");

            return View();
        }

        public ActionResult History(int? row)
        {
            History history = new History();
            GameDBEntities dbContext = new GameDBEntities();
            
            ViewBag.Number = row;

            if (row != null && dbContext.Games.Count(p => p.Num == row) > 0)
            {
                history.game = dbContext.Games.First(p => p.Num == row);
                history.found = true;

                var query =
                            from games in dbContext.Games
                            join moves in dbContext.Moves
                            on games.Id equals moves.Id
                            where games.Num == (int)row
                            orderby moves.MoveId ascending
                            select new
                            {
                                Id = moves.Id,
                                MoveId = moves.MoveId,
                                X = moves.X,
                                Y = moves.Y
                            };

                int j = history.game.FirstMove;
                int count = 0;

                foreach (var item in query)
                {
                    history.matrix[item.X, item.Y] = j;
                    history.trace[item.X, item.Y] = ++count;

                    history.moves.Add(new Moves
                    {
                        Id = item.Id,
                        MoveId = item.MoveId,
                        X = item.X,
                        Y = item.Y
                    });

                    j *= -1;
                }
            }

            dbContext.Dispose();

            return View(history);
        }

    }
}