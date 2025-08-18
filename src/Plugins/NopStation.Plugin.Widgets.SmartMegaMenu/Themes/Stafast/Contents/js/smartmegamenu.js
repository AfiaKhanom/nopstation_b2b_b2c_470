$(document).ready(function () {
  const windowWidth = $(window).width();

  function preventAndStopPropagation(e) {
    e.preventDefault();
    e.stopPropagation();
  }

  function hideMenu() {
    $(".header-menu, .header-menu .sublist, .menu-overlay").removeClass("show");
  }

  $(".mobile-nav-toggle").on("click", function (e) {
    if (windowWidth < 1200) {
      preventAndStopPropagation(e);
      $(".header-menu").addClass("show");
      $(".menu-overlay").addClass("show");
    }
  });

  $(".nav-menu .sublist-toggle").on("click", function (e) {
    if (windowWidth < 1200) {
      preventAndStopPropagation(e);
      $(this).siblings(".sublist").addClass("show");
    }
  });

  $(".parent-back-link").on("click", function (e) {
    if (windowWidth < 1200) {
      preventAndStopPropagation(e);
      $(this).parents(".sublist").removeClass("show");
    }
  });

  $(".mobile-menu-close, .menu-overlay").on("click", function (e) {
    preventAndStopPropagation(e);
    hideMenu();
  });

  $(".header-menu").on("click", function (e) {
    e.stopPropagation();
  });

  $("html,body").on("click", function (e) {
    //e.stopPropagation();
    hideMenu();
  });
});
