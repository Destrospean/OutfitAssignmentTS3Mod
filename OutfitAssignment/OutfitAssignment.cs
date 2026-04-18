using System;
using System.Collections.Generic;
using Sims3.Gameplay.CAS;

namespace Destrospean.OutfitAssignment
{
    [Sims3.SimIFace.Persistable]
    public class OutfitAssignment
    {
        public readonly InteractionInstanceCallbackTypes EntryCallbackType, ExitCallbackType;

        public readonly Type InteractionInstanceType;

        [Sims3.SimIFace.PersistableStatic(true)]
        public static readonly List<OutfitAssignment> OutfitAssignments = new List<OutfitAssignment>();

        public Sims3.SimIFace.CAS.OutfitCategories PreviousOutfitCategory = Sims3.SimIFace.CAS.OutfitCategories.Everyday;

        public int PreviousOutfitIndex = 0;

        public readonly SimDescription SimDescription;

        public readonly string SpecialOutfitKey;

        public OutfitAssignment(SimDescription simDescription, string specialOutfitKey, Type interactionInstanceType, InteractionInstanceCallbackTypes entryCallbackType, InteractionInstanceCallbackTypes exitCallbackType)
        {
            EntryCallbackType = entryCallbackType;
            ExitCallbackType = exitCallbackType;
            InteractionInstanceType = interactionInstanceType;
            SimDescription = simDescription;
            SpecialOutfitKey = specialOutfitKey;
        }

        public static void AssignOutfitToInteraction(SimDescription simDescription, string specialOutfitKey, Type interactionInstanceType, InteractionInstanceCallbackTypes entryCallbackType, InteractionInstanceCallbackTypes exitCallbackType)
        {
            UnassignOutfitToInteraction(simDescription, interactionInstanceType);
            OutfitAssignments.Add(new OutfitAssignment(simDescription, specialOutfitKey, interactionInstanceType, entryCallbackType, exitCallbackType));
        }

        public static OutfitAssignment[] GetAllOutfitAssignments(SimDescription simDescription)
        {
            return OutfitAssignments.FindAll(x => x.SimDescription == simDescription).ToArray();
        }

        public static void RemoveAllOutfitAssignments(SimDescription simDescription, bool removeSpecialOutfits = false)
        {
            foreach (var outfitAssignment in new List<OutfitAssignment>(OutfitAssignments))
            {
                if (outfitAssignment.SimDescription == simDescription)
                {
                    OutfitAssignments.Remove(outfitAssignment);
                    if (removeSpecialOutfits)
                    {
                        simDescription.RemoveSpecialOutfit(outfitAssignment.InteractionInstanceType.FullName);
                    }
                }
            }
        }

        public static bool TryGetOutfitAssignment(SimDescription simDescription, Sims3.Gameplay.Interactions.InteractionInstance interactionInstance, out OutfitAssignment outfitAssignment)
        {
            return TryGetOutfitAssignment(simDescription, interactionInstance.GetType(), out outfitAssignment);
        }

        public static bool TryGetOutfitAssignment(SimDescription simDescription, Type interactionInstanceType, out OutfitAssignment outfitAssignment)
        {
            List<OutfitAssignment> results = OutfitAssignments.FindAll(x => x.SimDescription == simDescription && x.InteractionInstanceType.IsAssignableFrom(interactionInstanceType));
            if (results.Count == 0)
            {
                outfitAssignment = null;
                return false;
            }
            outfitAssignment = results[0];
            return true;
        }

        public static void UnassignOutfitToInteraction(SimDescription simDescription, Type interactionInstanceType)
        {
            OutfitAssignment outfitAssignment;
            if (TryGetOutfitAssignment(simDescription, interactionInstanceType, out outfitAssignment))
            {
                OutfitAssignments.Remove(outfitAssignment);
            }
        }
    }
}
