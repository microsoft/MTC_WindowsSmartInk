/*
 * Copyright(c) Microsoft Corporation
 * All rights reserved.
 *
 * MIT License
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the ""Software""), to deal
 * in the Software without restriction, including without limitation the rights to
 * use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
 * the Software, and to permit persons to whom the Software is furnished to do so,
 * subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED *AS IS*, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
 * FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE AUTHORS OR
 * COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN
 * AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
 * WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System.Collections;
using System.Collections.Generic;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace SmartInkLaboratory.Views.Controls
{
    public class SelectionListItem : Control
    {
        public object Content
        {
            get { return (string)GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ContentProperty =
            DependencyProperty.Register("Content", typeof(object), typeof(SelectionListItem), new PropertyMetadata(null));

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Command.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(ICommand), typeof(SelectionListItem), new PropertyMetadata(null));

        public SelectionListItem()
        {
            this.DefaultStyleKey = typeof(SelectionListItem);
        }

        public override string ToString()
        {
            return this.Content.ToString();
        }
    }

    [TemplatePart(Name = "PART_Menu", Type = typeof(Flyout))]
    [TemplatePart(Name = "PART_MainGrid", Type = typeof(Grid))]
    [TemplatePart(Name = "PART_CommandList", Type = typeof(ListBox))]
    [TemplatePart(Name = "PART_DisplayValue", Type = typeof(TextBlock))]
    public sealed class SelectionList : ContentControl
    {
        Flyout _flyout;
        private bool _isInitialized;
        Grid _mainGrid;
        ListBox _commandList;
        TextBlock _displayValue;

        public string Label
        {
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Label.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register("Label", typeof(string), typeof(SelectionList), new PropertyMetadata(null));

        public int SelectedIndex
        {
            get { return (int)GetValue(SelectedIndexProperty); }
            set { SetValue(SelectedIndexProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedIndex.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedIndexProperty =
            DependencyProperty.Register("SelectedIndex", typeof(int), typeof(SelectionList), new PropertyMetadata(-1, new PropertyChangedCallback(OnSelectedIndexChange)));

        private static void OnSelectedIndexChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var me = d as SelectionList;
            if (me?._commandList != null)
                me._commandList.SelectedIndex = (int)e.NewValue;
        }


        public object SelectedItem
        {
            get { return (object)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedItem.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register("SelectedItem", typeof(object), typeof(SelectionList), new PropertyMetadata(null));


        public List<ListViewItem> Items
        {
            get { return (List<ListViewItem>)GetValue(ItemsProperty); }
            set { SetValue(ItemsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Items.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemsProperty =
            DependencyProperty.Register("Items", typeof(List<ListViewItem>), typeof(SelectionList), new PropertyMetadata(null));


        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ItemsSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(SelectionList), new PropertyMetadata(null, new PropertyChangedCallback(OnItemsSourceChange)));

        private static void OnItemsSourceChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
          
        }

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Command.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(ICommand), typeof(SelectionList), new PropertyMetadata(null));

        public object CommandParameter
        {
            get { return (object)GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CommandParameter.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register("CommandParameter", typeof(object), typeof(SelectionList), new PropertyMetadata(null));


        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate)GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for dataTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemTemplateProperty =
            DependencyProperty.Register("ItemTemplate", typeof(DataTemplate), typeof(SelectionList), new PropertyMetadata(null));

        public SelectionList()
        {
            this.DefaultStyleKey = typeof(SelectionList);
            this.Items = new List<ListViewItem>();
        }

        protected override void OnApplyTemplate()
        {
            _flyout = this.GetTemplateChild("PART_Menu") as Flyout;
            _commandList = this.GetTemplateChild("PART_CommandList") as ListBox;
            _commandList.SelectionChanged += _commandList_SelectionChanged;
            _displayValue = this.GetTemplateChild("PART_DisplayValue") as TextBlock;

            var panel = _commandList.ItemsPanel;

            if (_commandList.Items.Count > 0)
                _commandList.SelectedIndex = 0;

            _isInitialized = true;

            _mainGrid = this.GetTemplateChild("PART_MainGrid") as Grid;
            _mainGrid.Tapped += (s, e) =>
            {
                var sender = s as FrameworkElement;
                FlyoutBase.ShowAttachedFlyout(sender);
            };
        }

        private void _commandList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string output = e.AddedItems[0] is ListViewItem ? ((ListViewItem)e.AddedItems[0]).Content.ToString() : e.AddedItems[0].ToString();
            var listItem = e.AddedItems[0]; 
         
            this.SelectedIndex = _commandList.Items.IndexOf(e.AddedItems[0]);
            this.SelectedItem = e.AddedItems[0];

            if (this.Command != null)
            {
                if (this.CommandParameter != null && this.Command.CanExecute(this.CommandParameter))
                    this.Command.Execute(this.CommandParameter);
                else
                    this.Command.Execute(null);
            }


            _flyout.Hide();
        }


    }
}
