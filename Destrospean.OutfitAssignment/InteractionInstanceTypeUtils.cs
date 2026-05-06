using System;
using System.Collections.Generic;
using Sims3.UI;

namespace Destrospean.OutfitAssignment
{
    public static class InteractionInstanceTypeUtils
    {
        static Type[] sInteractionInstanceTypes;

        public static Type[] InteractionInstanceTypes
        {
            get
            {
                if (sInteractionInstanceTypes == null)
                {
                    InitInteractionInstanceTypes();
                }
                return sInteractionInstanceTypes;
            }
        }

        public enum CallbackTypes
        {
            Never,
            InteractionStarted,
            InteractionEnded,
            StandardEntry,
            StandardExit,
            SyncLevelRouted,
            SyncLevelCommitted,
            SyncLevelCompleted,
            OutfitChanged
        }

        public static void InitInteractionInstanceTypes()
        {
            List<Type> interactionInstanceTypes = new List<Type>();
            foreach (System.Reflection.Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                interactionInstanceTypes.AddRange(Array.FindAll(assembly.GetTypes(), x => typeof(Sims3.Gameplay.Interactions.InteractionInstance).IsAssignableFrom(x) && x.IsClass && !x.IsAbstract));
            }
            sInteractionInstanceTypes = interactionInstanceTypes.ToArray();
        }

        public static bool TryGetSelectedInteractionInstanceTypes(out Type[] selectedInteractionInstanceTypes, Type[] allInteractionInstanceTypes = null, string namespaceListTitle = null, string interactionListTitle = null)
        {
            try
            {
                const string interactionListDialogLocalizationPath = Common.kLocalizationPath + "/Dialogs/InteractionListDialog",
                namespaceListDialogLocalizationPath = Common.kLocalizationPath + "/Dialogs/NamespaceListDialog";
                allInteractionInstanceTypes = allInteractionInstanceTypes ?? InteractionInstanceTypes;
                Array.Sort(allInteractionInstanceTypes, (a, b) => a.FullName.CompareTo(b.FullName));
                List<string> namespaces = new List<string>();
                foreach (Type interactionInstanceType in allInteractionInstanceTypes)
                {
                    if (!namespaces.Contains(interactionInstanceType.Namespace))
                    {
                        namespaces.Add(interactionInstanceType.Namespace);
                    }
                }
                bool cancelled, confirmed;
                while (true)
                {
                    List<string> selectedNamespaces = UI.Dialogs.ObjectPickerDialog.Show(namespaceListTitle ?? Responder.Instance.LocalizationModel.LocalizeString(namespaceListDialogLocalizationPath + ":Title"), new List<ObjectPicker.TabInfo>
                        {
                            new ObjectPicker.TabInfo("shop_all_r2", Responder.Instance.LocalizationModel.LocalizeString("Ui/Caption/ObjectPicker:All"), namespaces.ConvertAll(x => new ObjectPicker.RowInfo(x, new List<ObjectPicker.ColumnInfo>())))
                        }, new List<UI.Dialogs.ObjectPickerDialog.CommonHeaderInfo<string>>
                        {
                            new UI.Columns.TextColumn(namespaceListDialogLocalizationPath)
                        }, 1, out confirmed, out cancelled);
                    if (cancelled)
                    {
                        selectedInteractionInstanceTypes = null;
                        return false;
                    }
                    selectedInteractionInstanceTypes = (UI.Dialogs.ObjectPickerDialog.Show(interactionListTitle ?? Responder.Instance.LocalizationModel.LocalizeString(interactionListDialogLocalizationPath + ":Title"), new List<ObjectPicker.TabInfo>
                        {
                            new ObjectPicker.TabInfo("shop_all_r2", selectedNamespaces[0], new List<Type>(allInteractionInstanceTypes).FindAll(x => x.Namespace == selectedNamespaces[0]).ConvertAll(x => new ObjectPicker.RowInfo(x, new List<ObjectPicker.ColumnInfo>())))
                        }, new List<UI.Dialogs.ObjectPickerDialog.CommonHeaderInfo<Type>>
                        {
                            new UI.Columns.TypeColumn(interactionListDialogLocalizationPath)
                        }, int.MaxValue, out confirmed, out cancelled) ?? new List<Type>()).ToArray();
                    if (confirmed)
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                ((Sims3.SimIFace.IScriptErrorWindow)AppDomain.CurrentDomain.GetData("ScriptErrorWindow")).DisplayScriptError(null, ex);
                selectedInteractionInstanceTypes = null;
                return false;
            }
        }
    }
}
