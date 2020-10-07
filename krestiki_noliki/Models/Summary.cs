using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace krestiki_noliki.Models
{
    public class Summary
    {
        public Dictionary<int, int> gameSummary = new Dictionary<int, int>();
        public List<Games> gameData = new List<Games>();
    }
}