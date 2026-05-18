using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using ПрилТехно.Services;
using ПрилТехно.Models;
namespace ПрилТехно.Repositories
{
    public class ReportRepository : IReportRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public ReportRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public List<OrdersReportRow> GetOrdersReport(DateTime start, DateTime end)
        {
            const string sql = @"
                SELECT 
                    po.order_number AS НомерЗаказа,
                    p.name AS Продукт,
                    po.planned_quantity_kg AS ПланКг,
                    po.status AS Статус,
                    po.planned_start_date AS ПлановаяДата,
                    po.actual_start_date AS ФактНачало,
                    po.actual_end_date AS ФактКонец
                FROM production_orders po
                LEFT JOIN products p ON po.product_id = p.id
                WHERE po.planned_start_date >= @Start AND po.planned_start_date <= @End
                ORDER BY po.planned_start_date";

            using var conn = _connectionFactory.CreateConnection();
            return conn.Query<OrdersReportRow>(sql, new { Start = start, End = end }).ToList();
        }

        public List<BatchesReportRow> GetBatchesReport(DateTime start, DateTime end)
        {
            const string sql = @"
                SELECT 
                    pb.batch_number AS НомерПартии,
                    p.name AS Продукт,
                    pb.start_time AS ДатаЗапуска,
                    pb.status AS Статус,
                    pb.planned_quantity_kg AS ПланКг,
                    ISNULL(pb.actual_quantity_kg, 0) AS ФактКг,
                    pb.lab_decision AS ЛабРешение
                FROM production_batches pb
                LEFT JOIN products p ON pb.product_id = p.id
                WHERE pb.start_time >= @Start AND pb.start_time <= @End
                ORDER BY pb.start_time";

            using var conn = _connectionFactory.CreateConnection();
            return conn.Query<BatchesReportRow>(sql, new { Start = start, End = end }).ToList();
        }

        public List<DeviationsReportRow> GetDeviationsReport(DateTime start, DateTime end)
        {
            const string sql = @"
                SELECT 
                    pb.batch_number AS Партия,
                    bse.step_name AS Шаг,
                    d.parameter_name AS Параметр,
                    d.planned_value AS План,
                    d.actual_value AS Факт,
                    d.deviation_type AS Тип,
                    d.severity AS Серьёзность,
                    d.created_at AS Дата
                FROM deviations d
                JOIN production_batches pb ON d.production_batch_id = pb.id
                LEFT JOIN batch_step_execution bse ON d.step_execution_id = bse.id
                WHERE d.created_at >= @Start AND d.created_at <= @End
                ORDER BY d.created_at";

            using var conn = _connectionFactory.CreateConnection();
            return conn.Query<DeviationsReportRow>(sql, new { Start = start, End = end }).ToList();
        }

        public List<RecipeUsageReportRow> GetRecipeUsageReport()
        {
            const string sql = @"
                SELECT 
                    p.name AS Продукт,
                    r.name AS Рецептура,
                    r.version AS Версия,
                    COUNT(pb.id) AS КолвоПартий,
                    ISNULL(SUM(pb.actual_quantity_kg), 0) AS ОбъемКг
                FROM recipes r
                JOIN products p ON r.product_id = p.id
                LEFT JOIN production_batches pb ON pb.recipe_id = r.id AND pb.status IN ('completed','quality_control')
                GROUP BY p.name, r.name, r.version
                ORDER BY p.name";

            using var conn = _connectionFactory.CreateConnection();
            return conn.Query<RecipeUsageReportRow>(sql).ToList();
        }

        public List<ExtruderReportRow> GetExtruderReport(DateTime start, DateTime end)
        {
            const string sql = @"
                SELECT 
                    pb.batch_number AS Партия,
                    et.zone_number AS Зона,
                    et.temperature_c AS Температура,
                    et.pressure_bar AS Давление,
                    et.screw_speed_rpm AS Скорость,
                    et.recorded_at AS Время
                FROM extruder_telemetry et
                JOIN production_batches pb ON et.production_batch_id = pb.id
                WHERE et.recorded_at >= @Start AND et.recorded_at <= @End
                ORDER BY et.recorded_at";

            using var conn = _connectionFactory.CreateConnection();
            return conn.Query<ExtruderReportRow>(sql, new { Start = start, End = end }).ToList();
        }

        public List<LabBlockedReportRow> GetLabBlockedReport(DateTime start, DateTime end)
        {
            const string sql = @"
                SELECT 
                    pb.batch_number AS Партия,
                    p.name AS Продукт,
                    pb.lab_decision_date AS ДатаБлокировки,
                    pb.lab_decision_reason AS Причина,
                    u.full_name AS Ответственный
                FROM production_batches pb
                JOIN products p ON pb.product_id = p.id
                LEFT JOIN users u ON pb.lab_decision_by = u.id
                WHERE pb.lab_decision = 'blocked'
                  AND pb.lab_decision_date >= @Start AND pb.lab_decision_date <= @End
                ORDER BY pb.lab_decision_date";

            using var conn = _connectionFactory.CreateConnection();
            return conn.Query<LabBlockedReportRow>(sql, new { Start = start, End = end }).ToList();
        }

        public List<SystemEventsReportRow> GetSystemEventsReport(DateTime start, DateTime end)
        {
            const string sql = @"
                SELECT 
                    e.event_type AS Тип,
                    e.source_type AS Источник,
                    e.message AS Сообщение,
                    e.created_at AS Дата,
                    u.full_name AS Пользователь
                FROM events e
                LEFT JOIN users u ON e.user_id = u.id
                WHERE e.created_at >= @Start AND e.created_at <= @End
                ORDER BY e.created_at DESC";

            using var conn = _connectionFactory.CreateConnection();
            return conn.Query<SystemEventsReportRow>(sql, new { Start = start, End = end }).ToList();
        }
    }
}