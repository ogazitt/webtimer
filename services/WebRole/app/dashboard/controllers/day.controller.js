
dashboard.controller('DayController',
    ['$scope', 'breeze', 'datacontext', 'logger',
    function ($scope, breeze, datacontext, logger) {

        logger.log("creating DayController");

        $scope.error = "";
        $scope.currentDate = datacontext.getCurrentDate();
        $scope.refresh = refresh;
        $scope.clearErrorMessage = clearErrorMessage;

        $scope.chartConfig = {
            options: {
                chart: {
                    type: 'column'
                },
                yAxis: {
                    title: {
                        text: 'Total number of minutes'
                    }
                },
            },
            series: [],
            xAxis: {
                categories: []
            },
            title: {
                text: chartTitle()
            },
            loading: false
        };

        $scope.$on('seriesDataChange', function $seriesDataChange(event, dataType) {
            $scope.chartConfig.series = datacontext.getCurrentSeries(Queries.Categories);
            $scope.currentDate = datacontext.getCurrentDate();
            $scope.chartConfig.title = { text: chartTitle() };
            refreshView();
        });

        // reset person filter to no filter
        datacontext.setCurrentPerson(null);
        // reset query to Categories
        datacontext.setCurrentQuery(Queries.Categories);
        // get the data
        getData();
        

        //#region private functions 
        function getData() {
            return datacontext.getData()
                .then(getSucceeded).fail(failed);
            function getSucceeded(data) {
            }
        }

        function chartTitle() {
            return 'Web usage for ' + $scope.currentDate.toString('M/d/yyyy') + ' by category';
        }

        function refresh() { getCategoryTotals(true); }

        function failed(error) {
            $scope.error = error.message;
        }
        function refreshView() {
            $scope.$apply();
        }
        function clearErrorMessage(obj) {
            if (obj && obj.errorMessage) {
                obj.errorMessage = null;
            }
        }
        //#endregion
    }]);

