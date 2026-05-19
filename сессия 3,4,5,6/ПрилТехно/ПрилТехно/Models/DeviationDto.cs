using System;
using System.Collections.Generic;
using System.Text;

namespace ПрилТехно.Models
{
    public class DeviationDto
    {
        public int Id { get; set; }
        public string BatchNumber { get; set; } = string.Empty;
        public string? StepName { get; set; }
        public string DeviationType { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string? ParameterName { get; set; }
        public string? PlannedValue { get; set; }
        public string? ActualValue { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
