using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ML.Web.Models
{
    public class EvaluationStreamItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Stream { get; set; }
        public string Code { get; set; }
        public bool IsActive { get; set; }
    }
}
