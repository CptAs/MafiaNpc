using System;
using System.Collections.Generic;
using System.Linq;

namespace MafiaNpc.MafiaImproved
{
    public enum Action
    {
        Accuse,
        Defend,
        SmallTalk,
        Collaborate,
        FeelSorryForYourself
    }
    public class ExecuteAction
    {
        public string SourceName { get; set; }
        public string TargetName { get; set; }
        public Action Action { get; set; }
        public double Probability { get; set; }
    }
    public class Game
    {
        public int NumberOfCitizens { get; set; }
        public int NumberOfMafia { get; set; }
        public int NumberOfTurns { get; set; }
        public List<NpcModel> Citizens { get; set; }
        public List<string> Names { get; set; }
        private Random _random { get; set; }
        
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
            Citizens = new List<NpcModel>();
            GenerateMafia();
            GenerateCitizens();
            AssignRoles();
            GenerateExileProbability();
        }

        public void GenerateMafia()
        {
            for (int i = 0; i < NumberOfMafia; i++)
            {
                Citizens.Add(new NpcModel(Names[i], 0, Function.Mafia));
            }
        }
        public void GenerateCitizens()
        {
            for (int i = 0; i < NumberOfCitizens; i++)
            {
                Citizens.Add(new NpcModel(Names[NumberOfMafia + i], 50, Function.Citizen));
            }
        }

        public void AssignRoles()
        {
            var index = _random.Next(0, Citizens.Count);;
            while (Citizens[index].IsActive == false || Citizens[index].Function != Function.Citizen)
            {
                index = _random.Next(0, Citizens.Count);
            }

            Citizens[index].Function = Function.PoliceOfficer;
            while (Citizens[index].IsActive == false || Citizens[index].Function != Function.Citizen)
            {
                index = _random.Next(0, Citizens.Count);
            }

            Citizens[index].Function = Function.Hunter;
            while (Citizens[index].IsActive == false || Citizens[index].Function != Function.Citizen)
            {
                index = _random.Next(0, Citizens.Count);
            }

            Citizens[index].Function = Function.Doctor;
        }

        public void GenerateExileProbability()
        {
            foreach (var mafia in Citizens.Where(x => x.Function == Function.Mafia))
            {
                foreach (var citizen in Citizens.Where(x=> x.Name != mafia.Name))
                {
                    if (citizen.Function == Function.Mafia)
                    {
                        mafia.RelationFactor.Add(citizen.Name, 100);
                    }
                    else
                    {
                        mafia.RelationFactor.Add(citizen.Name, 50);
                    }
                }
            }
            foreach (var citizen in Citizens.Where(x => x.Function != Function.Mafia))
            {
                foreach (var person in Citizens.Where(x=> x.Name != citizen.Name))
                {
                    citizen.RelationFactor.Add(person.Name, 50);
                }
            }
        }

        public List<ExecuteAction> GenerateActions()
        {
            var actions = new List<ExecuteAction>();
            foreach (var citizen in Citizens)
            {
                actions.Add(new ExecuteAction
                {
                    Action = Action.SmallTalk,
                    SourceName = citizen.Name,
                    TargetName = null,
                    Probability = 50
                });
                actions.Add(new ExecuteAction
                {
                    Action = Action.FeelSorryForYourself,
                    SourceName = citizen.Name,
                    TargetName = null,
                    Probability = 50
                });
                foreach (var target in Citizens.Where(x=> x.Name != citizen.Name))
                {
                    actions.Add(new ExecuteAction
                    {
                        Action = Action.Accuse,
                        SourceName = citizen.Name,
                        TargetName = target.Name,
                        Probability = 100 - citizen.RelationFactor[target.Name]
                    });
                    
                    actions.Add(new ExecuteAction
                    {
                        Action = Action.Defend,
                        SourceName = citizen.Name,
                        TargetName = target.Name,
                        Probability = citizen.RelationFactor[target.Name]
                    });

                    if (citizen.RelationFactor[target.Name] > 80)
                    {
                        actions.Add(new ExecuteAction
                        {
                            Action = Action.Collaborate,
                            SourceName = citizen.Name,
                            TargetName = target.Name,
                            Probability = citizen.RelationFactor[target.Name] / 2
                        });
                    }
                }
            }

            return actions;
        }
    }
}