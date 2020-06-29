using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ML.Web.Models
{
    public class CustomImageDetails
    {
        [FromForm(Name = "labelID")]
        public string LabelID { get; set; }
        
        [FromForm(Name = "image")]
        public IFormFile ImageFile { get; set; }
    }
}
