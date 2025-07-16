function qtyUpdate(event, thisItem) {
  console.log("qty clicked");
  var qtyInput = thisItem.siblings(".qty-input");
  var currentValue = parseInt(qtyInput.val())  ;
  var btnValue = parseInt(thisItem.val()) ;
  currentValue = currentValue + btnValue;
  if (currentValue >= 1) {
    qtyInput.val(currentValue);
  } f

  return currentValue;
}
function ProductDetailsToolTip(thisButton, tippyContent) {
  tippy(thisButton, {
    content: tippyContent,
    placement: 'top',
  });
}
$(function () {
  $(".not-loggedin > a").on("click", function (e) {
    if ($('html').hasClass('not-authenticated') && $(window).width() <= 768) {
      e.stopPropagation();
      e.preventDefault();
      mobileDropdownDialog.dialog("open")
      return;
    }
    e.stopPropagation();
    e.preventDefault();
    $(".login-form-popup").toggleClass("show");
  });
  $(".login-form-popup").on("click", function (e) {
    e.stopPropagation();
  });
  $("html,body").on("click", function (e) {
    $(".login-form-popup").removeClass("show");
  });
  $(".qty-btn").on("click", function(e) {
    e.stopPropagation();
    qtyUpdate(e, $(this));
  })
});
$(function () {
  $(".block-manufacturer-navigation, .block-recently-viewed-products , .block-popular-tags").accordion({
    collapsible: true,
    active: false
  });
  
  $(".block-category-navigation").accordion({
    collapsible: true,
    animate: false
  });
  
});