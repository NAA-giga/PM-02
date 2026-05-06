-- ============================================================
-- СКРИПТ СОЗДАНИЯ БАЗЫ ДАННЫХ ManufacturingDB
-- БЕЗ НАСТРОЕК СЕРВЕРА (для импорта на другой сервер)
-- ============================================================

USE [master];
GO

-- Создание базы данных
CREATE DATABASE [ManufacturingDB];
GO

USE [ManufacturingDB];
GO

-- ============================================================
-- 1. ТАБЛИЦЫ
-- ============================================================

-- Таблица аудита
CREATE TABLE [dbo].[audit_log](
    [id] [int] IDENTITY(1,1) NOT NULL,
    [table_name] [varchar](100) NOT NULL,
    [record_id] [int] NOT NULL,
    [action] [varchar](20) NOT NULL,
    [old_value] [nvarchar](max) NULL,
    [new_value] [nvarchar](max) NULL,
    [changed_by] [int] NOT NULL,
    [changed_at] [datetime] NULL,
    CONSTRAINT [PK_audit_log] PRIMARY KEY CLUSTERED ([id] ASC)
);
GO

-- Таблица связи партий продукции с сырьем
CREATE TABLE [dbo].[batch_raw_material_usage](
    [id] [int] IDENTITY(1,1) NOT NULL,
    [production_batch_id] [int] NOT NULL,
    [raw_material_batch_id] [int] NOT NULL,
    [quantity_used] [decimal](12, 2) NOT NULL,
    [created_at] [datetime] NULL,
    CONSTRAINT [PK_batch_raw_material_usage] PRIMARY KEY CLUSTERED ([id] ASC)
);
GO

-- Таблица выполнения шагов партий
CREATE TABLE [dbo].[batch_step_execution](
    [id] [int] IDENTITY(1,1) NOT NULL,
    [production_batch_id] [int] NOT NULL,
    [step_id] [int] NOT NULL,
    [step_order] [int] NOT NULL,
    [step_name] [nvarchar](200) NOT NULL,
    [status] [varchar](20) NULL,
    [actual_temp_c] [decimal](10, 2) NULL,
    [actual_pressure_bar] [decimal](10, 2) NULL,
    [actual_duration_min] [int] NULL,
    [actual_speed_rpm] [int] NULL,
    [deviation_flag] [bit] NULL,
    [deviation_description] [nvarchar](500) NULL,
    [start_time] [datetime] NULL,
    [end_time] [datetime] NULL,
    [started_by] [int] NULL,
    [completed_by] [int] NULL,
    [operator_comment] [nvarchar](500) NULL,
    [created_at] [datetime] NULL,
    CONSTRAINT [PK_batch_step_execution] PRIMARY KEY CLUSTERED ([id] ASC)
);
GO

-- Таблица подразделений
CREATE TABLE [dbo].[departments](
    [id] [int] IDENTITY(1,1) NOT NULL,
    [name] [varchar](100) NOT NULL,
    [description] [nvarchar](500) NULL,
    [created_at] [datetime] NULL,
    CONSTRAINT [PK_departments] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [UQ_departments_name] UNIQUE NONCLUSTERED ([name] ASC)
);
GO

-- Таблица отклонений
CREATE TABLE [dbo].[deviations](
    [id] [int] IDENTITY(1,1) NOT NULL,
    [production_batch_id] [int] NOT NULL,
    [step_execution_id] [int] NULL,
    [deviation_type] [varchar](50) NOT NULL,
    [severity] [varchar](20) NULL,
    [description] [nvarchar](500) NOT NULL,
    [planned_value] [nvarchar](100) NULL,
    [actual_value] [nvarchar](100) NULL,
    [parameter_name] [nvarchar](200) NULL,
    [resolution_status] [varchar](20) NULL,
    [resolution_comment] [nvarchar](500) NULL,
    [resolved_by] [int] NULL,
    [resolved_at] [datetime] NULL,
    [created_by] [int] NOT NULL,
    [created_at] [datetime] NULL,
    CONSTRAINT [PK_deviations] PRIMARY KEY CLUSTERED ([id] ASC)
);
GO

-- Таблица оборудования
CREATE TABLE [dbo].[equipment](
    [id] [int] IDENTITY(1,1) NOT NULL,
    [code] [varchar](50) NOT NULL,
    [name] [nvarchar](200) NOT NULL,
    [equipment_type] [nvarchar](100) NULL,
    [line_number] [varchar](20) NULL,
    [is_active] [bit] NULL,
    [created_at] [datetime] NULL,
    CONSTRAINT [PK_equipment] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [UQ_equipment_code] UNIQUE NONCLUSTERED ([code] ASC)
);
GO

-- Таблица событий
CREATE TABLE [dbo].[events](
    [id] [int] IDENTITY(1,1) NOT NULL,
    [event_type] [varchar](50) NOT NULL,
    [source_type] [varchar](50) NOT NULL,
    [source_id] [int] NOT NULL,
    [message] [nvarchar](500) NOT NULL,
    [user_id] [int] NULL,
    [is_read] [bit] NULL,
    [created_at] [datetime] NULL,
    CONSTRAINT [PK_events] PRIMARY KEY CLUSTERED ([id] ASC)
);
GO

