using API.Models;
using API.Models.DTOs;
using API.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Требует валидный JWT токен
public class ReferenceController : ControllerBase
{
    private readonly IProductRepository _productRepository;
    private readonly IRawMaterialRepository _rawMaterialRepository;
    private readonly IEquipmentRepository _equipmentRepository;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IRoleRepository _roleRepository;

    public ReferenceController(
        IProductRepository productRepository,
        IRawMaterialRepository rawMaterialRepository,
        IEquipmentRepository equipmentRepository,
        IDepartmentRepository departmentRepository,
        IRoleRepository roleRepository)
    {
        _productRepository = productRepository;
        _rawMaterialRepository = rawMaterialRepository;
        _equipmentRepository = equipmentRepository;
        _departmentRepository = departmentRepository;
        _roleRepository = roleRepository;
    }

    /// <summary>
    /// Получить список всех продуктов (готовая продукция)
    /// </summary>
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

    /// <summary>
    /// Получить список сырья и компонентов
    /// </summary>
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

    /// <summary>
    /// Получить список оборудования
    /// </summary>
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

    /// <summary>
    /// Получить список подразделений
    /// </summary>
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

    /// <summary>
    /// Получить список ролей пользователей
    /// </summary>
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
}