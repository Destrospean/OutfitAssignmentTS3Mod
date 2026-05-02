using System;
using System.Collections.Generic;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.CAS;
using Sims3.SimIFace;
using Sims3.SimIFace.CAS;
using Tuning = Sims3.Gameplay.Destrospean.OutfitAssignment;

namespace Destrospean.OutfitAssignment
{
    public static class OutfitAssignmentUtils
    {
        public static readonly BodyTypes[] ClothingTypes =
            {
                BodyTypes.Accessories,
                BodyTypes.Armband,
                BodyTypes.Bracelet,
                BodyTypes.Earrings,
                BodyTypes.FullBody,
                BodyTypes.Glasses,
                BodyTypes.Gloves,
                BodyTypes.Hair,
                BodyTypes.LeftEarring,
                BodyTypes.LeftGarter,
                BodyTypes.LowerBody,
                BodyTypes.Necklace,
                BodyTypes.NoseRing,
                BodyTypes.RightEarring,
                BodyTypes.RightGarter,
                BodyTypes.Ring,
                BodyTypes.Shoes,
                BodyTypes.Socks,
                BodyTypes.UpperBody
            };

        [PersistableStatic(true)]
        public static Dictionary<string, AssignedOutfit> GlobalOutfits = new Dictionary<string, AssignedOutfit>();

        [PersistableStatic(true)]
        public static List<string> GlobalOutfitsIncludingHair = new List<string>();

        public const string OutfitAssignmentCategoryPrefix = "OutfitAssignment_Category_";

        [PersistableStatic(true)]
        public static List<OutfitAssignment> OutfitAssignments = new List<OutfitAssignment>();

        [PersistableStatic(true)]
        public static List<Outfit> PreviousOutfits = new List<Outfit>();

        public static List<SimDescription> TimeToChangeBackList = new List<SimDescription>();

        [Persistable]
        public class AssignedOutfit
        {
            public List<BodyTypes> PartOverrides = new List<BodyTypes>(ClothingTypes);

            public List<SavedPart> Parts;

            [Persistable]
            public class SavedPart
            {
                public CASPart Part;

                public string Preset;

                public SavedPart()
                {
                }

                public SavedPart(CASPart part, string preset)
                {
                    Part = part;
                    Preset = preset;
                }
            }
            
            public AssignedOutfit()
            {
            }

            public AssignedOutfit(AssignedOutfit outfit)
            {
                Parts = outfit.Parts.ConvertAll(x => new SavedPart(x.Part, x.Preset));
                PartOverrides = new List<BodyTypes>(outfit.PartOverrides);
            }

            public AssignedOutfit(SimOutfit outfit)
            {
                Parts = new List<CASPart>(outfit.Parts).ConvertAll(x => new SavedPart(x, outfit.GetPartPreset(x.Key)));
            }
        }

        [Persistable]
        public class Outfit
        {
            public OutfitCategories Category;

            public int Index;

            public SimDescription SimDescription;
        }

        [Persistable]
        public class OutfitAssignment
        {
            public InteractionInstanceTypeUtils.CallbackTypes EntryCallbackType, ExitCallbackType;

            public string InteractionInstanceType, SpecialOutfitKey;

            public SimDescription SimDescription;

            public OutfitAssignment()
            {
            }

            public OutfitAssignment(SimDescription simDescription, string specialOutfitKey, Type interactionInstanceType, InteractionInstanceTypeUtils.CallbackTypes entryCallbackType, InteractionInstanceTypeUtils.CallbackTypes exitCallbackType)
            {
                EntryCallbackType = entryCallbackType;
                ExitCallbackType = exitCallbackType;
                InteractionInstanceType = interactionInstanceType.FullName;
                SimDescription = simDescription;
                SpecialOutfitKey = specialOutfitKey;
            }
        }

