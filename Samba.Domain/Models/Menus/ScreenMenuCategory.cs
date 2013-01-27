﻿using System;
using System.Collections.Generic;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Menus
{
    public class ScreenMenuCategory : IOrderable
    {
        public ScreenMenuCategory()
        {
            _screenMenuItems = new List<ScreenMenuItem>();
            MButtonHeight = 65;
            ButtonHeight = 65;
            SubButtonHeight = 65;
            ColumnCount = 0;
            WrapText = false;
            ButtonColor = "Green";
            MButtonColor = "Orange";
            NumeratorType = 2;
            PageCount = 1;
        }

        public ScreenMenuCategory(string name)
            : this()
        {
            Name = name;
        }

        public int Id { get; set; }
        public string Name { get; set; }

        public int Order { get; set; }
        public string UserString { get { return Name; } }

        public int ScreenMenuId { get; set; }

        public bool MostUsedItemsCategory { get; set; }

        private IList<ScreenMenuItem> _screenMenuItems;
        public virtual IList<ScreenMenuItem> ScreenMenuItems
        {
            get { return _screenMenuItems; }
            set { _screenMenuItems = value; }
        }

        public int MenuItemCount { get { return ScreenMenuItems.Count; } }
        public int ColumnCount { get; set; }

        public int ButtonHeight { get; set; }
        public bool WrapText { get; set; }
        public string ButtonColor { get; set; }
        public int PageCount { get; set; }

        public int MButtonHeight { get; set; }
        public string MButtonColor { get; set; }

        public int SubButtonHeight { get; set; }

        public int NumeratorType { get; set; }
        public string NumeratorValues { get; set; }
        public string AlphaButtonValues { get; set; }

        public string ImagePath { get; set; }

        public bool IsQuickNumeratorVisible { get { return NumeratorType == 1; } }
        public bool IsNumeratorVisible { get { return NumeratorType == 2; } }
        public int MaxItems { get; set; }
        public int SortType { get; set; }

        public void AddMenuItem(MenuItem menuItem)
        {
            var smi = new ScreenMenuItem { MenuItemId = menuItem.Id, Name = menuItem.Name };
            ScreenMenuItems.Add(smi);
        }

        public int ItemCountPerPage
        {
            get
            {
                var itemCount = ScreenMenuItems.Count / PageCount;
                if (ScreenMenuItems.Count % PageCount > 0) itemCount++;
                return itemCount;
            }
        }

    }
}
