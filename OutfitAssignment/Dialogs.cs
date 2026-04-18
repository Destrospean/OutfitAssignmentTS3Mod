using System;
using System.Collections.Generic;
using Sims3.SimIFace;
using Sims3.UI;

namespace Destrospean.OutfitAssignment
{
    public class Dialogs
    {
        public class ComboSelectionDialog : ModalDialog
        {
            public const string kLayoutName = "ComboSelectionDialog";

            public const int kWinExportID = 4096;

            public Button mCancelButton, mOkButton;

            public ComboBox mCombo;

            public object mResult;

            public object Result
            {
                get
                {
                    return mResult;
                }
            }

            public enum ControlID : uint
            {
                kComboBoxId = 2,
                kOKButtonID,
                kCancelButtonID,
                kTitleTextID
            }

            public ComboSelectionDialog(string titleText, IDictionary<string, object> entries, object defaultEntry, Vector2 position, PauseMode pauseMode) : base(kLayoutName, kWinExportID, true, pauseMode, null)
            {
                if (mModalDialogWindow == null)
                {
                    return;
                }
                Text text = mModalDialogWindow.GetChildByID(5, true) as Text;
                if (text != null)
                {
                    text.Caption = titleText;
                }
                mCombo = mModalDialogWindow.GetChildByID(2, true) as ComboBox;
                foreach (KeyValuePair<string, object> entry in entries)
                {
                    mCombo.ValueList.Add(entry.Key, entry.Value);
                    if (entry.Value as string == defaultEntry as string)
                    {
                        mCombo.CurrentSelection = (uint)(mCombo.ValueList.Count - 1);
                    }
                }
                Rect area = mModalDialogWindow.Area;
                float height = area.BottomRight.y - area.TopLeft.y,
                width = area.BottomRight.x - area.TopLeft.x,
                x = position.x,
                y = position.y;
                if (x < 0 && y < 0)
                {
                    Rect parentArea = mModalDialogWindow.Parent.Area;
                    x = (float)Math.Round((parentArea.BottomRight.x - parentArea.TopLeft.x - width) / 2);
                    y = (float)Math.Round((parentArea.BottomRight.y - parentArea.TopLeft.y - height) / 2);
                }
                area.Set(x, y, x + width, y + height);
                mModalDialogWindow.Area = area;
                mOkButton = mModalDialogWindow.GetChildByID(3, false) as Button;
                if (mOkButton != null)
                {
                    mOkButton.Click += OnButtonClick;
                }
                mCancelButton = mModalDialogWindow.GetChildByID(4, false) as Button;
                if (mCancelButton != null)
                {
                    mCancelButton.Click += OnButtonClick;
                }
                OkayID = 3;
                SelectedID = OkayID;
                CancelID = 4;
            }

            public void OnButtonClick(WindowBase sender, UIButtonClickEventArgs eventArgs)
            {
                eventArgs.Handled = true;
                EndDialog(sender.ID);
            }

            public override bool OnEnd(uint buttonID)
            {
                if (buttonID == 3)
                {
                    mResult = mCombo.EntryTags[(int)mCombo.CurrentSelection];
                }
                else
                {
                    mResult = null;
                }
                return true;
            }

            public static object Show(string titleText, IDictionary<string, object> entries, object defaultEntry)
            {
                return Show(titleText, entries, defaultEntry, new Vector2(-1, -1), PauseMode.PauseSimulator);
            }

            public static object Show(string titleText, IDictionary<string, object> entries, object defaultEntry, Vector2 position, PauseMode pauseMode)
            {
                if (EnableModalDialogs)
                {
                    using (ComboSelectionDialog comboSelectionDialog = new ComboSelectionDialog(titleText, entries, defaultEntry, position, pauseMode))
                    {
                        comboSelectionDialog.StartModal();
                        return comboSelectionDialog.Result;
                    }
                }
                return defaultEntry;
            }
        }

        public class ObjectPickerDialog : ModalDialog
        {
            Button mOkayButton;

            List<ObjectPicker.RowInfo> mResult;

            ObjectPicker mTable;

            Vector2 mTableOffset;

            bool mWasOkay;

            public List<ObjectPicker.RowInfo> Result
            {
                get
                {
                    return mResult;
                }
            }

            public abstract class CommonHeaderInfo<T> : ObjectPicker.HeaderInfo where T : class
            {
                public virtual bool IsStub
                {
                    get
                    {
                        return false;
                    }
                }

                public CommonHeaderInfo(string headerKey, string toolTipKey, int width) : base(headerKey, toolTipKey, width)
                {
                }

                public CommonHeaderInfo(string headerKey, string toolTipKey, int width, bool textIsImage) : base(headerKey, toolTipKey, width, textIsImage)
                {
                }

