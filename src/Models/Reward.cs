using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarframeRelicsHelper.src.Models
{
    public class Reward
    {
        private string name;
        private int platiniumValue;
        private int ducatValue;
        private double lootChance;

        public Reward(string name, int platiniumValue, int ducatValue, double lootChance)
        {
            this.name = name;
            this.lootChance = lootChance;
            this.ducatValue = ducatValue;
            this.platiniumValue = platiniumValue;
        }

        public string Name { get => this.name; }
        public int PlatiniumValue { get => this.platiniumValue;}
        public int DucatValue { get => this.ducatValue;}
        public string RarityToString { get => $"{Convert.ToString(this.lootChance)}%"; }
        public double LootChance { get => this.lootChance;}
    }
}