-- Таблица программ экструдера
CREATE TABLE [dbo].[extruder_programs](
    [id] [int] IDENTITY(1,1) NOT NULL,
    [name] [nvarchar](200) NOT NULL,
    [version] [int] NOT NULL,
    [production_batch_id] [int] NULL,
    [zone_params] [nvarchar](max) NULL,
    [status] [varchar](20) NULL,
    [created_by] [int] NOT NULL,
    [created_at] [datetime] NULL,
    CONSTRAINT [PK_extruder_programs] PRIMARY KEY CLUSTERED ([id] ASC)
);
GO

-- Таблица телеметрии экструдера
CREATE TABLE [dbo].[extruder_telemetry](
    [id] [int] IDENTITY(1,1) NOT NULL,
    [production_batch_id] [int] NOT NULL,
    [zone_number] [int] NOT NULL,
    [temperature_c] [decimal](10, 2) NULL,
    [pressure_bar] [decimal](10, 2) NULL,
    [screw_speed_rpm] [int] NULL,
    [recorded_at] [datetime] NULL,
    CONSTRAINT [PK_extruder_telemetry] PRIMARY KEY CLUSTERED ([id] ASC)
);
GO

-- Таблица производственных партий
CREATE TABLE [dbo].[production_batches](
    [id] [int] IDENTITY(1,1) NOT NULL,
    [batch_number] [varchar](50) NOT NULL,
    [order_id] [int] NOT NULL,
    [product_id] [int] NOT NULL,
    [recipe_id] [int] NOT NULL,
    [tech_card_id] [int] NOT NULL,
    [status] [varchar](20) NULL,
    [planned_quantity_kg] [decimal](12, 2) NOT NULL,
    [actual_quantity_kg] [decimal](12, 2) NULL,
    [start_time] [datetime] NULL,
    [end_time] [datetime] NULL,
    [lab_decision] [varchar](20) NULL,
    [lab_decision_date] [datetime] NULL,
    [lab_decision_by] [int] NULL,
    [lab_decision_reason] [nvarchar](500) NULL,
    [created_by] [int] NOT NULL,
    [created_at] [datetime] NULL,
    [updated_at] [datetime] NULL,
    CONSTRAINT [PK_production_batches] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [UQ_production_batches_number] UNIQUE NONCLUSTERED ([batch_number] ASC)
);
GO

-- Таблица производственных заказов
CREATE TABLE [dbo].[production_orders](
    [id] [int] IDENTITY(1,1) NOT NULL,
    [order_number] [varchar](50) NOT NULL,
    [product_id] [int] NOT NULL,
    [recipe_id] [int] NOT NULL,
    [tech_card_id] [int] NOT NULL,
    [planned_quantity_kg] [decimal](12, 2) NOT NULL,
    [status] [varchar](20) NULL,
    [planned_start_date] [date] NOT NULL,
    [actual_start_date] [datetime] NULL,
    [actual_end_date] [datetime] NULL,
    [created_by] [int] NOT NULL,
    [created_at] [datetime] NULL,
    CONSTRAINT [PK_production_orders] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [UQ_production_orders_number] UNIQUE NONCLUSTERED ([order_number] ASC)
);
GO

-- Таблица продуктов
CREATE TABLE [dbo].[products](
    [id] [int] IDENTITY(1,1) NOT NULL,
    [code] [varchar](50) NOT NULL,
    [name] [nvarchar](200) NOT NULL,
    [product_type] [nvarchar](100) NOT NULL,
    [form_type] [nvarchar](50) NOT NULL,
    [status] [varchar](20) NULL,
    [created_at] [datetime] NULL,
    [updated_at] [datetime] NULL,
    CONSTRAINT [PK_products] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [UQ_products_code] UNIQUE NONCLUSTERED ([code] ASC)
);
GO

-- Таблица результатов испытаний
CREATE TABLE [dbo].[quality_test_results](
    [id] [int] IDENTITY(1,1) NOT NULL,
    [test_id] [int] NOT NULL,
    [parameter_name] [nvarchar](200) NOT NULL,
    [measured_value] [decimal](12, 4) NULL,
    [standard_value_min] [decimal](12, 4) NULL,
    [standard_value_max] [decimal](12, 4) NULL,
    [standard_text] [varchar](100) NULL,
    [unit] [varchar](20) NULL,
    [result] [varchar](20) NULL,
    [is_critical] [bit] NULL,
    [analyst_comment] [nvarchar](500) NULL,
    [measured_at] [datetime] NULL,
    CONSTRAINT [PK_quality_test_results] PRIMARY KEY CLUSTERED ([id] ASC)
);
GO

-- Таблица испытаний
CREATE TABLE [dbo].[quality_tests](
    [id] [int] IDENTITY(1,1) NOT NULL,
    [test_number] [varchar](50) NOT NULL,
    [batch_id] [int] NOT NULL,
    [test_type] [varchar](50) NOT NULL,
    [status] [varchar](20) NULL,
    [priority] [varchar](20) NULL,
    [created_date] [datetime] NULL,
    [scheduled_date] [date] NOT NULL,
    [completed_date] [datetime] NULL,
    [assigned_to] [int] NULL,
    [created_by] [int] NOT NULL,
    CONSTRAINT [PK_quality_tests] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [UQ_quality_tests_number] UNIQUE NONCLUSTERED ([test_number] ASC)
);
GO

