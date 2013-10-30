// toolbar controller
window.toolbar = angular.module('toolbar', [])
.controller('ToolbarController',
    ['$scope', 'datacontext', '$location',
    function ($scope, datacontext, $location) {
        $scope.settings = {
            period: Periods.Day,
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
                Events.Track(Events.Categories.DashboardToolbar, Events.DashboardToolbar.PersonFilter);

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
                    datacontext.getData(Queries.Timeline).then(getSucceeded).fail(getFailed);
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
            Events.Track(Events.Categories.DashboardToolbar, Events.DashboardToolbar.Day);
            $scope.settings.period = Periods.Day;
            datacontext.setCurrentPeriod(this.settings.period);
            $scope.settings.value = datacontext.getCurrentDate();
            $scope.settings.format = 'shortDate';
            $scope.settings.buttonTitle = 'Today';
            datacontext.getData();
            if ($scope.settings.person !== null) {
                datacontext.getData(Queries.Timeline);
            }
        };

        $scope.selectWeek = function $selectWeek() {
            Events.Track(Events.Categories.DashboardToolbar, Events.DashboardToolbar.Week);
            $scope.settings.period = Periods.Week;
            datacontext.setCurrentPeriod(this.settings.period);
            $scope.settings.value = datacontext.getCurrentDate();
            $scope.settings.format = 'shortDate';
            $scope.settings.buttonTitle = 'This Week';
            datacontext.getData();
        };

        $scope.selectMonth = function $selectMonth() {
            Events.Track(Events.Categories.DashboardToolbar, Events.DashboardToolbar.Month);
            $scope.settings.period = Periods.Month;
            datacontext.setCurrentPeriod(this.settings.period);
            $scope.settings.value = datacontext.getCurrentDate();
            $scope.settings.format = 'MMMM';
            $scope.settings.buttonTitle = 'This Month';
            datacontext.getData();
        };

        $scope.selectNow = function $selectNow() {
            Events.Track(Events.Categories.DashboardToolbar, Events.DashboardToolbar.Now);
            $scope.settings.value = Date.today();
            datacontext.setCurrentDate(this.settings.value);
            datacontext.getData();
            if ($scope.settings.person !== null && $scope.settings.period === Periods.Day) {
                datacontext.getData(Queries.Timeline);
            }
        };

        $scope.forward = function $forward() {
            Events.Track(Events.Categories.DashboardToolbar, Events.DashboardToolbar.Forward);
            datacontext.moveForward();
            datacontext.getData();
            if ($scope.settings.person !== null && $scope.settings.period === Periods.Day) {
                datacontext.getData(Queries.Timeline);
            }
        };

        $scope.back = function $back() {
            Events.Track(Events.Categories.DashboardToolbar, Events.DashboardToolbar.Back);
            datacontext.moveBack();
            datacontext.getData();
            if ($scope.settings.person !== null && $scope.settings.period === Periods.Day) {
                datacontext.getData(Queries.Timeline);
            }
        };

        $scope.refresh = function $refresh() {
            Events.Track(Events.Categories.DashboardToolbar, Events.DashboardToolbar.Refresh);
            datacontext.getData();
            if ($scope.settings.person !== null && $scope.settings.period === Periods.Day) {
                datacontext.getData(Queries.Timeline);
            }
        };

        function getPeople() {
            datacontext.getPeople().then(getSucceeded);
            function getSucceeded(data) {
                $scope.people = data;
                $scope.$apply();
            }
        }
    }]);