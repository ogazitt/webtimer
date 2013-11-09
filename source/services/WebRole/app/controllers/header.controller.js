window.header = angular.module('header', []);

header.controller('HeaderController',
    ['$scope', '$location', 
    function ($scope, $location) {
        $scope.isActive = function (viewLocation) {
            // if the path is /detail, we are still in the dashboard
            if (viewLocation === '/' && $location.path() === '/detail') {
                return true;
            }
            return viewLocation === $location.path();
        };
        $scope.dashboard = function () {
            Events.Track(Events.Categories.DashboardHeader, Events.DashboardHeader.Dashboard);
            collapse();
        }
        $scope.people = function () {
            Events.Track(Events.Categories.DashboardHeader, Events.DashboardHeader.People);
            collapse();
        }
        $scope.devices = function () {
            Events.Track(Events.Categories.DashboardHeader, Events.DashboardHeader.Devices);
            collapse();
        }

        function collapse() {
            $(".nav-collapse").collapse('hide');
        }
    }]);