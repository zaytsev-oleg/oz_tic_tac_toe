//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace krestiki_noliki
{
    using System;
    using System.Collections.Generic;
    
    public partial class Games
    {
        public Games()
        {
            this.Moves = new HashSet<Moves>();
        }
    
        public System.Guid Id { get; set; }
        public int Num { get; set; }
        public int FirstMove { get; set; }
        public int CountMoves { get; set; }
        public int Winner { get; set; }
        public int WinX { get; set; }
        public int WinY { get; set; }
        public System.DateTime Date { get; set; }
    
        public virtual ICollection<Moves> Moves { get; set; }
    }
}