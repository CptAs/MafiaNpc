using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenAI_API;

namespace MafiaNpc.MafiaImproved
{
    public enum Action
    {
        Accuse,
        Defend,
        SmallTalk,
        Collaborate,
        FeelSorryForYourself,
        Kill,
        Save,
        Check,
        Hunt
    }
    public class ExecuteAction
    {
        public string SourceName { get; set; }
        public string TargetName { get; set; }
        public Action Action { get; set; }
        public double Probability { get; set; }
    }
    public class ImprovedGame
    {
        public int NumberOfCitizens { get; set; }
        public int NumberOfMafia { get; set; }
        public int NumberOfTurns { get; set; }
        public List<NpcModel> Citizens { get; set; }
        public List<string> Names { get; set; }
        public List<Event> Events { get; set; }
        private Random _random;
        private string _savedPerson;
        private Dictionary<string, bool> _policeCheckings;
        private Dictionary<string, string> _collaborators;
        private OpenAIAPI _openAiApi;
        private int _forgetCoefficient = 10;

        public ImprovedGame(int numberOfCitizens, int numberOfMafia, int numberOfTurns)
        {
            NumberOfCitizens = numberOfCitizens;
            NumberOfMafia = numberOfMafia;
            NumberOfTurns = numberOfTurns;
            _policeCheckings = new Dictionary<string, bool>();
            _collaborators = new Dictionary<string, string>();
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
            Citizens = Citizens.OrderBy(a => _random.Next()).ToList();
            Console.WriteLine("Generated citizens: ");
            foreach (var citizen in Citizens)
            {
                var message = "Citizen " + citizen.Name + ", role: " + citizen.Function
                              + "\nOpenness: " + citizen.Character.Openness
                              + "\nAgreeableness: " + citizen.Character.Agreeableness
                              + "\nExtraversion: " + citizen.Character.Extraversion
                              + "\nConscientiousness: " + citizen.Character.Conscientiousness
                              + "\nNeuroticism: " + citizen.Character.Neuroticism
                              + "\nEmotion: " + citizen.EmotionalModel.BaseEmotion;
                Console.WriteLine(message);
            }

            var key = File.ReadAllText("../../../OpenAiSecretKey.txt");
            _openAiApi = new OpenAIAPI(key);
            Events = new List<Event>();
        }

        public string GetAnswerFromChatGpt(string question)
        {
            var completions = _openAiApi.Completions.CreateCompletionAsync(
                prompt: question,
                model: "text-davinci-002",
                max_tokens: 200,
                temperature: 0.5f
            );

            return completions.Result.Completions.FirstOrDefault().Text;
        }

