$(function () {
    const $loginForm = $("#loginForm");
    const $submitBtn = $loginForm.find("button[type=submit]");
    const $userName = $("#LoginInput_UserNameOrEmailAddress");
    const $passwordInput = $("#LoginInput_Password");
    const $showpassIcon = $("#showpassicon");
    const $capslockIcon = $("#capslockicon");
    const $rememberMe = $("#LoginInput_RememberMe");
    //const localization = abp.localization.getResource('Pms');

    $userName.val(localStorage.getItem('username'));

    $submitBtn.removeAttr("disabled");
    $submitBtn.find('i').removeClass('spinner-border spinner-border-sm').addClass('bi-box-arrow-in-right');

    $loginForm.on("submit", function (e) {
        if (!$(this).valid()) return;
        setTimeout(() => $submitBtn.prop('disabled', true), 10);
        $submitBtn.find('i').removeClass('bi-box-arrow-in-right').addClass('spinner-border spinner-border-sm');
        if ($rememberMe.prop('checked')) {
            localStorage.setItem('username', $userName.val());
        }
    });

    $showpassIcon.on("click", function () {
        if (!$passwordInput.length) return;
        const isPassword = $passwordInput.attr("type") === "password";
        $passwordInput.attr("type", isPassword ? "text" : "password");
        $showpassIcon && $showpassIcon.toggleClass("bi-eye-slash").toggleClass("bi-eye");
    });
    
    $passwordInput && $capslockIcon && document.getElementById("LoginInput_Password").addEventListener("keyup", e => {
        if (typeof e.getModifierState == "function") {
            $capslockIcon.css("display", e.getModifierState("CapsLock") ? "inline" : "none");
        }
    });
});
