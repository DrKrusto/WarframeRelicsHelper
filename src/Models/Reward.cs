using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;

namespace WarframeRelicsHelper.src.Models
{
    public class Reward : INotifyPropertyChanged
    {
        private string name;
        private int platiniumValue;
        private int ducatValue;
        private double lootChance;

        public event PropertyChangedEventHandler? PropertyChanged;

        public Reward(string name, int ducatValue, double lootChance)
        {
            this.name = name;
            this.lootChance = lootChance;
            this.ducatValue = ducatValue;
        }

        public static async Task<int> FetchMedianPlatiniumValue(Reward reward)
        {
            if (reward.ducatValue > 0)
            {
                string standardisedRewardName = Reward.StandardizedItemName(reward.name);
                string apiUrl = $"https://api.warframe.market/v1/items/{standardisedRewardName}/orders";
                using (HttpClient client = new HttpClient())
                {
                    string json = await client.GetStringAsync(apiUrl);
                    Root deserializedJson = JsonSerializer.Deserialize<Root>(JsonDocument.Parse(json))!;
                    int count = deserializedJson.Payload.Orders.Count;
                    if (count % 2 != 0)
                        return deserializedJson.Payload.Orders[(count + 1) / 2].Platinum;
                    return deserializedJson.Payload.Orders[count / 2].Platinum + deserializedJson.Payload.Orders[(count / 2) + 1].Platinum;
                }
            }
            return 0;
        }

        public static async Task<int> FetchAveragePlatiniumValue(Reward reward)
        {
            if (reward.ducatValue > 0)
            {
                string standardisedRewardName = Reward.StandardizedItemName(reward.Name);
                string apiUrl = $"https://api.warframe.market/v1/items/{standardisedRewardName}/orders";
                using (HttpClient client = new HttpClient())
                {
                    string json = await client.GetStringAsync(apiUrl);
                    Thread.Sleep(250);
                    Root deserializedJson = JsonSerializer.Deserialize<Root>(JsonDocument.Parse(json))!;
                    int count = deserializedJson.Payload.Orders.Count, sum = 0;
                    for (int i = 0; i < count; i++)
                    {
                        sum += deserializedJson.Payload.Orders[i].Platinum;
                    }
                    return sum / count;
                }
            }
            return 0;
        }

        public static int FetchMedianPlatiniumValueByHtmlParsing(Reward reward)
        {
            // !! NOT WORKING: It seems that the website made a js script to prevent HTML scrapping
            if (reward.ducatValue == 0) return 0;
            string standardisedRewardName = Reward.StandardizedItemName(reward.name);
            string url = $"https://warframe.market/fr/items/{standardisedRewardName}";
            int count = 0;
            List<int> platiniumValues = new List<int>();
            HtmlWeb htmlWeb = new HtmlWeb();
            HtmlDocument? htmlDoc = htmlWeb.Load(url);
            if (htmlDoc is null) throw new Exception("ERROR: Couldn't fetch the HTML of the Warframe Market!");
            HtmlNode ordersNode = htmlDoc.
                DocumentNode.
                SelectSingleNode("/html/body/section/section/div/section[2]/div[3]/div[2]/div[2]/div");
                //SelectNodes("/div[contains(@class, 'order')]");
            foreach (HtmlNode orderNode in ordersNode.SelectNodes("/div[contains(@class, 'order')]"))
            {
                int platinium = Convert.ToInt32(orderNode.SelectSingleNode("//b[contains(concat(' ', normalize-space(@class), ' '), ' price ')]").InnerText);
                platiniumValues.Add(platinium);
            }
            count = platiniumValues.Count;
            if (platiniumValues.Count % 2 != 0)
                return platiniumValues[(count + 1) / 2];
            return platiniumValues[count / 2] + platiniumValues[(count / 2) + 1];
        }

        private static string StandardizedItemName(string itemName)
        {
            Regex regex = new Regex("Neuroptics|Systems|Chassis");
            if (regex.IsMatch(itemName)) return itemName.ToLower().Replace(" blueprint", "").Replace(' ', '_');
            return itemName.ToLower().Replace(' ', '_');
        }

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public string Name { get => this.name; }
        public int PlatiniumValue 
        {
            get => this.platiniumValue; 
            set
            {
                this.platiniumValue = value;
                this.NotifyPropertyChanged(nameof(PlatiniumValue));
            }
        }
        public int DucatValue { get => this.ducatValue;}
        public string RarityToString { get => $"{Convert.ToString(this.lootChance)}%"; }
        public double LootChance { get => this.lootChance;}
    }
}
