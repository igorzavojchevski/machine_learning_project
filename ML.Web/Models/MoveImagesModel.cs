using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ML.Web.Models
{
    public class MoveImagesModel
    {
        public string NewClassNameId { get; set; }
        public List<string> ImagesIds { get; set; }
    }
}
