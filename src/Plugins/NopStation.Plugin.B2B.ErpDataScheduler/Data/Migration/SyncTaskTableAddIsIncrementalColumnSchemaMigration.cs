using FluentMigrator;
using Nop.Core;
using Nop.Data.Mapping;
using Nop.Data.Migrations;
using NopStation.Plugin.B2B.ErpDataScheduler.Domain;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Data.Migration;

[NopMigration("2025/01/16 09:27:23:6999999", "ErpDataScheduler synctask add IsIncremental column schema", MigrationProcessType.NoMatter)]
public class SyncTaskTableAddIsIncrementalColumnSchemaMigration : AutoReversingMigration
{
    public static string TableName<T>() where T : BaseEntity
    {
        return NameCompatibilityManager.GetTableName(typeof(T));
    }

    public override void Up()
    {
        if (Schema.Table(TableName<SyncTask>()).Exists() && 
            !Schema.Table(TableName<SyncTask>()).Column(nameof(SyncTask.IsIncremental)).Exists())
        {
            Create.Column(nameof(SyncTask.IsIncremental))
                .OnTable(TableName<SyncTask>())
                .AsBoolean()
                .WithDefaultValue(true);
        }        
    }
}
