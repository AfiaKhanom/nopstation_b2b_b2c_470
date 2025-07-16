using FluentMigrator;
using Nop.Core;
using Nop.Data.Mapping;
using Nop.Data.Migrations;
using NopStation.Plugin.B2B.ErpDataScheduler.Domain;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Data.Migration;

[NopMigration("2024/11/16 09:27:23:6999999", "ErpDataScheduler synctask add isRunning schema", MigrationProcessType.NoMatter)]
public class SyncTaskTableAddIsRunningColumnSchemaMigration : AutoReversingMigration
{
    public static string TableName<T>() where T : BaseEntity
    {
        return NameCompatibilityManager.GetTableName(typeof(T));
    }

    public override void Up()
    {
        if (Schema.Table(TableName<SyncTask>()).Exists())
        {
            var columnExist = Schema.Table(TableName<SyncTask>())
                .Column(nameof(SyncTask.IsRunning)).Exists();

            if (!columnExist)
            {
                Create.Column(nameof(SyncTask.IsRunning))
                    .OnTable(NameCompatibilityManager.GetTableName(typeof(SyncTask)))
                    .AsBoolean()
                    .WithDefaultValue(false);
            }
        }
    }
}
