using System;
using System.Collections.Generic;
using System.Text;

namespace прогОпер.Models
{
    public class BatchDetailsDto
    {
        public int Id { get; set; }
        public string BatchNumber { get; set; } = string.Empty;
        public int TechCardId { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? StartTime { get; set; }
        public List<StepExecutionDto> Steps { get; set; } = new();
    }
}