        public void GenerateMafia()
        {
            for (int i = 0; i < NumberOfMafia; i++)
            {
                var character = new BigFivePersonalityTraits
                {
                    Openness = _random.Next(100),
                    Agreeableness = _random.Next(100),
                    Extraversion = _random.Next(100),
                    Conscientiousness = _random.Next(100),
                    Neuroticism = _random.Next(100),
                };
                var emotions = Enum.GetValues(typeof(Emotion));
                var padModel = new PadEmotionalModel((Emotion)emotions.GetValue(_random.Next(emotions.Length))!);
                Citizens.Add(new NpcModel(Names[i], 0, Function.Mafia, character, padModel));
            }
        }
        public void GenerateCitizens()
        {
            for (int i = 0; i < NumberOfCitizens; i++)
            {
                var character = new BigFivePersonalityTraits
                {
                    Openness = _random.Next(100),
                    Agreeableness = _random.Next(100),
                    Extraversion = _random.Next(100),
                    Conscientiousness = _random.Next(100),
                    Neuroticism = _random.Next(100),
                };
                var emotions = Enum.GetValues(typeof(Emotion));
                var padModel = new PadEmotionalModel((Emotion)emotions.GetValue(_random.Next(emotions.Length))!);
                Citizens.Add(new NpcModel(Names[NumberOfMafia + i], 50, Function.Citizen, character, padModel));
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
        
        public double GenerateCharacterCoefficient(BigFivePersonalityTraits character, double opennessCoefficient, 
            double conscientiousnessCoefficient, double extraversionCoefficient, 
            double agreeablenessCoefficient, double neuroticismCoefficient)
            {
                var coefficient = (opennessCoefficient * character.Openness) 
                                                               + (conscientiousnessCoefficient * character.Conscientiousness)
                                                               + (extraversionCoefficient * character.Extraversion)
                                                               + (agreeablenessCoefficient * character.Agreeableness)
                                                               + (neuroticismCoefficient * character.Neuroticism);
                var maximumCoefficient = 0.0;
                var minimumCoefficient = 0.0;
                maximumCoefficient += opennessCoefficient > 0 ? opennessCoefficient * 100 : opennessCoefficient * 0;
                maximumCoefficient += conscientiousnessCoefficient > 0 ? conscientiousnessCoefficient * 100 : conscientiousnessCoefficient * 0;
                maximumCoefficient += extraversionCoefficient > 0 ? extraversionCoefficient * 100 : extraversionCoefficient * 0;
                maximumCoefficient += agreeablenessCoefficient > 0 ? agreeablenessCoefficient * 100 : agreeablenessCoefficient * 0;
                maximumCoefficient += neuroticismCoefficient > 0 ? neuroticismCoefficient * 100 : neuroticismCoefficient * 0;
                minimumCoefficient += opennessCoefficient < 0 ? opennessCoefficient * 100 : opennessCoefficient * 0;
                minimumCoefficient += conscientiousnessCoefficient < 0 ? conscientiousnessCoefficient * 100 : conscientiousnessCoefficient * 0;
                minimumCoefficient += extraversionCoefficient < 0 ? extraversionCoefficient * 100 : extraversionCoefficient * 0;
                minimumCoefficient += agreeablenessCoefficient < 0 ? agreeablenessCoefficient * 100 : agreeablenessCoefficient * 0;
                minimumCoefficient += neuroticismCoefficient < 0 ? neuroticismCoefficient * 100 : neuroticismCoefficient * 0;

                var oldRange = maximumCoefficient - minimumCoefficient;
                var scaledValue = (((coefficient - minimumCoefficient) * 2.0) / oldRange);

                return scaledValue;
            }

        public List<ExecuteAction> GenerateActions()
        {
            var actions = new List<ExecuteAction>();
            var activeCitizens = Citizens.Where(x => x.IsActive);
            foreach (var citizen in activeCitizens)
            {
                var smallTalkProbabilityCharacterCoefficient = GenerateCharacterCoefficient(citizen.Character, 
                    1, -1, 2, 0.5, 0);
                actions.Add(new ExecuteAction
                {
                    Action = Action.SmallTalk,
                    SourceName = citizen.Name,
                    TargetName = null,
                    Probability = 20 * smallTalkProbabilityCharacterCoefficient
                });
                var feelSorryForYourselfProbabilityCharacterCoefficient = GenerateCharacterCoefficient(citizen.Character, 
                    1, -1, 1, -0.5, 0);
                actions.Add(new ExecuteAction
                {
                    Action = Action.FeelSorryForYourself,
                    SourceName = citizen.Name,
                    TargetName = null,
                    Probability = 20 * feelSorryForYourselfProbabilityCharacterCoefficient
                });
                foreach (var target in activeCitizens.Where(x=> x.Name != citizen.Name))
                {   
                    var accuseProbabilityCharacterCoefficient = GenerateCharacterCoefficient(citizen.Character, 
                        1, 2, 1, -2, 0);
                    actions.Add(new ExecuteAction
                    {
                        Action = Action.Accuse,
                        SourceName = citizen.Name,
                        TargetName = target.Name,
                        Probability = (100 - citizen.RelationFactor[target.Name]) * accuseProbabilityCharacterCoefficient
                    });
                    var defendProbabilityCharacterCoefficient = GenerateCharacterCoefficient(citizen.Character, 
                        1, 1, 1, 2, 0);
                    actions.Add(new ExecuteAction
                    {
                        Action = Action.Defend,
                        SourceName = citizen.Name,
                        TargetName = target.Name,
                        Probability = (citizen.RelationFactor[target.Name]) * defendProbabilityCharacterCoefficient
                    });

                    if (citizen.RelationFactor[target.Name] > 80 && 
                        !_collaborators.Keys.Contains(citizen.Name) && 
                        !_collaborators.Keys.Contains(target.Name) )
                    {
                        var collaborationProbabilityCharacterCoefficient = GenerateCharacterCoefficient(citizen.Character, 
                            1, -2, 1, 2, 0);
                        actions.Add(new ExecuteAction
                        {
                            Action = Action.Collaborate,
                            SourceName = citizen.Name,
                            TargetName = target.Name,
                            Probability = (citizen.RelationFactor[target.Name] / 2) * collaborationProbabilityCharacterCoefficient
                        });
                    }
                }
            }

            return actions;
        }

        public void MafiaKillingAction()
        {
            var activeCitizens = Citizens.Where(x => x.IsActive && x.Function != Function.Mafia).ToList();
            var sumOfKillRate = activeCitizens.Sum(x => x.KillingProbability);

            var randomIndexKill = _random.NextDouble() * sumOfKillRate;
            var currentValueKill = 0.0;
            foreach (var c in activeCitizens)
            {
                currentValueKill += c.KillingProbability;
                if (currentValueKill > randomIndexKill)
                {
                    if (c.Name != _savedPerson)
                    {
                        ExileAction(c);
                        Console.WriteLine($"{c.Name} was killed that night");
                        Events.Add(new Event
                        {
                            Action = Action.Kill,
                            IsSuccessful = true,
                            Source = null,
                            Target = c
                        });
                    }
                    else
                    {
                        Console.WriteLine($"{c.Name} was saved by doctor that night");
                        Events.Add(new Event
                        {
                            Action = Action.Kill,
                            IsSuccessful = false,
                            Source = null,
                            Target = c
                        });
                    }
                    return;
                }
            }
        }

        public void DoctorSaveAction()
        {
            var activeCitizens = Citizens.Where(x => x.IsActive).ToList();
            var doctor = Citizens.FirstOrDefault(x => x.Function == Function.Doctor);
            if (!doctor.IsActive)
            {
                _savedPerson = null;
                return;
            }

            var sumOfRelationFactor = doctor.RelationFactor.Values.Sum();
            var randomIndexRelation = _random.NextDouble() * sumOfRelationFactor;
            var currentValueSave = 0.0;
            foreach (var c in doctor.RelationFactor)
            {
                currentValueSave += c.Value;
                if (currentValueSave > randomIndexRelation)
                {
                    _savedPerson = c.Key;
                    Console.WriteLine($"{c.Key} was selected for save that night");
                    Events.Add(new Event
                    {
                        Action = Action.Save,
                        IsSuccessful = true,
                        Source = doctor,
                        Target = activeCitizens.First(x=>x.Name == c.Key)
                    });
                    return;
                }
            }
        }

        public void PolicemanCheckAction()
        {
            var activeCitizens = Citizens
                .Where(x => x.IsActive
                            && x.Function!=Function.PoliceOfficer
                            && !_policeCheckings.Keys.Contains(x.Name))
                .ToList();
            var policeman = Citizens.FirstOrDefault(x => x.Function == Function.PoliceOfficer);
            if (!policeman.IsActive)
            {
                return;
            }

            var possibleRelations = policeman.RelationFactor
                .Where(x => activeCitizens.Exists(y => y.Name == x.Key));
            var sumOfRelationFactor = possibleRelations.Sum(x=> 1/(10*x.Value + 1));
            if (!possibleRelations.Any())
            {
                return;
            }
            while (_policeCheckings.Count != policeman.RelationFactor.Count)
            {
                var randomIndexRelation = _random.NextDouble() * sumOfRelationFactor;
                var currentValueSave = 0.0;
                foreach (var c in possibleRelations)
                {
                    currentValueSave += 1/(10*c.Value + 1);
                    if (currentValueSave > randomIndexRelation)
                    {
                        var selectedCitizen = Citizens.FirstOrDefault(x => x.Name == c.Key);
                        if (selectedCitizen.Function == Function.Mafia)
                        {
                            policeman.RelationFactor[selectedCitizen.Name] = 0;
                            _policeCheckings.Add(selectedCitizen.Name, true);
                            Console.WriteLine($"{c.Key} was selected for check that night and turn out to be mafia");
                            Events.Add(new Event
                            {
                                Action = Action.Check,
                                IsSuccessful = true,
                                Source = policeman,
                                Target = activeCitizens.First(x=>x.Name == c.Key)
                            });
                        }
                        else
                        {
                            policeman.RelationFactor[selectedCitizen.Name] = 100;
                            _policeCheckings.Add(selectedCitizen.Name, false);
                            Console.WriteLine($"{c.Key} was selected for check that night and turn out to be citizen");
                            Events.Add(new Event
                            {
                                Action = Action.Check,
                                IsSuccessful = false,
                                Source = policeman,
                                Target = activeCitizens.First(x=>x.Name == c.Key)
                            });
                        }
                        
                        return;
                    }
                }
            }
        }

        public void HunterKillAction()
        {
            var activeCitizens = Citizens.Where(x => x.IsActive).ToList();
            var hunter = activeCitizens.FirstOrDefault(x => x.Function == Function.Hunter);
            if (hunter is null)
            {
                return;
            }
            var sumOfRelationFactor = hunter.RelationFactor.Values.Sum(x=>1/x);
            
            var randomIndexRelation = _random.NextDouble() * sumOfRelationFactor;
            var currentValueSave = 0.0;
            foreach (var c in hunter.RelationFactor)
            {
                currentValueSave += c.Value;
                if (currentValueSave > randomIndexRelation)
                {
                    var selectedCitizen = activeCitizens.FirstOrDefault(x => x.Name == c.Key);
                    ExileAction(selectedCitizen);
                    Console.WriteLine($"{c.Key} was selected by hunter to be also killed");
                    Events.Add(new Event
                    {
                        Action = Action.Hunt,
                        IsSuccessful = true,
                        Source = hunter,
                        Target = activeCitizens.First(x=>x.Name == c.Key)
                    });
                    return;
                }
            }
        }

        public void ExileAction(NpcModel selectedCitizen)
        {
            selectedCitizen.IsActive = false;
            foreach (var citizen in Citizens.Where(x => x.Name != selectedCitizen.Name && x.IsActive))
            {
                citizen.RelationFactor.Remove(selectedCitizen.Name);
            }
        }

        public void FeelSorryForYourselfAction(NpcModel source)
        {
            Console.WriteLine($"{source.Name} feels sorry for self");
            var message = GetAnswerFromChatGpt("Generate response for person with character defined by " +
                                               $"Big Five Personality traits with coefficients: Openness - {source.Character.Openness}, Conscientiousness - {source.Character.Conscientiousness}" +
                                               $", Extraversion - {source.Character.Extraversion}, Agreeableness - {source.Character.Agreeableness}, " +
                                               $"Agreeableness - {source.Character.Agreeableness}. In response person should feel sorry for himself because other people thinks that he is mafia.");
            Console.WriteLine(message);
            var activeCitizens = Citizens.Where(x => x.IsActive && x.Name != source.Name).ToList();
            foreach (var citizen in activeCitizens)
            {
                var feelSorryEffectProbabilityCharacterCoefficient = GenerateCharacterCoefficient(citizen.Character, 
                    0, 0, 1, 1, -1);

                var randomIndex = _random.Next(1, 100) * feelSorryEffectProbabilityCharacterCoefficient;
                if (randomIndex < 50)
                {
                    var possibleEmotions = new[] { Emotion.Bored, Emotion.Inhibited, Emotion.Puzzled, Emotion.Unconcerned, Emotion.Angry };
                    citizen.ChangeRelationFactor(source.Name, -30 * feelSorryEffectProbabilityCharacterCoefficient);
                    citizen.Memories.Add(new Memory
                    {
                        ActionType = Action.FeelSorryForYourself,
                        ActorName = source.Name,
                        Emotion = (Emotion) possibleEmotions.GetValue(_random.Next(possibleEmotions.Length)),
                        TimeToForget = _forgetCoefficient
                    });
                }
                else
                {
                    var possibleEmotions = new[]
                        { Emotion.Curious, Emotion.Loved, Emotion.Elated, Emotion.Unconcerned };
                    citizen.ChangeRelationFactor(source.Name, 50 * feelSorryEffectProbabilityCharacterCoefficient);
                    citizen.Memories.Add(new Memory
                    {
                        ActionType = Action.FeelSorryForYourself,
                        ActorName = source.Name,
                        Emotion = (Emotion) possibleEmotions.GetValue(_random.Next(possibleEmotions.Length)),
                        TimeToForget = _forgetCoefficient
                    });
                }
            }
            
            source.ChangeKillingProbability(10);
            Events.Add(new Event
            {
                Action = Action.FeelSorryForYourself,
                IsSuccessful = true,
                Source = source,
                Target = null
            });
        }

        public void SmallTalkAction(NpcModel source)
        {
            Console.WriteLine($"{source.Name} started small talk");
            var activeCitizens = Citizens.Where(x => x.IsActive && x.Name != source.Name).ToList();
            foreach (var citizen in activeCitizens)
            {
                var possibleEmotions = new[]
                    { Emotion.Bored, Emotion.Curious, Emotion.Inhibited, Emotion.Puzzled, Emotion.Unconcerned };
                var smallTalkEffectCharacterCoefficient = GenerateCharacterCoefficient(citizen.Character, 
                    0, 0, 0, 1, -1);
                citizen.ChangeRelationFactor(source.Name, 10 * smallTalkEffectCharacterCoefficient);
                citizen.Memories.Add(new Memory
                {
                    ActionType = Action.FeelSorryForYourself,
                    ActorName = source.Name,
                    Emotion = (Emotion) possibleEmotions.GetValue(_random.Next(possibleEmotions.Length)),
                    TimeToForget = _forgetCoefficient
                });
            }

            source.ChangeKillingProbability(-30);
            Events.Add(new Event
            {
                Action = Action.SmallTalk,
                IsSuccessful = true,
                Source = source,
                Target = null
            });
        }

        public void CollaborateAction(NpcModel source, NpcModel target)
        {
            Console.WriteLine($"{source.Name} wants to collaborate with {target.Name}");
            source.ChangeKillingProbability(20);
            if (target.Function == Function.PoliceOfficer && _policeCheckings.Keys.Contains(source.Name) && _policeCheckings[source.Name])
            {
                Console.WriteLine($"{target.Name} doesn't want to collaborate with {source.Name}");
                source.ChangeRelationFactor(target.Name, -30);
                target.KillingProbability += -10;
                return;
            }
            var sourceCharacterCoefficient = GenerateCharacterCoefficient(source.Character, 
                1, -1, 1, 2, -0.5);
            var targetCharacterCoefficient = GenerateCharacterCoefficient(target.Character, 
                1, -1, 1, 2, -0.5);
            var sourceRelationFactor = source.RelationFactor[target.Name] * sourceCharacterCoefficient;
            var targetRelationFactor = target.RelationFactor[source.Name] * targetCharacterCoefficient;
            var randomIndex = _random.NextDouble() * 200;
            if (randomIndex > sourceRelationFactor + targetRelationFactor)
            {
                source.ChangeRelationFactor(target.Name, -20 * sourceRelationFactor);
                target.ChangeRelationFactor(source.Name, 10 * targetRelationFactor);
                Console.WriteLine($"{target.Name} doesn't want to collaborate with {source.Name}");
                target.ChangeKillingProbability(-10);
                Events.Add(new Event
                {
                    Action = Action.Collaborate,
                    IsSuccessful = false,
                    Source = source,
                    Target = target
                });
                var possibleEmotions = new[] { Emotion.Angry, Emotion.Violent, Emotion.Inhibited };
                target.Memories.Add(new Memory
                {
                    ActionType = Action.FeelSorryForYourself,
                    ActorName = source.Name,
                    Emotion = (Emotion) possibleEmotions.GetValue(_random.Next(possibleEmotions.Length)),
                    TimeToForget = _forgetCoefficient
                });
                source.Memories.Add(new Memory
                {
                    ActionType = Action.FeelSorryForYourself,
                    ActorName = source.Name,
                    Emotion = (Emotion) possibleEmotions.GetValue(_random.Next(possibleEmotions.Length)),
                    TimeToForget = _forgetCoefficient
                });
            }
            else
            {
                source.ChangeRelationFactor(target.Name, 100);
                target.ChangeRelationFactor(source.Name, 100);
                _collaborators.Add(source.Name, target.Name);
                Console.WriteLine($"{target.Name} wants to collaborate with {source.Name}");
                target.ChangeKillingProbability(20);
                Events.Add(new Event
                {
                    Action = Action.Collaborate,
                    IsSuccessful = true,
                    Source = source,
                    Target = target
                });
                var possibleEmotions = new[] { Emotion.Dignified, Emotion.Elated, Emotion.Loved};
                target.Memories.Add(new Memory
                {
                    ActionType = Action.FeelSorryForYourself,
                    ActorName = source.Name,
                    Emotion = (Emotion) possibleEmotions.GetValue(_random.Next(possibleEmotions.Length)),
                    TimeToForget = _forgetCoefficient
                });
                source.Memories.Add(new Memory
                {
                    ActionType = Action.FeelSorryForYourself,
                    ActorName = source.Name,
                    Emotion = (Emotion) possibleEmotions.GetValue(_random.Next(possibleEmotions.Length)),
                    TimeToForget = _forgetCoefficient
                });
            }
        }

        public void AccuseAction(NpcModel source, NpcModel target)
        {
            Console.WriteLine($"{source.Name} accuse {target.Name}");
            var activeCitizens = Citizens.Where(x => x.IsActive && x.Name != source.Name && x.Name != target.Name).ToList();
            source.ChangeRelationFactor(target.Name, -10);
            target.ChangeRelationFactor(source.Name, -20);
            foreach (var citizen in activeCitizens)
            {
                var citizenCharacterCoefficient = GenerateCharacterCoefficient(citizen.Character, 
                    1, 1, 0, -1, 1);
                var randomIndex = _random.NextDouble() * 100;
                randomIndex += citizen.RelationFactor[source.Name] * citizenCharacterCoefficient;
                randomIndex += -citizen.RelationFactor[target.Name] * citizenCharacterCoefficient;
                if (randomIndex > 50)
                {
                    citizen.ChangeRelationFactor(target.Name, -20 * citizenCharacterCoefficient);
                }
                else
                {
                    citizen.ChangeRelationFactor(source.Name, -10 * citizenCharacterCoefficient);
                }
            }

            source.ChangeKillingProbability(10);
            target.ChangeKillingProbability(20);
            Events.Add(new Event
            {
                Action = Action.Accuse,
                IsSuccessful = true,
                Source = source,
                Target = target
            });
            var possibleEmotions = new[] { Emotion.Angry, Emotion.Violent, Emotion.Puzzled,Emotion.Inhibited};
            source.Memories.Add(new Memory
            {
                ActionType = Action.FeelSorryForYourself,
                ActorName = source.Name,
                Emotion = (Emotion) possibleEmotions.GetValue(_random.Next(possibleEmotions.Length)),
                TimeToForget = _forgetCoefficient
            });
        }

        public void DefendAction(NpcModel source, NpcModel target)
        {
            Console.WriteLine($"{source.Name} defended {target.Name}");
            var activeCitizens = Citizens.Where(x => x.IsActive && x.Name != source.Name && x.Name != target.Name).ToList();
            source.ChangeRelationFactor(target.Name, 10);
            target.ChangeRelationFactor(source.Name, 20);
            foreach (var citizen in activeCitizens)
            {
                var citizenCharacterCoefficient = GenerateCharacterCoefficient(citizen.Character, 
                    1, 1, 0, 1, 1);
                var randomIndex = _random.NextDouble() * 100;
                randomIndex += -(20/(citizen.RelationFactor[source.Name] * citizenCharacterCoefficient));
                randomIndex += -(20/(citizen.RelationFactor[target.Name] * citizenCharacterCoefficient));
                if (randomIndex < 70)
                {
                    citizen.ChangeRelationFactor(target.Name, 20 * citizenCharacterCoefficient);
                    citizen.ChangeRelationFactor(source.Name, 10 * citizenCharacterCoefficient);
                }
                else
                {
                    citizen.ChangeRelationFactor(source.Name, -20 * citizenCharacterCoefficient);
                    citizen.ChangeRelationFactor(target.Name, -10 * citizenCharacterCoefficient);
                }
            }

            source.ChangeKillingProbability(20);
            target.ChangeKillingProbability(10);
            Events.Add(new Event
            {
                Action = Action.Defend,
                IsSuccessful = true,
                Source = source,
                Target = target
            });
            var possibleEmotions = new[] { Emotion.Dignified, Emotion.Elated, Emotion.Loved, Emotion.Puzzled};
            source.Memories.Add(new Memory
            {
                ActionType = Action.FeelSorryForYourself,
                ActorName = source.Name,
                Emotion = (Emotion) possibleEmotions.GetValue(_random.Next(possibleEmotions.Length)),
                TimeToForget = _forgetCoefficient
            });
        }

        public bool DoDayTurn()
        {
            Console.WriteLine("People that still live in the city:");
            var activeCitizens = Citizens.Where(x => x.IsActive).ToList();
            foreach (var citizen in activeCitizens)
            {
                var message = "Citizen " + citizen.Name + ", role: " + citizen.Function;
                Console.WriteLine(message);
            }

            for (int i = 0; i < NumberOfTurns; i++)
            {
                var actionList = GenerateActions().OrderBy(a => _random.Next()).ToList();
                var sumOfActionsProbability = actionList.Sum(x => x.Probability);

                var randomIndex = _random.NextDouble() * sumOfActionsProbability;
                
                var currentRandomValue = 0.0;
                foreach (var action in actionList)
                {
                    currentRandomValue += action.Probability;
                    if (currentRandomValue > randomIndex)
                    {
                        var source = Citizens.FirstOrDefault(x => x.Name == action.SourceName);
                        var target = Citizens.FirstOrDefault(x => x.Name == action.TargetName);
                        switch (action.Action)
                        {
                            case Action.Accuse:
                                AccuseAction(source, target);
                                break;
                            case Action.Defend:
                                DefendAction(source, target);
                                break;
                            case Action.Collaborate:
                                CollaborateAction(source, target);
                                break;
                            case Action.FeelSorryForYourself:
                                FeelSorryForYourselfAction(source);
                                break;
                            case Action.SmallTalk:
                                SmallTalkAction(source);
                                break;
                        }
                        break;
                    }
                }
            }

            var votes = new List<string>();
            foreach (var citizen in activeCitizens)
            {
                votes.Add(citizen.Vote(activeCitizens.Select(x => x.Name).ToList()));
            }

            var mostOccurrences = votes.GroupBy(x => x).OrderByDescending(x => x.Count()).First().Count();
            var mostVotes = votes.GroupBy(x => x).Where(x => x.Count() == mostOccurrences);
            if (mostVotes.Count() != 1)
            {
                votes = new List<string>();
                foreach (var citizen in activeCitizens)
                {
                    votes.Add(citizen.Vote(activeCitizens.Select(x => x.Name).ToList()));
                    mostOccurrences = votes.GroupBy(x => x).OrderByDescending(x => x.Count()).First().Count(); 
                    mostVotes = votes.GroupBy(x => x).Where(x => x.Count() == mostOccurrences);
                    if (mostVotes.Count() != 1)
                    {
                        return false;
                    }
                }
            }

            var personToExile = Citizens.FirstOrDefault(x => x.Name == mostVotes.FirstOrDefault().Key);
            if (personToExile.Function == Function.Hunter)
            {
                HunterKillAction();
            }
            ExileAction(personToExile);
            Console.WriteLine($"{personToExile.Name} was exiled from city");
            return true;
        }
        
    }
}