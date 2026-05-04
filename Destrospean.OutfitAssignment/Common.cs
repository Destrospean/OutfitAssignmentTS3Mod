using System;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.CAS;
using Sims3.Gameplay.Utilities;
using Sims3.UI;

namespace Destrospean.OutfitAssignment
{
    public static class Common
    {
        internal const string kLocalizationPath = "Destrospean/OutfitAssignment";

        public static void CopyTuning(Type baseType, Type oldType, Type newType)
        {
            if (AutonomyTuning.GetTuning(newType.FullName, baseType.FullName) == null)
            {
                InteractionTuning tuning = AutonomyTuning.GetTuning(oldType, oldType.FullName, baseType);
                if (tuning != null)
                {
                    AutonomyTuning.AddTuning(newType.FullName, baseType.FullName, tuning);
                }
            }
            InteractionObjectPair.sTuningCache.Remove(new Pair<Type, Type>(newType, baseType));
        }

        public static SimDescription GetSimDescription(this Sims3.Gameplay.Actors.Sim sim)
        {
            return sim == null ? null : sim.SimDescription;
        }

        public static string Localize(string entryKey)
        {
            return Localization.LocalizeString(kLocalizationPath + entryKey);
        }

        public static string Localize(string entryKey, params object[] parameters)
        {
            return Localization.LocalizeString(kLocalizationPath + entryKey, parameters);
        }

        public static string Localize(bool isFemale, string entryKey, params object[] parameters)
        {
            return Localization.LocalizeString(isFemale, kLocalizationPath + entryKey, parameters);
        }

        public static void Notify(string message, SimDescription simDescription, StyledNotification.NotificationStyle style)
        {
            Notify(message, simDescription, style, true);
        }

        public static void Notify(string message, SimDescription fakeSimDescription, StyledNotification.NotificationStyle style, bool checkForFake)
        {
            SimDescription simDescription = fakeSimDescription;
            if (simDescription == null)
            {
                StyledNotification.Show(new StyledNotification.Format(message, style));
                return;
            }
            if (checkForFake)
            {
                simDescription = SimDescription.Find(fakeSimDescription.SimDescriptionId);
                if (simDescription == null)
                {
                    StyledNotification.Show(new StyledNotification.Format(message, style));
                    return;
                }
            }
            if (simDescription.CreatedSim != null)
            {
                StyledNotification.Show(new StyledNotification.Format(message, Sims3.SimIFace.ObjectGuid.InvalidObjectGuid, simDescription.CreatedSim.ObjectId, style));
            }
            else
            {
                StyledNotification.Show(new StyledNotification.Format(message, style));
            }
        }
    }
}
