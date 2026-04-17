
namespace AritmaEnvanter.Models.DTOs
{
    public class FormDefinitionDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public int? ParentId { get; set; } 
        public List<FieldDto> Fields { get; set; } = new();
        public int? KategoriId { get; set; }
    }

    public class FieldDto
    {
        public int Id { get; set; }
        public string Label { get; set; } = "";
        public int FieldType { get; set; }
        public bool IsRequired { get; set; }
        public int DisplayOrder { get; set; }
        public string? Placeholder { get; set; }
        public List<FieldOptionDto> FieldOptions { get; set; } = new();
    }

    public class FieldOptionDto
    {
        public int Id { get; set; }
        public string Value { get; set; } = "";
        public int DisplayOrder { get; set; }
    }
}
