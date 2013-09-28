
window.toolbar = angular.module('toolbar', []);
//var dashboard = angular.module('dashboard', ["highcharts-ng"]);

// Add global "services" (like breeze and Q) to the Ng injector
// Learn about Angular dependency injection in this video
// http://www.youtube.com/watch?feature=player_embedded&v=1CpiB3Wk25U#t=2253s
//dashboard.value('breeze', window.breeze)
//         .value('Q', window.Q);

toolbar.controller('ToolbarController',
    ['$scope', 'datacontext', '$location',
    function ($scope, datacontext, $location) {

        $scope.settings = {
            period: 'day',
            person: null,
            value: datacontext.getCurrentDate(),
            format: 'shortDate',
            buttonTitle: 'Today'
        };

        $scope.people = getPeople();

        $scope.showToolbar = function $showToolbar() {
            return $location.path() !== '/people' &&
                   $location.path() !== '/devices';
        }

        $scope.isActivePeriod = function $isActivePeriod(period) {
            return $scope.settings.period === period;
        };

        $scope.isActivePerson = function $isActivePerson(person) {
            return $scope.settings.person === person;
        };

        $scope.selectPerson = function $selectPerson(newPerson) {
            var oldPerson = $scope.settings.person;

            // only do anything if the person actually changed
            if (oldPerson !== newPerson) {
                // set the new person
                datacontext.setCurrentPerson(newPerson !== null ? newPerson.personId : null);
                $scope.settings.person = newPerson;

                // only reload the data if we switched from a person to another person
                // in all other cases, the view transition handles the data access
                if (oldPerson !== null && newPerson !== null) {
                    datacontext.getCatTotals().then(getSucceeded).fail(getFailed);
                    function getSucceeded(data) {
                        $scope.$apply();
                    }
                    function getFailed() {
                        var i = 0;
                    }
                }
            }
        };

        $scope.selectDay = function $selectDay() {
            this.settings.period = 'day';
            datacontext.setCurrentPeriod(this.settings.period);
            this.settings.value = datacontext.getCurrentDate();
            this.settings.format = 'shortDate';
            this.settings.buttonTitle = 'Today';
            datacontext.getCatTotals();
        };

        $scope.selectWeek = function $selectWeek() {
            this.settings.period = 'week';
            datacontext.setCurrentPeriod(this.settings.period);
            this.settings.value = datacontext.getCurrentDate();
            this.settings.format = 'shortDate';
            this.settings.buttonTitle = 'This Week';
            datacontext.getCatTotals();
        };

        $scope.selectMonth = function $selectMonth() {
            this.settings.period = 'month';
            datacontext.setCurrentPeriod(this.settings.period);
            this.settings.value = datacontext.getCurrentDate();
            this.settings.format = 'MMMM';
            this.settings.buttonTitle = 'This Month';
            datacontext.getCatTotals();
        };

        $scope.selectNow = function $selectNow() {
            this.settings.value = Date.today();
            datacontext.setCurrentDate(this.settings.value);
            datacontext.getCatTotals();
        };

        $scope.forward = function $forward() {
            datacontext.moveForward();
            datacontext.getCatTotals();
        };

        $scope.back = function $back() {
            datacontext.moveBack();
            datacontext.getCatTotals();
        };

        function getPeople() {
            datacontext.getPeople().then(getSucceeded);

            function getSucceeded(data) {
                $scope.people = data;
                $scope.$apply();
            }
        }

    }]);