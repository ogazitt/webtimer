// toolbar controller
window.toolbar = angular.module('toolbar', [])
.controller('ToolbarController',
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
            if ($scope.settings.person === null || person === null) {
                return $scope.settings.person === person;
            } else {
                return $scope.settings.person.personId === person.personId;
            }
        };

        $scope.selectPerson = function $selectPerson(newPerson) {
            var oldPerson = $scope.settings.person;

            // only do anything if the person actually changed
            if (oldPerson !== newPerson) {
                // set the new person
                datacontext.setCurrentPerson(newPerson);
                $scope.settings.person = newPerson;

                // only reload the data if we switched from a person to another person
                // in all other cases, the view transition handles the data access
                if (oldPerson !== null && newPerson !== null) {
                    // pop out to category view
                    datacontext.setCurrentCategory(null);
                    datacontext.setCurrentQuery(Queries.Categories);
                    datacontext.getData().then(getSucceeded).fail(getFailed);
                    function getSucceeded(data) {
                        $scope.$apply();
                    }
                    function getFailed() {
                        var i = 0;
                    }
                }
            }
        };

        $scope.$on('seriesDataChange', function $seriesDataChange() {
            $scope.settings.person = datacontext.getCurrentPerson();
            $scope.$apply();
        });

        $scope.selectDay = function $selectDay() {
            this.settings.period = 'day';
            datacontext.setCurrentPeriod(this.settings.period);
            this.settings.value = datacontext.getCurrentDate();
            this.settings.format = 'shortDate';
            this.settings.buttonTitle = 'Today';
            datacontext.getData();
        };

        $scope.selectWeek = function $selectWeek() {
            this.settings.period = 'week';
            datacontext.setCurrentPeriod(this.settings.period);
            this.settings.value = datacontext.getCurrentDate();
            this.settings.format = 'shortDate';
            this.settings.buttonTitle = 'This Week';
            datacontext.getData();
        };

        $scope.selectMonth = function $selectMonth() {
            this.settings.period = 'month';
            datacontext.setCurrentPeriod(this.settings.period);
            this.settings.value = datacontext.getCurrentDate();
            this.settings.format = 'MMMM';
            this.settings.buttonTitle = 'This Month';
            datacontext.getData();
        };

        $scope.selectNow = function $selectNow() {
            this.settings.value = Date.today();
            datacontext.setCurrentDate(this.settings.value);
            datacontext.getData();
        };

        $scope.forward = function $forward() {
            datacontext.moveForward();
            datacontext.getData();
        };

        $scope.back = function $back() {
            datacontext.moveBack();
            datacontext.getData();
        };

        function getPeople() {
            datacontext.getPeople().then(getSucceeded);
            function getSucceeded(data) {
                $scope.people = data;
                $scope.$apply();
            }
        }
    }]);