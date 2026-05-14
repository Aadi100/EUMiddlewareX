using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace smart_attendance_middleware.Classes
{
    /// <summary>
    /// Provides lightweight pagination functionality for ListView controls
    /// Optimized for memory efficiency - stores items without cloning until needed
    /// </summary>
    public class ListViewPagination
    {
        private readonly ListView _listView;
        private readonly Panel _paginationPanel;
        private readonly Label _pageInfoLabel;
        private readonly Button _firstButton;
        private readonly Button _prevButton;
        private readonly Button _nextButton;
        private readonly Button _lastButton;
        private readonly ComboBox _pageSizeComboBox;

        private int _currentPage = 1;
        private int _pageSize = 100; // Increased default for better performance
        private int _totalItems = 0;
        private List<ListViewItem> _allItems = new List<ListViewItem>();

        public int CurrentPage => _currentPage;
        public int PageSize => _pageSize;
        public int TotalPages => _totalItems > 0 ? (int)Math.Ceiling((double)_totalItems / _pageSize) : 1;
        public int TotalItems => _totalItems;

        public event EventHandler PageChanged;

        public ListViewPagination(ListView listView, Control containerControl)
        {
            _listView = listView;

            // Create pagination panel with minimal height
            _paginationPanel = new Panel
            {
                Height = 30,
                Dock = DockStyle.Bottom,
                BackColor = System.Drawing.Color.FromArgb(245, 245, 245)
            };

            // Create compact page info label
            _pageInfoLabel = new Label
            {
                AutoSize = true,
                Location = new System.Drawing.Point(5, 7),
                Text = "Page 1 of 1",
                Font = new System.Drawing.Font("Segoe UI", 8.25F)
            };

            // Create page size combo - optimized options
            Label pageSizeLabel = new Label
            {
                AutoSize = true,
                Location = new System.Drawing.Point(120, 7),
                Text = "Per page:",
                Font = new System.Drawing.Font("Segoe UI", 8.25F)
            };

            _pageSizeComboBox = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new System.Drawing.Point(175, 4),
                Width = 60,
                Font = new System.Drawing.Font("Segoe UI", 8.25F)
            };
            _pageSizeComboBox.Items.AddRange(new object[] { 50, 100, 200, 500 });
            _pageSizeComboBox.SelectedItem = _pageSize;
            _pageSizeComboBox.SelectedIndexChanged += PageSizeComboBox_SelectedIndexChanged;

            // Create compact navigation buttons
            _firstButton = new Button
            {
                Text = "<<",
                Width = 35,
                Location = new System.Drawing.Point(250, 4),
                Height = 22,
                FlatStyle = FlatStyle.Flat
            };
            _firstButton.Click += (s, e) => GoToPage(1);

            _prevButton = new Button
            {
                Text = "<",
                Width = 35,
                Location = new System.Drawing.Point(290, 4),
                Height = 22,
                FlatStyle = FlatStyle.Flat
            };
            _prevButton.Click += (s, e) => GoToPage(_currentPage - 1);

            _nextButton = new Button
            {
                Text = ">",
                Width = 35,
                Location = new System.Drawing.Point(330, 4),
                Height = 22,
                FlatStyle = FlatStyle.Flat
            };
            _nextButton.Click += (s, e) => GoToPage(_currentPage + 1);

            _lastButton = new Button
            {
                Text = ">>",
                Width = 35,
                Location = new System.Drawing.Point(370, 4),
                Height = 22,
                FlatStyle = FlatStyle.Flat
            };
            _lastButton.Click += (s, e) => GoToPage(TotalPages);

            // Add controls to pagination panel
            _paginationPanel.Controls.Add(_pageInfoLabel);
            _paginationPanel.Controls.Add(pageSizeLabel);
            _paginationPanel.Controls.Add(_pageSizeComboBox);
            _paginationPanel.Controls.Add(_firstButton);
            _paginationPanel.Controls.Add(_prevButton);
            _paginationPanel.Controls.Add(_nextButton);
            _paginationPanel.Controls.Add(_lastButton);

            // Add pagination panel to container
            Control targetContainer = containerControl;
            if (containerControl == null || containerControl is ListView)
            {
                targetContainer = listView.Parent;
            }

            if (targetContainer != null)
            {
                targetContainer.Controls.Add(_paginationPanel);
                _paginationPanel.BringToFront();
            }

            UpdateButtonStates();
        }

        private void PageSizeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_pageSizeComboBox.SelectedItem != null)
            {
                _pageSize = (int)_pageSizeComboBox.SelectedItem;
                _currentPage = 1;
                RefreshDisplay();
            }
        }

        public void SetItems(List<ListViewItem> items)
        {
            // Don't clone - use references for memory efficiency
            _allItems = items ?? new List<ListViewItem>();
            _totalItems = _allItems.Count;
            _currentPage = 1;
            RefreshDisplay();
        }

        public void Clear()
        {
            _allItems = null; // Allow GC
            _allItems = new List<ListViewItem>();
            _totalItems = 0;
            _currentPage = 1;
            _listView.Items.Clear();
            UpdatePageInfo();
            UpdateButtonStates();
        }

        private void GoToPage(int page)
        {
            if (page < 1 || page > TotalPages)
                return;

            _currentPage = page;
            RefreshDisplay();
            PageChanged?.Invoke(this, EventArgs.Empty);
        }

        private void RefreshDisplay()
        {
            try
            {
                _listView.BeginUpdate();
                _listView.Items.Clear();

                if (_totalItems == 0 || _allItems == null)
                {
                    UpdatePageInfo();
                    UpdateButtonStates();
                    return;
                }

                // Calculate range
                int startIndex = (_currentPage - 1) * _pageSize;
                int endIndex = Math.Min(startIndex + _pageSize, _totalItems);

                // Add items efficiently - use AddRange instead of individual adds
                var itemsToAdd = new List<ListViewItem>(endIndex - startIndex);
                for (int i = startIndex; i < endIndex; i++)
                {
                    var originalItem = _allItems[i];
                    // Clone only when displaying for thread safety
                    var clonedItem = (ListViewItem)originalItem.Clone();
                    itemsToAdd.Add(clonedItem);
                }

                _listView.Items.AddRange(itemsToAdd.ToArray());

                UpdatePageInfo();
                UpdateButtonStates();
            }
            finally
            {
                _listView.EndUpdate();
            }
        }

        private void UpdatePageInfo()
        {
            if (_totalItems > 0)
            {
                int startItem = (_currentPage - 1) * _pageSize + 1;
                int endItem = Math.Min(_currentPage * _pageSize, _totalItems);
                _pageInfoLabel.Text = $"Page {_currentPage}/{TotalPages} ({startItem}-{endItem} of {_totalItems})";
            }
            else
            {
                _pageInfoLabel.Text = "No items";
            }
        }

        private void UpdateButtonStates()
        {
            _firstButton.Enabled = _currentPage > 1;
            _prevButton.Enabled = _currentPage > 1;
            _nextButton.Enabled = _currentPage < TotalPages;
            _lastButton.Enabled = _currentPage < TotalPages;
        }

        public void Refresh()
        {
            RefreshDisplay();
        }

        public void Dispose()
        {
            _allItems?.Clear();
            _allItems = null;
            _paginationPanel?.Dispose();
        }
    }
}
