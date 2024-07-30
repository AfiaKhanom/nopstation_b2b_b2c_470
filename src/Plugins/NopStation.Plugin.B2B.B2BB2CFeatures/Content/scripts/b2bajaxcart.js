/*
** nopCommerce ajax cart implementation
*/
var timeoutFlyout_cart;

function removeActiveClassFromFlyout_cart(minicartWaitingTime) {
  clearTimeout(timeoutFlyout_cart);
  timeoutFlyout_cart = setTimeout(function () {
    $('.flyout-cart').removeClass('active');
  }, minicartWaitingTime);
}

var timeToWait;
var B2BAjaxCart = {
  loadWaiting: false,
  usepopupnotifications: false,
  topcartselector: '',
  topwishlistselector: '',
  flyoutcartselector: '',

  init: function (usepopupnotifications, topcartselector, topwishlistselector, flyoutcartselector) {
    this.loadWaiting = false;
    this.usepopupnotifications = usepopupnotifications;
    this.topcartselector = topcartselector;
    this.topwishlistselector = topwishlistselector;
    this.flyoutcartselector = flyoutcartselector;
  },

  setLoadWaiting: function (display) {
    displayAjaxLoading(display);
    this.loadWaiting = display;
  },

  //add a product to the cart/wishlist from the catalog pages
  addproducttocart_catalog: function (urladd) {
    if (this.loadWaiting != false) {
      return;
    }
    this.setLoadWaiting(true);

    $.ajax({
      cache: false,
      url: urladd,
      async: false,
      type: "POST",
      success: this.success_process,
      complete: this.resetLoadWaiting,
      error: this.ajaxFailure
    });
  },

  //add a product to the cart/wishlist from the product details page
  addproducttocart_details: function (urladd, formselector) {
    if (this.loadWaiting != false) {
      return;
    }
    this.setLoadWaiting(true);

    $.ajax({
      cache: false,
      url: urladd,
      data: $(formselector).serialize(),
      type: "POST",
      success: this.success_process,
      complete: this.resetLoadWaiting,
      error: this.ajaxFailure
    });
  },

  plusMinusQuantityChange: function (actionType, productId, url) {
    var currentQuantity = parseInt($('.b2bProductQty_' + productId).val(), 10);

    if (isNaN(currentQuantity) || currentQuantity < 0) {
      B2BAccountCommon.b2bLoadProductInCartQuantity(productId);
    }
    else if (actionType == 'plus' || currentQuantity > 0) {
      B2BAjaxCart.addproducttocart_catalog(url);
    }
  },

  updateProductQuantityValueInCart_Catalog: function (currentQuantity, productId, shoppingCartTypeId) {
    if (!isNaN(currentQuantity) && currentQuantity >= 0) {
      var urladd = '/updateproductqtyincart/catalog/' + productId + '/' + shoppingCartTypeId + '/' + currentQuantity;
      B2BAjaxCart.addproducttocart_catalog(urladd);
    }
    else {
      B2BAccountCommon.b2bLoadProductInCartQuantity(productId);
    }
  },

  checkUserRegisteredAndUpdateProductQuantity: function (currentQuantity, productId, url, shoppingCartTypeId) {
    $.ajax({
      cache: false,
      type: "GET",
      url: url,
      success: function (data, textStatus, jqXHR) {
        if (data.success) {
          B2BAjaxCart.updateProductQuantityValueInCart_Catalog(currentQuantity, productId, shoppingCartTypeId);
        }
        else {
          B2BAjaxCart.promptForLogin();
        }
      },
      error: function (jqXHR, textStatus, errorThrown) {
        console.log("Can not get registered user info.");
      }
    });
  },

  checkUserRegisteredAndUpdateProductQuantityInMiniCart: function (currentQuantity, productId, url, shoppingCartTypeId) {
    displayAjaxLoading(true)
    $.ajax({
      cache: false,
      type: "GET",
      url: url,
      success: function (data, textStatus, jqXHR) {
        if (data.success) {
          B2BAjaxCart.updateProductQuantityValueInCartMini_Catalog(currentQuantity, productId, shoppingCartTypeId);
          displayAjaxLoading(false)
        }
        else {
          displayAjaxLoading(false)
          B2BAjaxCart.promptForLogin();
        }
      },
      error: function (jqXHR, textStatus, errorThrown) {
        console.log("Can not get registered user info.");
      }
    });
  },

  checkUserRegistered: function (actionType, productId, checkUserRegisteredUrl, actionUrl) {
    $.ajax({
      cache: false,
      type: "GET",
      url: checkUserRegisteredUrl,
      success: function (data, textStatus, jqXHR) {
        if (data.success) {
          B2BAjaxCart.plusMinusQuantityChange(actionType, productId, actionUrl); return false;
        }
        else {
          B2BAjaxCart.promptForLogin();
        }
      },
      error: function (jqXHR, textStatus, errorThrown) {
        console.log("Can not get registered user info.");
      }
    });
  },

  promptForLogin: function () {
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

  b2bproductAddToCart: function (productId, shoppingCartTypeId, urlCheck) {
    var currentQuantity = parseInt($('.b2bProductQty_' + productId).val(), 10);
    //var shoppingCartTypeId = @shoppingCartTypeId;
    //var urlCheck = '@checkUserRegisteredLink';

    B2BAjaxCart.checkUserRegisteredAndUpdateProductQuantity(currentQuantity, productId, urlCheck, shoppingCartTypeId)
  },

  success_process: function (response) {
    if (response.updatetopcartsectionhtml) {
      $(B2BAjaxCart.topcartselector).html(response.updatetopcartsectionhtml);
    }
    if (response.updatetopwishlistsectionhtml) {
      $(B2BAjaxCart.topwishlistselector).html(response.updatetopwishlistsectionhtml);
    }
    if (response.updateflyoutcartsectionhtml) {
      $(B2BAjaxCart.flyoutcartselector).replaceWith(response.updateflyoutcartsectionhtml);
    }
    //B2BAccountCommon.b2bLoadProductInCartQuantity(response.productId);
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

  resetLoadWaiting: function () {
    B2BAjaxCart.setLoadWaiting(false);
  },

  ajaxFailure: function () {
    alert('Failed to add the product. Please refresh the page and try one more time.');
    location.reload()
  },

  delayFun: function (fn, ms) {
    //alert("delay Fun")
    let cartTimer = 0
    return function (args) {
      clearTimeout(cartTimer)
      cartTimer = setTimeout(fn.bind(this, args), ms || 0)
    }
  },

  adjustMiniCartQuantity: function (increase, productId, shoppingCartTypeId) {
    var currentQuantity = parseInt($('.b2bProductQty_' + productId).val(), 10);
    if (increase) {
      $('.b2bProductQty_' + productId).val(currentQuantity + 1)
    }
    else {
      if (currentQuantity > 1) {
        $('.b2bProductQty_' + productId).val(currentQuantity - 1)
      }
      else {
        return;
      }
    }
    B2BAjaxCart.updateCartQuantity(productId, shoppingCartTypeId);
  },

  updateCartQuantity: function (productId, shoppingCartTypeId) {
    clearTimeout(timeToWait);
    var currentQuantity = parseInt($('.b2bProductQty_' + productId).val(), 10);
    //var shoppingCartTypeId = @shoppingCartTypeId;
    var urlCheck = '/IsShowAddToCart/';

    timeToWait = setTimeout(function () {
      B2BAjaxCart.checkUserRegisteredAndUpdateProductQuantityInMiniCart(currentQuantity, productId, urlCheck, shoppingCartTypeId)
    }, 1000);
  },

  b2BClearCart: function (deleteWishlistAndShoppingCartItems) {
    $.ajax({
      cache: false,
      type: "GET",
      dataType: "json",
      async: false,
      url: "/B2BClearCart/",
      data: { deleteWishlistAndShoppingCartItems: deleteWishlistAndShoppingCartItems },
      success: function (response) {
        console.log("Shopping cart cleared");
      },
      error: function (jqXHR, textStatus, errorThrown) {
        console.log("Error. Can not clear cart");
      }
    });
  },

  disablePopUpAddToCartButton: function () {
    $('.qoute-order-details-popup-clear-cart-button').prop('disabled', true);
    $('.qoute-order-details-popup-add-to-cart-button').prop('disabled', true);
  },

  disableAddToCartButton: function () {
    $('.quote-order-details-reorder-button').prop('disabled', true);
  },

  disableReorderButton: function (orderId) {
    $('#quoteOrderListReorderButton' + orderId).prop('disabled', true);
  },

  disableWishlistPopupButton: function () {
    $('.wishlist-popup-clear-cart').prop('disabled', true);
    $('.wishlist-popup-add-to-cart').prop('disabled', true);
  },

  disableWishlistAddToCartButton: function () {
    $('.wishlist-add-to-cart-button').prop('disabled', true);
  },

  adjustCartQuantity: function (increase, productId, shoppingCartTypeId) {
    var currentQuantity = parseInt($('.b2bProductQty_' + productId).val(), 10);
    if (increase) {
      $('.b2bProductQty_' + productId).val(currentQuantity + 1)
    }
    else {
      if (currentQuantity > 1) {
        $('.b2bProductQty_' + productId).val(currentQuantity - 1)
      }
      else {
        $('.b2bProductQty_' + productId).val("1")
      }
    }
    return;
  },
};
