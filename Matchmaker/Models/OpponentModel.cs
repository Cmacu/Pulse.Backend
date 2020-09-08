﻿using System;
using System.Collections.Generic;
using System.Text;
using Pulse.Rank.Entities;

namespace Pulse.Matchmaker.Models
{
    public class OpponentModel
    {
        public int Id { get; set; }
        public string Avatar { get; set; }
        public string CgeUsername { get; set; }
        public int Position { get; set; }
        public Division Division { get; set; }
        public int Level { get; set; }
        public int Score { get; set; }
        public bool IsWin { get; set; }
        public string Country { get; set; }
        public bool IsResigned { get; set; }
        public bool IsExpired { get; set; }
        public List<string> Cards { get; set; }
        public double RatingDelta { get; set; }
        public int DecayValue { get; set; }
    }
}