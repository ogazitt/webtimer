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
        var removeList = breeze.core.arrayRemoveItem;

        $scope.people = getPeople();
        $scope.devices = [];
        $scope.error = "";
        $scope.refresh = refresh;
        $scope.endEdit = endEdit;
        $scope.endEditPerson = endEditPerson;
        $scope.deleteDevice = deleteDevice;
        $scope.toggleEnabled = toggleEnabled;
        $scope.clearErrorMessage = clearErrorMessage;
        $scope.noDevices = false;

        // load Devices immediately (from cache if possible)
        getDevices();

        //#region private functions 
        function getDevices() {
            datacontext.getDevices()
                .then(getSucceeded).fail(failed).fin(refreshView);
            function getSucceeded(data) {
                $scope.devices = data;
                if (data.length == 0) {
                    $scope.noDevices = true;
                }
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
            Events.Track(Events.Categories.Devices, Events.Devices.Change);
            datacontext.saveEntity(entity).fin(refreshView);
        }
        function deleteDevice(device) {
            // confirm the operation
            Control.confirm(
                'Are you sure you want to delete this device?  If you proceed, you will need to re-associate ' +
                'the device using the WebTimer Configuration tool if you ever want to monitor it again.',
                'Delete Device?',
                function $deleteOK() {
                    // delete the device
                    Events.Track(Events.Categories.Devices, Events.Devices.Delete);
                    removeList($scope.devices, device);
                    datacontext.deleteDevice(device)
                        .fail(deleteFailed)
                        .fin(refreshView);

                    function deleteFailed() {
                        showAddedDevice(person); // re-show the restored list
                    }
                },
                function $deleteCancel() {
                });
        }
        function toggleEnabled(device) {
            Events.Track(Events.Categories.Devices, Events.Devices.EnabledChange);
            device.enabled = !device.enabled;
            endEdit(device);
        }
        function clearErrorMessage(obj) {
            if (obj && obj.errorMessage) {
                obj.errorMessage = null;
            }
        }
        function showAddedDevice(device) {
            $scope.devices.push(device); // Insert device at the back
        }
        //#endregion
    }]);