using System;
using System.Collections.Generic;
using System.Text;

namespace ML.Domain.RequestModels
{
    public class AdPointerCommercialBlockModel
    {
        public string AdId { get; set; }
        public string ChannelId { get; set; }
        public DateTime AdEventTime { get; set; }
        public DateTime AdStartEventTime { get; set; }
        public DateTime AdEndEventTime { get; set; }
        public string VideoUrl { get; set; }
    }
}
