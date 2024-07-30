var QuickOrderJs = {
  clearCart: function (deleteWishlistAndShoppingCartItems) {
    $.ajax({
      cache: false,
      type: "Post",
      dataType: "json",
      async: false,
      url: "/QuickOrder/ClearCart",
      data: { deleteWishlistAndShoppingCartItems: deleteWishlistAndShoppingCartItems },
      success: function (data) {
        if (data.success) {
          console.log(data.success);
          console.log("Shopping cart cleared");
        }
        else {
          console.log(data.success);
          console.log(data.message);
        }
      },
      error: function (jqXHR, textStatus, errorThrown) {
        console.log("Error. Can not clear cart");
      }
    });
  },

  disablePopUpAddToCartButton: function () {
    $('.quick-order-popup-clear-cart-button').prop('disabled', true);
    $('.quick-order-popup-add-to-cart-button').prop('disabled', true);
  },

  disableAddToCartButton: function () {
    $('.quick-order-add-to-cart-button').prop('disabled', true);
  },

  disableQuickOrderListAddToCartButton: function (templateId) {
    $('#quickOrderListOrderButton' + templateId).prop('disabled', true);
  },
};