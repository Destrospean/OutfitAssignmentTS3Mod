using System.Collections.Generic;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.Interactions;
using Tuning = Sims3.Gameplay.Destrospean.OutfitAssignment;

namespace Destrospean.OutfitAssignment
{
    public class Interactions
    {
        public class AssignOutfitToInteraction : ImmediateInteraction<Sim, Sim>
        {
            public static InteractionDefinition Singleton = new Definition();

            public const string sLocalizationKey = "/AssignOutfitToInteraction";

            class DummyComparer : IComparer<string>
            {
                public int Compare(string a, string b)
                {
                    return 1;
                }
            }

            public class Definition : ImmediateInteractionDefinition<Sim, Sim, AssignOutfitToInteraction>
            {
                public override string GetInteractionName(Sim actor, Sim target, InteractionObjectPair iop)
                {
                    return "Assign Outfit to Interaction" /*Common.Localize(actor.IsFemale, sLocalizationKey + ":Name")*/;
                }

                public override string[] GetPath(bool isFemale)
                {
                    return new[]
                    {
                        "Outfit Assignments..." /*Common.Localize(isFemale, sLocalizationKey + ":Path")*/, 
                    };
                }

                public override bool Test(Sim actor, Sim target, bool isAutonomous, ref Sims3.SimIFace.GreyedOutTooltipCallback greyedOutTooltipCallback)
                {
                    return target.IsHuman;
                }
            }

            protected static bool TryGetApplyOutfitCallbackType(Sim target, out InteractionInstanceCallbackTypes? callbackType)
            {
                string localizationKey = "/Dialogs/WhenToApplyOutfitDialog",
                text = Dialogs.ComboSelectionDialog.Show(entries: new SortedDictionary<string, object>(new DummyComparer())
                    {
                        {
                            //Common.Localize(target.IsFemale, localizationKey + "/Options:InteractionStarted"),
                            "Interaction Started (for Solo)",
                            InteractionInstanceCallbackTypes.InteractionStarted.ToString()
                        },
                        {
                            //Common.Localize(target.IsFemale, localizationKey + "/Options:StandardEntry"),
                            "Standard Entry (for Objects)",
                            InteractionInstanceCallbackTypes.StandardEntry.ToString()
                        }
                    }, titleText: /*Common.Localize(target.IsFemale, localizationKey + ":Title")*/ "Select Entry Type", defaultEntry: InteractionInstanceCallbackTypes.InteractionStarted.ToString()) as string;
                if (text == null)
                {
                    callbackType = null;
                    return false;
                }
                callbackType = (InteractionInstanceCallbackTypes)System.Enum.Parse(typeof(InteractionInstanceCallbackTypes), text);
                return true;
            }

            protected static bool TryGetRemoveOutfitCallbackType(Sim target, out InteractionInstanceCallbackTypes? callbackType)
            {
                string localizationKey = "/Dialogs/WhenToRemoveOutfitDialog",
                text = Dialogs.ComboSelectionDialog.Show(entries: new SortedDictionary<string, object>(new DummyComparer())
                    {
                        {
                            //Common.Localize(target.IsFemale, localizationKey + "/Options:InteractionEnded"),
                            "Interaction Ended (for Solo)",
                            InteractionInstanceCallbackTypes.InteractionEnded.ToString()
                        },
                        {
                            //Common.Localize(target.IsFemale, localizationKey + "/Options:StandardExit"),
                            "Standard Exit (for Objects)",
                            InteractionInstanceCallbackTypes.StandardExit.ToString()
                        },
                        {
                            //Common.Localize(target.IsFemale, localizationKey + "/Options:Never"),
                            "Never",
                            InteractionInstanceCallbackTypes.Never.ToString()
                        }
                    }, titleText: /*Common.Localize(target.IsFemale, localizationKey + ":Title")*/ "Select Exit Type", defaultEntry: InteractionInstanceCallbackTypes.InteractionEnded.ToString()) as string;
                if (text == null)
                {
                    callbackType = null;
                    return false;
                }
                callbackType = (InteractionInstanceCallbackTypes)System.Enum.Parse(typeof(InteractionInstanceCallbackTypes), text);
                return true;
            }

