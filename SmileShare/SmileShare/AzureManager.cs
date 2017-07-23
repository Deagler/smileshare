using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmileShare
{
    public sealed class AzureManager // thread safe singleton
    {
        private static AzureManager instance = null;
        private static readonly object padlock = new object();

        private MobileServiceClient client = null;
        private IMobileServiceTable<ImageInfo> imageInfoTable;



        private AzureManager()

        {

            this.client = new MobileServiceClient("https://msatodolist.azurewebsites.net");

            this.imageInfoTable = this.client.GetTable<ImageInfo>();
        }

        public static AzureManager Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new AzureManager();
                    }

                    return (instance);
                }
            }
        }

        public async Task<List<ImageInfo>> GetImages()
        {
            return (await this.imageInfoTable.ToListAsync());
        }

        public async Task AddImage(ImageInfo img)
        {
            await this.imageInfoTable.InsertAsync(img);
        }


    }
}
