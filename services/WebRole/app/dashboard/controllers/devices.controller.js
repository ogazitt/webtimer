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

        $scope.people = getPeople();
        $scope.devices = [];
        $scope.error = "";
        $scope.refresh = refresh;
        $scope.endEdit = endEdit;
        $scope.endEditPerson = endEditPerson;
        $scope.clearErrorMessage = clearErrorMessage;

        // load Devices immediately (from cache if possible)
        getDevices();

        //#region private functions 
        function getDevices() {
            datacontext.getDevices()
                .then(getSucceeded).fail(failed).fin(refreshView);
            function getSucceeded(data) {
                $scope.devices = data;
            }
        }
        function getPeople() {
            datacontext.getPeople()
                .then(getSucceeded);
            function getSucceeded(data) {
                $scope.people = data;
            }
        }
        function refresh() { getDevices(true); }

        function failed(error) {
            $scope.error = error.message;
        }
        function refreshView() {
            $scope.$apply();
        }
        function endEditPerson(entity) {
            // set the correct personId based on the name
            for (var i in $scope.people) {
                var person = $scope.people[i];
                if (person === entity.person) {
                    entity.personId = person.personId;
                    break;
                }
            }
            endEdit(entity);
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