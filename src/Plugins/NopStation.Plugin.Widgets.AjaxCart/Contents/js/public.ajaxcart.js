
var NopAjaxCart = {
  loadWaiting: false,
  updateShoppingCartButtonSelector: '',
  applyGiftCardCouponCodeButtonSelector: '',
  applyDiscountCouponCodeInputSelector: '',
  applyDiscountCouponCodeButtonSelector: '',
  applyGiftCardCouponCodeInputSelector: '',
  shoppingCartFormSelector: '',
  orderSummaryContainerSelector: '',
  updateCartUrl: '',
  applyDiscountCouponUrl: '',
  applyGiftCardUrl: '',
  removeDiscountCouponUrl: '',
  removeGiftCardUrl: '',
  updateWishlistUrl: '',
  localized_data: false,
  isWishlist: false,

  init: function (updateShoppingCartButtonSelector, applyDiscountCouponCodeButtonSelector, applyDiscountCouponCodeInputSelector,
    applyGiftCardCouponCodeButtonSelector, applyGiftCardCouponCodeInputSelector, shoppingCartFormSelector, orderSummaryContainerSelector,
    updateCartUrl, applyDiscountCouponUrl, applyGiftCardUrl, removeDiscountCouponUrl, removeGiftCardUrl, updateWishlistUrl, localized_data) {
    this.loadWaiting = false;
    this.updateShoppingCartButtonSelector = updateShoppingCartButtonSelector;
    this.applyDiscountCouponCodeButtonSelector = applyDiscountCouponCodeButtonSelector;
    this.applyDiscountCouponCodeInputSelector = applyDiscountCouponCodeInputSelector;
    this.applyGiftCardCouponCodeButtonSelector = applyGiftCardCouponCodeButtonSelector;
    this.applyGiftCardCouponCodeInputSelector = applyGiftCardCouponCodeInputSelector;
    this.shoppingCartFormSelector = shoppingCartFormSelector;
    this.orderSummaryContainerSelector = orderSummaryContainerSelector;
    this.updateCartUrl = updateCartUrl;
    this.applyDiscountCouponUrl = applyDiscountCouponUrl;
    this.applyGiftCardUrl = applyGiftCardUrl;
    this.removeDiscountCouponUrl = removeDiscountCouponUrl;
    this.removeGiftCardUrl = removeGiftCardUrl;
    this.updateWishlistUrl = updateWishlistUrl;
    this.localized_data = localized_data;
    this.isWishlist = !!$('.page.wishlist-page').length;

    this.preventDefaults();
  },

  preventDefaults: function () {
    $(NopAjaxCart.updateShoppingCartButtonSelector).click(function (event) {
      event.preventDefault();
      NopAjaxCart.updateCart();
    });

    $(NopAjaxCart.applyDiscountCouponCodeButtonSelector).click(function (event) {
      event.preventDefault();
      NopAjaxCart.applyCoupon();
    });

    $(NopAjaxCart.applyGiftCardCouponCodeButtonSelector).click(function (event) {
      event.preventDefault();
      NopAjaxCart.applyGiftCard();
    });

    $('button[name*="removediscount-"]').click(function (event) {
      event.preventDefault();
      NopAjaxCart.removeCoupon(this);
    });

    $('button[name*="removegiftcard-"]').click(function (event) {
      event.preventDefault();
      NopAjaxCart.removeGiftCard(this);
    });

    $(document).trigger({ type: "ajaxcart_initialized" });
  },

  updateCart: function () {
    NopAjaxCart.setLoadWaiting(true);

    var url = this.isWishlist ? this.updateWishlistUrl : this.updateCartUrl;
    var form = this.isWishlist ? $('.page.wishlist-page form:first-of-type') : $(NopAjaxCart.shoppingCartFormSelector);

    $.ajax({
      cache: false,
      url: url,
      data: form.serialize(),
      type: "POST",
      success: this.success_process,
      complete: this.resetLoadWaiting,
      error: NopAjaxCart.ajaxFailure
    });
  },

  applyCoupon: function () {
    NopAjaxCart.setLoadWaiting(true);

    var postData = {};
    postData.discountcouponcode = $(NopAjaxCart.applyDiscountCouponCodeInputSelector).val();
    addAntiForgeryToken(postData);

    $.ajax({
      cache: false,
      url: NopAjaxCart.applyDiscountCouponUrl,
      data: postData,
      type: "POST",
      success: this.success_process,
      complete: this.resetLoadWaiting,
      error: NopAjaxCart.ajaxFailure
    });
  },

  applyGiftCard: function () {
    NopAjaxCart.setLoadWaiting(true);

    var postData = {};
    postData.giftcardcouponcode = $(NopAjaxCart.applyGiftCardCouponCodeInputSelector).val();
    addAntiForgeryToken(postData);

    $.ajax({
      cache: false,
      url: NopAjaxCart.applyGiftCardUrl,
      data: postData,
      type: "POST",
      success: this.success_process,
      complete: this.resetLoadWaiting,
      error: NopAjaxCart.ajaxFailure
    });
  },

  removeCoupon: function (elem) {
    NopAjaxCart.setLoadWaiting(true);

    var postData = {};
    postData.key = $(elem).attr('name');
    addAntiForgeryToken(postData);

    $.ajax({
      cache: false,
      url: NopAjaxCart.removeDiscountCouponUrl,
      data: postData,
      type: "POST",
      success: this.success_process,
      complete: this.resetLoadWaiting,
      error: NopAjaxCart.ajaxFailure
    });
  },

  removeGiftCard: function (elem) {
    NopAjaxCart.setLoadWaiting(true);

    var postData = {};
    postData.key = $(elem).attr('name');
    addAntiForgeryToken(postData);

    $.ajax({
      cache: false,
      url: NopAjaxCart.removeGiftCardUrl,
      data: postData,
      type: "POST",
      success: this.success_process,
      complete: this.resetLoadWaiting,
      error: NopAjaxCart.ajaxFailure
    });
  },

  success_process: function (response) {
    NopAjaxCart.setLoadWaiting(false);
    if (response.html) {
      if (!NopAjaxCart.isWishlist)
        $(NopAjaxCart.orderSummaryContainerSelector).replaceWith(response.html);
      else {
        var page = $(response.html);
        $('.page.wishlist-page').replaceWith($('.page.wishlist-page', page));
      }
      NopAjaxCart.preventDefaults();
    }
    if (response.updatetopwishlistsectionhtml) {
      AjaxCart.success_process(response);
    }
  },

  setLoadWaiting: function (display) {
    displayAjaxCartLoader(display);
    NopAjaxCart.loadWaiting = display;
  },

  resetLoadWaiting: function () {
    NopAjaxCart.setLoadWaiting(false);
  },

  ajaxFailure: function () {
    alert(NopAjaxCart.localized_data.AjaxCartFailure);
  }
};
