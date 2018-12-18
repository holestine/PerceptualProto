namespace UIHelpers
{
    #region Using Statements

    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using CodePlex.Diagnostics.Silverlight;

    #endregion Using Statements

    /// <summary>
    /// Class to maintain relationships between containers, data grids and 
    /// text boxes in order to layout controls dynamically.
    /// </summary>
    public class LayoutHelper
    {
        #region Types

        /// <summary>
        /// Column state information used for dynamic 
        /// behaviors like sizing and tracking
        /// </summary>
        private class ColumnProperties
        {
            ///<summary>
            /// The current location of a column
            ///</summary>
            public int DisplayIndex;
            /// <summary>
            /// The initial location of a column
            /// </summary>
            public int ActualIndex;
            /// <summary>
            /// Left margin value taking into account the scroll bar positon when necessary
            /// </summary>
            public double LeftMargin;
            /// <summary>
            /// The text box to display the total
            /// </summary>
            public TextBox SummaryBox;
        }

        /// <summary>
        /// Defines the running state for the class
        /// </summary>
        private enum State
        {
            /// <summary>
            /// When running layout will happen dynamically
            /// </summary>
            Running, 
            /// <summary>
            /// When paused dynamic layout is disabled
            /// </summary>
            Paused
        }

        #endregion Types

        #region Fields

        /// <summary>
        /// Dictionary allowing column properties to be looked up based on the header value
        /// </summary>
        private readonly Dictionary<object, ColumnProperties> _columnProperties;        

        /// <summary>
        /// The grid to dynamically size
        /// </summary>
        private readonly DataGrid _grid;        

        /// <summary>
        /// Stores the position of the horizontal scroll bar on the data grid
        /// </summary>
        private double _horizontalScrollPostion;

        /// <summary>
        /// The data pager associated with the grid
        /// </summary>
        private readonly DataPager _pager;

        /// <summary>
        /// The stack panel that contains the grid and controls the 
        /// layout. Not necessarily the first level parent, should be 
        /// the stack panel that uses the StackPanelStyle.
        /// </summary>
        private readonly StackPanel _panel;        

        /// <summary>
        /// The view that contains the grid
        /// </summary>
        private readonly UserControl _view;

        /// <summary>
        /// Used to control the state of the layout helper
        /// </summary>
        private State _state;

        #endregion Fields

        #region Public Methods
        
        /// <summary>
        /// Constructs a LayoutHelper to manage a view's dynamic layout 
        /// </summary>
        /// <param name="view">The container for the view/screen</param>
        /// <param name="panel">The stack panel that contains the grid and contains 
        /// layout information such as the margin. Not necessarily the first level parent, 
        /// should be the stack panel that uses the StackPanelStyle.</param>
        /// <param name="grid">The data grid to manage</param>
        /// <example> 
        /// This sample shows how to call the <see cref="LayoutHelper"/> constructor.
        /// <code>
        /// class View : UserControl 
        /// {
        ///     LayoutHelper layoutHelper;
        /// 
        ///     void View()
        ///     {
        ///         layoutHelper = new LayoutHelper(this, xamlStackPanel, xamlDataGrid);
        ///     }
        /// }
        /// </code>
        /// </example>
        public LayoutHelper(UserControl view, StackPanel panel, DataGrid grid):
            this(view, panel, grid, null)
        {
            
        }

        /// <summary>
        /// Constructs a LayoutHelper to manage a view's dynamic layout 
        /// </summary>
        /// <param name="view">The container for the view/screen</param>
        /// <param name="panel">The stack panel that contains the grid and contains 
        /// layout information such as the margin. Not necessarily the first level parent, 
        /// should be the stack panel that uses the StackPanelStyle.</param>
        /// <param name="grid">The data grid to manage</param>
        /// <param name="pager">The grid's data pager</param>
        /// <example> 
        /// This sample shows how to call the <see cref="LayoutHelper"/> constructor.
        /// <code>
        /// class View : UserControl 
        /// {
        ///     LayoutHelper layoutHelper;
        /// 
        ///     void View()
        ///     {
        ///         layoutHelper = new LayoutHelper(this, xamlStackPanel, xamlDataGrid, xamlDataPager);
        ///     }
        /// }
        /// </code>
        /// </example>
        public LayoutHelper(UserControl view, StackPanel panel, DataGrid grid, DataPager pager)
        {
            _view = view;
            _grid = grid;
            _panel = panel;
            _pager = pager;

            _horizontalScrollPostion = 0;

            _columnProperties = new Dictionary<object, ColumnProperties>();

            if (_grid != null)
            {
                // Add all columns to _columnProperties in their original state
                foreach (var column in _grid.Columns)
                {
                    if(column.Header != null)
                    {
                        _columnProperties.Add(
                        column.Header,
                        new ColumnProperties
                            {
                                ActualIndex = column.DisplayIndex,
                                DisplayIndex = column.DisplayIndex,
                                LeftMargin = 0,
                                SummaryBox = null
                            });
                    }
                }

                _grid.Loaded += OnLoaded;
                _grid.SizeChanged += OnGridSizeChanged;
            }

            _state = State.Running;
        }

        /// <summary>
        /// Associates a DataGrid Column with a TextBox in order to track the position
        /// of the column and dynically move the text box accordingly. 
        /// </summary>
        /// <param name="header">The string that is displayed in the 
        /// column header. This value cannot be null or empty.</param>
        /// <param name="textbox">This textbox to associate, this should be located
        /// in a canvas horizontally aligned with the grid in order for the layout  
        /// algorithm to work properly.</param>
        /// <example> 
        /// This sample shows how to call the <see cref="AssignTotalBoxToColumn"/> method.
        /// <code>
        /// class View : UserControl 
        /// {
        ///     void View()
        ///     {
        ///         LayoutHelper layoutHelper = new LayoutHelper(this, xamlStackPanel, xamlDataGrid);
        ///         layoutHelper.AssignTotalBoxToColumn("Column Header", xamlTextBox);
        ///     }
        /// }
        /// </code>
        /// </example>
        public void AssignTotalBoxToColumn(string header, TextBox textbox)
        {
            if (!string.IsNullOrEmpty(header))
            {
                _columnProperties[header].SummaryBox = textbox;                
            }
        }

        /// <summary>
        /// Updates the text shown in the column header and maintains the  
        /// relationship with the text box associated with the column.
        /// </summary>
        /// <param name="column">The column to rename. The Header property must be 
        /// of type string and cannot be null empty.</param>
        /// <param name="newHeader">The new text for the column header. This value
        /// cannot be null empty.</param>
        /// <example> 
        /// This sample shows how to call the <see cref="RenameHeader"/> method.
        /// <code>
        /// class View : UserControl 
        /// {
        ///     LayoutHelper layoutHelper;
        /// 
        ///     void View()
        ///     {
        ///         layoutHelper = new LayoutHelper(this, xamlStackPanel, xamlDataGrid);
        ///     }
        /// 
        ///     private void OnLoadingRow(object sender, DataGridRowEventArgs e)
        ///     {
        ///         layoutHelper.RenameHeader(xamlDataGrid.Columns[2], "New Column Header");
        ///     }
        /// }
        /// </code>
        /// </example>
        public void RenameHeader(DataGridColumn column, string newHeader)
        {
            if ( !string.IsNullOrEmpty(column.Header.ToString()) && !string.IsNullOrEmpty(newHeader) && _columnProperties.ContainsKey(column.Header)) 
            {
                _columnProperties.Remove(column.Header);   

                column.Header = newHeader;

                _columnProperties.Add(
                        column.Header,
                        new ColumnProperties
                            {
                                ActualIndex = column.DisplayIndex,
                                DisplayIndex = column.DisplayIndex,
                                LeftMargin = 0,
                                SummaryBox = null
                            });
            }
        }

        /// <summary>
        /// Pause dynamic layout
        /// </summary>
        public void Pause()
        {
            _state = State.Paused;
        }

        /// <summary>
        /// Start dynamic layout
        /// </summary>
        public void Start()
        {
            _state = State.Running;
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Modifies the size and position of the summary data so it lines up 
        /// with the appropriate column in the DataGrid
        /// </summary>
        private void PositionSummaryInformation()
        {
            try
            {
                if (_grid != null)
                {
                    // Initialize summary box margins to DataGrid margin
                    foreach (var property in _columnProperties)
                    {
                        property.Value.LeftMargin = _grid.Margin.Left - _horizontalScrollPostion;
                    }

                    // Calculate margins for summary boxes
                    foreach (var column in _grid.Columns)
                    {
                        foreach (var property in _columnProperties)
                        {
                            // If summary info is to the right of the current column add margin
                            if (column.DisplayIndex < property.Value.DisplayIndex)
                            {
                                property.Value.LeftMargin += column.ActualWidth;
                            }
                        }
                    }

                    // Adjust summary box width and border margins
                    foreach (var property in _columnProperties)
                    {
                        var columnWidth = _grid.Columns[property.Value.ActualIndex].ActualWidth;
                        var dataGridWidth = _grid.ActualWidth;

                        if (property.Value.SummaryBox != null)
                        {
                            // Needs to disapear completely
                            if (property.Value.LeftMargin + columnWidth <= 0)
                            {
                                property.Value.SummaryBox.Visibility = Visibility.Collapsed;
                            }
                            // Needs to be cut off on left
                            else if (property.Value.LeftMargin < 0)
                            {
                                property.Value.SummaryBox.Width = columnWidth + property.Value.LeftMargin;
                                                                
                                property.Value.SummaryBox.Margin = new Thickness(
                                0,
                                property.Value.SummaryBox.Margin.Top,
                                property.Value.SummaryBox.Margin.Right,
                                property.Value.SummaryBox.Margin.Bottom );

                                property.Value.SummaryBox.Visibility = Visibility.Visible;
                            }
                            // Needs to disapear completely
                            else if (property.Value.LeftMargin >= dataGridWidth)
                            {
                                property.Value.SummaryBox.Visibility = Visibility.Collapsed;
                            }
                            // Needs to be cut off on right
                            else if (property.Value.LeftMargin + columnWidth > dataGridWidth)
                            {
                                property.Value.SummaryBox.Width = dataGridWidth - property.Value.LeftMargin;

                                property.Value.SummaryBox.Margin = new Thickness(
                                    property.Value.LeftMargin,
                                    property.Value.SummaryBox.Margin.Top,
                                    property.Value.SummaryBox.Margin.Right,
                                    property.Value.SummaryBox.Margin.Bottom );

                                property.Value.SummaryBox.Visibility = Visibility.Visible;
                            }
                            // Normal
                            else
                            {
                                property.Value.SummaryBox.Width = columnWidth;

                                property.Value.SummaryBox.Margin = new Thickness(
                                    property.Value.LeftMargin,
                                    property.Value.SummaryBox.Margin.Top,
                                    property.Value.SummaryBox.Margin.Right,
                                    property.Value.SummaryBox.Margin.Bottom );

                                property.Value.SummaryBox.Visibility = Visibility.Visible;                                    
                            }
                        }                        
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Publish();
            }
        }

        /// <summary>
        /// Recursive method that finds horizontal scrollbars and assigns events for scrolling
        /// </summary>
        /// <param name="obj">The top level object</param>
        private void SetHorizontalScrollBarEvents(DependencyObject obj)
        {
            foreach (var child in GetObservableCollection(obj.GetVisualChildren()))
            {
                if (child != null)
                {
                    if (child.GetType() == typeof(ScrollBar))
                    {
                        if (((ScrollBar)child).Orientation == Orientation.Horizontal)
                        {
                            ((ScrollBar) child).LayoutUpdated += OnGridLayoutUpdated;
                            ((ScrollBar) child).ValueChanged += OnHorizontalScrollValueChanged;
                            break;
                        }
                    }
                    else
                    {
                        SetHorizontalScrollBarEvents(child);
                    }
                }
            }
        }

        /// <summary>
        /// Converts an enumerable collection to an observable collection
        /// </summary>
        /// <typeparam name="T">The type of collection items</typeparam>
        /// <param name="enumerableList">The collection</param>
        /// <returns></returns>
        private static ObservableCollection<T> GetObservableCollection<T>(IEnumerable<T> enumerableList)
        {
            if (enumerableList != null)
            {
                //create an emtpy observable collection object
                var observableCollection = new ObservableCollection<T>();

                //loop through all the records and add to observable collection object
                foreach (var item in enumerableList)
                    observableCollection.Add(item);

                //return the populated observable collection
                return observableCollection;
            }
            return null;
        }

        /// <summary>
        /// Sets the width of the data pager
        /// </summary>
        /// <param name="width">The new width</param>
        private void SetPagerWidth(double width)
        {
            if (_pager != null)
            {
                _pager.Width = width;
            }
        }

        /// <summary>
        /// Sets the width of the data grid
        /// </summary>
        private void SetGridWidth()
        {
            if (_grid.MaxWidth != _panel.ActualWidth)
            {
                _grid.MaxWidth = _panel.ActualWidth;
                PositionSummaryInformation();
            }
        }

        #endregion Private Methods

        #region Events

        /// <summary>        
        /// Event handler called after the data grid is loaded
        /// </summary>
        /// <param name="sender">The data grid</param>
        /// <param name="e">The event args</param>
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if(_grid != null)
            {
                _grid.ColumnReordered += OnColumnReordered;
                _grid.ColumnDisplayIndexChanged += OnColumnDisplayIndexChanged;
            }

            if (_view != null)
            {
                _view.SizeChanged += OnWindowSizeChanged;
            }

            SetHorizontalScrollBarEvents(_grid);

            SetGridWidth();
        }

        /// <summary>
        /// Repositions summary information as the grid layout changes
        /// </summary>
        /// <param name="sender">The scroll bar</param>
        /// <param name="e">The event args</param>
        private void OnGridLayoutUpdated(object sender, EventArgs e)
        {
            if (_state == State.Running)
            {
                PositionSummaryInformation();
            }
        }

        /// <summary>
        /// Event to capture the position of the scroll bar
        /// </summary>
        /// <param name="sender">The scroll bar</param>
        /// <param name="e">The event args</param>
        private void OnHorizontalScrollValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
        	_horizontalScrollPostion = e.NewValue;
        }
        
        /// <summary>
        /// Modifies layout of summary data after columns are re-ordered
        /// </summary>
        /// <param name="sender">The DataGrad that had its columns re-ordered</param>
        /// <param name="e">Event data</param>
        private void OnColumnReordered(object sender, DataGridColumnEventArgs e)
        {
            if (_state == State.Running)
            {
                PositionSummaryInformation();
            }
        }

        /// <summary>
        /// Modifies size of the grid based on the window size
        /// and positions controls accordingly
        /// </summary>
        /// <param name="sender">The window</param>
        /// <param name="e">The event arguments</param>
        private void OnWindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
            SetGridWidth();
        }

        /// <summary>
        /// Tracks the visual order of columns in a DataGrid
        /// </summary>
        /// <param name="sender">A DataGrid Column that has been re-ordered</param>
        /// <param name="e">Event data</param>
        private void OnColumnDisplayIndexChanged(object sender, DataGridColumnEventArgs e)
        {
            if(_state == State.Running)
            {
                if (_columnProperties.ContainsKey(e.Column.Header))
                {
                    _columnProperties[e.Column.Header].DisplayIndex = e.Column.DisplayIndex;
                }
            }
        }        
        
        /// <summary>
        /// Repositions controls after a size change in the DataGrid
        /// </summary>
        /// <param name="sender">A DataGrid that has been resized</param>
        /// <param name="e">Event data</param>
        private void OnGridSizeChanged(object sender, SizeChangedEventArgs e)
        {
            SetPagerWidth(e.NewSize.Width);
        }

        #endregion Events
    }
}
