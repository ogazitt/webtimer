/* Defines the "device view" controller
 * Constructor function relies on Ng injector to provide:
 *     $scope - context variable for the view to which the view binds
 *     breeze - breeze is a "module" known to the injector thanks to dashboard.js
 *     datacontext - injected data and model access component (dashboard.datacontext.js)
 *     logger - records notable events during the session (logger.js)
 */
dashboard.controller('DevicesController',
    ['$scope', 'breeze', 'datacontext', 'logger',
    function ($scope, breeze, datacontext, logger) {

        logger.log("creating DevicesController");

        $scope.devices = [];
        $scope.error = "";
        $scope.getDevices = getDevices;
        $scope.refresh = refresh;
        $scope.endEdit = endEdit;
        $scope.clearErrorMessage = clearErrorMessage;

        // load Devices immediately (from cache if possible)
        $scope.getDevices();

        //#region private functions 
        function getDevices(forceRefresh) {
            datacontext.getDevices(forceRefresh)
                .then(getSucceeded).fail(failed).fin(refreshView);
        }
        function refresh() { getDevices(true); }

        function getSucceeded(data) {
            $scope.devices = data;
        }
        function failed(error) {
            $scope.error = error.message;
        }
        function refreshView() {
            $scope.$apply();
        }
        function endEdit(entity) {
            datacontext.saveEntity(entity).fin(refreshView);
        }
        function clearErrorMessage(obj) {
            if (obj && obj.errorMessage) {
                obj.errorMessage = null;
            }
        }
        //#endregion
    }]);