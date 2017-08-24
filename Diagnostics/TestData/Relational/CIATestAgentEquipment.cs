using System;
using System.Data;
using System.Text;

namespace Diagnostics.TestData.Relational
{
    public class CIATestAgentEquipment
    {
        public int PKFKAgentID;
        public string PKName;

        public int AmmoCurrent;
        public int AmmoMax;

        public CIATestAgentEquipment(Random random, CIATestAgent parent)
        {
            PKFKAgentID = parent.PKAgentID;
            PKName = GetRandomEquipmentName(random, parent);

            AmmoMax = random.Next(1000);
            AmmoCurrent = random.Next(AmmoMax);

        }

        private string GetRandomEquipmentName(Random random, CIATestAgent parent)
        {
            StringBuilder name = new StringBuilder();
            name.Append(parent.AgentCodeName + "'s ");

            name.Append(GetRandomEquipmentWord(random) + "("+random.Next(random.Next(int.MaxValue)) + ")");

            return name.ToString();
        }

        private string GetRandomEquipmentWord(Random random)
        {
            switch (random.Next(20))
            {
                case 0: return "Battered Fedora ";
                case 1: return "Pistol ";
                case 2: return "Grappel ";
                case 3: return "Rifle ";
                case 4: return "Rope ";
                case 5: return "TommyGun ";
                case 6: return "Psi Beam ";
                case 7: return "Trenchcoat ";
                case 8: return "Rocket ";
                case 9: return "Missile Launcher ";
                case 10: return "Bazooka ";
                case 11: return "Meltagun ";
                case 12: return "Plasmagun ";
                case 13: return "Flamer ";
                case 14: return "8mm ";
                case 15: return "9mm ";
                case 16: return "Heavy Ordinance ";
                case 17: return "Exlosives ";
                case 18: return "Concealed Blade ";
                case 19: return "Mind Wiper ";

                default:throw new ArgumentOutOfRangeException();
                    }
        }

        public static void AddColumnsToDataTable(DataTable dt)
        {
            dt.Columns.Add("PKFKAgentID");
            dt.Columns.Add("PKName");
            dt.Columns.Add("AmmoCurrent");
            dt.Columns.Add("AmmoMax");
        }

        public void AddToDataTable(DataTable dt)
        {
            dt.Rows.Add(new object[] { PKFKAgentID, PKName, AmmoCurrent, AmmoMax });

            
        }
    }
}