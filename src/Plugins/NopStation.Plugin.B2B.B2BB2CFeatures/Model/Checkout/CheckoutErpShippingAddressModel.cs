using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Shipping;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Models.Checkout;
using Nop.Web.Models.ShoppingCart;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Model.Checkout
{
    public record CheckoutErpShippingAddressModel : BaseNopModel
    {
        public CheckoutErpShippingAddressModel()
        {
            ExistingErpShipToAddresses = new List<ErpShipToAddressModelForCheckout>();
            AvailableDeliveryDates = new List<SelectListItem>();
            B2CShoppingCartItemModels = new List<B2CShoppingCartItemModel>();
            SelectedShipToAddress = new ErpShipToAddressModelForCheckout();
            PickupPointsModel = new CheckoutPickupPointsModel();
        }

        public IList<ErpShipToAddressModelForCheckout> ExistingErpShipToAddresses { get; set; }
        public int ErpShipToAddressId { get; set; }
        public ErpShipToAddressModelForCheckout SelectedShipToAddress { get; set; }
        public bool NewAddressPreselected { get; set; }
        public bool AllowAddressEdit { get; set; }
        public bool IsQuoteOrder { get; set; }
        public bool DisplayPickupInStore { get; set; }

        // for Delivery Date
        [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpOrder.Fields.ExpectedDeliveryDate")]
        public DateTime DeliveryDate { get; set; }
        public string FormatedDeliveryDate { get; set; }

        public string MinDeliveryDate { get; set; }

        public string MaxDeliveryDate { get; set; }

        public bool ErpToDetermineDate { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpOrder.Fields.ExpectedDeliveryDate")]
        public string DeliveryDateString { get; set; }
        public CheckoutPickupPointsModel PickupPointsModel { get; set; }

        public IList<SelectListItem> AvailableDeliveryDates { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpOrder.Fields.SpecialInstructions")]
        public string SpecialInstructions { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpOrder.Fields.CustomerReference")]
        public string CustomerReference { get; set; }
        public int ErpAccountId { get; set; }
        public int B2CUserId { get; set; }

        public bool IsFullLoadRequired { get; set; }
        public bool IsB2BUser { get; set; }
        public bool IsB2CShippingCostERPCallUnSuccessful { get; set; }
        public int B2CShipToAddressId { get; set; }
        public int HasAddressChanged { get; set; }

        //public DeliveryOption DeliveryOptions { get; set; }
        public string ThemeName { get; set; }
        public List<B2CShoppingCartItemModel> B2CShoppingCartItemModels { get; set; }
    }

    public class B2CShoppingCartItemModel
    {
        public B2CShoppingCartItemModel()
        {
            B2CSalesOrgWarehouse = new ErpWarehouseSalesOrgMap();
            NopWarehouse = new Warehouse();
            AvailableDeliveryDates = new List<SelectListItem>();
            ShoppingCartItemModel = new ShoppingCartModel.ShoppingCartItemModel();
            NopWarehouseAddress = new Address();
        }
        public int NopWarehouseId { get; set; }
        public int ShoppingCartItemModelId { get; set; }
        public int B2CSalesOrgWarehouseId { get; set; }
        public string NopWarehouseName { get; set; }
        public string WarehouseCode { get; set; }
        public string UOM { get; set; }
        public string SpecialInstructions { get; set; }
        public bool IsDeliveryDatesCallSuccessful { get; set; }
        public bool IsFullLoadRequired { get; set; }
        [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpOrder.Fields.ExpectedDeliveryDate")]
        public DateTime DeliveryDate { get; set; }
        public DateTime DeliveryDateString { get; set; }
        public IList<SelectListItem> AvailableDeliveryDates { get; set; }
        public ShoppingCartModel.ShoppingCartItemModel ShoppingCartItemModel { get; set; }
        public ErpWarehouseSalesOrgMap B2CSalesOrgWarehouse { get; set; }
        public Warehouse NopWarehouse { get; set; }
        public Address NopWarehouseAddress { get; set; }
    }
}