        public static bool AddGlobalAssignedOutfit(this Sim sim, string globalAssignedSpecialOutfitKey, string simSpecialOutfitKey = null)
        {
            SimOutfit baseOutfit = sim.SimDescription.GetOutfit(OutfitCategories.Everyday, 0);
            AssignedOutfit globalAssignedOutfit;
            if (!GlobalOutfits.TryGetValue(globalAssignedSpecialOutfitKey, out globalAssignedOutfit))
            {
                return false;
            }
            bool includeHair = GlobalOutfitsIncludingHair.Contains(globalAssignedSpecialOutfitKey);
            simSpecialOutfitKey = simSpecialOutfitKey ?? globalAssignedSpecialOutfitKey;
            if (sim.SimDescription.HasSpecialOutfit(simSpecialOutfitKey))
            {
                SimOutfit simSpecialOutfit = sim.SimDescription.GetSpecialOutfit(simSpecialOutfitKey);
                if (Array.TrueForAll(simSpecialOutfit.Parts, x => !Array.Exists(ClothingTypes, y => y == x.BodyType) || x.BodyType == BodyTypes.Hair && !includeHair || globalAssignedOutfit.Parts.Exists(y => x.Equals(y.Part) && y.Preset == simSpecialOutfit.GetPartPreset(x.Key))))
                {
                    return true;
                }
                sim.SimDescription.RemoveSpecialOutfit(simSpecialOutfitKey);
            }
            using (SimBuilder simBuilder = new SimBuilder
                {
                    UseCompression = true
                })
            {
                simBuilder.PrepareForOutfit(baseOutfit);
                foreach (AssignedOutfit.SavedPart savedPart in globalAssignedOutfit.Parts)
                {
                    if (Array.Exists(ClothingTypes, x => x == savedPart.Part.BodyType))
                    {
                        switch (savedPart.Part.BodyType)
                        {
                            case BodyTypes.FullBody:
                                simBuilder.RemoveParts(BodyTypes.LowerBody, BodyTypes.UpperBody);
                                break;
                            case BodyTypes.Hair:
                                if (!includeHair)
                                {
                                    continue;
                                }
                                break;
                            case BodyTypes.LowerBody:
                            case BodyTypes.UpperBody:
                                simBuilder.RemoveParts(BodyTypes.FullBody);
                                break;
                        }
                        simBuilder.RemoveParts(savedPart.Part.BodyType);
                        simBuilder.AddPart(savedPart.Part);
                        if (!string.IsNullOrEmpty(savedPart.Preset))
                        {
                            if (CASUtils.ApplyPresetToPart(simBuilder, savedPart.Part, savedPart.Preset))
                            {
                                simBuilder.SetPartPreset(savedPart.Part.Key, null, savedPart.Preset);
                            }
                        }
                    }
                }
                return sim.SimDescription.AddSpecialOutfit(new SimOutfit(simBuilder.CacheOutfit(simSpecialOutfitKey + "_" + sim.SimDescription.SimDescriptionId)), simSpecialOutfitKey) > -1;
            }
        }

        public static void AssignOutfitToInteraction(this SimDescription simDescription, string specialOutfitKey, Type interactionInstanceType, InteractionInstanceTypeUtils.CallbackTypes entryCallbackType, InteractionInstanceTypeUtils.CallbackTypes exitCallbackType)
        {
            UnassignOutfitToInteraction(simDescription, interactionInstanceType);
            OutfitAssignments.Add(new OutfitAssignment(simDescription, specialOutfitKey, interactionInstanceType, entryCallbackType, exitCallbackType));
        }

        public static void CreateOutfitForCategoryIfNecessary(this SimDescription simDescription, OutfitCategories outfitCategory)
        {
            switch (outfitCategory)
            {
                case OutfitCategories.Singed:
                    BuffSinged.SetupSingedOutfit(simDescription.CreatedSim);
                    break;
                case OutfitCategories.SkinnyDippingTowel:
                    simDescription.RemoveOutfits(OutfitCategories.SkinnyDippingTowel, true);
                    if (simDescription.HasSpecialOutfit("SkinnyDipTowel"))
                    {
                        simDescription.AddOutfit(simDescription.GetSpecialOutfit("SkinnyDipTowel"), OutfitCategories.SkinnyDippingTowel, true);
                    }
                    else
                    {
                        OutfitUtils.CreateOutfitForSim(simDescription, ResourceKey.CreateOutfitKeyFromProductVersion((simDescription.IsMale ? "m" : "f") + OutfitUtils.GetAgePrefix(simDescription.Age, true) + "_towel", ProductVersion.EP3), OutfitCategories.SkinnyDippingTowel, OutfitCategories.Swimwear, true);
                    }
                    break;
            }
        }

        public static OutfitAssignment[] GetAllOutfitAssignments(this SimDescription simDescription)
        {
            return OutfitAssignments.FindAll(x => x.SimDescription == simDescription).ToArray();
        }

        public static string GetGlobalAssignedOutfitPrefix(this Sim sim)
        {
            return "OutfitAssignment_Global_" + OutfitUtils.GetAgePrefix(sim.SimDescription.Age, true) + OutfitUtils.GetGenderPrefix(sim.SimDescription.Gender) + "_";
        }

        public static void RemoveAllOutfitAssignments(this SimDescription simDescription, bool removeSpecialOutfits = false)
        {
            foreach (OutfitAssignment outfitAssignment in new List<OutfitAssignment>(OutfitAssignments))
            {
                if (outfitAssignment.SimDescription == simDescription)
                {
                    OutfitAssignments.Remove(outfitAssignment);
                    if (removeSpecialOutfits && simDescription.HasSpecialOutfit(outfitAssignment.SpecialOutfitKey))
                    {
                        simDescription.RemoveSpecialOutfit(outfitAssignment.SpecialOutfitKey);
                    }
                }
            }
        }