-- Таблица партий сырья
CREATE TABLE [dbo].[raw_material_batches](
    [id] [int] IDENTITY(1,1) NOT NULL,
    [batch_number] [varchar](50) NOT NULL,
    [raw_material_id] [int] NOT NULL,
    [supplier_batch_number] [varchar](100) NULL,
    [supplier_name] [nvarchar](200) NULL,
    [quantity] [decimal](12, 2) NOT NULL,
    [unit] [varchar](20) NULL,
    [receipt_date] [date] NOT NULL,
    [expiration_date] [date] NULL,
    [lab_status] [varchar](20) NULL,
    [storage_location] [varchar](50) NULL,
    [created_at] [datetime] NULL,
    CONSTRAINT [PK_raw_material_batches] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [UQ_raw_material_batches_number] UNIQUE NONCLUSTERED ([batch_number] ASC)
);
GO

-- Таблица результатов испытаний сырья
CREATE TABLE [dbo].[raw_material_test_results](
    [id] [int] IDENTITY(1,1) NOT NULL,
    [test_id] [int] NOT NULL,
    [parameter_name] [nvarchar](200) NOT NULL,
    [measured_value] [decimal](12, 4) NULL,
    [standard_value_min] [decimal](12, 4) NULL,
    [standard_value_max] [decimal](12, 4) NULL,
    [standard_text] [varchar](100) NULL,
    [unit] [varchar](20) NULL,
    [result] [varchar](20) NULL,
    [analyst_comment] [nvarchar](500) NULL,
    [measured_at] [datetime] NULL,
    CONSTRAINT [PK_raw_material_test_results] PRIMARY KEY CLUSTERED ([id] ASC)
);
GO

-- Таблица испытаний сырья
CREATE TABLE [dbo].[raw_material_tests](
    [id] [int] IDENTITY(1,1) NOT NULL,
    [test_number] [varchar](50) NOT NULL,
    [raw_material_batch_id] [int] NOT NULL,
    [test_type] [varchar](50) NOT NULL,
    [status] [varchar](20) NULL,
    [decision] [varchar](20) NULL,
    [decision_reason] [nvarchar](500) NULL,
    [created_date] [datetime] NULL,
    [completed_date] [datetime] NULL,
    [assigned_to] [int] NULL,
    [created_by] [int] NOT NULL,
    CONSTRAINT [PK_raw_material_tests] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [UQ_raw_material_tests_number] UNIQUE NONCLUSTERED ([test_number] ASC)
);
GO

-- Таблица сырья
CREATE TABLE [dbo].[raw_materials](
    [id] [int] IDENTITY(1,1) NOT NULL,
    [code] [varchar](50) NOT NULL,
    [name] [nvarchar](200) NOT NULL,
    [category] [nvarchar](100) NULL,
    [unit_of_measure] [varchar](20) NULL,
    [is_active] [bit] NULL,
    [created_at] [datetime] NULL,
    CONSTRAINT [PK_raw_materials] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [UQ_raw_materials_code] UNIQUE NONCLUSTERED ([code] ASC)
);
GO

-- Таблица компонентов рецептур
CREATE TABLE [dbo].[recipe_components](
    [id] [int] IDENTITY(1,1) NOT NULL,
    [recipe_id] [int] NOT NULL,
    [raw_material_id] [int] NOT NULL,
    [percentage] [decimal](10, 2) NOT NULL,
    [load_order] [int] NOT NULL,
    [tolerance_min] [decimal](10, 2) NULL,
    [tolerance_max] [decimal](10, 2) NULL,
    [created_at] [datetime] NULL,
    CONSTRAINT [PK_recipe_components] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [UQ_recipe_component] UNIQUE NONCLUSTERED ([recipe_id] ASC, [raw_material_id] ASC)
);
GO

-- Таблица рецептур
CREATE TABLE [dbo].[recipes](
    [id] [int] IDENTITY(1,1) NOT NULL,
    [product_id] [int] NOT NULL,
    [version] [int] NOT NULL,
    [name] [nvarchar](200) NOT NULL,
    [status] [varchar](20) NULL,
    [approved_at] [datetime] NULL,
    [approved_by] [int] NULL,
    [created_by] [int] NOT NULL,
    [created_at] [datetime] NULL,
    [updated_at] [datetime] NULL,
    CONSTRAINT [PK_recipes] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [UQ_recipe_product_version] UNIQUE NONCLUSTERED ([product_id] ASC, [version] ASC)
);
GO

-- Таблица ролей
CREATE TABLE [dbo].[roles](
    [id] [int] IDENTITY(1,1) NOT NULL,
    [name] [varchar](50) NOT NULL,
    [description] [nvarchar](500) NULL,
    CONSTRAINT [PK_roles] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [UQ_roles_name] UNIQUE NONCLUSTERED ([name] ASC)
);
GO

