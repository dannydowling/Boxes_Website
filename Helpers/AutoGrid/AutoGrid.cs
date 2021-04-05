using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Boxes
{
    public class AutoGrid : Grid
    {
        public AutoGrid()
        {
            //default constructor fills the current view
            HorizontalOptions = LayoutOptions.Fill;
            VerticalOptions = LayoutOptions.Fill;
        }

        //set the columns and rows to the top left of the view
        private int current_column = 0;
        private int current_row = 0;

        public enum Orientation
        //which way the display is facing
        { Landscape, Portrait };

        //initialize cell width and height values
        private double width = 0;
        private double height = 0;

        //initialize the thing that can hold saved state
        List<ItemState> RestoreList = null;

        public void DefineGrid(int Width, int Height)
        { DefineCells(Width, Height); }

        public void AutoAdd(View Item, int Width = 1)
        {
            //AutoAdd will fill the current view with cells
            //should always be 1 less than the number of rows.
            if (current_row == RowDefinitions.Count) throw new Exception("Too many items.");

            AddView(Item, current_column, current_row, Width);
            current_column += Width;
            if (current_column == ColumnDefinitions.Count)
            {
                current_column = 0;
                current_row++;
            }
        }

        public void RestoreItems(List<ItemState> RestoreList)
        {
            if (RestoreList != null)
            {
                base.BatchBegin();

                foreach (var item in RestoreList) item.Restore();
                RestoreList = null;

                base.BatchCommit();
                DelayedRefreshVisibility();
            }
        }

        protected ContentView MergedCell(int x, int y, int width, int height)
        {
            var view = new ContentView();
            AddView(view, x, y, width, height);
            return view;
        }

        protected void DefineCells(int ColumnCount, int RowCount)
        {
            // This is an atomic operation that defines the cells in the grid
            Parallel.For(0, RowCount + ColumnCount, i =>
            {
                for (int c = 0; c < ColumnCount; ++c)
                {
                    ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                }
                for (int r = 0; r < RowCount; ++r)
                {
                    ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                }
            });
        }

        protected void FormatCell(int? y, int? x, GridUnitType Format)
        {
            //This will figure out how big the cells can be.
            //The user will pass in how many cells they want.

            switch (x, y)
            {
                case (int, int):
                    ColumnDefinitions[y.Value].Width = new GridLength(1, Format);
                    RowDefinitions[x.Value].Height = new GridLength(1, Format);

                    //format the first column to index 0
                    FormatCell(null, 0, Format);
                    //format the first row to index 0
                    FormatCell(0, null, Format);
                    //format the last column by the count of columns that can fit
                    FormatCell(null, ColumnDefinitions.Count - 1, Format);
                    //format the last row by the count of the rows that can fit
                    FormatCell(RowDefinitions.Count - 1, null, Format);
                    break;

                case (null, int):
                    RowDefinitions[x.Value].Height = new GridLength(1, Format);
                    FormatCell(0, null, Format);
                    FormatCell(RowDefinitions.Count - 1, null, Format);
                    break;

                case (int, null):
                    ColumnDefinitions[y.Value].Width = new GridLength(1, Format);
                    FormatCell(null, 0, Format);
                    FormatCell(null, ColumnDefinitions.Count - 1, Format);
                    break;

                //case (null, null):            
                default:
                    //assign a grid unit format property to the loaded cell
                    FormatCell(null, current_column - 1, Format);
                    FormatCell(current_row - 1, null, Format);
                    break;

            }
        }

        protected void AddView(View pInput, int pX, int pY, int pXSpan = 1, int pYSpan = 1)
        {
            pInput.Margin = 0;
            Children.Add(pInput, pX, pX + pXSpan, pY, pY + pYSpan);
        }

        protected async void DelayedRefreshVisibility()
        {
            //This will try to ensure the item is visible.
            //It reads whether the thing should be displayed, inverts that and then sets it back.

            await Task.Delay(100);

            Device.BeginInvokeOnMainThread(() =>
            {
                var count = Children.Count;
                if (count > 0)
                {
                    bool val = Children[count - 1].IsVisible;

                    Children[count - 1].IsVisible = !val;
                    Children[count - 1].IsVisible = val;
                }
            });
        }

        protected void MaximizeItem(View pItem)
        {
            if (RestoreList == null)
            {
                RestoreList = new List<ItemState>();

                //might be a bug, the compiler would determine the itemstate struct when it's called.
                //In the for loop, it would expect a method name and the access protection gave a warning.
                dynamic itemState = new ItemState();

                base.BatchBegin();
                foreach (var child in Children)
                {
                    var restoreitem = itemState(child);
                    RestoreList.Add(restoreitem);
                    if (!child.Equals(pItem))
                        restoreitem.SetVisibility(child, false);
                }

                Grid.SetRow(pItem, 0);
                Grid.SetColumn(pItem, 0);
                Grid.SetRowSpan(pItem, RowDefinitions.Count);
                Grid.SetColumnSpan(pItem, ColumnDefinitions.Count);
                base.BatchCommit();
                DelayedRefreshVisibility();
            }
        }

        protected Orientation CurrentOrientation()
        {
            base.OnSizeAllocated(WidthRequest, HeightRequest);
            width = WidthRequest;
            height = HeightRequest;

            if (width > height)
            {
                return Orientation.Landscape;
            }
            else
            {
                return Orientation.Portrait;
            }
        }
        public struct ItemState
        {
            //An ItemState is the model definition of a cell and its location.

            //set the properties
            View Item;
            int row, column, row_span, column_span;
            bool visibility;

            //bring in an item from the view and deconstruct it
            ItemState(View pItem)
            {
                Item = pItem;
                visibility = Item.IsVisible;
                row = Grid.GetRow(Item);
                column = Grid.GetColumn(Item);
                row_span = Grid.GetRowSpan(Item);
                column_span = Grid.GetColumnSpan(Item);
            }

            //set whether or not the item is visible in the view
            void SetVisibility(View pItem, bool state)
            {
                Type item_type = pItem.GetType();
                pItem.IsVisible = state;
            }

            //the restore method for loading the item back into the view
            public void Restore()
            {
                SetVisibility(Item, visibility);
                Grid.SetRow(Item, row);
                Grid.SetColumn(Item, column);
                Grid.SetRowSpan(Item, row_span);
                Grid.SetColumnSpan(Item, column_span);
            }
        }

    }
}