using System;
using System.Collections.Generic;
using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.CAS;
using Sims3.SimIFace;
using Sims3.SimIFace.CAS;
using Tuning = Sims3.Gameplay.Destrospean.OutfitAssignment;

namespace Destrospean.OutfitAssignment
{
    public static class OutfitAssignmentUtils
    {
        public const string OutfitAssignmentCategoryPrefix = "OutfitAssignment_Category_";

        [PersistableStatic(true)]
        public static List<OutfitAssignment> OutfitAssignments = new List<OutfitAssignment>();

        [PersistableStatic(true)]
        public static List<Outfit> PreviousOutfits = new List<Outfit>();

        public static List<SimDescription> TimeToChangeBackList = new List<SimDescription>();

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

        public static void SwitchToAssignedOutfit(this Sims3.Gameplay.Actors.Sim sim, OutfitAssignment outfitAssignment, bool spin = true)
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
                outfitIndex = Tuning.kPickRandomOutfitIndex ? Sims3.Gameplay.Core.RandomUtil.GetInt(sim.SimDescription.GetOutfitCount(outfitCategory) - 1) : 0;
            }
            else
            {
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

        public static void SwitchToPreviousOutfit(this Sims3.Gameplay.Actors.Sim sim, bool spin = true)
        {
            TimeToChangeBackList.Add(sim.SimDescription);
            int previousOutfitIndex = OutfitAssignmentUtils.PreviousOutfits.FindIndex(x => x.SimDescription == sim.SimDescription);
            if (previousOutfitIndex > -1)
            {
                if (!sim.BuffManager.HasElement(BuffNames.Singed) && !sim.BuffManager.HasElement(BuffNames.SingedElectricity) && !sim.BuffManager.HasElement(BuffNames.EmbarrassedClothesHidden) && !sim.BuffManager.DisallowClothesChange() && !sim.OccultManager.DisallowClothesChange())
                {
                    if (spin)
                    {
                        sim.SwitchToOutfitWithSpin(OutfitAssignmentUtils.PreviousOutfits[previousOutfitIndex].Category, OutfitAssignmentUtils.PreviousOutfits[previousOutfitIndex].Index);
                    }
                    else
                    {
                        sim.SwitchToOutfitWithoutSpin(OutfitAssignmentUtils.PreviousOutfits[previousOutfitIndex].Category, OutfitAssignmentUtils.PreviousOutfits[previousOutfitIndex].Index);
                    }
                }
                OutfitAssignmentUtils.PreviousOutfits.RemoveAt(previousOutfitIndex);
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
