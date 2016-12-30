using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Data;
using Microsoft.Windows.Controls;

namespace Microsoft.Windows.Automation.Peers
{
    /// <summary>
    ///     AutomationPeer for an item in a DataGrid
    ///     This automation peer correspond to a row data item which may not have a visual container
    /// </summary>
    public sealed class DataGridItemAutomationPeer : AutomationPeer,
        IInvokeProvider, IScrollItemProvider, ISelectionItemProvider, ISelectionProvider
    {
        #region Constructors

        /// <summary>
        ///     AutomationPeer for an item in a DataGrid
        /// </summary>
        public DataGridItemAutomationPeer(object item, DataGrid dataGrid)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            if (dataGrid == null)
            {
                throw new ArgumentNullException("dataGrid");
            }

            _item = item;
            _dataGridAutomationPeer = UIElementAutomationPeer.CreatePeerForElement(dataGrid);
        }

        #endregion

        #region IInvokeProvider

        // Invoking DataGrid item should commit the item if it is in edit mode
        // or BeginEdit if item is not in edit mode
        void IInvokeProvider.Invoke()
        {
            EnsureEnabled();

            if (OwningRowPeer == null)
            {
                OwningDataGrid.ScrollIntoView(_item);
            }

            var success = false;
            if (OwningRow != null)
            {
                IEditableCollectionView iecv = OwningDataGrid.Items;
                if (iecv.CurrentEditItem == _item)
                {
                    success = OwningDataGrid.CommitEdit();
                }
                else
                {
                    if (OwningDataGrid.Columns.Count > 0)
                    {
                        var cell = OwningDataGrid.TryFindCell(_item, OwningDataGrid.Columns[0]);
                        if (cell != null)
                        {
                            OwningDataGrid.UnselectAll();
                            cell.Focus();
                            success = OwningDataGrid.BeginEdit();
                        }
                    }
                }
            }

            // Invoke on a NewItemPlaceholder row creates a new item.
            // BeginEdit on a NewItemPlaceholder row returns false.
            if (!success && !IsNewItemPlaceholder)
            {
                throw new InvalidOperationException(SR.Get(SRID.DataGrid_AutomationInvokeFailed));
            }
        }

        #endregion

        #region IScrollItemProvider

        void IScrollItemProvider.ScrollIntoView()
        {
            OwningDataGrid.ScrollIntoView(_item);
        }

        #endregion

        #region AutomationPeer Overrides

        ///
        protected override string GetAcceleratorKeyCore()
        {
            return (OwningRowPeer != null) ? OwningRowPeer.GetAcceleratorKey() : string.Empty;
        }

        ///
        protected override string GetAccessKeyCore()
        {
            return (OwningRowPeer != null) ? OwningRowPeer.GetAccessKey() : string.Empty;
        }

