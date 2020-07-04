using System;
using System.Collections.Generic;
using System.Text;

namespace ML.Domain.ReturnModels
{
    public class EvaluationStreamModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Stream { get; set; }
        public string Code { get; set; }
        public bool IsActive { get; set; }
    }
}