                public abstract ObjectPicker.ColumnInfo GetValue(T item);
            }

            public ObjectPickerDialog(string title, List<ObjectPicker.TabInfo> listObjs, List<ObjectPicker.HeaderInfo> headers, int numSelectableRows, List<ObjectPicker.RowInfo> preSelectedRows) : base("UiObjectPicker", 1, true, PauseMode.PauseSimulator, null)
            {
                if (mModalDialogWindow == null)
                {
                    return;
                }
                Rect area = mModalDialogWindow.Area;
                area.mBottomRight.x += 200;
                mModalDialogWindow.Area = area;
                Text text = mModalDialogWindow.GetChildByID(99576787, false) as Text;
                text.Caption = title;
                mTable = mModalDialogWindow.GetChildByID(99576784, false) as ObjectPicker;
                TableContainer objectTable = mTable.ObjectTable;
                objectTable.TableChanged += OnTableChanged;
                ObjectPicker objectPicker0 = mTable,
                objectPicker1 = mTable;
                objectPicker0.SelectionChanged += OnSelectionChanged;
                objectPicker1.RowSelected += OnSelectionChanged;
                mTable.mViewButton.Visible = false;
                TableContainer tableContainer = mTable.mTable;
                tableContainer.mPopulationCompletedCallback += OnComplete;
                area = mTable.Area;
                area.mBottomRight.x += 200;
                mTable.Area = area;
                mOkayButton = mModalDialogWindow.GetChildByID(99576785, false) as Button;
                mOkayButton.TooltipText = Responder.Instance.LocalizationModel.LocalizeString("Ui/Caption/Global:Accept");
                mOkayButton.Enabled = true;
                mOkayButton.Click += OnOkayButtonClick;
                OkayID = mOkayButton.ID;
                SelectedID = mOkayButton.ID;
                Button button = mModalDialogWindow.GetChildByID(99576786, false) as Button;
                button.TooltipText = Responder.Instance.LocalizationModel.LocalizeString("Ui/Caption/ObjectPicker:Cancel");
                button.Click += OnCloseButtonClick;
                CancelID = button.ID;
                mTableOffset = mModalDialogWindow.Area.BottomRight - mModalDialogWindow.Area.TopLeft - (mTable.Area.BottomRight - mTable.Area.TopLeft);
                mTable.Populate(listObjs, headers, numSelectableRows);
                TabContainer tabs0 = mTable.mTabs,
                tabs1 = mTable.mTabs;
                tabs0.TabSelect -= mTable.OnTabSelect;
                tabs1.TabSelect += OnTabSelect;
                mTable.ViewTypeToggle = true;
                mTable.Selected = preSelectedRows;
                ResizeWindow(true);
            }

            void ResizeWindow(bool center)
            {
                Rect area = mModalDialogWindow.Parent.Area;
                float width = area.Width;
                float height = area.Height;
                int num = (int)height - (int)(mTableOffset.y * 2);
                num /= (int)mTable.mTable.RowHeight;
                if (num > mTable.mTable.NumberRows)
                {
                    num = mTable.mTable.NumberRows;
                }
                mTable.mTable.VisibleRows = (uint)num;
                mTable.mTable.GridSizeDirty = true;
                mTable.OnPopulationComplete();
                mModalDialogWindow.Area = new Rect(mModalDialogWindow.Area.TopLeft, mModalDialogWindow.Area.TopLeft + mTable.TableArea.BottomRight + mTableOffset);
                if (center)
                {
                    Rect area1 = mModalDialogWindow.Area;
                    float w = area1.Width, 
                    h = area1.Height,
                    x = (float)Math.Round((width - w) / 2),
                    y = (float)Math.Round((height - h) / 2);
                    area1.Set(x, y, x + w, y + h);
                    mModalDialogWindow.Area = area1;
                    Text text = mModalDialogWindow.GetChildByID(99576787, false) as Text;
                    Rect area2 = text.Area;
                    area2.Set(area2.TopLeft.x, 20, area2.BottomRight.x, 50 - area1.Height);
                    text.Area = area2;
                    mModalDialogWindow.Visible = true;
                }
            }

            void OnTabSelect(TabControl oldTab, TabControl newTab)
            {
                try
                {
                    int num = (int)newTab.Tag;
                    if (mTable.mSortedTab != num)
                    {
                        mTable.mSortedTab = num;
                        mTable.mSortText.Caption = mTable.mItems[mTable.mSortedTab].TabText;
                        TableContainer tableContainer0 = mTable.mTable,
                        tableContainer1 = mTable.mTable;
                        tableContainer0.mPopulationCompletedCallback += mTable.OnPopulationComplete;
                        tableContainer1.mPopulationCompletedCallback += OnComplete;
                        if (mTable.RepopulateTable())
                        {
                            OnComplete();
                        }
                    }
                }
                catch (Exception ex)
                {
                    ((IScriptErrorWindow)AppDomain.CurrentDomain.GetData("ScriptErrorWindow")).DisplayScriptError(null, ex);
                }
            }

