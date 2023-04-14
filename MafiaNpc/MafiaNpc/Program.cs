using System;
using System.Linq;
using MafiaNpc.MafiaImproved;

namespace MafiaNpc
{
    class Program
    {
        static void Main(string[] args)
        {
            /*var game = new Game(12, 2, 5);
            while (true)
            {
                game.DoTurn();
                var numberOfMafia = game.Citizens.Where(x => x.IsMafia && x.IsActive).ToList().Count;
                var numberOfCitizens = game.Citizens.Where(x => !x.IsMafia && x.IsActive).ToList().Count;
                if (numberOfMafia == 0)
                {
                    Console.WriteLine("Mafia is no longer in the city. Citizens won");
                    break;
                }
                if (numberOfMafia >= numberOfCitizens)
                {
                    Console.WriteLine("Mafia is more than half of city. Mafia won");
                    break;
                }
            }*/

            var game = new ImprovedGame(12, 2, 5);
            while (true)
            {
                var numberOfMafia = game.Citizens.Where(x => x.Function == Function.Mafia && x.IsActive).ToList().Count;
                var numberOfCitizens = game.Citizens.Where(x => x.Function != Function.Mafia && x.IsActive).ToList().Count;
                if (numberOfMafia == 0)
                {
                    Console.WriteLine("Mafia is no longer in the city. Citizens won");
                    break;
                }
                if (numberOfMafia >= numberOfCitizens)
                {
                    Console.WriteLine("Mafia is more than half of city. Mafia won");
                    break;
                }
                var result = false;
                while (!result)
                {
                    result = game.DoDayTurn();
                }
                numberOfMafia = game.Citizens.Where(x => x.Function == Function.Mafia && x.IsActive).ToList().Count;
                numberOfCitizens = game.Citizens.Where(x => x.Function != Function.Mafia && x.IsActive).ToList().Count;
                if (numberOfMafia == 0)
                {
                    Console.WriteLine("Mafia is no longer in the city. Citizens won");
                    break;
                }
                if (numberOfMafia >= numberOfCitizens)
                {
                    Console.WriteLine("Mafia is more than half of city. Mafia won");
                    break;
                }
                game.DoctorSaveAction();
                game.PolicemanCheckAction();
                game.MafiaKillingAction();
            }
        }
    }
}