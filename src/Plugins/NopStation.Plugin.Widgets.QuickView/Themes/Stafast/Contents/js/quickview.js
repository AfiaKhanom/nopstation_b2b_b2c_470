
var QuickView = {
  loadWaiting: false,
  picturezoomenabled: false,
  productdetailsurl: '',
  modalselector: '',
  modalbuttonselector: '',
  loaderselector: '',
  localized_data: false,

  init: function (picturezoomenabled, productdetailsurl, modalselector, modalbuttonselector, loaderselector, localized_data) {
    this.loadWaiting = false;
    this.picturezoomenabled = picturezoomenabled;
    this.productdetailsurl = productdetailsurl;
    this.modalselector = modalselector;
    this.modalbuttonselector = modalbuttonselector;
    this.loaderselector = loaderselector;
    this.localized_data = localized_data;
  },

  setLoadWaiting: function (display) {
    if (display) {
      $(QuickView.loaderselector).show();
    }
    else {
      $(QuickView.loaderselector).hide();
    }
    this.loadWaiting = display;
  },

  render_quickview_button: function (label) {
    $('.product-item[data-quick-view!="true"]').each(function (i, obj) {
      var productid = $(this).data('productid');
      var html = '<button type="button" class="button-2 quick-view-btn" onmouseover="ProductToolTip(this, \'' + label + '\')" data-productid="' + productid + '" title="' + label + '">' + label + '</button>';
      $(this).children('.details').children('.buttons').append(html);
      $(this).attr("data-quick-view", true);
    });

    $('.quick-view-btn').click(function (event) {
      event.preventDefault();
      var productid = $(this).data('productid');
      QuickView.load_product_details(productid);
    })
  },

  load_product_details: function (productid) {
    if (this.loadWaiting !== false) {
      return;
    }
    this.setLoadWaiting(true);
    $(QuickView.modalselector + ' .qv-details').html('');
    $(QuickView.modalselector).css('display', 'block');

    $(QuickView.modalbuttonselector).magnificPopup({
      mainClass: 'quickview-popup',
      items: {
        src: QuickView.modalselector,
        type: 'inline'
      },
      preloader: true
    });
    $(QuickView.modalbuttonselector).click();

    $.ajax({
      cache: false,
      type: 'POST',
      data: { productId: productid },
      url: QuickView.productdetailsurl,
      success: this.success_process,
      error: this.ajaxFailure,
      complete: this.resetLoadWaiting
    });
  },

  success_process: function (response) {
    QuickView.setLoadWaiting(false);
    $(QuickView.modalselector + ' .qv-details').html(response.html);

    $(QuickView.modalselector + " .mfp-close").on("click", function () {
      $(QuickView.modalselector).hide();
      $(QuickView.modalselector + ' .qv-details').html('');
    });

    $('.qv-modal .also-purchased-products-grid > .item-grid').wrap("<div class='swiper' />")
    $(QuickView.modalselector + ' .item-grid').addClass("swiper-wrapper");
    $(QuickView.modalselector + ' .item-box').addClass("swiper-slide");;
    $('.qv-modal .related-products-grid > .item-grid ').wrapAll("<div class='swiper' />");
    $(QuickView.modalselector + ' .swiper').append('<div class="swiper-button-next"/>');
    $(QuickView.modalselector + ' .swiper').append('<div class="swiper-button-prev"/>');

    var qVrelatedSwiper = new Swiper(".qv-modal .related-products-grid .swiper", {
      navigation: {
        nextEl: ".qv-modal .related-products-grid .swiper-button-next",
        prevEl: ".qv-modal .related-products-grid .swiper-button-prev",
      },
      slidesPerView: "auto",
    });
    var alsoPurchasedSwiper = new Swiper(".qv-modal .also-purchased-products-grid .swiper", {
      navigation: {
        nextEl: ".qv-modal .also-purchased-products-grid  .swiper-button-next",
        prevEl: ".qv-modal .also-purchased-products-grid  .swiper-button-prev",
      },
      slidesPerView: "auto",
    });
  },

  resetLoadWaiting: function () {
    QuickView.setLoadWaiting(false);
  },

  ajaxFailure: function () {
    alert(this.localized_data.QuickViewFailure);
  }
};