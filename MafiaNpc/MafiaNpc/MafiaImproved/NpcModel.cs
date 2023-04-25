using System;
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

    public class BigFivePersonalityTraits
    {
        public double Openness;
        public double Conscientiousness;
        public double Extraversion;
        public double Agreeableness;
        public double Neuroticism;
    }
    public class NpcModel
    {
        public string Name { get; set; }
        public double KillingProbability { get; set; }
        public Dictionary<string, double> RelationFactor { get; set; }
        public bool IsActive { get; set; }
        public Function Function { get; set; }
        public BigFivePersonalityTraits Character { get; set; }
        private Random _random;

        public NpcModel(string name, double killingProbability, Function function, BigFivePersonalityTraits character)
        {
            Name = name;
            KillingProbability = killingProbability;
            RelationFactor = new Dictionary<string, double>();
            Function = function;
            IsActive = true;
            _random = new Random();
            Character = character;
        }

        public string Vote(List<string> activeCitizens)
        {
            var peopleToVote = RelationFactor.Where(x => activeCitizens.Contains(x.Key));
            var sumOfExileRate = peopleToVote.Sum(x => 1/((x.Value*10)+1));
            var randomIndex = _random.NextDouble() * sumOfExileRate;
            var currentValue = 0.0;
            var selectedPerson = "";
            foreach (var c in peopleToVote)
            {
                currentValue += 1/((c.Value*10) + 1);
                if (currentValue > randomIndex)
                {
                    selectedPerson = c.Key;
                    break;
                }
            }
            return selectedPerson;
        }

        public void ChangeRelationFactor(string citizen, double value)
        {
            RelationFactor[citizen] += value;
            if (RelationFactor[citizen] > 100)
            {
                RelationFactor[citizen] = 100;
            }

            if (RelationFactor[citizen] < 0)
            {
                RelationFactor[citizen] = 0;
            }
        }

        public void ChangeKillingProbability(double value)
        {
            KillingProbability += value;
            if (KillingProbability > 100)
            {
                KillingProbability = 100;
            }

            if (KillingProbability < 0)
            {
                KillingProbability = 0;
            }
        }
    }
}