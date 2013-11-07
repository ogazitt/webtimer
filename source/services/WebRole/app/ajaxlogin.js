$(function () {
    var getValidationSummaryErrors = function ($form) {
        var errorSummary = $form.find('.validation-summary-errors, .validation-summary-valid');
        return errorSummary;
    };

    var displayErrors = function (form, errors) {
        var errorSummary = getValidationSummaryErrors(form)
            .removeClass('validation-summary-valid')
            .addClass('validation-summary-errors');

        var items = $.map(errors, function (error) {
            return '<li>' + error + '</li>';
        }).join('');

        errorSummary.find('ul').empty().append(items);
    };

    var formSubmitHandler = function ($form, e, spinner) {
        // We check if jQuery.validator exists on the form
        if (!$form.valid || $form.valid()) {
            $.post($form.attr('action'), $form.serializeArray())
                .done(function (json) {
                    json = json || {};

                    // In case of success, we redirect to the provided URL or the same page.
                    if (json.success) {
                        window.location = json.redirect || location.href;
                    } else if (json.errors) {
                        displayErrors($form, json.errors);
                        // stop the spinner
                        spinner.stop();
                    }
                })
                .error(function () {
                    displayErrors($form, ['An unknown error happened.']);
                    // stop the spinner
                    spinner.stop();
                });
        }

        // Prevent the normal behavior since we opened the dialog
        e.preventDefault();
    };

    $(".showRegister").click(function () {
        Events.Track(Events.Categories.LandingPage, Events.LandingPage.SignUpButton);
        $(".loginPanel").fadeOut("fast", function () {
            $("#registerPanel").fadeIn("fast", function () {
                $("#registerName").focus();
            });
        });
    });

    $(".showLogin").click(function () {
        Events.Track(Events.Categories.LandingPage, Events.LandingPage.SignInButton);
        $("#registerPanel").fadeOut("fast", function () {
            $(".loginPanel").fadeIn("fast", function () {
                $("#loginName").focus();
            });
        });
    });

    $("#loginForm").submit(function (e) {
        Events.Track(Events.Categories.LandingPage, Events.LandingPage.SignInFormPost);
        var spinner = createSpinner('loginSpinner');
        return formSubmitHandler($(this), e, spinner);
    });
    $("#registerForm").submit(function (e) {
        Events.Track(Events.Categories.LandingPage, Events.LandingPage.SignUpFormPost);
        var spinner = createSpinner('registerSpinner');
        return formSubmitHandler($(this), e, spinner);
    });

    var spinners = {};
    function createSpinner(spinnerName) {
        var target = document.getElementById(spinnerName);
        // reuse if already created
        if (spinners[spinnerName] !== undefined) {
            var spinner = spinners[spinnerName];
            spinner.spin(target);
            return spinner;
        }
        // create a new spinner
        var opts = {
            lines: 15, // The number of lines to draw
            length: 9, // The length of each line
            width: 2, // The line thickness
            radius: 2, // The radius of the inner circle
            corners: 1, // Corner roundness (0..1)
            rotate: 0, // The rotation offset
            direction: 1, // 1: clockwise, -1: counterclockwise
            color: '#000', // #rgb or #rrggbb or array of colors
            speed: 1.2, // Rounds per second
            trail: 100, // Afterglow percentage
            shadow: false, // Whether to render a shadow
            hwaccel: false, // Whether to use hardware acceleration
            className: 'spinner', // The CSS class to assign to the spinner
            zIndex: 2e9, // The z-index (defaults to 2000000000)
            top: 'auto', // Top position relative to parent in px
            left: 'auto' // Left position relative to parent in px
        };
        var spinner = new Spinner(opts).spin(target);
        spinners[spinnerName] = spinner;
        return spinner;
    }
});