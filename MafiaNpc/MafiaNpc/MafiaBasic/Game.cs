using System;
using System.Collections.Generic;
using System.Linq;
using MafiaNpc.Actions;

namespace MafiaNpc
{
    public class Game
    {
        public int NumberOfCitizens { get; set; }
        public int NumberOfMafia { get; set; }
        public int NumberOfTurns { get; set; }
        public List<BaseNpc> Citizens { get; set; }
        public List<string> Names { get; set; }
        private Random _random { get; set; }
        
        private List<BaseAction> Actions { get; set; }

        public Game(int numberOfCitizens, int numberOfMafia, int numberOfTurns)
        {
            NumberOfCitizens = numberOfCitizens;
            NumberOfMafia = numberOfMafia;
            NumberOfTurns = numberOfTurns;
            _random = new Random();
            Names = new List<string>
            {
                "Kenzie Berry", "Celeste Floyd", "Sutton Robinson", "Ariel Douglas", 
                "Clara Johnson", "Mateo Douglas", "Elizabeth Newman", "Alyssa Reyes",
                "Melanie Hopkins", "Reign Perez", "Chris Robinson", "Jerry Douglas",
                "Emilio Ross", "Maeve Wood", "Chris Campbell", "Titus Harrison"
            };
            Citizens = new List<BaseNpc>();
            GenerateMafia();
            GenerateCitizens();
        }

        public void GenerateMafia()
        {
            for (int i = 0; i < NumberOfMafia; i++)
            {
                Citizens.Add(new BaseNpc(Names[i], 0, 50, true));
            }
        }
        public void GenerateCitizens()
        {
            for (int i = 0; i < NumberOfCitizens; i++)
            {
                Citizens.Add(new BaseNpc(Names[NumberOfMafia + i], 50, 50, false));
            }
        }

        public void DoTurn()
        {
            Console.WriteLine("People that still live in the city:");
            var activeCitizens = Citizens.Where(x => x.IsActive).ToList();
            foreach (var citizen in activeCitizens)
            {
                var message = "Citizen " + citizen.Name + ", role: " + (citizen.IsMafia ? "mafia" : "citizen");
                Console.WriteLine(message);
            }
            for (int i = 0; i < NumberOfTurns; i++)
            {
                var npcs = activeCitizens.OrderBy(x => _random.Next()).Take(2).ToList();
                var index = _random.Next(2);
                BaseAction action;
                if (index == 0)
                {
                    action = new AccuseAction(npcs[0], npcs[1], 10);
                    Console.WriteLine($"{npcs[0].Name} accused {npcs[1].Name}");
                }
                else
                {
                    action = new DefendAction(npcs[0], npcs[1], 10);
                    Console.WriteLine($"{npcs[0].Name} defended {npcs[1].Name}");
                }
                action.Act();
            }

            var sumOfExileRate = activeCitizens.Sum(x => x.ExileProbability);
            var votes = new List<BaseNpc>();
            foreach (var citizen in activeCitizens)
            {
                var randomIndex = _random.NextDouble() * sumOfExileRate;
                var currentValue = 0.0;
                foreach (var c in activeCitizens)
                {
                    currentValue += c.ExileProbability;
                    if (currentValue > randomIndex && c != citizen)
                    {
                        votes.Add(c);
                    }
                }
            }
            var maxRepeatedItem = votes.GroupBy(x => x)
                .OrderByDescending(x => x.Count())
                .First().Key;
            maxRepeatedItem.IsActive = false;
            Console.WriteLine($"{maxRepeatedItem.Name} was exiled from city");
            
            
            var sumOfKillRate = activeCitizens.Sum(x => x.KillingProbability);

            var randomIndexKill = _random.NextDouble() * sumOfKillRate;
            var currentValueKill = 0.0;
            foreach (var c in activeCitizens)
            {
                currentValueKill += c.KillingProbability;
                if (currentValueKill > randomIndexKill && c.IsMafia == false)
                {
                    var killAction = new KillAction(Citizens.First(x => x.IsMafia), c, 0);
                    Console.WriteLine($"{c.Name} was killed that night");
                }
            }
        }
    }
}