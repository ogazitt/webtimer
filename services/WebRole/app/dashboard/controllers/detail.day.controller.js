
dashboard.controller('DetailController',
    ['$scope', 'breeze', 'datacontext', 'logger', '$location',
    function ($scope, breeze, datacontext, logger, $location) {

        logger.log("creating DetailController");

        if (datacontext.getCurrentPerson() === null) {
            $location.path('/');
        }

        $scope.error = "";
        $scope.currentDate = datacontext.getCurrentDate();
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
                    },
                    column: {
                        cursor: 'pointer',
                        point: {
                            events: {
                                click: function () {
                                    var currentQuery = datacontext.getCurrentQuery();
                                    switch (currentQuery) {
                                        case Queries.Categories:
                                            datacontext.setCurrentQuery(Queries.Sites)
                                            datacontext.setCurrentCategory(this.name);
                                            getData();
                                            break;
                                        case Queries.Sites:
                                            datacontext.setCurrentQuery(Queries.Categories)
                                            datacontext.setCurrentCategory(null);
                                            getData();
                                            break;
                                    }
                                }
                            }
                        },
/*
                        dataLabels: {
                            enabled: true,
                            color: colors[0],
                            style: {
                                fontWeight: 'bold'
                            },
                            formatter: function () {
                                return this.y + '%';
                            }
                        }
                        */
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
            $scope.columnChartConfig.xAxis.categories = [];
            refreshView();
        });

        getData();

        //#region private functions 
        function getData() {
            return datacontext.getData().fail(failed).fin(refreshView);
        }

        function calcPieChartSeries() {
            if ($scope.columnChartConfig.series === null || $scope.columnChartConfig.series.length == 0) {
                return;
            }
            var data = $scope.columnChartConfig.series[0].data;
            //return [ { data: pieData } ];
            return $scope.columnChartConfig.series;
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