-- Таблица статусов
CREATE TABLE [dbo].[status_dict](
    [id] [int] IDENTITY(1,1) NOT NULL,
    [entity_type] [varchar](50) NOT NULL,
    [status_code] [varchar](50) NOT NULL,
    [status_name] [nvarchar](100) NOT NULL,
    [sort_order] [int] NULL,
    CONSTRAINT [PK_status_dict] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [UQ_status_entity] UNIQUE NONCLUSTERED ([entity_type] ASC, [status_code] ASC)
);
GO

-- Таблица технологических карт
CREATE TABLE [dbo].[tech_cards](
    [id] [int] IDENTITY(1,1) NOT NULL,
    [product_id] [int] NOT NULL,
    [version] [int] NOT NULL,
    [name] [nvarchar](200) NOT NULL,
    [description] [nvarchar](500) NULL,
    [status] [varchar](20) NULL,
    [approved_at] [datetime] NULL,
    [approved_by] [int] NULL,
    [created_by] [int] NOT NULL,
    [created_at] [datetime] NULL,
    [updated_at] [datetime] NULL,
    CONSTRAINT [PK_tech_cards] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [UQ_techcard_product_version] UNIQUE NONCLUSTERED ([product_id] ASC, [version] ASC)
);
GO

-- Таблица шагов технологических карт
CREATE TABLE [dbo].[tech_steps](
    [id] [int] IDENTITY(1,1) NOT NULL,
    [tech_card_id] [int] NOT NULL,
    [step_order] [int] NOT NULL,
    [step_name] [nvarchar](200) NOT NULL,
    [step_type] [nvarchar](50) NOT NULL,
    [equipment_id] [int] NULL,
    [planned_temp_c] [decimal](10, 2) NULL,
    [planned_pressure_bar] [decimal](10, 2) NULL,
    [planned_duration_min] [int] NULL,
    [planned_speed_rpm] [int] NULL,
    [temp_tolerance_min] [decimal](10, 2) NULL,
    [temp_tolerance_max] [decimal](10, 2) NULL,
    [pressure_tolerance_min] [decimal](10, 2) NULL,
    [pressure_tolerance_max] [decimal](10, 2) NULL,
    [is_mandatory] [bit] NULL,
    [instruction] [nvarchar](500) NULL,
    [created_at] [datetime] NULL,
    CONSTRAINT [PK_tech_steps] PRIMARY KEY CLUSTERED ([id] ASC)
);
GO

-- Таблица пользователей
CREATE TABLE [dbo].[users](
    [id] [int] IDENTITY(1,1) NOT NULL,
    [username] [varchar](50) NOT NULL,
    [password_hash] [varchar](255) NOT NULL,
    [full_name] [nvarchar](150) NOT NULL,
    [email] [varchar](100) NULL,
    [role_id] [int] NOT NULL,
    [department_id] [int] NOT NULL,
    [is_active] [bit] NULL,
    [created_at] [datetime] NULL,
    [last_login] [datetime] NULL,
    CONSTRAINT [PK_users] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [UQ_users_username] UNIQUE NONCLUSTERED ([username] ASC)
);
GO

-- ============================================================
-- 2. DEFAULT ОГРАНИЧЕНИЯ
-- ============================================================

