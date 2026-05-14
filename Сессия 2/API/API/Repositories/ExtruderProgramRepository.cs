using Dapper;
using API.Models.Entities;
using API.Models.DTOs;
using API.Repositories.Interfaces;
using System.Text.Json;
using System.Data;

namespace API.Repositories
{
    public class ExtruderProgramRepository : BaseRepository, IExtruderProgramRepository
    {
        public ExtruderProgramRepository(IConfiguration configuration) : base(configuration) { }

        public async Task<IEnumerable<ExtruderProgram>> GetAllAsync()
        {
            using var conn = CreateConnection();
            const string sql = "SELECT * FROM extruder_programs ORDER BY created_at DESC";
            var programs = await conn.QueryAsync<ExtruderProgram>(sql);
            return programs;
        }

        public async Task<ExtruderProgram?> GetByIdAsync(int id)
        {
            using var conn = CreateConnection();
            const string sql = "SELECT * FROM extruder_programs WHERE id = @Id";
            return await conn.QueryFirstOrDefaultAsync<ExtruderProgram>(sql, new { Id = id });
        }

        public async Task<int> CreateAsync(ExtruderProgramDto dto, int userId)
        {
            using var conn = CreateConnection();
            var zoneParamsJson = JsonSerializer.Serialize(dto.ZoneParameters);
            const string sql = @"
            INSERT INTO extruder_programs (name, version, production_batch_id, zone_params, status, created_by, created_at)
            VALUES (@Name, @Version, @ProductionBatchId, @ZoneParams, @Status, @UserId, GETDATE());
            SELECT CAST(SCOPE_IDENTITY() AS INT)";
            return await conn.ExecuteScalarAsync<int>(sql, new
            {
                dto.Name,
                dto.Version,
                dto.ProductionBatchId,
                ZoneParams = zoneParamsJson,
                dto.Status,
                UserId = userId
            });
        }

        public async Task<bool> UpdateAsync(int id, ExtruderProgramDto dto, int userId)
        {
            using var conn = CreateConnection();
            var zoneParamsJson = JsonSerializer.Serialize(dto.ZoneParameters);
            const string sql = @"
            UPDATE extruder_programs 
            SET name = @Name,
                version = @Version,
                production_batch_id = @ProductionBatchId,
                zone_params = @ZoneParams,
                status = @Status,
                created_by = @UserId
            WHERE id = @Id";
            var rows = await conn.ExecuteAsync(sql, new
            {
                Id = id,
                dto.Name,
                dto.Version,
                dto.ProductionBatchId,
                ZoneParams = zoneParamsJson,
                dto.Status,
                UserId = userId
            });
            return rows > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var conn = CreateConnection();
            const string sql = "DELETE FROM extruder_programs WHERE id = @Id";
            var rows = await conn.ExecuteAsync(sql, new { Id = id });
            return rows > 0;
        }
    }
}
