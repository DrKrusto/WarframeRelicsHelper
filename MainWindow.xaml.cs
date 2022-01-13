using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using WarframeRelicsHelper.src.Models;
using WarframeRelicsHelper.src.Enums;
using System.Text.RegularExpressions;

namespace WarframeRelicsHelper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Relic[] relics;
        private TextBox[] textBoxes;
        private ListBox[] listBoxes;

        public MainWindow()
        {
            InitializeComponent();
            this.textBoxes = new TextBox[4]
            {
                this.Player1_Relic_textBox,
                this.Player2_Relic_textBox,
                this.Player3_Relic_textBox, 
                this.Player4_Relic_textBox
            };
            this.relics = new Relic[4] { new Relic(), new Relic(), new Relic(), new Relic() };
            this.listBoxes = new ListBox[4]
            {
                this.Player1_RelicRewards_list,
                this.Player2_RelicRewards_list,
                this.Player3_RelicRewards_list,
                this.Player4_RelicRewards_list
            };
        }

        public async Task<bool> UpdateRelic(int playerId, string relicText)
        {
            if(playerId >= 0 && playerId <= 3)
            {
                Relic_Types relicType = Relic_Types.Lith;
                string[] relicSplit = relicText.Split(' ');
                try
                {
                    relicType = (Relic_Types)Enum.Parse(typeof(Relic_Types), relicSplit[0]);
                }
                catch
                {
                    return false;
                }
                this.relics[playerId] = new Relic(playerId, relicType, relicSplit[1]);
                for (int i = 0; i < this.relics[playerId].Rewards.Count; i++)
                {
                    this.relics[playerId].Rewards[i].PlatiniumValue = await Reward.FetchAveragePlatiniumValue(this.relics[playerId].Rewards[i]); 
                }
                this.listBoxes[playerId].DataContext = this.relics[playerId].Rewards;
                this.listBoxes[playerId].ItemsSource = this.relics[playerId].Rewards;
            }
            return true;
        }

        private void UpdateRelics_Click(object sender, RoutedEventArgs e)
        {
            Regex regex = new Regex("^([A-z]+)(\\s)([A-Z])([0-9]+)$");
            for (int i = 0; i < 4; i++)
            {
                string relicText = this.textBoxes[i].Text;
                if (!regex.IsMatch(relicText))
                    continue;
                this.UpdateRelic(i, relicText);
            }
        }
    }
}