        public static void SwitchToAssignedOutfit(this Sim sim, OutfitAssignment outfitAssignment, bool spin = true)
        {
            if (sim.BuffManager.HasElement(BuffNames.Singed) || sim.BuffManager.HasElement(BuffNames.SingedElectricity) || sim.BuffManager.HasElement(BuffNames.EmbarrassedClothesHidden) || sim.BuffManager.DisallowClothesChange() || sim.OccultManager.DisallowClothesChange())
            {
                return;
            }
            OutfitCategories outfitCategory;
            int outfitIndex;
            if (outfitAssignment.SpecialOutfitKey.StartsWith(OutfitAssignmentCategoryPrefix))
            {
                outfitCategory = (OutfitCategories)Enum.Parse(typeof(OutfitCategories), outfitAssignment.SpecialOutfitKey.Substring(OutfitAssignmentCategoryPrefix.Length));
                if (outfitCategory == 0)
                {
                    return;
                }
                outfitIndex = Tuning.kPickRandomOutfitIndex ? Sims3.Gameplay.Core.RandomUtil.GetInt(sim.SimDescription.GetOutfitCount(outfitCategory) - 1) : 0;
            }
            else
            {
                if (outfitAssignment.SimDescription == null)
                {
                    sim.AddGlobalAssignedOutfit(outfitAssignment.SpecialOutfitKey);
                }
                outfitCategory = OutfitCategories.Special;
                outfitIndex = sim.SimDescription.GetSpecialOutfitIndexFromKey(ResourceUtils.HashString32(outfitAssignment.SpecialOutfitKey));
            }
            sim.SimDescription.CreateOutfitForCategoryIfNecessary(outfitCategory);
            if (spin)
            {
                sim.SwitchToOutfitWithSpin(outfitCategory, outfitIndex);
            }
            else
            {
                sim.SwitchToOutfitWithoutSpin(outfitCategory, outfitIndex);
            }
        }

        public static void SwitchToPreviousOutfit(this Sim sim, bool spin = true)
        {
            TimeToChangeBackList.Add(sim.SimDescription);
            int previousOutfitIndex = PreviousOutfits.FindIndex(x => x.SimDescription == sim.SimDescription);
            if (previousOutfitIndex > -1)
            {
                if (!sim.BuffManager.HasElement(BuffNames.Singed) && !sim.BuffManager.HasElement(BuffNames.SingedElectricity) && !sim.BuffManager.HasElement(BuffNames.EmbarrassedClothesHidden) && !sim.BuffManager.DisallowClothesChange() && !sim.OccultManager.DisallowClothesChange())
                {
                    if (spin)
                    {
                        sim.SwitchToOutfitWithSpin(PreviousOutfits[previousOutfitIndex].Category, PreviousOutfits[previousOutfitIndex].Index);
                    }
                    else
                    {
                        sim.SwitchToOutfitWithoutSpin(PreviousOutfits[previousOutfitIndex].Category, PreviousOutfits[previousOutfitIndex].Index);
                    }
                }
                PreviousOutfits.RemoveAt(previousOutfitIndex);
            }
            TimeToChangeBackList.RemoveAll(x => x == sim.SimDescription);
        }

        public static bool TryGetOutfitAssignment(this SimDescription simDescription, Sims3.Gameplay.Interactions.InteractionInstance interactionInstance, out OutfitAssignment outfitAssignment)
        {
            return TryGetOutfitAssignment(simDescription, interactionInstance.GetType(), out outfitAssignment);
        }

        public static bool TryGetOutfitAssignment(this SimDescription simDescription, Type interactionInstanceType, out OutfitAssignment outfitAssignment)
        {
            List<OutfitAssignment> results = OutfitAssignments.FindAll(x => x.SimDescription == simDescription && Array.Find(InteractionInstanceTypeUtils.InteractionInstanceTypes, y => y.FullName == x.InteractionInstanceType).IsAssignableFrom(interactionInstanceType));
            if (results.Count == 0)
            {
                outfitAssignment = null;
                return false;
            }
            outfitAssignment = results[0];
            return true;
        }

        public static void UnassignOutfitToInteraction(this SimDescription simDescription, Type interactionInstanceType)
        {
            OutfitAssignment outfitAssignment;
            if (TryGetOutfitAssignment(simDescription, interactionInstanceType, out outfitAssignment))
            {
                OutfitAssignments.Remove(outfitAssignment);
            }
        }
    }
}
