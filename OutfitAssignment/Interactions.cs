using System;
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

            public const string sLocalizationKey = "/Interactions/AssignOutfitToInteraction";

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
                    return Common.Localize(actor.IsFemale, sLocalizationKey + ":Name");
                }

                public override string[] GetPath(bool isFemale)
                {
                    return new[]
                    {
                        Common.Localize(isFemale, sLocalizationKey + ":Path")
                    };
                }

                public override bool Test(Sim actor, Sim target, bool isAutonomous, ref Sims3.SimIFace.GreyedOutTooltipCallback greyedOutTooltipCallback)
                {
                    return target.IsHuman;
                }
            }

            protected static bool TryGetEntryCallbackType(Sim target, out InteractionInstanceCallbackTypes? callbackType)
            {
                string localizationKey = "/Dialogs/EntryCallbackTypeDialog",
                text = Dialogs.ComboSelectionDialog.Show(entries: new SortedDictionary<string, object>(new DummyComparer())
                    {
                        {
                            Common.Localize(target.IsFemale, localizationKey + "/Options:InteractionStarted"),
                            InteractionInstanceCallbackTypes.InteractionStarted.ToString()
                        },
                        {
                            Common.Localize(target.IsFemale, localizationKey + "/Options:StandardEntry"),
                            InteractionInstanceCallbackTypes.StandardEntry.ToString()
                        }
                    }, titleText: Common.Localize(target.IsFemale, localizationKey + ":Title"), defaultEntry: InteractionInstanceCallbackTypes.InteractionStarted.ToString()) as string;
                if (text == null)
                {
                    callbackType = null;
                    return false;
                }
                callbackType = (InteractionInstanceCallbackTypes)Enum.Parse(typeof(InteractionInstanceCallbackTypes), text);
                return true;
            }

            protected static bool TryGetExitCallbackType(Sim target, out InteractionInstanceCallbackTypes? callbackType)
            {
                string localizationKey = "/Dialogs/ExitCallbackTypeDialog",
                text = Dialogs.ComboSelectionDialog.Show(entries: new SortedDictionary<string, object>(new DummyComparer())
                    {
                        {
                            Common.Localize(target.IsFemale, localizationKey + "/Options:InteractionEnded"),
                            InteractionInstanceCallbackTypes.InteractionEnded.ToString()
                        },
                        {
                            Common.Localize(target.IsFemale, localizationKey + "/Options:StandardExit"),
                            InteractionInstanceCallbackTypes.StandardExit.ToString()
                        },
                        {
                            Common.Localize(target.IsFemale, localizationKey + "/Options:Never"),
                            InteractionInstanceCallbackTypes.Never.ToString()
                        }
                    }, titleText: Common.Localize(target.IsFemale, localizationKey + ":Title"), defaultEntry: InteractionInstanceCallbackTypes.InteractionEnded.ToString()) as string;
                if (text == null)
                {
                    callbackType = null;
                    return false;
                }
                callbackType = (InteractionInstanceCallbackTypes)Enum.Parse(typeof(InteractionInstanceCallbackTypes), text);
                return true;
            }

            public override bool Run()
            {
                Type[] interactionInstanceTypes;
                InteractionInstanceCallbackTypes? entryCallbackType, exitCallbackType;
                if (Common.TryGetInteractionInstanceTypes(out interactionInstanceTypes) && TryGetEntryCallbackType(Target, out entryCallbackType) && TryGetExitCallbackType(Target, out exitCallbackType))
                {
                    string specialOutfitKey = "";
                    foreach (Type interactionInstanceType in interactionInstanceTypes)
                    {
                        OutfitAssignment outfitAssignment;
                        if (OutfitAssignment.TryGetOutfitAssignment(Target.SimDescription, interactionInstanceType, out outfitAssignment) && specialOutfitKey != outfitAssignment.SpecialOutfitKey)
                        {
                            if (!string.IsNullOrEmpty(specialOutfitKey))
                            {
                                Common.Notify(Common.Localize(Target.IsFemale, sLocalizationKey + "/Messages:MultipleOutfitsForInteractionsFound", Target), Target.SimDescription, Sims3.UI.StyledNotification.NotificationStyle.kSystemMessage);
                                return true;
                            }
                            specialOutfitKey = outfitAssignment.SpecialOutfitKey;
                        }
                    }
                    specialOutfitKey = string.IsNullOrEmpty(specialOutfitKey) ? "OutfitAssignment_" + Sims3.SimIFace.CustomContent.DownloadContent.GenerateGUID() : specialOutfitKey;
                    bool outfitIsPreexisting = Actor.SimDescription.HasSpecialOutfit(specialOutfitKey);
                    if (Actor.EditSpecialOutfit(specialOutfitKey))
                    {
                        foreach (Type interactionInstanceType in interactionInstanceTypes)
                        {
                            OutfitAssignment.AssignOutfitToInteraction(Target.SimDescription, specialOutfitKey, interactionInstanceType, entryCallbackType.Value, exitCallbackType.Value);
                        }
                    }
                    else if (!outfitIsPreexisting)
                    {
                        Actor.SimDescription.RemoveSpecialOutfit(specialOutfitKey);
                    }
                }
                return true;
            }
        }

        public class UnassignOutfitToInteraction : ImmediateInteraction<Sim, Sim>
        {
            public static InteractionDefinition Singleton = new Definition();

            public const string sLocalizationKey = "/Interactions/UnassignOutfitToInteraction";

            public class Definition : ImmediateInteractionDefinition<Sim, Sim, UnassignOutfitToInteraction>
            {
                public override string GetInteractionName(Sim actor, Sim target, InteractionObjectPair iop)
                {
                    return Common.Localize(actor.IsFemale, sLocalizationKey + ":Name");
                }

                public override string[] GetPath(bool isFemale)
                {
                    return new[]
                    {
                        Common.Localize(isFemale, sLocalizationKey + ":Path")
                    };
                }

                public override bool Test(Sim actor, Sim target, bool isAutonomous, ref Sims3.SimIFace.GreyedOutTooltipCallback greyedOutTooltipCallback)
                {
                    return OutfitAssignment.GetAllOutfitAssignments(target.SimDescription).Length > 0;
                }
            }

            public override bool Run()
            {
                Type[] interactionInstanceTypes;
                if (Common.TryGetInteractionInstanceTypes(out interactionInstanceTypes, Array.ConvertAll(OutfitAssignment.GetAllOutfitAssignments(Target.SimDescription), x => x.InteractionInstanceType)))
                {
                    foreach (Type interactionInstanceType in interactionInstanceTypes)
                    {
                        OutfitAssignment outfitAssignment;
                        if (OutfitAssignment.TryGetOutfitAssignment(Target.SimDescription, interactionInstanceType, out outfitAssignment) && Array.FindAll(OutfitAssignment.GetAllOutfitAssignments(Target.SimDescription), x => x.SpecialOutfitKey == outfitAssignment.SpecialOutfitKey).Length == 1)
                        {
                            Target.SimDescription.RemoveSpecialOutfit(outfitAssignment.SpecialOutfitKey);
                        }
                        OutfitAssignment.UnassignOutfitToInteraction(Target.SimDescription, interactionInstanceType);
                    }
                }
                return true;
            }
        }
    }
}
