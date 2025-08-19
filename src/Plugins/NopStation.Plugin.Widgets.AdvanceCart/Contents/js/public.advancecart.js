
var NopAdvanceCart = {
  loadWaiting: false,
  selectquantityfromproductbox: false,
  productboxselector: '',
  topcartselector: '',
  flyoutcartselector: '',
  addtocartcatalogurl: '',
  addtocartdetailsurl: '',
  localized_data: false,


  init: function (selectquantityfromproductbox, productboxselector, topcartselector, flyoutcartselector, addtocartcatalogurl, addtocartdetailsurl, localized_data) {
    this.loadwaiting = false;
    this.selectquantityfromproductbox = selectquantityfromproductbox;
    this.productboxselector = productboxselector;
    this.topcartselector = topcartselector;
    this.flyoutcartselector = flyoutcartselector;
    this.addtocartcatalogurl = addtocartcatalogurl;
    this.addtocartdetailsurl = addtocartdetailsurl;
    this.localized_data = localized_data;
  },

  setLoadWaiting: function (display) {
    displayAjaxLoading(display);
    this.loadWaiting = display;
  },

  addproducttocart_catalog: function (productid, buynow, elem) {
    if (this.loadWaiting) {
      return;
    }
    this.setLoadWaiting(true);

    var quantity = 1;
    if (this.selectquantityfromproductbox) {
      var input = $(elem).closest(this.productboxselector).find(':input[name="EnteredQuantity"]');
      quantity = input.val();
    }

    var postData = {};
    postData.productid = productid;
    postData.buynow = buynow;
    postData.quantity = quantity;
    addAntiForgeryToken(postData);

    $.ajax({
      cache: false,
      url: this.addtocartcatalogurl,
      type: "POST",
      data: postData,
      success: this.success_process,
      complete: this.resetLoadWaiting,
      error: this.ajaxFailure
    });
  },

  addproducttocart_details: function (productid, formselector, buynow) {
    if (this.loadWaiting !== false) {
      return;
    }
    this.setLoadWaiting(true);

    var formData = $(formselector).serialize();
    formData += '&buynow=' + buynow;

    $.ajax({
      cache: false,
      url: this.addtocartdetailsurl,
      data: formData,
      type: "POST",
      success: this.success_process,
      complete: this.resetLoadWaiting,
      error: this.ajaxFailure
    });
  },

  getflyoutcart: function (url) {
    if (this.loadWaiting !== false) {
      return;
    }
    this.setLoadWaiting(true);

    $.ajax({
      cache: false,
      url: url,
      type: "GET",
      success: this.success_process,
      complete: this.resetLoadWaiting,
      error: this.ajaxFailure
    });
  },

  flyOutCartQuantityChange: function (shoppingCartItemId, isPlusIconPressed) {
    if (this.loadWaiting != false) {
      return;
    }
    this.loadWaiting = true;
    var currentQuantity = $("#itemquantity" + shoppingCartItemId).val();
    var incrementedQuantity = 0;
    if (isPlusIconPressed) {
      incrementedQuantity = ++currentQuantity;
    }
    else {
      incrementedQuantity = --currentQuantity;
    }
    var postData = {
      ShoppingCartItemId: shoppingCartItemId,
      ShoppingCartItemNewQuantity: incrementedQuantity
    };
    addAntiForgeryToken(postData);
    $.ajax({
      cache: false,
      url: "/AdvanceCart/GetFlyoutCartQuantityChange",
      type: "POST",
      data: postData,
      success: function (response) {
        if (response.Success) {
          var shoppingCartId = "#itemquantity" + response.data.ShoppingCartItemId;
          var flyoutShoppingCatItemUnitPrice = "#itemunitpriceflyout" + response.data.ShoppingCartItemId;
          $(shoppingCartId).val(response.data.ShoppingCartItemUpdatedQuantity);
          $(flyoutShoppingCatItemUnitPrice).text(response.data.UnitPrice);

          $("#miniCartSubTotal").text(response.data.Subtotal);
          var totalQuantity = "(" + response.data.TotalQuantity + ")";
          $('.header-links .cart-qty').text(totalQuantity);
        }
        else {
          if (AjaxCart.usepopupnotifications == true) {
            displayPopupNotification(response.message, 'error', true);
          }
          else {
            displayBarNotification(response.message, 'error', 3500);
          }
        }
        this.loadWaiting = false;
      },
      complete: this.resetLoadWaiting,
    });
  },
  flyOutCartDeleteItem: function (shoppingCartItemId) {
    if (this.loadWaiting != false) {
      return;
    }
    this.loadWaiting = true;
    var postData = {
      ShoppingCartItemId: shoppingCartItemId
    };
    addAntiForgeryToken(postData);
    $.ajax({
      cache: false,
      url: "/AdvanceCart/DeleteFlyoutCartItem",
      type: "POST",
      data: postData,
      success: function (response) {
        if (response.Success) {
          $("#shoppingCartItem_" + shoppingCartItemId).remove();
          $("#miniCartSubTotal").text(response.data.Subtotal);
          var totalQuantity = "(" + response.data.TotalQuantity + ")";
          $('.header-links .cart-qty').text(totalQuantity);
          if (response.data.TotalQuantity == 0) {
            $("#miniCartWithProduct").css("display", "none");
          }
          else {
            $("#miniCartWithProduct").css("display", "block");
          }
        }
        else {
          if (AjaxCart.usepopupnotifications == true) {
            displayPopupNotification(response.message, 'error', true);
          }
          else {
            displayBarNotification(response.message, 'error', 3500);
          }
        }
        this.loadWaiting = false;
      },
      complete: this.resetLoadWaiting,
    });
  },
  success_process: function (response) {
    if (AjaxCart) {
      if (response.updatetopcartsectionhtml) {
        $(AjaxCart.topcartselector).html(response.updatetopcartsectionhtml);
      }
      if (response.updatetopwishlistsectionhtml) {
        $(AjaxCart.topwishlistselector).html(response.updatetopwishlistsectionhtml);
      }
      if (response.updateflyoutcartsectionhtml) {
        $(AjaxCart.flyoutcartselector).replaceWith(response.updateflyoutcartsectionhtml);
      }
    }
    if (response.updatetopcartsectionhtml) {
      $(NopAdvanceCart.topcartselector).html(response.updatetopcartsectionhtml);
    }
    if (response.updateflyoutcartsectionhtml) {
      $(NopAdvanceCart.flyoutcartselector).replaceWith(response.updateflyoutcartsectionhtml);
    }
    if (response.popupnotificationhtml) {
      if (AjaxCart.usepopupnotifications === true) {
        displayPopupNotification(response.popupnotificationhtml, 'success', true);
      }
      else {
        //specify timeout for success messages
        displayBarNotification(response.popupnotificationhtml, 'success', 3500);
      }
      return false;
    }
    else if (response.message) {
      //display notification
      if (response.success === true) {
        //success
        if (AjaxCart.usepopupnotifications === true) {
          displayPopupNotification(response.message, 'success', true);
        }
        else {
          //specify timeout for success messages
          displayBarNotification(response.message, 'success', 3500);
        }
      }
      else {
        //error
        if (AjaxCart.usepopupnotifications === true) {
          displayPopupNotification(response.message, 'error', true);
        }
        else {
          //no timeout for errors
          displayBarNotification(response.message, 'error', 0);
        }
      }
      return false;
    }
    if (response.openquickview) {
      QuickView.load_product_details(response.productid)
      return true;
    }
    else if (response.redirect) {
      location.href = response.redirect;
      return true;
    }
    return false;
  },

  resetLoadWaiting: function () {
    NopAdvanceCart.setLoadWaiting(false);
  },

  ajaxFailure: function () {
    alert(NopAdvanceCart.localized_data.AdvanceCartFailure);
  }
};