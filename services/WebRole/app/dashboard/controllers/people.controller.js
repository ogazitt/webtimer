/* Defines the "people view" controller
 * Constructor function relies on Ng injector to provide:
 *     $scope - context variable for the view to which the view binds
 *     breeze - breeze is a "module" known to the injector thanks to dashboard.js
 *     datacontext - injected data and model access component (dashboard.datacontext.js)
 *     logger - records notable events during the session (logger.js)
 */
dashboard.controller('PeopleController',
    ['$scope', 'breeze', 'datacontext', 'logger',
    function ($scope, breeze, datacontext, logger) {

        logger.log("creating PeopleController");
        var removeList = breeze.core.arrayRemoveItem;

        $scope.people = [];
        $scope.error = "";
        $scope.getPeople = getPeople;
        $scope.refresh = refresh;
        $scope.endEdit = endEdit;
        $scope.addPerson = addPerson;
        $scope.deletePerson = deletePerson;
        $scope.toggleIsChild = toggleIsChild;
        $scope.clearErrorMessage = clearErrorMessage;

        // load People immediately (from cache if possible)
        $scope.getPeople();

        //#region private functions 
        function getPeople(forceRefresh) {
            datacontext.getPeople(forceRefresh)
                .then(getSucceeded).fail(failed).fin(refreshView);
        }
        function refresh() { getTodos(true); }

        function getSucceeded(data) {
            // clone the array
            $scope.people = data.slice(0);
            // remove "shared"
            $scope.people.shift();
        }
        function failed(error) {
            $scope.error = error.message;
        }
        function refreshView() {
            /*
            $(".pick-a-color").each(function (index) {
                alert($(this).text());
            });
            */
            $scope.$apply();
        }
        function endEdit(entity) {
            datacontext.saveEntity(entity)
                .then(clearEditMode)
                .fin(refreshView);
        }
        function addPerson() {
            var person = datacontext.createPerson();
            person.isEditingPersonName = true;
            datacontext.saveEntity(person)
                .then(addSucceeded)
                .fail(addFailed)
                .fin(refreshView);

            function addSucceeded() {
                showAddedPerson(person);
            }

            function addFailed(error) {
                failed({ message: "Save of new todoList failed" });
            }
        }
        function deletePerson(person) {
            removeList($scope.people, person);
            datacontext.deletePerson(person)
                .fail(deleteFailed)
                .fin(refreshView);

            function deleteFailed() {
                showAddedPerson(person); // re-show the restored list
            }
        }
        function toggleIsChild(person) {
            person.isChild = !person.isChild;
            endEdit(person);
        }
        function clearErrorMessage(obj) {
            if (obj && obj.errorMessage) {
                obj.errorMessage = null;
            }
        }
        function showAddedPerson(person) {
            $scope.people.push(person); // Insert person at the back
        }
        function clearEditMode(person) {
            person.isEditingPersonName = false;
        }
        //#endregion
    }]);