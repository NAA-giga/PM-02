// Program.cs

using API.Repositories;
using API.Repositories.Interfaces;
using API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// 1. Настройка JWT аутентификации
// ============================================================
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["Secret"];
if (string.IsNullOrEmpty(secretKey))
    throw new InvalidOperationException("JWT Secret is missing in appsettings.json");

var key = Encoding.UTF8.GetBytes(secretKey);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSettings["Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// ============================================================
// 2. Регистрация репозиториев
// ============================================================
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITokenService, TokenService>();

builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IRawMaterialRepository, RawMaterialRepository>();
builder.Services.AddScoped<IEquipmentRepository, EquipmentRepository>();
builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IRecipeRepository, RecipeRepository>();
builder.Services.AddScoped<ITechCardRepository, TechCardRepository>();
builder.Services.AddScoped<IProductionOrderRepository, ProductionOrderRepository>();
builder.Services.AddScoped<IProductionBatchRepository, ProductionBatchRepository>();
builder.Services.AddScoped<IBatchStepExecutionRepository, BatchStepExecutionRepository>();
builder.Services.AddScoped<IQualityTestRepository, QualityTestRepository>();
builder.Services.AddScoped<IDeviationRepository, DeviationRepository>();
builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddScoped<ITechCardRepository, TechCardRepository>();
builder.Services.AddScoped<IProductionOrderRepository, ProductionOrderRepository>();
builder.Services.AddScoped<IProductionBatchRepository, ProductionBatchRepository>();
builder.Services.AddScoped<IBatchStepExecutionRepository, BatchStepExecutionRepository>();
builder.Services.AddScoped<IQualityTestRepository, QualityTestRepository>();
builder.Services.AddScoped<IDeviationRepository, DeviationRepository>();
builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddScoped<IRawMaterialRepository, RawMaterialRepository>();
builder.Services.AddScoped<IExtruderProgramRepository, ExtruderProgramRepository>();

// ============================================================
// 3. Регистрация сервисов
// ============================================================
builder.Services.AddScoped<ITokenService, TokenService>();

// ============================================================
// 4. Добавление контроллеров и Swagger
// ============================================================
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Manufacturing API",
        Version = "v1",
        Description = "API для управления производством средств защиты растений"
    });

    // Настройка авторизации через JWT в Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Введите токен в формате: Bearer {token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
});

// ============================================================
// 5. Настройка CORS (для разработки)
// ============================================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// ============================================================
// 6. Настройка конвейера обработки запросов
// ============================================================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();