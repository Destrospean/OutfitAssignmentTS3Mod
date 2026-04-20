using System;
using System.Collections.Generic;
using Sims3.Gameplay.CAS;
using Sims3.SimIFace;
using Sims3.SimIFace.CAS;

namespace Destrospean.OutfitAssignment
{
    public static class OutfitAssignmentUtils
    {
        [PersistableStatic(true)]
        public static readonly List<OutfitAssignment> OutfitAssignments = new List<OutfitAssignment>();

        [Persistable]
        public class OutfitAssignment
        {
            [Persistable]
            public readonly InteractionInstanceTypeUtils.CallbackTypes EntryCallbackType, ExitCallbackType;

            [Persistable]
            public readonly string InteractionInstanceType, SpecialOutfitKey;

            [Persistable]
            public OutfitCategories PreviousOutfitCategory = OutfitCategories.Everyday;

            [Persistable]
            public int PreviousOutfitIndex = 0;

            [Persistable]
            public readonly SimDescription SimDescription;

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
                    if (removeSpecialOutfits)
                    {
                        simDescription.RemoveSpecialOutfit(outfitAssignment.InteractionInstanceType);
                    }
                }
            }
        }

        public static void SwitchSimToAssignedOutfit(this Sims3.Gameplay.Actors.Sim sim, OutfitAssignment outfitAssignment)
        {
            int specialOutfitIndex = outfitAssignment.SimDescription.GetSpecialOutfitIndexFromKey(ResourceUtils.HashString32(outfitAssignment.SpecialOutfitKey));
            if (sim.CurrentOutfitCategory != OutfitCategories.Special || sim.CurrentOutfitIndex != specialOutfitIndex)
            {
                outfitAssignment.PreviousOutfitCategory = sim.CurrentOutfitCategory;
                outfitAssignment.PreviousOutfitIndex = sim.CurrentOutfitIndex;
            }
            sim.SwitchToOutfitWithSpin(OutfitCategories.Special, specialOutfitIndex);
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
