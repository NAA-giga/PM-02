using System;
using System.Collections.Generic;
using System.Text;

namespace прогОпер.Models
{
    public class ActiveBatchDto
    {
        public int Id { get; set; }
        public string BatchNumber { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string LineNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int? CurrentStepOrder { get; set; }
        public string? CurrentStepName { get; set; }
        public int? CurrentStepId { get; set; }
    }
}
