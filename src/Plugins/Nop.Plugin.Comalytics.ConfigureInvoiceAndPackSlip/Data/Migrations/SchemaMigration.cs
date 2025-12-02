using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using Nop.Plugin.Comalytics.ConfigureInvoiceAndPackSlip.Domain;

namespace Nop.Plugin.Comalytics.ConfigureInvoiceAndPackSlip.Data.Migrations
{
    [NopMigration("2024/12/02 00:00:00", "Comalytics.ConfigureInvoiceAndPackSlip base schema", MigrationProcessType.Installation)]
    public class SchemaMigration : AutoReversingMigration
    {
        public override void Up()
        {
            Create.TableFor<InvoiceTemplate>();
            Create.TableFor<PackingSlipTemplate>();
            Create.TableFor<PdfJob>();
            Create.TableFor<GeneratedPdf>();
        }
    }
}
