using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SmileShare
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SmilesPage : ContentPage
    {
        public SmilesPage()
        {
            InitializeComponent();
            Refresh();
            smileList.ItemSelected += (sender, e) => {
                    ((ListView)sender).SelectedItem = null;
            };
            smileList.Refreshing += (sender, e) => {
                Refresh();
            };
        }

        async void Refresh()
        {
            smileList.IsRefreshing = true;
            List<ImageInfo> imageList = await AzureManager.Instance.GetImages();
            imageList.Reverse();
            smileList.ItemsSource = imageList;
            smileList.IsRefreshing = false;
        }
    }
}
