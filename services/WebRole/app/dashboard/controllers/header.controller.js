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
        //$scope.username = window.UserName;
    }]);