window.header = angular.module('header', []);

header.controller('HeaderController',
    ['$scope', '$location', 
    function ($scope, $location) {
        $scope.isActive = function (viewLocation) {
            return viewLocation === $location.path();
        };
        //$scope.username = window.UserName;
    }]);