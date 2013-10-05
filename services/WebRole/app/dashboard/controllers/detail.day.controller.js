
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
        $scope.showTimeline = showTimeline;

        $scope.columnChartConfig = {
            options: {
                chart: {
                    type: 'column'
                },
                yAxis: {
                    title: {
                        text: 'Total number of minutes'
                    }
                },
                legend: {
                    enabled: false
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
                    }
                },
            },
            series: [],
            xAxis: {
                type: "category",
                categories: []
            },
            title: {
                text: null
            },
            loading: false
        };

        $scope.pieChartConfig = {
            options: {
                chart: {
                    type: 'pie'
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
            },
            series: [],
            xAxis: {
                categories: []
            },
            title: {
                text: null
            },
            loading: false
        };

        $scope.timelineChartConfig = {
            options: {
                chart: {
                    type: 'area',
                    height: 88,
                    plotBorderWidth: 1
                },
                legend: {
                    enabled: false
                },
                plotOptions: {
                    area: {
                        marker: {
                            enabled: false,
                            states: {
                                hover: {
                                    enabled: false
                                }
                            }
                        }
                    }
                },
                tooltip: {
                    enabled: false,
                },
                yAxis: {
                    title: {
                        text: null
                    },
                    labels: {
                        enabled: false
                    },
                    min: 0,
                    max: 1
                },
            },
            series: [],
            xAxis: {
                type: "category",
                categories: [],
                tickmarkPlacement: "on"
            },
            title: {
                text: null
            },
            loading: false,
        };

        $scope.$on('seriesDataChange', function $seriesDataChange(event, dataType) {
            switch (dataType) {
                case Queries.Categories:
                case Queries.Sites:
                    $scope.columnChartConfig.series = datacontext.getCurrentSeries(dataType);
                    $scope.pieChartConfig.series = $scope.columnChartConfig.series;
                    $scope.currentDate = datacontext.getCurrentDate();
                    break;
                case Queries.Timeline:
                    $scope.timelineChartConfig.series = datacontext.getCurrentSeries(dataType);
                    break;
            }
            refreshView();
        });

        // get the main data for column and pie charts
        getData();
        // get the timeline data if the person isn't null
        if (datacontext.getCurrentPerson() !== null && datacontext.getCurrentPeriod() === Periods.Day) {
            getData(Queries.Timeline);
        }

        //#region private functions 
        function getData(queryType) {
            return datacontext.getData(queryType).fail(failed);
        }

        function showTimeline() {
            return datacontext.getCurrentPerson() !== null && datacontext.getCurrentPeriod() === Periods.Day;
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
