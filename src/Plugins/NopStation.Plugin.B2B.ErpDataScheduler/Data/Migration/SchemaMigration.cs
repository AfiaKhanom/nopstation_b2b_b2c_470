using FluentMigrator;
using Nop.Core;
using Nop.Data.Extensions;
using Nop.Data.Mapping;
using Nop.Data.Migrations;
using NopStation.Plugin.B2B.ErpDataScheduler.Domain;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Data.Migration
{
    [NopMigration("2023/11/17 12:00:00", "Erp Data Scheduler schema with Sync Tasks", MigrationProcessType.Installation)]
    public class SchemaMigration : AutoReversingMigration
    {
        /// <summary>
        /// Collect the UP migration expressions
        /// </summary>

        public static string TableName<T>() where T : BaseEntity
        {
            return NameCompatibilityManager.GetTableName(typeof(T));
        }

        public static string ColumnName(Type tableType, string columnName)
        {
            return NameCompatibilityManager.GetColumnName(tableType, columnName);
        }

        public override void Up()
        {
            Create.TableFor<SyncTask>();
        }
    }
}
