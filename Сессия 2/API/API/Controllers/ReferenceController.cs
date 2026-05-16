using API.Models;
using API.Models.DTOs;
using API.Models.Entities;
using API.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReferenceController : ControllerBase
{
    private readonly IProductRepository _productRepository;
    private readonly IRawMaterialRepository _rawMaterialRepository;
    private readonly IEquipmentRepository _equipmentRepository;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly ITechCardRepository _techCardRepository;

    public ReferenceController(
        IProductRepository productRepository,
        IRawMaterialRepository rawMaterialRepository,
        IEquipmentRepository equipmentRepository,
        IDepartmentRepository departmentRepository,
        IRoleRepository roleRepository,
        ITechCardRepository techCardRepository)
    {
        _productRepository = productRepository;
        _rawMaterialRepository = rawMaterialRepository;
        _equipmentRepository = equipmentRepository;
        _departmentRepository = departmentRepository;
        _roleRepository = roleRepository;
        _techCardRepository = techCardRepository;
    }

    [HttpGet("products")]
    public async Task<IActionResult> GetProducts()
    {
        var products = await _productRepository.GetAllAsync();
        var dtos = products.Select(p => new ProductDto
        {
            Id = p.Id,
            Code = p.Code,
            Name = p.Name,
            ProductType = p.ProductType,
            FormType = p.FormType,
            Status = p.Status
        });
        return Ok(new ApiResponse<IEnumerable<ProductDto>> { IsSuccess = true, Data = dtos });
    }

    [HttpGet("materials")]
    public async Task<IActionResult> GetMaterials()
    {
        var materials = await _rawMaterialRepository.GetAllAsync();
        var dtos = materials.Select(m => new RawMaterialDto
        {
            Id = m.Id,
            Code = m.Code,
            Name = m.Name,
            Category = m.Category,
            UnitOfMeasure = m.UnitOfMeasure,
            IsActive = m.IsActive
        });
        return Ok(new ApiResponse<IEnumerable<RawMaterialDto>> { IsSuccess = true, Data = dtos });
    }

    [HttpGet("equipment")]
    public async Task<IActionResult> GetEquipment()
    {
        var equipment = await _equipmentRepository.GetAllAsync();
        var dtos = equipment.Select(e => new EquipmentDto
        {
            Id = e.Id,
            Code = e.Code,
            Name = e.Name,
            EquipmentType = e.EquipmentType,
            LineNumber = e.LineNumber,
            IsActive = e.IsActive
        });
        return Ok(new ApiResponse<IEnumerable<EquipmentDto>> { IsSuccess = true, Data = dtos });
    }

    [HttpGet("departments")]
    public async Task<IActionResult> GetDepartments()
    {
        var departments = await _departmentRepository.GetAllAsync();
        var dtos = departments.Select(d => new DepartmentDto
        {
            Id = d.Id,
            Name = d.Name,
            Description = d.Description
        });
        return Ok(new ApiResponse<IEnumerable<DepartmentDto>> { IsSuccess = true, Data = dtos });
    }

    [HttpGet("roles")]
    public async Task<IActionResult> GetRoles()
    {
        var roles = await _roleRepository.GetAllAsync();
        var dtos = roles.Select(r => new RoleDto
        {
            Id = r.Id,
            Name = r.Name,
            Description = r.Description
        });
        return Ok(new ApiResponse<IEnumerable<RoleDto>> { IsSuccess = true, Data = dtos });
    }

    [HttpGet("techcards/list")]
    public async Task<IActionResult> GetTechCardsList()
    {
        var cards = await _techCardRepository.GetAllAsync();
        var dtos = cards.Select(c => new TechCardListItemDto
        {
            Id = c.Id,
            Name = c.Name,
            Version = c.Version,
            ProductName = c.ProductId.ToString(), // можно подтянуть имя через JOIN
            Status = c.Status,
            CreatedAt = c.CreatedAt
        });
        return Ok(new ApiResponse<IEnumerable<TechCardListItemDto>> { IsSuccess = true, Data = dtos });
    }
    [HttpPost("products")]
    [Authorize(Roles = "Technologist,admin")]
    public async Task<IActionResult> CreateProduct([FromBody] ProductDto productDto)
    {
        if (productDto == null || string.IsNullOrWhiteSpace(productDto.Code) || string.IsNullOrWhiteSpace(productDto.Name))
            return BadRequest(new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Код и наименование обязательны" });

        var product = new Product
        {
            Code = productDto.Code,
            Name = productDto.Name,
            ProductType = productDto.ProductType,
            FormType = productDto.FormType,
            Status = "active"
        };
        var id = await _productRepository.CreateAsync(product);
        return Ok(new ApiResponse<object> { IsSuccess = true, Data = new { Id = id } });
    }

    [HttpPut("products/{id}")]
    [Authorize(Roles = "Technologist,admin")]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] ProductDto productDto)
    {
        if (productDto == null || id != productDto.Id)
            return BadRequest(new ApiResponse<object> { IsSuccess = false, ErrorMessage = "ID в запросе и теле не совпадают" });

        var existing = await _productRepository.GetByIdAsync(id);
        if (existing == null)
            return NotFound(new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Продукт не найден" });

        existing.Code = productDto.Code;
        existing.Name = productDto.Name;
        existing.ProductType = productDto.ProductType;
        existing.FormType = productDto.FormType;
        existing.Status = productDto.Status; // "active" или "archived"
        var success = await _productRepository.UpdateAsync(existing);
        if (!success)
            return StatusCode(500, new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Ошибка обновления" });

        return Ok(new ApiResponse<object> { IsSuccess = true });
    }

    [HttpDelete("products/{id}")]
    [Authorize(Roles = "Technologist,admin")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var success = await _productRepository.ArchiveAsync(id);
        if (!success)
            return NotFound(new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Продукт не найден" });
        return Ok(new ApiResponse<object> { IsSuccess = true });
    }
}