ALTER TABLE [dbo].[audit_log] ADD DEFAULT (GETDATE()) FOR [changed_at];
ALTER TABLE [dbo].[batch_raw_material_usage] ADD DEFAULT (GETDATE()) FOR [created_at];
ALTER TABLE [dbo].[batch_step_execution] ADD DEFAULT ('pending') FOR [status];
ALTER TABLE [dbo].[batch_step_execution] ADD DEFAULT ((0)) FOR [deviation_flag];
ALTER TABLE [dbo].[batch_step_execution] ADD DEFAULT (GETDATE()) FOR [created_at];
ALTER TABLE [dbo].[departments] ADD DEFAULT (GETDATE()) FOR [created_at];
ALTER TABLE [dbo].[deviations] ADD DEFAULT ('warning') FOR [severity];
ALTER TABLE [dbo].[deviations] ADD DEFAULT ('new') FOR [resolution_status];
ALTER TABLE [dbo].[deviations] ADD DEFAULT (GETDATE()) FOR [created_at];
ALTER TABLE [dbo].[equipment] ADD DEFAULT ((1)) FOR [is_active];
ALTER TABLE [dbo].[equipment] ADD DEFAULT (GETDATE()) FOR [created_at];
ALTER TABLE [dbo].[events] ADD DEFAULT ((0)) FOR [is_read];
ALTER TABLE [dbo].[events] ADD DEFAULT (GETDATE()) FOR [created_at];
ALTER TABLE [dbo].[extruder_programs] ADD DEFAULT ('draft') FOR [status];
ALTER TABLE [dbo].[extruder_programs] ADD DEFAULT (GETDATE()) FOR [created_at];
ALTER TABLE [dbo].[extruder_telemetry] ADD DEFAULT (GETDATE()) FOR [recorded_at];
ALTER TABLE [dbo].[production_batches] ADD DEFAULT ('created') FOR [status];
ALTER TABLE [dbo].[production_batches] ADD DEFAULT (GETDATE()) FOR [created_at];
ALTER TABLE [dbo].[production_batches] ADD DEFAULT (GETDATE()) FOR [updated_at];
ALTER TABLE [dbo].[production_orders] ADD DEFAULT ('draft') FOR [status];
ALTER TABLE [dbo].[production_orders] ADD DEFAULT (GETDATE()) FOR [created_at];
ALTER TABLE [dbo].[products] ADD DEFAULT ('active') FOR [status];
ALTER TABLE [dbo].[products] ADD DEFAULT (GETDATE()) FOR [created_at];
ALTER TABLE [dbo].[products] ADD DEFAULT (GETDATE()) FOR [updated_at];
ALTER TABLE [dbo].[quality_test_results] ADD DEFAULT ((0)) FOR [is_critical];
ALTER TABLE [dbo].[quality_test_results] ADD DEFAULT (GETDATE()) FOR [measured_at];
ALTER TABLE [dbo].[quality_tests] ADD DEFAULT ('scheduled') FOR [status];
ALTER TABLE [dbo].[quality_tests] ADD DEFAULT ('normal') FOR [priority];
ALTER TABLE [dbo].[quality_tests] ADD DEFAULT (GETDATE()) FOR [created_date];
ALTER TABLE [dbo].[raw_material_batches] ADD DEFAULT ('kg') FOR [unit];
ALTER TABLE [dbo].[raw_material_batches] ADD DEFAULT ('pending') FOR [lab_status];
ALTER TABLE [dbo].[raw_material_batches] ADD DEFAULT (GETDATE()) FOR [created_at];
ALTER TABLE [dbo].[raw_material_test_results] ADD DEFAULT (GETDATE()) FOR [measured_at];
ALTER TABLE [dbo].[raw_material_tests] ADD DEFAULT ('scheduled') FOR [status];
ALTER TABLE [dbo].[raw_material_tests] ADD DEFAULT (GETDATE()) FOR [created_date];
ALTER TABLE [dbo].[raw_materials] ADD DEFAULT ('kg') FOR [unit_of_measure];
ALTER TABLE [dbo].[raw_materials] ADD DEFAULT ((1)) FOR [is_active];
ALTER TABLE [dbo].[raw_materials] ADD DEFAULT (GETDATE()) FOR [created_at];
ALTER TABLE [dbo].[recipe_components] ADD DEFAULT ((0)) FOR [tolerance_min];
ALTER TABLE [dbo].[recipe_components] ADD DEFAULT ((0)) FOR [tolerance_max];
ALTER TABLE [dbo].[recipe_components] ADD DEFAULT (GETDATE()) FOR [created_at];
ALTER TABLE [dbo].[recipes] ADD DEFAULT ('draft') FOR [status];
ALTER TABLE [dbo].[recipes] ADD DEFAULT (GETDATE()) FOR [created_at];
ALTER TABLE [dbo].[recipes] ADD DEFAULT (GETDATE()) FOR [updated_at];
ALTER TABLE [dbo].[status_dict] ADD DEFAULT ((0)) FOR [sort_order];
ALTER TABLE [dbo].[tech_cards] ADD DEFAULT ('draft') FOR [status];
ALTER TABLE [dbo].[tech_cards] ADD DEFAULT (GETDATE()) FOR [created_at];
ALTER TABLE [dbo].[tech_cards] ADD DEFAULT (GETDATE()) FOR [updated_at];
ALTER TABLE [dbo].[tech_steps] ADD DEFAULT ((0)) FOR [temp_tolerance_min];
ALTER TABLE [dbo].[tech_steps] ADD DEFAULT ((0)) FOR [temp_tolerance_max];
ALTER TABLE [dbo].[tech_steps] ADD DEFAULT ((0)) FOR [pressure_tolerance_min];
ALTER TABLE [dbo].[tech_steps] ADD DEFAULT ((0)) FOR [pressure_tolerance_max];
ALTER TABLE [dbo].[tech_steps] ADD DEFAULT ((1)) FOR [is_mandatory];
ALTER TABLE [dbo].[tech_steps] ADD DEFAULT (GETDATE()) FOR [created_at];
ALTER TABLE [dbo].[users] ADD DEFAULT ((1)) FOR [is_active];
ALTER TABLE [dbo].[users] ADD DEFAULT (GETDATE()) FOR [created_at];

-- ============================================================
-- 3. CHECK ОГРАНИЧЕНИЯ
-- ============================================================

ALTER TABLE [dbo].[batch_raw_material_usage] ADD CONSTRAINT [CHK_usage_quantity] CHECK ([quantity_used] > 0);
ALTER TABLE [dbo].[production_batches] ADD CONSTRAINT [CHK_pb_planned_quantity] CHECK ([planned_quantity_kg] > 0);
ALTER TABLE [dbo].[production_orders] ADD CONSTRAINT [CHK_planned_quantity] CHECK ([planned_quantity_kg] > 0);
ALTER TABLE [dbo].[raw_material_batches] ADD CONSTRAINT [CHK_rm_quantity] CHECK ([quantity] > 0);
ALTER TABLE [dbo].[recipe_components] ADD CONSTRAINT [CHK_percentage_positive] CHECK ([percentage] > 0);

