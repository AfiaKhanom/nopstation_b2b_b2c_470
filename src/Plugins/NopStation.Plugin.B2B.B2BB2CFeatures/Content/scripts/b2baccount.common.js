var B2BAccountCommon = {
  productB2BDataUrl: '',
  noItemsMsgUrl: '',
  productInCartQuantityUrl: '',

  init: function (productB2BDataUrl, noItemsMsgUrl, productInCartQuantityUrl) {
    this.productB2BDataUrl = productB2BDataUrl;
    this.noItemsMsgUrl = noItemsMsgUrl;
    this.productInCartQuantityUrl = productInCartQuantityUrl;
  },

  loadProductB2BData: function (productIds) {

    var productlist = {
      productIds: productIds
    }

    $.ajax({
      cache: true,
      url: this.productB2BDataUrl,
      type: 'GET',
      data: productlist,
      dataType: "json",
      success: function (response) {
        if (response.IsHideWeightinfo) {
          $('#b2bWeightValue').hide();
        }
        if (response.Data) {
          for (const element of response.Data) {
            let productId = element.Id;

            $('#b2bUOM_' + productId).html("" + element.UOM + "");

            $('#b2bStockAvailability_' + productId).html("" + element.StockAvailability + "");

            if (element.IsOutOfStock) {
              $('#b2bStockAvailability_' + productId).addClass("out-of-stock")
            }

            if (element.DisplayBackInStockSubscription) {
              $('#backInStockSubscription_' + productId).html("<div title='Notify me when available' class='back-in-stock-subscription b2blist-back-in-stock-subscription' onclick=\"backInStockSubscriptionClick(" + productId + ")\"></div>");
            }

            if (!response.IsHidePricingnote) {
              $('#b2bPricingNotes_' + productId).html("" + element.PricingNotes + "");
            }

            if (!response.IsHideWeightinfo) {
              $('#b2bWeightValue_' + productId).html("" + element.WeightValue + "");
            }

            //$('#b2bProductQty_' + productId).val("" + element.Quantity + "");
            //$('.b2bProductQty_' + productId).val("" + element.Quantity + "");
          }
        }
      },
      error: function () {
        console.log("Can Not Load B2B Data");
        alert("Can Not Load B2B Data");
      }
    });
  },

  b2bLoadProductInCartQuantity: function (productId) {
    $.ajax({
      cache: true,
      url: this.productInCartQuantityUrl,
      type: 'GET',
      data: { productId: productId },
      dataType: "json",
      success: function (response) {
        if (response.Data) {
          var productId = response.Data.Id;
          $('#b2bProductQty_' + productId).val("" + response.Data.Quantity + "");
          $('.b2bProductQty_' + productId).val("" + response.Data.Quantity + "");
        }
      },
      error: function () {
        console.log("Can Not Load Cart Info");
      }
    });
  },

  loadPasswordRequirmentsAjaxLoad: function () {
    $.ajax({
      cache: true,
      url: '/GetPasswordRequirements',
      type: 'GET',
      dataType: "json",
      success: function (response) {
        if (response.message.length > 0) {
          $(".password-rules").append(response.message)
        }
      },
      error: function () {
        console.log("Can Not Load Password Requirment Info Info");
      }
    });
  },

  downloadFromUrl: function (url) {
    displayAjaxLoading(true);

    $.ajax({
      cache: false,
      type: "GET",
      url: url,
      dataType: "json",
      success: function (data, textStatus, jqXHR) {
        B2BAccountCommon.downloadFile(data);
      },
      error: function (jqXHR, textStatus, errorThrown) {
        if (jqXHR.status == '404' || jqXHR.status == '200') {
          location.href = url;
        }
        else {
          //console.log("error");
          alert('Failed to download document');
        }
      },
      complete: function () {
        displayAjaxLoading(false);
      }
    });

    displayAjaxLoading(true);
  },

  downloadFile: function (data) {
    const decodedData = atob(data['file']['FileContents']);
    const array = Uint8Array.from(decodedData, b => b.charCodeAt(0));
    var blob = new Blob([array], { type: "application/pdf" });
    var link = document.createElement("a");
    link.href = window.URL.createObjectURL(blob);
    link.download = data['fileName'];
    link.click();
  }
};