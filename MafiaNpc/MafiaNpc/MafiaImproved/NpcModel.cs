﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace MafiaNpc.MafiaImproved
{
    public enum Function
    {
        Citizen,
        Mafia,
        PoliceOfficer,
        Doctor,
        Hunter
    }
    public class NpcModel
    {
        public string Name { get; set; }
        public double KillingProbability { get; set; }
        public Dictionary<string, double> RelationFactor { get; set; }
        public bool IsActive { get; set; }
        public Function Function { get; set; }
        private Random _random;

        public NpcModel(string name, double killingProbability, Function function)
        {
            Name = name;
            KillingProbability = killingProbability;
            Function = function;
            IsActive = true;
            _random = new Random();
        }

        public string Vote(List<string> activeCitizens)
        {
            var peopleToVote = RelationFactor.Where(x => activeCitizens.Contains(x.Key));
            var sumOfExileRate = peopleToVote.Sum(x => 1/x.Value);
            var randomIndex = _random.NextDouble() * sumOfExileRate;
            var currentValue = 0.0;
            var selectedPerson = "";
            foreach (var c in peopleToVote)
            {
                currentValue += c.Value;
                if (currentValue > randomIndex)
                {
                    selectedPerson = c.Key;
                }
            }

            return selectedPerson;
        }
    }
}