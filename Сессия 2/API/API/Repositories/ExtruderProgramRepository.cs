using Dapper;
using System.Text.Json;
using API.Models.DTOs;
using API.Models.Entities;
using API.Repositories.Interfaces;

namespace API.Repositories
{
    public class ExtruderProgramRepository : BaseRepository, IExtruderProgramRepository
    {
        public ExtruderProgramRepository(IConfiguration configuration) : base(configuration) { }

        public async Task<IEnumerable<ExtruderProgram>> GetAllAsync()
        {
            using var conn = CreateConnection();
            const string sql = "SELECT * FROM extruder_programs ORDER BY created_at DESC";
            return await conn.QueryAsync<ExtruderProgram>(sql);
        }

        public async Task<ExtruderProgram?> GetByIdAsync(int id)
        {
            using var conn = CreateConnection();
            const string sql = "SELECT * FROM extruder_programs WHERE id = @Id";
            return await conn.QueryFirstOrDefaultAsync<ExtruderProgram>(sql, new { Id = id });
        }

        public async Task<int> CreateAsync(CreateExtruderProgramDto dto, int userId)
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

        public async Task<bool> UpdateAsync(int id, CreateExtruderProgramDto dto, int userId)
        {
            using var conn = CreateConnection();
            var zoneParamsJson = JsonSerializer.Serialize(dto.ZoneParameters);
            const string sql = @"
                UPDATE extruder_programs 
                SET name = @Name, version = @Version, production_batch_id = @ProductionBatchId,
                    zone_params = @ZoneParams, status = @Status
                WHERE id = @Id";
            var rows = await conn.ExecuteAsync(sql, new
            {
                Id = id,
                dto.Name,
                dto.Version,
                dto.ProductionBatchId,
                ZoneParams = zoneParamsJson,
                dto.Status
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

        public async Task<bool> AssignToBatchAsync(int programId, int batchId)
        {
            using var conn = CreateConnection();
            const string sql = "UPDATE extruder_programs SET production_batch_id = @BatchId WHERE id = @Id";
            var rows = await conn.ExecuteAsync(sql, new { Id = programId, BatchId = batchId });
            return rows > 0;
        }
    }
}