function qtyUpdate(event, thisItem) {
  var qtyInput = thisItem.siblings(".qty-input");
  var currentValue = parseInt(qtyInput.val())  ;
  var btnValue = parseInt(thisItem.val()) ;
  currentValue = currentValue + btnValue;
  if (currentValue >= 1) {
    qtyInput.val(currentValue);
  } 

  return currentValue;
}

function ProductDetailsToolTip(thisButton, tippyContent) {
  tippy(thisButton, {
    content: tippyContent,
    placement: 'top',
  });
}

$(function () {
  $(".qty-btn").on("click", function(e) {
    e.stopPropagation();
    qtyUpdate(e, $(this));
  })
});