            public void OnComplete()
            {
                try
                {
                    ResizeWindow(true);
                }
                catch (Exception ex)
                {
                    ((IScriptErrorWindow)AppDomain.CurrentDomain.GetData("ScriptErrorWindow")).DisplayScriptError(null, ex);
                }
            }

            void OnCloseButtonClick(WindowBase sender, UIButtonClickEventArgs eventArgs)
            {
                eventArgs.Handled = true;
                EndDialog(CancelID);
            }

            public override bool OnEnd(uint endID)
            {
                if (endID == OkayID)
                {
                    if (!mOkayButton.Enabled)
                    {
                        return false;
                    }
                    mResult = mTable.Selected;
                    mWasOkay = true;
                }
                else
                {
                    mResult = null;
                    mWasOkay = false;
                }
                mTable.Populate(null, null, 0);
                return true;
            }

            void OnOkayButtonClick(WindowBase sender, UIButtonClickEventArgs eventArgs)
            {
                eventArgs.Handled = true;
                EndDialog(OkayID);
            }

            void OnSelectionChanged(List<ObjectPicker.RowInfo> selectedRows)
            {
                Audio.StartSound("ui_tertiary_button");
                OnTableChanged();
            }

            void OnTableChanged()
            {
                int selectedItem = mTable.ObjectTable.SelectedItem;
                if (selectedItem >= 0 && mTable.ObjectTable.GetRow(selectedItem) != null && mTable.mTable.NumSelectableRows == 1)
                {
                    EndDialog(OkayID);
                }
            }

            public static List<T> Show<T>(string title, List<ObjectPicker.TabInfo> listObjs, List<CommonHeaderInfo<T>> headers, int numSelectableRows, out bool okayed) where T : class
            {
                List<ObjectPicker.RowInfo> preSelectedRows = null;
                return Show(title, listObjs, headers, numSelectableRows, preSelectedRows, out okayed);
            }

            public static List<T> Show<T>(string title, List<ObjectPicker.TabInfo> tabInfo, List<CommonHeaderInfo<T>> paramHeaders, int numSelectableRows, List<ObjectPicker.RowInfo> preSelectedRows, out bool okayed) where T : class
            {
                Simulator.Sleep(0);
                Dictionary<List<ObjectPicker.ColumnInfo>, bool> dictionary = new Dictionary<List<ObjectPicker.ColumnInfo>, bool>();
                foreach (ObjectPicker.TabInfo item in tabInfo)
                {
                    foreach (ObjectPicker.RowInfo subItem in item.RowInfo)
                    {
                        if (dictionary.ContainsKey(subItem.ColumnInfo))
                        {
                            continue;
                        }
                        dictionary.Add(subItem.ColumnInfo, true);
                        foreach (CommonHeaderInfo<T> paramHeader in paramHeaders)
                        {
                            ObjectPicker.ColumnInfo columnInfo = paramHeader.GetValue(subItem.Item as T);
                            if (columnInfo == null)
                            {
                                if (paramHeader.IsStub)
                                {
                                    continue;
                                }
                                columnInfo = new ObjectPicker.TextColumn("");
                            }
                            subItem.ColumnInfo.Add(columnInfo);
                        }
                    }
                }
                List<ObjectPicker.HeaderInfo> list = new List<ObjectPicker.HeaderInfo>();
                foreach (CommonHeaderInfo<T> paramHeader in paramHeaders)
                {
                    list.Add(paramHeader);
                }
                return Show<T>(title, tabInfo, list, numSelectableRows, preSelectedRows, out okayed);
            }

            public static List<T> Show<T>(string title, List<ObjectPicker.TabInfo> tabInfo, List<ObjectPicker.HeaderInfo> headers, int numSelectableRows, List<ObjectPicker.RowInfo> preSelectedRows, out bool okayed) where T : class
            {
                using (ObjectPickerDialog objectPickerDialog = new ObjectPickerDialog(title, tabInfo, headers, numSelectableRows, preSelectedRows))
                {
                    objectPickerDialog.StartModal();
                    okayed = objectPickerDialog.mWasOkay;
                    if (objectPickerDialog.Result == null || objectPickerDialog.Result.Count == 0)
                    {
                        return null;
                    }
                    List<T> list = new List<T>();
                    foreach (ObjectPicker.RowInfo item in objectPickerDialog.Result)
                    {
                        list.Add(item.Item as T);
                    }
                    return list;
                }
            }
        }
    }
}