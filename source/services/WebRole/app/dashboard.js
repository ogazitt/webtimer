// dashboard module
// this script hooks up all the components and must come first

window.dashboard = angular.module('dashboard', ["highcharts-ng", "toolbar", "header", "colorpicker-ng"]);
//var dashboard = angular.module('dashboard', ["highcharts-ng"]);

// Add global "services" to the Ng injector
dashboard.value('breeze', window.breeze)
         .value('Q', window.Q);

// Configure routes
dashboard.config(['$routeProvider', function ($routeProvider) {
    $routeProvider.
        when('/', { templateUrl: 'app/views/day.view.html', controller: 'DayController' }).
        when('/detail', { templateUrl: 'app/views/detail.day.view.html', controller: 'DetailController' }).
        when('/people', { templateUrl: 'app/views/people.view.html', controller: 'PeopleController' }).
        when('/devices', { templateUrl: 'app/views/devices.view.html', controller: 'DevicesController' }).
        when('/log', { templateUrl: 'app/views/log.view.html', controller: 'LogController' }).
        otherwise({ redirectTo: '/' });
}]);

//#region Ng directives
/*  We extend Angular with custom data bindings written as Ng directives */
dashboard.directive('onFocus', function () {
    return {
        restrict: 'A',
        link: function (scope, elm, attrs) {
            elm.bind('focus', function () {
                scope.$apply(attrs.onFocus);
            });
        }
    };
})
.directive('onBlur', function () {
    return {
        restrict: 'A',
        link: function (scope, elm, attrs) {
            elm.bind('blur', function () {
                scope.$apply(attrs.onBlur);
            });
        }
    };
})
.directive('onChange', function () {
    return {
        restrict: 'A',
        link: function (scope, elm, attrs) {
            elm.bind('change', function () {
                scope.$apply(attrs.onChange);
            });
        }
    };
})
.directive('onEnter', function () {
    return function (scope, element, attrs) {
        element.bind("keydown keypress", function (event) {
            if (event.which === 13) {
                scope.$apply(function () {
                    scope.$eval(attrs.onEnter);
                });
                // remove focus
                element.blur();
                event.preventDefault();
            }
        });
    };
})
.directive('selectedWhen', function () {
    return function (scope, elm, attrs) {
        scope.$watch(attrs.selectedWhen, function (shouldBeSelected) {
            if (shouldBeSelected) {
                elm.select();
            }
        });
    };
});
if (!Modernizr.input.placeholder) {
    // this browser does not support HTML5 placeholders
    // see http://stackoverflow.com/questions/14777841/angularjs-inputplaceholder-directive-breaking-with-ng-model
    dashboard.directive('placeholder', function () {
        return {
            restrict: 'A',
            require: 'ngModel',
            link: function (scope, element, attr, ctrl) {

                var value;

                var placeholder = function () {
                    element.val(attr.placeholder);
                };
                var unplaceholder = function () {
                    element.val('');
                };

                scope.$watch(attr.ngModel, function (val) {
                    value = val || '';
                });

                element.bind('focus', function () {
                    if (value == '') unplaceholder();
                });

                element.bind('blur', function () {
                    if (element.val() == '') placeholder();
                });

                ctrl.$formatters.unshift(function (val) {
                    if (!val) {
                        placeholder();
                        value = '';
                        return attr.placeholder;
                    }
                    return val;
                });
            }
        };
    });
}
//#endregion 