-- ============================================================
-- 4. FOREIGN KEY ОГРАНИЧЕНИЯ
-- ============================================================

ALTER TABLE [dbo].[audit_log] ADD CONSTRAINT [FK_audit_log_users] FOREIGN KEY ([changed_by]) REFERENCES [dbo].[users] ([id]);
ALTER TABLE [dbo].[batch_raw_material_usage] ADD CONSTRAINT [FK_batch_usage_production_batch] FOREIGN KEY ([production_batch_id]) REFERENCES [dbo].[production_batches] ([id]);
ALTER TABLE [dbo].[batch_raw_material_usage] ADD CONSTRAINT [FK_batch_usage_raw_material_batch] FOREIGN KEY ([raw_material_batch_id]) REFERENCES [dbo].[raw_material_batches] ([id]);
ALTER TABLE [dbo].[batch_step_execution] ADD CONSTRAINT [FK_batch_step_production_batch] FOREIGN KEY ([production_batch_id]) REFERENCES [dbo].[production_batches] ([id]);
ALTER TABLE [dbo].[batch_step_execution] ADD CONSTRAINT [FK_batch_step_tech_step] FOREIGN KEY ([step_id]) REFERENCES [dbo].[tech_steps] ([id]);
ALTER TABLE [dbo].[batch_step_execution] ADD CONSTRAINT [FK_batch_step_started_by] FOREIGN KEY ([started_by]) REFERENCES [dbo].[users] ([id]);
ALTER TABLE [dbo].[batch_step_execution] ADD CONSTRAINT [FK_batch_step_completed_by] FOREIGN KEY ([completed_by]) REFERENCES [dbo].[users] ([id]);
ALTER TABLE [dbo].[deviations] ADD CONSTRAINT [FK_deviations_production_batch] FOREIGN KEY ([production_batch_id]) REFERENCES [dbo].[production_batches] ([id]);
ALTER TABLE [dbo].[deviations] ADD CONSTRAINT [FK_deviations_step_execution] FOREIGN KEY ([step_execution_id]) REFERENCES [dbo].[batch_step_execution] ([id]);
ALTER TABLE [dbo].[deviations] ADD CONSTRAINT [FK_deviations_created_by] FOREIGN KEY ([created_by]) REFERENCES [dbo].[users] ([id]);
ALTER TABLE [dbo].[deviations] ADD CONSTRAINT [FK_deviations_resolved_by] FOREIGN KEY ([resolved_by]) REFERENCES [dbo].[users] ([id]);
ALTER TABLE [dbo].[events] ADD CONSTRAINT [FK_events_user] FOREIGN KEY ([user_id]) REFERENCES [dbo].[users] ([id]);
ALTER TABLE [dbo].[extruder_programs] ADD CONSTRAINT [FK_extruder_programs_batch] FOREIGN KEY ([production_batch_id]) REFERENCES [dbo].[production_batches] ([id]);
ALTER TABLE [dbo].[extruder_programs] ADD CONSTRAINT [FK_extruder_programs_created_by] FOREIGN KEY ([created_by]) REFERENCES [dbo].[users] ([id]);
ALTER TABLE [dbo].[extruder_telemetry] ADD CONSTRAINT [FK_extruder_telemetry_batch] FOREIGN KEY ([production_batch_id]) REFERENCES [dbo].[production_batches] ([id]);
ALTER TABLE [dbo].[production_batches] ADD CONSTRAINT [FK_production_batches_order] FOREIGN KEY ([order_id]) REFERENCES [dbo].[production_orders] ([id]);
ALTER TABLE [dbo].[production_batches] ADD CONSTRAINT [FK_production_batches_product] FOREIGN KEY ([product_id]) REFERENCES [dbo].[products] ([id]);
ALTER TABLE [dbo].[production_batches] ADD CONSTRAINT [FK_production_batches_recipe] FOREIGN KEY ([recipe_id]) REFERENCES [dbo].[recipes] ([id]);
ALTER TABLE [dbo].[production_batches] ADD CONSTRAINT [FK_production_batches_tech_card] FOREIGN KEY ([tech_card_id]) REFERENCES [dbo].[tech_cards] ([id]);
ALTER TABLE [dbo].[production_batches] ADD CONSTRAINT [FK_production_batches_created_by] FOREIGN KEY ([created_by]) REFERENCES [dbo].[users] ([id]);
ALTER TABLE [dbo].[production_batches] ADD CONSTRAINT [FK_production_batches_lab_decision_by] FOREIGN KEY ([lab_decision_by]) REFERENCES [dbo].[users] ([id]);
ALTER TABLE [dbo].[production_orders] ADD CONSTRAINT [FK_production_orders_product] FOREIGN KEY ([product_id]) REFERENCES [dbo].[products] ([id]);
ALTER TABLE [dbo].[production_orders] ADD CONSTRAINT [FK_production_orders_recipe] FOREIGN KEY ([recipe_id]) REFERENCES [dbo].[recipes] ([id]);
ALTER TABLE [dbo].[production_orders] ADD CONSTRAINT [FK_production_orders_tech_card] FOREIGN KEY ([tech_card_id]) REFERENCES [dbo].[tech_cards] ([id]);
ALTER TABLE [dbo].[production_orders] ADD CONSTRAINT [FK_production_orders_created_by] FOREIGN KEY ([created_by]) REFERENCES [dbo].[users] ([id]);
ALTER TABLE [dbo].[quality_test_results] ADD CONSTRAINT [FK_quality_test_results_test] FOREIGN KEY ([test_id]) REFERENCES [dbo].[quality_tests] ([id]);
ALTER TABLE [dbo].[quality_tests] ADD CONSTRAINT [FK_quality_tests_batch] FOREIGN KEY ([batch_id]) REFERENCES [dbo].[production_batches] ([id]);
ALTER TABLE [dbo].[quality_tests] ADD CONSTRAINT [FK_quality_tests_assigned_to] FOREIGN KEY ([assigned_to]) REFERENCES [dbo].[users] ([id]);
ALTER TABLE [dbo].[quality_tests] ADD CONSTRAINT [FK_quality_tests_created_by] FOREIGN KEY ([created_by]) REFERENCES [dbo].[users] ([id]);
ALTER TABLE [dbo].[raw_material_batches] ADD CONSTRAINT [FK_raw_material_batches_material] FOREIGN KEY ([raw_material_id]) REFERENCES [dbo].[raw_materials] ([id]);
ALTER TABLE [dbo].[raw_material_test_results] ADD CONSTRAINT [FK_raw_material_test_results_test] FOREIGN KEY ([test_id]) REFERENCES [dbo].[raw_material_tests] ([id]);
ALTER TABLE [dbo].[raw_material_tests] ADD CONSTRAINT [FK_raw_material_tests_batch] FOREIGN KEY ([raw_material_batch_id]) REFERENCES [dbo].[raw_material_batches] ([id]);
ALTER TABLE [dbo].[raw_material_tests] ADD CONSTRAINT [FK_raw_material_tests_assigned_to] FOREIGN KEY ([assigned_to]) REFERENCES [dbo].[users] ([id]);
ALTER TABLE [dbo].[raw_material_tests] ADD CONSTRAINT [FK_raw_material_tests_created_by] FOREIGN KEY ([created_by]) REFERENCES [dbo].[users] ([id]);
ALTER TABLE [dbo].[recipe_components] ADD CONSTRAINT [FK_recipe_components_recipe] FOREIGN KEY ([recipe_id]) REFERENCES [dbo].[recipes] ([id]);
ALTER TABLE [dbo].[recipe_components] ADD CONSTRAINT [FK_recipe_components_material] FOREIGN KEY ([raw_material_id]) REFERENCES [dbo].[raw_materials] ([id]);
ALTER TABLE [dbo].[recipes] ADD CONSTRAINT [FK_recipes_product] FOREIGN KEY ([product_id]) REFERENCES [dbo].[products] ([id]);
ALTER TABLE [dbo].[recipes] ADD CONSTRAINT [FK_recipes_approved_by] FOREIGN KEY ([approved_by]) REFERENCES [dbo].[users] ([id]);
ALTER TABLE [dbo].[recipes] ADD CONSTRAINT [FK_recipes_created_by] FOREIGN KEY ([created_by]) REFERENCES [dbo].[users] ([id]);
ALTER TABLE [dbo].[tech_cards] ADD CONSTRAINT [FK_tech_cards_product] FOREIGN KEY ([product_id]) REFERENCES [dbo].[products] ([id]);
ALTER TABLE [dbo].[tech_cards] ADD CONSTRAINT [FK_tech_cards_approved_by] FOREIGN KEY ([approved_by]) REFERENCES [dbo].[users] ([id]);
ALTER TABLE [dbo].[tech_cards] ADD CONSTRAINT [FK_tech_cards_created_by] FOREIGN KEY ([created_by]) REFERENCES [dbo].[users] ([id]);
ALTER TABLE [dbo].[tech_steps] ADD CONSTRAINT [FK_tech_steps_card] FOREIGN KEY ([tech_card_id]) REFERENCES [dbo].[tech_cards] ([id]);
ALTER TABLE [dbo].[tech_steps] ADD CONSTRAINT [FK_tech_steps_equipment] FOREIGN KEY ([equipment_id]) REFERENCES [dbo].[equipment] ([id]);
ALTER TABLE [dbo].[users] ADD CONSTRAINT [FK_users_role] FOREIGN KEY ([role_id]) REFERENCES [dbo].[roles] ([id]);
ALTER TABLE [dbo].[users] ADD CONSTRAINT [FK_users_department] FOREIGN KEY ([department_id]) REFERENCES [dbo].[departments] ([id]);

