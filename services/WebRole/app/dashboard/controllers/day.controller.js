
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

        /*

        $scope.addPoints = function () {
            var seriesArray = $scope.chartConfig.series;
            var rndIdx = Math.floor(Math.random() * seriesArray.length);
            seriesArray[rndIdx].data = seriesArray[rndIdx].data.concat([1, 10, 20]);
        };

        $scope.addSeries = function () {
            var rnd = [];
            for (var i = 0; i < 10; i++) {
                rnd.push(Math.floor(Math.random() * 20) + 1);
            }
            $scope.chartConfig.series.push({
                data: rnd
            });
        };

        $scope.removeRandomSeries = function () {
            var seriesArray = $scope.chartConfig.series;
            var rndIdx = Math.floor(Math.random() * seriesArray.length);
            seriesArray.splice(rndIdx, 1);
        };

        $scope.swapChartType = function () {
            if (this.chartConfig.options.chart.type === 'line') {
                this.chartConfig.options.chart.type = 'bar';
            } else {
                this.chartConfig.options.chart.type = 'line';
                this.chartConfig.options.chart.zoomType = 'x';
            }
        };

        $scope.toggleLoading = function () {
            this.chartConfig.loading = !this.chartConfig.loading;
        };

        $scope.toggleHighCharts = function () {
            this.chartConfig.useHighStocks = !this.chartConfig.useHighStocks;
        };

/*
        var colors = Highcharts.getOptions().colors,
        categories = ['MSIE', 'Firefox', 'Chrome', 'Safari', 'Opera'],
        name = 'Browser brands',
        data = [{
            y: 55.11,
            color: colors[0],
            drilldown: {
                name: 'MSIE versions',
                categories: ['MSIE 6.0', 'MSIE 7.0', 'MSIE 8.0', 'MSIE 9.0'],
                data: [10.85, 7.35, 33.06, 2.81],
                color: colors[0]
            }
        }, {
            y: 21.63,
            color: colors[1],
            drilldown: {
                name: 'Firefox versions',
                categories: ['Firefox 2.0', 'Firefox 3.0', 'Firefox 3.5', 'Firefox 3.6', 'Firefox 4.0'],
                data: [0.20, 0.83, 1.58, 13.12, 5.43],
                color: colors[1]
            }
        }, {
            y: 11.94,
            color: colors[2],
            drilldown: {
                name: 'Chrome versions',
                categories: ['Chrome 5.0', 'Chrome 6.0', 'Chrome 7.0', 'Chrome 8.0', 'Chrome 9.0',
                    'Chrome 10.0', 'Chrome 11.0', 'Chrome 12.0'],
                data: [0.12, 0.19, 0.12, 0.36, 0.32, 9.91, 0.50, 0.22],
                color: colors[2]
            }
        }, {
            y: 7.15,
            color: colors[3],
            drilldown: {
                name: 'Safari versions',
                categories: ['Safari 5.0', 'Safari 4.0', 'Safari Win 5.0', 'Safari 4.1', 'Safari/Maxthon',
                    'Safari 3.1', 'Safari 4.1'],
                data: [4.55, 1.42, 0.23, 0.21, 0.20, 0.19, 0.14],
                color: colors[3]
            }
        }, {
            y: 2.14,
            color: colors[4],
            drilldown: {
                name: 'Opera versions',
                categories: ['Opera 9.x', 'Opera 10.x', 'Opera 11.x'],
                data: [0.12, 0.37, 1.65],
                color: colors[4]
            }
        }];

        function setChart(name, categories, data, color) {
            chart.xAxis[0].setCategories(categories, false);
            chart.series[0].remove(false);
            chart.addSeries({
                name: name,
                data: data,
                color: color || 'white'
            }, false);
            chart.redraw();
        }
    }]);
        /*
        var chart = $('#container').highcharts({
            chart: {
                type: 'column'
            },
            title: {
                text: 'Browser market share, April, 2011'
            },
            subtitle: {
                text: 'Click the columns to view versions. Click again to view brands.'
            },
            xAxis: {
                categories: categories
            },
            yAxis: {
                title: {
                    text: 'Total percent market share'
                }
            },
            plotOptions: {
                column: {
                    cursor: 'pointer',
                    point: {
                        events: {
                            click: function () {
                                var drilldown = this.drilldown;
                                if (drilldown) { // drill down
                                    setChart(drilldown.name, drilldown.categories, drilldown.data, drilldown.color);
                                } else { // restore
                                    setChart(name, categories, data);
                                }
                            }
                        }
                    },
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
                }
            },
            tooltip: {
                formatter: function () {
                    var point = this.point,
                        s = this.x + ':<b>' + this.y + '% market share</b><br/>';
                    if (point.drilldown) {
                        s += 'Click to view ' + point.category + ' versions';
                    } else {
                        s += 'Click to return to browser brands';
                    }
                    return s;
                }
            },
            series: [{
                name: name,
                data: data,
                color: 'white'
            }],
            exporting: {
                enabled: false
            }
        })
*/