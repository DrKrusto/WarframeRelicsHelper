using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarframeRelicsHelper.src.Enums;
using HtmlAgilityPack;
using System.Collections.ObjectModel;

namespace WarframeRelicsHelper.src.Models
{
    public class Relic : INotifyPropertyChanged
    {
        private int ownerId;
        private Relic_Types relicType;
        private string relicExtension;
        private ObservableCollection<Reward> rewards;

        public Relic() 
        { 
            this.rewards = new ObservableCollection<Reward>();
            this.relicExtension = String.Empty;
        }

        public Relic(int ownerId, Relic_Types relicType, string relicExtension)
        {
            this.ownerId = ownerId;
            this.relicType = relicType;
            this.relicExtension = relicExtension;
            this.rewards = this.ListToObservable(this.FetchRewardsByHtmlParsing());
        }

        public ObservableCollection<Reward> Rewards
        {
            get => this.rewards;
            set => this.rewards = value;
        }

        private List<Reward> FetchRewardsByHtmlParsing()
        {
            string url = $"https://warframe.fandom.com/wiki/{relicType.ToString()}_{relicExtension}";
            List<Reward> rewards = new List<Reward>();
            HtmlWeb htmlWeb = new HtmlWeb();
            HtmlDocument? htmlDoc = htmlWeb.Load(url);
            if (htmlDoc is null) throw new Exception("ERROR: Couldn't fetch the HTML of the Warframe Wiki!");
            HtmlNode rewardTableNode = htmlDoc
                .DocumentNode
                .SelectSingleNode
                ("//table[contains(concat(' ', normalize-space(@id), ' '), ' 72656C6963table ')]");
            HtmlNodeCollection rowNodes = rewardTableNode
                .SelectNodes("//tr");
            double lastRewardRarity = 0;
            foreach (HtmlNode node in rowNodes)
            {
                if (node.SelectSingleNode("td[1]/a[2]") is null) continue;
                string rewardName = node.SelectSingleNode("td[1]/a[2]").InnerText;
                int ducatValue = Convert.ToInt32(node.SelectSingleNode("td[2]/b/span[2]").InnerText.Split(";")[1]);
                double rewardRarity = lastRewardRarity;
                if (node.SelectSingleNode("td[3]/span[1]") is not null)
                {
                    rewardRarity = Convert.ToDouble(node.SelectSingleNode("td[3]/span[1]").InnerText.Split("(")[1].Trim(')', '%').Replace('.', ','));
                    lastRewardRarity = rewardRarity;
                }
                rewards.Add(new Reward(rewardName, ducatValue, rewardRarity));
            }
            return rewards;
        }

        private ObservableCollection<Reward> ListToObservable(List<Reward> rewards)
        {
            ObservableCollection<Reward> observableRewards = new ObservableCollection<Reward>();
            foreach (Reward reward in rewards)
            {
                observableRewards.Add(reward);
            }
            return observableRewards;
        }

        static public Array GetRelicTypes
        {
            get => Enum.GetValues(typeof(Relic_Types));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
