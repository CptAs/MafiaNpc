using System.Collections.Generic;

namespace MafiaNpc
{
    public class Game
    {
        public int NumberOfCitizens { get; set; }
        public int NumberOfMafia { get; set; }
        public List<BaseNpc> Mafia { get; set; }
        public List<BaseNpc> Citizens { get; set; }
        public List<string> Names { get; set; }

        public Game(int numberOfCitizens, int numberOfMafia)
        {
            NumberOfCitizens = numberOfCitizens;
            NumberOfMafia = numberOfMafia;
            Names = new List<string>
            {
                "Kenzie Berry", "Celeste Floyd", "Sutton Robinson", "Ariel Douglas", 
                "Clara Johnson", "Mateo Douglas", "Elizabeth Newman", "Alyssa Reyes",
                "Melanie Hopkins", "Reign Perez", "Chris Robinson", "Jerry Douglas",
                "Emilio Ross", "Maeve Wood", "Chris Campbell", "Titus Harrison"
            };
            Mafia = new List<BaseNpc>();
            Citizens = new List<BaseNpc>();
            GenerateMafia();
            GenerateCitizens();
        }

        public void GenerateMafia()
        {
            for (int i = 0; i < NumberOfMafia; i++)
            {
                Mafia.Add(new BaseNpc(Names[i], 0, 50, true));
            }
        }
        public void GenerateCitizens()
        {
            for (int i = 0; i < NumberOfCitizens; i++)
            {
                Citizens.Add(new BaseNpc(Names[NumberOfMafia + i], 50, 50, false));
            }
        }
    }
}