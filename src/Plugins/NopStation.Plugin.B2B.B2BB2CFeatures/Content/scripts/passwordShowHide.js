$(".password-toggle").click(function () {
  passwordToggle($(this));
});

function passwordToggle(element) {
  let passwordElement = element.prev();
  if (passwordElement.attr("type") === "password") {
    passwordElement.attr("type", "text");
    element.css('background', 'url("/Plugins/NopStation.Plugin.B2B.B2BB2CFeatures/Content/img/eye-show-password.png") center no-repeat');
  } else {
    passwordElement.attr("type", "password");
    element.css('background', 'url("/Plugins/NopStation.Plugin.B2B.B2BB2CFeatures/Content/img/eye-hide-password.png") center no-repeat');
  }
}