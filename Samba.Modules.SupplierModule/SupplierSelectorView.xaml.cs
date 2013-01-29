using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Samba.Presentation.Common;

namespace Samba.Modules.SupplierModule
{
    /// <summary>
    /// Interaction logic for SupplierSelectorView.xaml
    /// </summary>

    [Export]
    public partial class SupplierSelectorView : UserControl
    {
        readonly DependencyPropertyDescriptor _selectedIndexChange = DependencyPropertyDescriptor.FromProperty(Selector.SelectedIndexProperty, typeof(TabControl));

        [ImportingConstructor]
        public SupplierSelectorView(SupplierSelectorViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            _selectedIndexChange.AddValueChanged(MainTabControl, MyTabControlSelectedIndexChanged);
        }

        private void MyTabControlSelectedIndexChanged(object sender, EventArgs e)
        {
            if (((TabControl)sender).SelectedIndex == 1)
                PhoneNumberTextBox.BackgroundFocus();
        }

        private void FlexButton_Click(object sender, RoutedEventArgs e)
        {
            Reset();
        }

        private void Reset()
        {
           ((SupplierSelectorViewModel)DataContext).RefreshSelectedSupplier();
            PhoneNumber.BackgroundFocus();
        }

        private void HandleKeyDown(KeyEventArgs e)
        {
            
        }

        private void PhoneNumber_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            HandleKeyDown(e);
        }

        private void SupplierName_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            HandleKeyDown(e);
        }

        private void Address_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            HandleKeyDown(e);
        }

        private void PhoneNumberTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            HandleKeyDown(e);
        }

      
        private void PhoneNumber_Loaded(object sender, RoutedEventArgs e)
        {
            PhoneNumber.BackgroundFocus();
        }
    }
}
