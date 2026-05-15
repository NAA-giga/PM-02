using Dapper;
using API.Models.Entities;
using API.Repositories.Interfaces;

namespace API.Repositories
{
    public class ExtruderTelemetryRepository : BaseRepository, IExtruderTelemetryRepository
    {
        public ExtruderTelemetryRepository(IConfiguration configuration) : base(configuration) { }

        public async Task<IEnumerable<ExtruderTelemetry>> GetByBatchIdAsync(int batchId)
        {
            using var conn = CreateConnection();
            const string sql = "SELECT * FROM extruder_telemetry WHERE production_batch_id = @BatchId ORDER BY recorded_at";
            return await conn.QueryAsync<ExtruderTelemetry>(sql, new { BatchId = batchId });
        }

        public async Task<bool> AddTelemetryAsync(ExtruderTelemetry telemetry)
        {
            using var conn = CreateConnection();
            const string sql = @"
                INSERT INTO extruder_telemetry (production_batch_id, zone_number, temperature_c, pressure_bar, screw_speed_rpm, recorded_at)
                VALUES (@ProductionBatchId, @ZoneNumber, @TemperatureC, @PressureBar, @ScrewSpeedRpm, @RecordedAt)";
            var rows = await conn.ExecuteAsync(sql, telemetry);
            return rows > 0;
        }

        public async Task<bool> AddBatchAsync(IEnumerable<ExtruderTelemetry> telemetryList)
        {
            using var conn = CreateConnection();
            const string sql = @"
                INSERT INTO extruder_telemetry (production_batch_id, zone_number, temperature_c, pressure_bar, screw_speed_rpm, recorded_at)
                VALUES (@ProductionBatchId, @ZoneNumber, @TemperatureC, @PressureBar, @ScrewSpeedRpm, @RecordedAt)";
            var rows = await conn.ExecuteAsync(sql, telemetryList);
            return rows > 0;
        }
    }
}