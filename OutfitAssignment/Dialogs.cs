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
            const string kLayoutName = "ComboSelectionDialog";

            const int kWinExportID = 4096;

            Button mCancelButton, mOkButton;

            ComboBox mCombo;

            object mResult;

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

            public ComboSelectionDialog(string title, IDictionary<string, object> entries, object defaultEntry, Vector2 position, PauseMode pauseMode) : base(kLayoutName, kWinExportID, true, pauseMode, null)
            {
                if (mModalDialogWindow == null)
                {
                    return;
                }
                Text text = mModalDialogWindow.GetChildByID(5, true) as Text;
                if (text != null)
                {
                    text.Caption = title;
                }
                mCombo = (ComboBox)mModalDialogWindow.GetChildByID(2, true);
                foreach (KeyValuePair<string, object> entry in entries)
                {
                    mCombo.ValueList.Add(entry.Key, entry.Value);
                    if (entry.Value == defaultEntry)
                    {
                        mCombo.CurrentSelection = (uint)(mCombo.ValueList.Count - 1);
                    }
                }
                float x = position.x,
                y = position.y;
                if (x < 0 && y < 0)
                {
                    x = (float)Math.Round((mModalDialogWindow.Parent.Area.Width - mModalDialogWindow.Area.Width) / 2);
                    y = (float)Math.Round((mModalDialogWindow.Parent.Area.Height - mModalDialogWindow.Area.Height) / 2);
                }
                mModalDialogWindow.Area = new Rect(x, y, x + mModalDialogWindow.Area.Width, y + mModalDialogWindow.Area.Height);
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
                mResult = buttonID == 3 ? mCombo.EntryTags[(int)mCombo.CurrentSelection] : null;
                return true;
            }

            public static object Show(string title, IDictionary<string, object> entries, object defaultEntry)
            {
                return Show(title, entries, defaultEntry, new Vector2(-1, -1), PauseMode.PauseSimulator);
            }

            public static object Show(string title, IDictionary<string, object> entries, object defaultEntry, Vector2 position, PauseMode pauseMode)
            {
                if (EnableModalDialogs)
                {
                    using (ComboSelectionDialog comboSelectionDialog = new ComboSelectionDialog(title, entries, defaultEntry, position, pauseMode))
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

                public CommonHeaderInfo(string headerKey, string tooltipKey, int width) : base(headerKey, tooltipKey, width)
                {
                }

                public CommonHeaderInfo(string headerKey, string tooltipKey, int width, bool textIsImage) : base(headerKey, tooltipKey, width, textIsImage)
                {
                }

                public abstract ObjectPicker.ColumnInfo GetValue(T item);
            }

            public ObjectPickerDialog(string title, List<ObjectPicker.TabInfo> tabs, List<ObjectPicker.HeaderInfo> headers, int selectableRowCount, List<ObjectPicker.RowInfo> preSelectedRows) : base("UiObjectPicker", 1, true, PauseMode.PauseSimulator, null)
            {
                if (mModalDialogWindow == null)
                {
                    return;
                }
                mModalDialogWindow.Area = new Rect(new Vector2(mModalDialogWindow.Area.TopLeft.x, mModalDialogWindow.Area.TopLeft.y), new Vector2(mModalDialogWindow.Area.BottomRight.x + 200, mModalDialogWindow.Area.BottomRight.y));
                ((Text)mModalDialogWindow.GetChildByID(99576787, false)).Caption = title;
                mTable = (ObjectPicker)mModalDialogWindow.GetChildByID(99576784, false);
                mTable.ObjectTable.TableChanged += OnTableChanged;
                mTable.SelectionChanged += OnSelectionChanged;
                mTable.RowSelected += OnSelectionChanged;
                mTable.mViewButton.Visible = false;
                mTable.mTable.mPopulationCompletedCallback += OnComplete;
                mTable.Area = new Rect(new Vector2(mTable.Area.TopLeft.x, mTable.Area.TopLeft.y), new Vector2(mTable.Area.BottomRight.x + 200, mTable.Area.BottomRight.y));
                mOkayButton = (Button)mModalDialogWindow.GetChildByID(99576785, false);
                mOkayButton.TooltipText = Responder.Instance.LocalizationModel.LocalizeString("Ui/Caption/Global:Accept");
                mOkayButton.Enabled = false;
                mOkayButton.Click += OnOkayButtonClick;
                OkayID = mOkayButton.ID;
                SelectedID = mOkayButton.ID;
                Button cancelButton = (Button)mModalDialogWindow.GetChildByID(99576786, false);
                cancelButton.TooltipText = Responder.Instance.LocalizationModel.LocalizeString("Ui/Caption/ObjectPicker:Cancel");
                cancelButton.Click += OnCloseButtonClick;
                CancelID = cancelButton.ID;
                mTableOffset = mModalDialogWindow.Area.BottomRight - mModalDialogWindow.Area.TopLeft - (mTable.Area.BottomRight - mTable.Area.TopLeft);
                mTable.Populate(tabs, headers, selectableRowCount);
                mTable.mTabs.TabSelect -= mTable.OnTabSelect;
                mTable.mTabs.TabSelect += OnTabSelect;
                mTable.ViewTypeToggle = true;
                mTable.Selected = preSelectedRows;
                ResizeWindow(true);
            }

            void OnCloseButtonClick(WindowBase sender, UIButtonClickEventArgs eventArgs)
            {
                eventArgs.Handled = true;
                EndDialog(CancelID);
            }

            void OnOkayButtonClick(WindowBase sender, UIButtonClickEventArgs eventArgs)
            {
                eventArgs.Handled = true;
                EndDialog(OkayID);
            }

            void OnSelectionChanged(List<ObjectPicker.RowInfo> selectedRows)
            {
                Audio.StartSound("ui_tertiary_button");
                mOkayButton.Enabled = mTable.ObjectTable.SelectedItem > -1 && mTable.ObjectTable.GetRow(mTable.ObjectTable.SelectedItem) != null;
                OnTableChanged();
            }

            void OnTableChanged()
            {
                if (mTable.ObjectTable.SelectedItem > -1 && mTable.ObjectTable.GetRow(mTable.ObjectTable.SelectedItem) != null && mTable.mTable.NumSelectableRows == 1)
                {
                    EndDialog(OkayID);
                }
            }

            void OnTabSelect(TabControl oldTab, TabControl newTab)
            {
                try
                {
                    if (mTable.mSortedTab == (int)newTab.Tag)
                    {
                        return;
                    }
                    mTable.mSortedTab = (int)newTab.Tag;
                    mTable.mSortText.Caption = mTable.mItems[mTable.mSortedTab].TabText;
                    mTable.mTable.mPopulationCompletedCallback += mTable.OnPopulationComplete;
                    mTable.mTable.mPopulationCompletedCallback += OnComplete;
                    if (mTable.RepopulateTable())
                    {
                        OnComplete();
                    }
                }
                catch (Exception ex)
                {
                    ((IScriptErrorWindow)AppDomain.CurrentDomain.GetData("ScriptErrorWindow")).DisplayScriptError(null, ex);
                }
            }

            void ResizeWindow(bool center)
            {
                uint rowCount = ((uint)mModalDialogWindow.Parent.Area.Height - (uint)(mTableOffset.y * 2)) / (uint)mTable.mTable.RowHeight;
                mTable.mTable.VisibleRows = rowCount > mTable.mTable.NumberRows ? (uint)mTable.mTable.NumberRows : rowCount;
                mTable.mTable.GridSizeDirty = true;
                mTable.OnPopulationComplete();
                mModalDialogWindow.Area = new Rect(mModalDialogWindow.Area.TopLeft, mModalDialogWindow.Area.TopLeft + mTable.TableArea.BottomRight + mTableOffset);
                if (!center)
                {
                    return;
                }
                float x = (float)Math.Round((mModalDialogWindow.Parent.Area.Width - mModalDialogWindow.Area.Width) / 2),
                y = (float)Math.Round((mModalDialogWindow.Parent.Area.Height - mModalDialogWindow.Area.Height) / 2);
                mModalDialogWindow.Area = new Rect(x, y, x + mModalDialogWindow.Area.Width, y + mModalDialogWindow.Area.Height);
                Text text = (Text)mModalDialogWindow.GetChildByID(99576787, false);
                text.Area = new Rect(text.Area.TopLeft.x, 20, text.Area.BottomRight.x, 50 - mModalDialogWindow.Area.Height);
                mModalDialogWindow.Visible = true;
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

            public static List<T> Show<T>(string title, List<ObjectPicker.TabInfo> tabs, List<CommonHeaderInfo<T>> headers, int selectableRowCount, out bool confirmed) where T : class
            {
                List<ObjectPicker.RowInfo> preSelectedRows = null;
                return Show(title, tabs, headers, selectableRowCount, preSelectedRows, out confirmed);
            }

            public static List<T> Show<T>(string title, List<ObjectPicker.TabInfo> tabs, List<CommonHeaderInfo<T>> headers, int selectableRowCount, List<ObjectPicker.RowInfo> preSelectedRows, out bool confirmed) where T : class
            {
                Simulator.Sleep(0);
                List<List<ObjectPicker.ColumnInfo>> columnInfoLists = new List<List<ObjectPicker.ColumnInfo>>();
                foreach (ObjectPicker.TabInfo tabInfo in tabs)
                {
                    foreach (ObjectPicker.RowInfo rowInfo in tabInfo.RowInfo)
                    {
                        if (columnInfoLists.Contains(rowInfo.ColumnInfo))
                        {
                            continue;
                        }
                        columnInfoLists.Add(rowInfo.ColumnInfo);
                        foreach (CommonHeaderInfo<T> headerInfo in headers)
                        {
                            ObjectPicker.ColumnInfo columnInfo = headerInfo.GetValue(rowInfo.Item as T);
                            if (columnInfo == null)
                            {
                                if (headerInfo.IsStub)
                                {
                                    continue;
                                }
                                columnInfo = new ObjectPicker.TextColumn("");
                            }
                            rowInfo.ColumnInfo.Add(columnInfo);
                        }
                    }
                }
                return Show<T>(title, tabs, headers.ConvertAll(x => (ObjectPicker.HeaderInfo)x), selectableRowCount, preSelectedRows, out confirmed);
            }

            public static List<T> Show<T>(string title, List<ObjectPicker.TabInfo> tabs, List<ObjectPicker.HeaderInfo> headers, int selectableRowCount, List<ObjectPicker.RowInfo> preSelectedRows, out bool confirmed) where T : class
            {
                using (ObjectPickerDialog objectPickerDialog = new ObjectPickerDialog(title, tabs, headers, selectableRowCount, preSelectedRows))
                {
                    objectPickerDialog.StartModal();
                    confirmed = objectPickerDialog.mWasOkay;
                    if (objectPickerDialog.Result == null || objectPickerDialog.Result.Count == 0)
                    {
                        return null;
                    }
                    List<T> results = new List<T>();
                    foreach (ObjectPicker.RowInfo rowInfo in objectPickerDialog.Result)
                    {
                        results.Add(rowInfo.Item as T);
                    }
                    return results;
                }
            }
        }
    }
}