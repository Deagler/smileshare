using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plugin.Media;
using Plugin.Media.Abstractions;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Net.Http;
using System.Net.Http.Headers;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;


namespace SmileShare
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class UploadPage : ContentPage
    {
        private string currentLink = "haha";// image link for current image.
        public UploadPage()
        {
            InitializeComponent();
        }

        private async void takePhoto(object sender, EventArgs e)
        {
            await CrossMedia.Current.Initialize();

            if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported) // MVP-VisualStudio-Dev MSDN
            {
                await DisplayAlert("No Camera", "No Camera found", "OK");
                return;
            }

            var file = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
            {
                Directory = "SmileShare",
                PhotoSize = PhotoSize.Small,
                Name = $"{DateTime.UtcNow}.jpg",
            });

            if (file == null)
                return;


            selectedImage.Source = ImageSource.FromStream(() => file.GetStream());
            currentStatus.TextColor = new Color(0, 0, 0);
            currentStatus.Text = "Uploading Image...";
            statusIndicator.IsRunning = true;
            await UploadImage(file);

        }

        private async void selectPhoto(object sender, EventArgs e)
        {
            if (!CrossMedia.Current.IsPickPhotoSupported)
            {
                await DisplayAlert("Can't Access Gallery", "Unable to access gallery", "OK");
                return;
            }

            var file = await CrossMedia.Current.PickPhotoAsync(); // Find alternative for UWP, wont work by default prolly.

            if (file == null)
                return;

            selectedImage.Source = ImageSource.FromStream(() => file.GetStream());
            currentStatus.TextColor = new Color(0, 0, 0);
            currentStatus.Text = "Uploading Image...";
            statusIndicator.IsRunning = true;
            await UploadImage(file);


        }

        private async void postPhoto(object sender, EventArgs e)
        {

            if (titleEntry.Text == null || titleEntry.Text == "" || titleEntry.Text.Length > 50)
            {
                await DisplayAlert("Error!", "You must enter a caption < 50 char", "OK");
                return;
            }

            ImageInfo img = new ImageInfo()
            {
                Caption = titleEntry.Text,
                Link = currentLink,
                Likes = 1,
            };

            currentStatus.TextColor = new Color(0, 0, 0);
            currentStatus.Text = "Posting...";
            statusIndicator.IsRunning = true;
            postButton.IsVisible = false;
            await AzureManager.Instance.AddImage(img);
            currentStatus.TextColor = new Color(0, 255, 0);
            currentStatus.Text = "Posted successfully!";
            statusIndicator.IsRunning = false;

        }

        private void errorMsg(String msg) // should probably use dialog/snackbar or something for this 
        {
            currentStatus.Text = msg;
            currentStatus.TextColor = new Color(255, 0, 0, 255);
            statusIndicator.IsRunning = false;
            postButton.IsVisible = false;
        }

        static byte[] GetImageAsByteArray(MediaFile file)
        {
            var stream = file.GetStream();
            BinaryReader binaryReader = new BinaryReader(stream);
            return binaryReader.ReadBytes((int)stream.Length);
        }
        /*
        static string GetImageAsBase64(MediaFile file) // idk if Convert.ToBase64(GetImageAsByteArray) is more efficient
        {
            var stream = file.GetStream();
            var bytes = new byte[stream.Length];
            stream.Read(bytes, 0, (int)stream.Length);
            return(Convert.ToBase64String(bytes));
            
            
        }
        */


        async Task UploadImage(MediaFile file) /*Might as well only upload once and just reuse link for emotion API*/
        {
            postButton.IsVisible = false;
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", "Client-ID 84ee824b617875f");
                string url = "https://api.imgur.com/3/image?";


                byte[] byteData = GetImageAsByteArray(file);

                var content = new ByteArrayContent(byteData);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");


                var resp = await client.PostAsync(url, content);
                if (!resp.IsSuccessStatusCode)
                {
                    errorMsg("Error Occurred uploading!");
                    return;
                }

                string jsonData = await resp.Content.ReadAsStringAsync();
                if (jsonData == null || jsonData == "[]")
                {
                    errorMsg("Error occurred uploading!");
                    return;
                }

                JObject data = JObject.Parse(jsonData);
                if (data["data"] == null || data["data"]["link"] == null)
                {
                    errorMsg("Failed to upload.");
                    return;
                }
                currentLink = (string)data["data"]["link"];

                await RequestEmotions(currentLink);

                file.Dispose();
            }




        }

        async Task RequestEmotions(string imageurl)
        {
            using (HttpClient client = new HttpClient())
            {



                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "59d170045aec4d7f8258d04c6be83c58");

                string url = "https://westus.api.cognitive.microsoft.com/emotion/v1.0/recognize?";
                HttpResponseMessage response;
                dynamic obj = new JObject();
                obj.url = imageurl;

                var content = new StringContent(obj.ToString(Formatting.None), Encoding.UTF8, "application/json");

                currentStatus.Text = "Analysing Emotions...";
                response = await client.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    if (responseContent == null || responseContent == "[]")
                    {
                        errorMsg("No face detected!");
                        return;
                    }

                    JArray faces = JArray.Parse((string)responseContent);

                    if (faces == null)
                    {
                        errorMsg("Error Occured!");
                        return;
                    }

                    foreach (var item in faces)
                    {
                        if (float.Parse((string)item["scores"]["happiness"]) < 0.5)
                        {
                            errorMsg("We found an unhappy person in this image :(");
                            return;
                        }
                    }

                    currentStatus.Text = "Ready to Post!";
                    currentStatus.TextColor = new Color(0, 255, 0);
                    statusIndicator.IsRunning = false;
                    postButton.IsVisible = true;


                }
            }

        }
    }
}
