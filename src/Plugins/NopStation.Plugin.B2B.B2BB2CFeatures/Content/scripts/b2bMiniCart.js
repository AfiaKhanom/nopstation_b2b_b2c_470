/*
** nopCommerce ajax cart implementation
*/
//var timeoutFlyout_cart;

//function removeActiveClassFromFlyout_cart(minicartWaitingTime) {
//    clearTimeout(timeoutFlyout_cart);
//    timeoutFlyout_cart = setTimeout(function () {
//        $('.flyout-cart').removeClass('active');
//    }, minicartWaitingTime);
//}

//var timeToWait;
var B2BMiniCart = {

  //add a product to the cart/wishlist from the catalog pages
  addproducttominicart_catalog: function (urladd) {
    if (B2BAjaxCart.loadWaiting != false) {
      return;
    }
B2BAjaxCart.setLoadWaiting(true);

    $.ajax({
      cache: false,
      url: urladd,
      async: false,
      type: "POST",
      success: this.minicart_success_process,
      complete: this.minicart_resetLoadWaiting,
      error: this.minicart_ajaxFailure
    });
  },

  updateProductQuantityValueInMiniCart_Catalog: function (currentQuantity, productId, shoppingCartTypeId) {
    if (!isNaN(currentQuantity) && currentQuantity >= 0) {
      clearTimeout(timeoutFlyout_cart);
      $('.flyout-cart').addClass('active');

      var urladd = '/updateproductqtyinminicart/catalog/' + productId + '/' + shoppingCartTypeId + '/' + currentQuantity;
      B2BMiniCart.addproducttominicart_catalog(urladd);

      if ($("#update-shoppingcart").val() != undefined)
        $("#update-shoppingcart").click();
      $('.flyout-cart').addClass('active');
    }
    else {
      $('.flyout-cart').addClass('active');
      clearTimeout(timeoutFlyout_cart);
      B2BMiniCart.b2bLoadProductInMiniCartQuantity(productId);
    }
    $('.flyout-cart').addClass('active');
    clearTimeout(timeoutFlyout_cart);
    removeActiveClassFromFlyout_cart(1000);
  },

  checkUserRegisteredAndUpdateProductQuantityInMiniCart: function (currentQuantity, productId, url, shoppingCartTypeId) {
    displayAjaxLoading(true)
    $.ajax({
      cache: false,
      type: "GET",
      url: url,
      success: function (data, textStatus, jqXHR) {
        if (data.success) {
          B2BMiniCart.updateProductQuantityValueInMiniCart_Catalog(currentQuantity, productId, shoppingCartTypeId);
          displayAjaxLoading(false)
        }
        else {
          displayAjaxLoading(false)
          B2BMiniCart.minicart_promptForLogin();
        }
      },
      error: function (jqXHR, textStatus, errorThrown) {
        console.log("Can not get registered user info.");
      }
    });
  },

  minicart_promptForLogin: function () {
    $(".logInToUpdateCartDialog").dialog({
      modal: true,
      draggable: false,
      closeOnEscape: false,
      open: function (event, ui) {
        //hide close button.
        $(this).parent().children().children('.ui-dialog-titlebar-close').hide();
      },
    });
  },
  closePopup: function () {
    $(".logInToUpdateCartDialog").dialog("destroy");
    location.replace("/login")
  },

  minicart_success_process: function (response) {
    if (response.updatetopcartsectionhtml) {
      $(B2BAjaxCart.topcartselector).html(response.updatetopcartsectionhtml);
    }
    if (response.updatetopwishlistsectionhtml) {
      $(B2BAjaxCart.topwishlistselector).html(response.updatetopwishlistsectionhtml);
    }
    if (response.updateflyoutcartsectionhtml) {
      $(B2BAjaxCart.flyoutcartselector).replaceWith(response.updateflyoutcartsectionhtml);
    }
    B2BMiniCart.b2bLoadProductInMiniCartQuantity(response.productId);
    if (response.message) {
      //display notification
      if (response.success == true) {
        // for noptemplate
        $(document).trigger("nopcommerceAjaxCartProductAddedToCartEvent");
        //success
        if (B2BAjaxCart.usepopupnotifications == true) {
          displayPopupNotification(response.message, response.isBackOrder ? 'warning' : 'success', true);
        }
        else {
          //specify timeout for success messages
          displayBarNotification(response.message, response.isBackOrder ? 'warning' : 'success', 3500);
        }
      }
      else {
        //error
        if (B2BAjaxCart.usepopupnotifications == true) {
          displayPopupNotification(response.message, 'error', true);
        }
        else {
          //no timeout for errors
          displayBarNotification(response.message, 'error', 0);
        }
      }
      return false;
    }
    if (response.redirect) {
      location.href = response.redirect;
      return true;
    }
    return false;
  },

  minicart_resetLoadWaiting: function () {
    B2BAjaxCart.setLoadWaiting(false);
  },

  minicart_ajaxFailure: function () {
    alert('Failed to add the product. Please refresh the page and try one more time.');
    location.reload()
  },

  adjustMiniCartQuantity: function (increase, productId, shoppingCartTypeId) {
    var currentQuantity = parseInt($('.b2bMiniCartProductQty_' + productId).val(), 10);
    if (increase) {
      $('.b2bMiniCartProductQty_' + productId).val(currentQuantity + 1)
      $('.shopping-cart-page-body .b2bProductQty_' + productId).val(currentQuantity + 1);
    }
    else {
      if (currentQuantity > 1) {
        $('.b2bMiniCartProductQty_' + productId).val(currentQuantity - 1)
        $('.shopping-cart-page-body .b2bProductQty_' + productId).val(currentQuantity - 1);
      }
      else {
        return;
      }
    }
    B2BMiniCart.updateMiniCartQuantity(productId, shoppingCartTypeId);
  },

  updateMiniCartQuantity: function (productId, shoppingCartTypeId) {
    clearTimeout(timeToWait);
    var currentQuantity = parseInt($('.b2bMiniCartProductQty_' + productId).val(), 10);
    $('.shopping-cart-page-body .b2bProductQty_' + productId).val(currentQuantity);
    //var shoppingCartTypeId = @shoppingCartTypeId;
    var urlCheck = '/IsShowAddToCart/';

    timeToWait = setTimeout(function () {
      B2BMiniCart.checkUserRegisteredAndUpdateProductQuantityInMiniCart(currentQuantity, productId, urlCheck, shoppingCartTypeId)
    }, 1000);
  },

  b2bLoadProductInMiniCartQuantity: function (productId) {
    $.ajax({
      cache: true,
      url: B2BAccountCommon.productInCartQuantityUrl,
      type: 'GET',
      data: { productId: productId },
      dataType: "json",
      success: function (response) {
        if (response.Data) {
          var productId = response.Data.Id;
          $('#b2bMiniCartProductQty_' + productId).val("" + response.Data.Quantity + "");
          $('.b2bMiniCartProductQty_' + productId).val("" + response.Data.Quantity + "");

          $('.shopping-cart-page-body #b2bProductQty_' + productId).val("" + response.Data.Quantity + "");
          $('.shopping-cart-page-body .b2bProductQty_' + productId).val("" + response.Data.Quantity + "");
        }
      },
      error: function () {
        console.log("Can Not Load Cart Info");
      }
    });
  }
};
