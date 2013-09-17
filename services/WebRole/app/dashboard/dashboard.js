// dashboard module
// this script hooks up all the components and must come first

window.dashboard = angular.module('dashboard', ["highcharts-ng", "toolbar"]);
//var dashboard = angular.module('dashboard', ["highcharts-ng"]);

// Add global "services" (like breeze and Q) to the Ng injector
// Learn about Angular dependency injection in this video
// http://www.youtube.com/watch?feature=player_embedded&v=1CpiB3Wk25U#t=2253s
dashboard.value('breeze', window.breeze)
         .value('Q', window.Q);

// Configure routes
dashboard.config(['$routeProvider', function ($routeProvider) {
    $routeProvider.
        when('/', { templateUrl: 'app/dashboard/day.view.html', controller: 'DayController' }).
        when('/week', { templateUrl: 'app/dashboard/week.view.html', controller: 'WeekController' }).
        otherwise({ redirectTo: '/' });
}]);

