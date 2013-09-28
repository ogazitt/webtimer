
dashboard.controller('DetailController',
    ['$scope', 'breeze', 'datacontext', 'logger', '$location',
    function ($scope, breeze, datacontext, logger, $location) {

        logger.log("creating DetailController");

        if (datacontext.getCurrentPerson() === null) {
            $location.path('/');
        }

        $scope.error = "";
        $scope.currentDate = datacontext.getCurrentDate();
        $scope.getCategoryTotals = getCategoryTotals;
        $scope.refresh = refresh;
        $scope.clearErrorMessage = clearErrorMessage;

        $scope.columnChartConfig = {
            options: {
                chart: {
                    type: 'column'
                },
                plotOptions: {
                    series: {
                        stacking: null
                    }
                },
                yAxis: {
                    title: {
                        text: 'Total number of minutes'
                    }
                },
                title: {
                    text: ''
                },
            },
            series: [],
            xAxis: {
                categories: []
            },
            loading: false
        };

        $scope.pieChartConfig = {
            options: {
                chart: {
                    type: 'pie'
                },
            },
            series: [],
            xAxis: {
                categories: []
            },
            loading: false
        };

        /*
        $scope.timeChartConfig = {
            options: {
                chart: {
                    type: 'bar'
                },
                plotOptions: {
                    series: {
                        stacking: 'normal'
                    }
                },
            },
            series: [],
            loading: false
        };
        */

        $scope.$on('seriesDataChange', function $seriesDataChange() {
            $scope.columnChartConfig.series = datacontext.getCurrentSeries();
            //$scope.pieChartConfig.series = calcPieChartSeries();
            $scope.pieChartConfig.series = $scope.columnChartConfig.series;
            $scope.currentDate = datacontext.getCurrentDate();
            refreshView();
        });

        $scope.getCategoryTotals();

        //#region private functions 
        function getCategoryTotals() {
            return datacontext.getCatTotals()
                .then(getCategoryTotalsSucceeded).fail(failed).fin(refreshView);
        }

        function calcPieChartSeries() {
            if ($scope.columnChartConfig.series === null || $scope.columnChartConfig.series.length == 0) {
                return;
            }
            var data = $scope.columnChartConfig.series[0].data;
            /*
            var total = 0.0;
            for (index in data) {
                total += data[index];
            }
            
            var pieData = [];
            for (index in data) {
                pieData.push(data[index]) / total;
            }
            */

            return [ { data: pieData } ];
        }

        function refresh() { getCategoryTotals(true); }

        function getCategoryTotalsSucceeded(data) {
            //$scope.chartConfig.series = data;
        }
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