        ///
        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.DataItem;
        }

        ///
        protected override string GetAutomationIdCore()
        {
            return (OwningRowPeer != null) ? OwningRowPeer.GetAutomationId() : string.Empty;
        }

        ///
        protected override Rect GetBoundingRectangleCore()
        {
            return (OwningRowPeer != null) ? OwningRowPeer.GetBoundingRectangle() : new Rect();
        }

        ///
        protected override List<AutomationPeer> GetChildrenCore()
        {
            AutomationPeer wrapperPeer = OwningRowPeer;

            if (wrapperPeer != null)
            {
                // We need to update children manually since wrapperPeer is not in the Automation Tree
                // When containers are recycled the visual (DataGridRow) will point to a new item. 
                // WrapperPeer's children are the peers for DataGridRowHeader, DataGridCells and DataGridRowDetails.
                wrapperPeer.ResetChildrenCache();
                return wrapperPeer.GetChildren();
            }

            return GetCellItemPeers();
        }

        ///
        protected override string GetClassNameCore()
        {
            return (OwningRowPeer != null) ? OwningRowPeer.GetClassName() : string.Empty;
        }

        ///
        protected override Point GetClickablePointCore()
        {
            return (OwningRowPeer != null) ? OwningRowPeer.GetClickablePoint() : new Point(double.NaN, double.NaN);
        }

        ///
        protected override string GetHelpTextCore()
        {
            return (OwningRowPeer != null) ? OwningRowPeer.GetHelpText() : string.Empty;
        }

        ///
        protected override string GetItemStatusCore()
        {
            return (OwningRowPeer != null) ? OwningRowPeer.GetItemStatus() : string.Empty;
        }

        ///
        protected override string GetItemTypeCore()
        {
            return (OwningRowPeer != null) ? OwningRowPeer.GetItemType() : string.Empty;
        }

        ///
        protected override AutomationPeer GetLabeledByCore()
        {
            return (OwningRowPeer != null) ? OwningRowPeer.GetLabeledBy() : null;
        }

        ///
        protected override string GetLocalizedControlTypeCore()
        {
            return (OwningRowPeer != null)
                ? OwningRowPeer.GetLocalizedControlType()
                : base.GetLocalizedControlTypeCore();
        }

        ///
        protected override string GetNameCore()
        {
            return _item.ToString();
        }

        ///
        protected override AutomationOrientation GetOrientationCore()
        {
            return (OwningRowPeer != null) ? OwningRowPeer.GetOrientation() : AutomationOrientation.None;
        }

        ///
        public override object GetPattern(PatternInterface patternInterface)
        {
            switch (patternInterface)
            {
                case PatternInterface.Invoke:
                {
                    if (!OwningDataGrid.IsReadOnly)
                    {
                        return this;
                    }

                    break;
                }

                case PatternInterface.ScrollItem:
                case PatternInterface.Selection:
                    return this;
                case PatternInterface.SelectionItem:
                    if (IsRowSelectionUnit)
                    {
                        return this;
                    }
                    break;
            }

            return null;
        }

        ///
        protected override bool HasKeyboardFocusCore()
        {
            return (OwningRowPeer != null) ? OwningRowPeer.HasKeyboardFocus() : false;
        }

        ///
        protected override bool IsContentElementCore()
        {
            return (OwningRowPeer != null) ? OwningRowPeer.IsContentElement() : true;
        }

        ///
        protected override bool IsControlElementCore()
        {
            return (OwningRowPeer != null) ? OwningRowPeer.IsControlElement() : true;
        }

        ///
        protected override bool IsEnabledCore()
        {
            return (OwningRowPeer != null) ? OwningRowPeer.IsEnabled() : true;
        }

        ///
        protected override bool IsKeyboardFocusableCore()
        {
            return (OwningRowPeer != null) ? OwningRowPeer.IsKeyboardFocusable() : false;
        }

        ///
        protected override bool IsOffscreenCore()
        {
            return (OwningRowPeer != null) ? OwningRowPeer.IsOffscreen() : true;
        }

        ///
        protected override bool IsPasswordCore()
        {
            return (OwningRowPeer != null) ? OwningRowPeer.IsPassword() : false;
        }

        ///
        protected override bool IsRequiredForFormCore()
        {
            return (OwningRowPeer != null) ? OwningRowPeer.IsRequiredForForm() : false;
        }

        ///
        protected override void SetFocusCore()
        {
            if (OwningRowPeer != null && OwningRowPeer.Owner.Focusable)
            {
                OwningRowPeer.SetFocus();
            }
        }

        #endregion

        #region ISelectionItemProvider

        bool ISelectionItemProvider.IsSelected
        {
            get { return OwningDataGrid.SelectedItems.Contains(_item); }
        }

        IRawElementProviderSimple ISelectionItemProvider.SelectionContainer
        {
            get { return ProviderFromPeer(_dataGridAutomationPeer); }
        }

        void ISelectionItemProvider.AddToSelection()
        {
            if (!IsRowSelectionUnit)
            {
                throw new InvalidOperationException(SR.Get(SRID.DataGridRow_CannotSelectRowWhenCells));
            }

            // If item is already selected - do nothing
            if (OwningDataGrid.SelectedItems.Contains(_item))
            {
                return;
            }

            EnsureEnabled();

            if (OwningDataGrid.SelectionMode == DataGridSelectionMode.Single &&
                OwningDataGrid.SelectedItems.Count > 0)
            {
                throw new InvalidOperationException();
            }

            if (OwningDataGrid.Items.Contains(_item))
            {
                OwningDataGrid.SelectedItems.Add(_item);
            }
        }

        void ISelectionItemProvider.RemoveFromSelection()
        {
            if (!IsRowSelectionUnit)
            {
                throw new InvalidOperationException(SR.Get(SRID.DataGridRow_CannotSelectRowWhenCells));
            }

            EnsureEnabled();

            if (OwningDataGrid.SelectedItems.Contains(_item))
            {
                OwningDataGrid.SelectedItems.Remove(_item);
            }
        }

        void ISelectionItemProvider.Select()
        {
            if (!IsRowSelectionUnit)
            {
                throw new InvalidOperationException(SR.Get(SRID.DataGridRow_CannotSelectRowWhenCells));
            }

            EnsureEnabled();

            OwningDataGrid.SelectedItem = _item;
        }

        #endregion

        #region ISelectionProvider

        bool ISelectionProvider.CanSelectMultiple
        {
            get { return OwningDataGrid.SelectionMode == DataGridSelectionMode.Extended; }
        }

        bool ISelectionProvider.IsSelectionRequired
        {
            get { return false; }
        }

        IRawElementProviderSimple[] ISelectionProvider.GetSelection()
        {
            var dataGrid = OwningDataGrid;
            if (dataGrid == null)
            {
                return null;
            }

            var rowIndex = dataGrid.Items.IndexOf(_item);

            // If row has selection
            if (rowIndex > -1 && dataGrid.SelectedCellsInternal.Intersects(rowIndex))
            {
                var selectedProviders = new List<IRawElementProviderSimple>();

                for (var i = 0; i < OwningDataGrid.Columns.Count; i++)
                {
                    // cell is selected
                    if (dataGrid.SelectedCellsInternal.Contains(rowIndex, i))
                    {
                        var column = dataGrid.ColumnFromDisplayIndex(i);
                        var peer = GetOrCreateCellItemPeer(column);
                        if (peer != null)
                        {
                            selectedProviders.Add(ProviderFromPeer(peer));
                        }
                    }
                }

                if (selectedProviders.Count > 0)
                {
                    return selectedProviders.ToArray();
                }
            }

            return null;
        }

        #endregion

        #region Private Methods

        internal List<AutomationPeer> GetCellItemPeers()
        {
            var peers = new List<AutomationPeer>();
            var oldChildren = new Dictionary<DataGridColumn, DataGridCellItemAutomationPeer>(_itemPeers);
            _itemPeers.Clear();

            foreach (var column in OwningDataGrid.Columns)
            {
                DataGridCellItemAutomationPeer peer = null;
                var peerExists = oldChildren.TryGetValue(column, out peer);
                if (!peerExists || peer == null)
                {
                    peer = new DataGridCellItemAutomationPeer(_item, column);
                }

                peers.Add(peer);
                _itemPeers.Add(column, peer);
            }

            return peers;
        }

        internal DataGridCellItemAutomationPeer GetOrCreateCellItemPeer(DataGridColumn column)
        {
            DataGridCellItemAutomationPeer peer = null;
            var peerExists = _itemPeers.TryGetValue(column, out peer);
            if (!peerExists || peer == null)
            {
                peer = new DataGridCellItemAutomationPeer(_item, column);
                _itemPeers.Add(column, peer);
            }

            return peer;
        }

        internal AutomationPeer RowHeaderAutomationPeer
        {
            get { return (OwningRowPeer != null) ? OwningRowPeer.RowHeaderAutomationPeer : null; }
        }

        private void EnsureEnabled()
        {
            if (!_dataGridAutomationPeer.IsEnabled())
            {
                throw new ElementNotEnabledException();
            }
        }

        #endregion

        #region Private Properties

        private bool IsRowSelectionUnit
        {
            get
            {
                return (OwningDataGrid != null &&
                        (OwningDataGrid.SelectionUnit == DataGridSelectionUnit.FullRow ||
                         OwningDataGrid.SelectionUnit == DataGridSelectionUnit.CellOrRowHeader));
            }
        }

        private bool IsNewItemPlaceholder
        {
            get { return (_item == CollectionView.NewItemPlaceholder) || (_item == DataGrid.NewItemPlaceholder); }
        }

        private DataGrid OwningDataGrid
        {
            get
            {
                var gridPeer = _dataGridAutomationPeer as DataGridAutomationPeer;
                return (DataGrid) gridPeer.Owner;
            }
        }

        private DataGridRow OwningRow
        {
            get { return OwningDataGrid.ItemContainerGenerator.ContainerFromItem(_item) as DataGridRow; }
        }

        internal DataGridRowAutomationPeer OwningRowPeer
        {
            get
            {
                DataGridRowAutomationPeer rowPeer = null;
                var row = OwningRow;
                if (row != null)
                {
                    rowPeer = UIElementAutomationPeer.CreatePeerForElement(row) as DataGridRowAutomationPeer;
                }

                return rowPeer;
            }
        }

        #endregion

        #region Data

        private readonly object _item;
        private readonly AutomationPeer _dataGridAutomationPeer;

        private readonly Dictionary<DataGridColumn, DataGridCellItemAutomationPeer> _itemPeers =
            new Dictionary<DataGridColumn, DataGridCellItemAutomationPeer>();

        #endregion
    }
}