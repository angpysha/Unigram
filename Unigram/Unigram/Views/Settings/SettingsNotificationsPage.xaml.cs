﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Unigram.Views;
using Unigram.ViewModels.Settings;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Unigram.Views.Settings
{
    public sealed partial class SettingsNotificationsPage : Page
    {
        public SettingsNotificationsViewModel ViewModel => DataContext as SettingsNotificationsViewModel;

        public SettingsNotificationsPage()
        {
            InitializeComponent();
            DataContext = TLContainer.Current.Resolve<SettingsNotificationsViewModel>();
        }

        private async void Private_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.PrivateAlert = PrivateAlert.IsChecked == true;
            ViewModel.PrivatePreview = PrivatePreview.IsChecked == true;

            await ViewModel.UpdatePrivateAsync();
        }

        private async void Group_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.GroupAlert = GroupAlert.IsChecked == true;
            ViewModel.GroupPreview = GroupPreview.IsChecked == true;

            await ViewModel.UpdateGroupAsync();
        }

        #region Binding

        private string ConvertCountInfo(bool count)
        {
            return count ? "Switch off to show the number of unread chats instead of messages" : "Switch on to show the number of unread messages instead of chats";
        }

        #endregion

    }
}
