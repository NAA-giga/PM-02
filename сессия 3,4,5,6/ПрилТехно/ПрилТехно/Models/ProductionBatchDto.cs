using System;
using System.Text.Json.Serialization;

namespace ПрилТехно.Models
{
    public class ProductionBatchDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("batchNumber")]
        public string BatchNumber { get; set; } = string.Empty;

        [JsonPropertyName("productId")]
        public int ProductId { get; set; }

        [JsonPropertyName("productName")]
        public string ProductName { get; set; } = string.Empty;

        [JsonPropertyName("recipeId")]
        public int RecipeId { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("plannedQuantityKg")]
        public decimal PlannedQuantityKg { get; set; }

        [JsonPropertyName("actualQuantityKg")]
        public decimal? ActualQuantityKg { get; set; }

        [JsonPropertyName("startTime")]
        public DateTime? StartTime { get; set; }

        [JsonPropertyName("labDecision")]
        public string? LabDecision { get; set; }

        [JsonPropertyName("labDecisionDate")]
        public DateTime? LabDecisionDate { get; set; }

        [JsonPropertyName("labDecisionReason")]
        public string? LabDecisionReason { get; set; }

        [JsonPropertyName("labDecisionBy")]
        public string? LabDecisionBy { get; set; }
    }
}