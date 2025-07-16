using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Common;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using System.Data;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Data.Builders;

public class ErpShipToAddressBuilder : NopEntityBuilder<ErpShipToAddress>
{
    #region Methods

    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(ErpShipToAddress.ShipToCode)).AsString()
            .WithColumn(nameof(ErpShipToAddress.ShipToName)).AsString()
            .WithColumn(nameof(ErpShipToAddress.AddressId)).AsInt32().ForeignKey<Address>(onDelete: Rule.None)
            .WithColumn(nameof(ErpShipToAddress.ProvinceCode)).AsString().Nullable()
            .WithColumn(nameof(ErpShipToAddress.DeliveryNotes)).AsString().Nullable()
            .WithColumn(nameof(ErpShipToAddress.EmailAddresses)).AsString().Nullable()
            .WithColumn(nameof(ErpShipToAddress.RepNumber)).AsString()
            .WithColumn(nameof(ErpShipToAddress.RepFullName)).AsString().Nullable()
            .WithColumn(nameof(ErpShipToAddress.RepPhoneNumber)).AsString().Nullable()
            .WithColumn(nameof(ErpShipToAddress.RepEmail)).AsString().Nullable()
            .WithColumn(nameof(ErpShipToAddress.Suburb)).AsString().Nullable()
            .WithColumn(nameof(ErpShipToAddress.LastShipToAddressSyncDate)).AsDateTime2().Nullable();
    }

    #endregion
}
