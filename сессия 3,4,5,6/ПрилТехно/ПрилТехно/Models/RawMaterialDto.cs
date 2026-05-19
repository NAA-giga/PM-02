namespace ПрилТехно.Models
{
    public class RawMaterialDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Category { get; set; }
        public string UnitOfMeasure { get; set; } = "kg";
        public bool IsActive { get; set; }
    }
}