-- ============================================================
-- 5. ТРИГГЕРЫ БИЗНЕС-ОГРАНИЧЕНИЙ
-- ============================================================

-- ОГРАНИЧЕНИЕ 1: Для одного продукта может существовать только одна действующая утвержденная рецептура
CREATE TRIGGER trg_check_unique_active_recipe
ON recipes
INSTEAD OF INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @ProductId INT;
    DECLARE @NewStatus VARCHAR(20);
    DECLARE @NewVersion INT;
    DECLARE @NewName NVARCHAR(200);
    DECLARE @NewCreatedBy INT;
    
    SELECT 
        @ProductId = product_id, 
        @NewStatus = status, 
        @NewVersion = version,
        @NewName = name,
        @NewCreatedBy = created_by
    FROM inserted;
    
    IF @NewStatus = 'approved'
    BEGIN
        IF EXISTS (SELECT 1 FROM recipes WHERE product_id = @ProductId AND status = 'approved')
        BEGIN
            UPDATE recipes 
            SET status = 'replaced', updated_at = GETDATE()
            WHERE product_id = @ProductId AND status = 'approved';
        END
    END
    
    IF EXISTS (SELECT 1 FROM inserted)
    BEGIN
        IF EXISTS (SELECT 1 FROM deleted)
        BEGIN
            UPDATE recipes 
            SET status = @NewStatus, updated_at = GETDATE()
            WHERE id IN (SELECT id FROM deleted);
        END
        ELSE
        BEGIN
            INSERT INTO recipes (product_id, version, name, status, created_by, created_at)
            VALUES (@ProductId, @NewVersion, @NewName, @NewStatus, @NewCreatedBy, GETDATE());
        END
    END
