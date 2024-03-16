﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBChestTracker
{
    [System.Serializable]
    public class ClanChestData
    {

        public string Clanmate { get; set; }
        public List<Chest> chests { get; set; }
        public int Points { get; set; }
        public ClanChestData()
        {

        }
        public ClanChestData(string name, List<Chest> chests)
        {
            Clanmate = name;
            this.chests = chests;
        }
        public ClanChestData(string name, List<Chest> chests, int points)
        {
            this.Clanmate = name;
            this.chests = chests;
            this.Points = points;
        }
    }
}
