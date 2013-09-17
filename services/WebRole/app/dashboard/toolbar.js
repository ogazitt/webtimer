
window.toolbar = angular.module('toolbar', []);
//var dashboard = angular.module('dashboard', ["highcharts-ng"]);

// Add global "services" (like breeze and Q) to the Ng injector
// Learn about Angular dependency injection in this video
// http://www.youtube.com/watch?feature=player_embedded&v=1CpiB3Wk25U#t=2253s
//dashboard.value('breeze', window.breeze)
//         .value('Q', window.Q);

toolbar.controller('ToolbarController', function ($scope) {
    $scope.settings = {
        period: 'day',
        value: new Date(),
        format: 'shortDate',
        buttonTitle: 'Today'
    };

    $scope.selectDay = function () {
        this.settings.period = 'day';
        this.settings.format = 'shortDate';
        this.settings.buttonTitle = 'Today';
    };

    $scope.selectWeek = function () {
        this.settings.period = 'week';
        this.settings.format = 'shortDate';
        this.settings.buttonTitle = 'This Week';
    };

    $scope.selectMonth = function () {
        this.settings.period = 'month';
        this.settings.format = 'MMMM';
        this.settings.buttonTitle = 'This Month';
    };

    $scope.selectNow = function () {
        this.settings.value = new Date();
    };

    $scope.forward = function () {
        switch (this.settings.period) {
            case 'day':
                this.settings.value.setDate(this.settings.value.getDate() + 1);
                break;
            case 'week':
                this.settings.value.setDate(this.settings.value.getDate() + 7);
                break;
            case 'month':
                this.settings.value.setMonth(this.settings.value.getMonth() + 1);
                break;
        }
    };

    $scope.back = function () {
        switch (this.settings.period) {
            case 'day':
                this.settings.value.setDate(this.settings.value.getDate() - 1);
                break;
            case 'week':
                this.settings.value.setDate(this.settings.value.getDate() - 7);
                break;
            case 'month':
                this.settings.value.setMonth(this.settings.value.getMonth() - 1);
                break;
        }
    };
});