END;
GO

-- ОГРАНИЧЕНИЕ 2: Для одного продукта может существовать только одна действующая технологическая карта
CREATE TRIGGER trg_check_unique_active_techcard
ON tech_cards
INSTEAD OF INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @ProductId INT;
    DECLARE @NewStatus VARCHAR(20);
    DECLARE @NewVersion INT;
    DECLARE @NewName NVARCHAR(200);
    DECLARE @NewDescription NVARCHAR(500);
    DECLARE @NewCreatedBy INT;
    
    SELECT 
        @ProductId = product_id, 
        @NewStatus = status, 
        @NewVersion = version,
        @NewName = name,
        @NewDescription = description,
        @NewCreatedBy = created_by
    FROM inserted;
    
    IF @NewStatus = 'approved'
    BEGIN
        IF EXISTS (SELECT 1 FROM tech_cards WHERE product_id = @ProductId AND status = 'approved')
        BEGIN
            UPDATE tech_cards 
            SET status = 'replaced', updated_at = GETDATE()
            WHERE product_id = @ProductId AND status = 'approved';
        END
    END
    
    IF EXISTS (SELECT 1 FROM inserted)
    BEGIN
        IF EXISTS (SELECT 1 FROM deleted)
        BEGIN
            UPDATE tech_cards 
            SET status = @NewStatus, updated_at = GETDATE()
            WHERE id IN (SELECT id FROM deleted);
        END
        ELSE
        BEGIN
            INSERT INTO tech_cards (product_id, version, name, description, status, created_by, created_at)
            VALUES (@ProductId, @NewVersion, @NewName, @NewDescription, @NewStatus, @NewCreatedBy, GETDATE());
        END
    END
END;
GO

-- ОГРАНИЧЕНИЕ 3: Утверждение рецептуры запрещено, если сумма долей компонентов не равна 100%
CREATE TRIGGER trg_check_recipe_before_approve
ON recipes
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    
    IF UPDATE(status)
    BEGIN
        DECLARE @RecipeId INT;
        DECLARE @NewStatus VARCHAR(20);
        DECLARE @TotalPercentage DECIMAL(10,2);
        
        SELECT @RecipeId = id, @NewStatus = status FROM inserted;
        
        IF @NewStatus IN ('approved', 'pending')
        BEGIN
            SELECT @TotalPercentage = ISNULL(SUM(percentage), 0)
            FROM recipe_components
            WHERE recipe_id = @RecipeId;
            
            IF ABS(@TotalPercentage - 100) > 0.01
            BEGIN
                RAISERROR('Невозможно утвердить рецептуру: сумма долей компонентов должна составлять 100%%. Текущая сумма: %.2f%%', 16, 1);
                ROLLBACK TRANSACTION;
                RETURN;
            END
        END
    END
END;
GO

-- Триггер автоматического пересчета статуса при изменении компонентов
CREATE TRIGGER trg_recipe_components_check
ON recipe_components
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @RecipeId INT;
    DECLARE @TotalPercentage DECIMAL(10,2);
    DECLARE cur CURSOR FOR
        SELECT DISTINCT recipe_id FROM inserted
        UNION
        SELECT DISTINCT recipe_id FROM deleted;
    
    OPEN cur;
    FETCH NEXT FROM cur INTO @RecipeId;
    
    WHILE @@FETCH_STATUS = 0
    BEGIN
        SELECT @TotalPercentage = ISNULL(SUM(percentage), 0)
        FROM recipe_components
        WHERE recipe_id = @RecipeId;
        
        IF ABS(@TotalPercentage - 100) > 0.01
        BEGIN
            UPDATE recipes 
            SET status = 'draft'
            WHERE id = @RecipeId AND status IN ('approved', 'pending');
        END
        
        FETCH NEXT FROM cur INTO @RecipeId;
    END
    
    CLOSE cur;
    DEALLOCATE cur;
END;
GO

PRINT '==========================================';
PRINT 'База данных ManufacturingDB успешно создана!';
PRINT '==========================================';