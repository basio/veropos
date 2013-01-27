﻿using System.Windows.Controls;
using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.Events;
using Samba.Presentation.Common;

namespace Samba.Modules.TicketModule
{
    /// <summary>
    /// Interaction logic for TicketEditorView.xaml
    /// </summary>
    /// 
    [Export]
    public partial class TicketEditorView : UserControl
    {
        [ImportingConstructor]
        public TicketEditorView(TicketEditorViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            EventServiceFactory.EventService.GetEvent<GenericEvent<TicketEditorViewModel>>().Subscribe(
                x =>
                {
                    if (x.Topic == EventTopicNames.FocusTicketScreen)
                        MainTabControl.BackgroundFocus();
                });
        }

        private void UserControl_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = ((TicketEditorViewModel)DataContext).HandleTextInput(e.Text);
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            MainTabControl.BackgroundFocus();
        }
    }
}
