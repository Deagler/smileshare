using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SmileShare
{
    public class ImageInfo
    {
        [JsonProperty(PropertyName = "Id")]
        public string ID { get; set; }

        [JsonProperty(PropertyName = "createdAt")]
        public string createdAt { get; set; }

        [JsonProperty(PropertyName = "Caption")]
        public string Caption { get; set; }

        [JsonProperty(PropertyName = "Link")]
        public string Link { get; set; }

        [JsonProperty(PropertyName = "Likes")]
        public int Likes { get; set; }
    }
}