            protected static bool TryGetInteractionInstanceType(Sim target, out System.Type interactionInstanceType)
            {
                IDictionary<string, object> interactionInstanceTypes = new SortedDictionary<string, object>(System.StringComparer.InvariantCultureIgnoreCase);
                foreach (System.Type type in Common.InteractionInstanceTypes)
                {
                    interactionInstanceTypes[type.FullName] = type.FullName;
                }
                string localizationKey = "/Dialogs/InteractionListDialog",
                text = Dialogs.ComboSelectionDialog.Show(entries: interactionInstanceTypes, titleText: /*Common.Localize(target.IsFemale, localizationKey + ":Title")*/ "Select Interaction", defaultEntry: new List<object>(interactionInstanceTypes.Values)[0]) as string;
                if (text == null)
                {
                    interactionInstanceType = null;
                    return false;
                }
                interactionInstanceType = System.Array.Find(Common.InteractionInstanceTypes, x => x.FullName == text);
                return true;
            }

            public override bool Run()
            {
                System.Type interactionInstanceType;
                InteractionInstanceCallbackTypes? entryCallbackType, exitCallbackType;
                if (TryGetInteractionInstanceType(Target, out interactionInstanceType) && TryGetApplyOutfitCallbackType(Target, out entryCallbackType) && TryGetRemoveOutfitCallbackType(Target, out exitCallbackType))
                {
                    bool outfitAlreadyExisted = Actor.SimDescription.HasSpecialOutfit(interactionInstanceType.FullName);
                    if (Actor.EditSpecialOutfit(interactionInstanceType.FullName))
                    {
                        OutfitAssignment.AssignOutfitToInteraction(Target.SimDescription, interactionInstanceType, entryCallbackType.Value, exitCallbackType.Value);
                    }
                    else if (!outfitAlreadyExisted)
                    {
                        Actor.SimDescription.RemoveSpecialOutfit(interactionInstanceType.FullName);
                    }
                }
                return true;
            }
        }

        public class UnassignOutfitToInteraction : ImmediateInteraction<Sim, Sim>
        {
            public static InteractionDefinition Singleton = new Definition();

            public const string sLocalizationKey = "/UnassignOutfitToInteraction";

            public class Definition : ImmediateInteractionDefinition<Sim, Sim, UnassignOutfitToInteraction>
            {
                public override string GetInteractionName(Sim actor, Sim target, InteractionObjectPair iop)
                {
                    return "Unassign Outfit to Interaction" /*Common.Localize(actor.IsFemale, sLocalizationKey + ":Name")*/;
                }

                public override string[] GetPath(bool isFemale)
                {
                    return new[]
                    {
                        "Outfit Assignments..." /*Common.Localize(isFemale, sLocalizationKey + ":Path")*/, 
                    };
                }

                public override bool Test(Sim actor, Sim target, bool isAutonomous, ref Sims3.SimIFace.GreyedOutTooltipCallback greyedOutTooltipCallback)
                {
                    return target.IsHuman;
                }
            }

            protected static bool TryGetInteractionInstanceType(Sim target, out System.Type interactionInstanceType)
            {
                IDictionary<string, object> interactionInstanceTypes = new SortedDictionary<string, object>(System.StringComparer.InvariantCultureIgnoreCase);
                foreach (OutfitAssignment outfitAssignment in OutfitAssignment.OutfitAssignments)
                {
                    if (outfitAssignment.SimDescription == target.SimDescription)
                    {
                        interactionInstanceTypes[outfitAssignment.InteractionInstanceType.FullName] = outfitAssignment.InteractionInstanceType.FullName;
                    }
                }
                string localizationKey = "/Dialogs/InteractionListDialog",
                text = Dialogs.ComboSelectionDialog.Show(entries: interactionInstanceTypes, titleText: /*Common.Localize(target.IsFemale, localizationKey + ":Title")*/ "Select Interaction", defaultEntry: new List<object>(interactionInstanceTypes.Values)[0]) as string;
                if (text == null)
                {
                    interactionInstanceType = null;
                    return false;
                }
                interactionInstanceType = System.Array.Find(Common.InteractionInstanceTypes, x => x.FullName == text);
                return true;
            }

            public override bool Run()
            {
                System.Type interactionInstanceType;
                if (TryGetInteractionInstanceType(Target, out interactionInstanceType))
                {
                    OutfitAssignment.UnassignOutfitToInteraction(Target.SimDescription, interactionInstanceType);
                    Target.SimDescription.RemoveSpecialOutfit(interactionInstanceType.FullName);
                }
                return true;
            }
        }
    }
}
