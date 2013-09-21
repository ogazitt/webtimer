
window.toolbar = angular.module('toolbar', []);
//var dashboard = angular.module('dashboard', ["highcharts-ng"]);

// Add global "services" (like breeze and Q) to the Ng injector
// Learn about Angular dependency injection in this video
// http://www.youtube.com/watch?feature=player_embedded&v=1CpiB3Wk25U#t=2253s
//dashboard.value('breeze', window.breeze)
//         .value('Q', window.Q);

toolbar.controller('ToolbarController',
    ['$scope', 'datacontext', 
    function ($scope, datacontext) {
        $scope.settings = {
            period: 'day',
            value: datacontext.getCurrentDate(),
            format: 'shortDate',
            buttonTitle: 'Today'
        };

        $scope.selectDay = function () {
            this.settings.period = 'day';
            datacontext.setCurrentPeriod(this.settings.period);
            this.settings.value = datacontext.getCurrentDate();
            this.settings.format = 'shortDate';
            this.settings.buttonTitle = 'Today';
            datacontext.getCatTotals();
        };

        $scope.selectWeek = function () {
            this.settings.period = 'week';
            datacontext.setCurrentPeriod(this.settings.period);
            this.settings.value = datacontext.getCurrentDate();
            this.settings.format = 'shortDate';
            this.settings.buttonTitle = 'This Week';
            datacontext.getCatTotals();
        };

        $scope.selectMonth = function () {
            this.settings.period = 'month';
            datacontext.setCurrentPeriod(this.settings.period);
            this.settings.value = datacontext.getCurrentDate();
            this.settings.format = 'MMMM';
            this.settings.buttonTitle = 'This Month';
            datacontext.getCatTotals();
        };

        $scope.selectNow = function () {
            this.settings.value = Date.today();
            datacontext.setCurrentDate(this.settings.value);
            datacontext.getCatTotals();
        };

        $scope.forward = function () {
            datacontext.moveForward();
            datacontext.getCatTotals();
        };

        $scope.back = function () {
            datacontext.moveBack();
            datacontext.getCatTotals();
        };
    }]);