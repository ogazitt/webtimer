/* datacontext: data access and model management layer */

var Queries = {
    Categories: "CategoryTotals",
    Sites: "ConsolidatedWebSessions",
    Timeline: "WebSessionTimeline",
};

var Periods = {
    Day: "day",
    Week: "week",
    Month: "month"
}

// create and add datacontext to the Ng injector
// constructor function relies on Ng injector
// to provide service dependencies
dashboard.factory('datacontext',
    ['breeze', 'Q', 'model', 'logger', '$timeout', '$rootScope',
    function (breeze, Q, model, logger, $timeout, $rootScope) {

        logger.log("creating datacontext");
        var initializedSessions, initializedPeople, initializedDevices;

        // model variables
        var currentDate = Date.today();
        var currentPeriod = 'day';
        var currentSeries = {};
        var currentPerson = null;
        var currentCategory = null;
        var currentQuery = Queries.Categories;

        configureBreeze();
        var manager = new breeze.EntityManager("api/dashboard");
        manager.enableSaveQueuing(true);

        var datacontext = {
            metadataStore:      manager.metadataStore,
            getCurrentDate:     getCurrentDate,
            setCurrentDate:     setCurrentDate,
            getCurrentPeriod:   getCurrentPeriod,
            setCurrentPeriod:   setCurrentPeriod,
            getCurrentPerson:   getCurrentPerson,
            setCurrentPerson:   setCurrentPerson,
            getCurrentCategory: getCurrentCategory,
            setCurrentCategory: setCurrentCategory,
            getCurrentQuery:    getCurrentQuery,
            setCurrentQuery:    setCurrentQuery,
            moveForward:        moveForward,
            moveBack:           moveBack,
            getCurrentSeries:   getCurrentSeries,
            setCurrentSeries:   setCurrentSeries,
            getData:            getData,
            getDevices:         getDevices,
            getPeople:          getPeople,
            createPerson:       createPerson,
            createDevice:       createDevice,
            removeDevice:       removeDevice,
            deletePerson:       deletePerson,
            deleteDevice:       deleteDevice,
            saveEntity:         saveEntity
        };
        model.initialize(datacontext);
        return datacontext;

        //#region private members

        function getCurrentDate() {
            return currentDate;
        }

        function setCurrentDate(value) {
            currentDate = value;
        }

        function getCurrentPeriod() {
            return currentPeriod;
        }

        function setCurrentPeriod(value) {
            currentPeriod = value;
            switch (value) {
                case Periods.Day:
                    break;
                case Periods.week:
                    // move to the previous Sunday
                    currentDate.moveToDayOfWeek(0, -1);
                    break;
                case Periods.Month:
                    // move to the first day of the current month
                    currentDate.moveToFirstDayOfMonth();
                    break;
                default:
                    break;
            }
        }

        function getCurrentPerson() {
            return currentPerson;
        }

        function setCurrentPerson(value) {
            currentPerson = value;
        }

        function getCurrentCategory() {
            return currentCategory;
        }

        function setCurrentCategory(value) {
            currentCategory = value;
        }

        function getCurrentQuery() {
            return currentQuery;
        }

        function setCurrentQuery(value) {
            currentQuery = value;
        }

        function moveForward() {
            switch (currentPeriod) {
                case Periods.Day:
                    currentDate.addDays(1);
                    break;
                case Periods.Week:
                    currentDate.addDays(7);
                    break;
                case Periods.Month:
                    // move to the first day of the next month
                    currentDate.addMonths(1).moveToFirstDayOfMonth();
                    break;
                default:
                    break;
            }
        }

        function moveBack() {
            switch (currentPeriod) {
                case Periods.Day:
                    currentDate.addDays(-1);
                    break;
                case Periods.Week:
                    currentDate.addDays(-7);
                    break;
                case Periods.Month:
                    // move to the first day of the next month
                    currentDate.addMonths(-1).moveToFirstDayOfMonth();
                    break;
                default:
                    break;
            }
        }

        function getCurrentSeries(query) {
            return currentSeries[query];
        }

        function setCurrentSeries(query, series) {
            currentSeries[query] = series;
        }

        function getData(queryType, category) {
            queryType = queryType || currentQuery;
            category = category || currentCategory;
            var start = dateAsString(currentDate);
            var end = dateAsString(getEndDate(currentDate, currentPeriod));
            query = breeze.EntityQuery
                            .from(queryType)
                            .withParameters({
                                Start: start,
                                End: end,
                                PersonId: currentPerson !== null ? currentPerson.personId : null,
                                Category: category
                            });

            // inform views that the data is being refreshed
            $rootScope.$broadcast('getData', queryType);

            return manager.executeQuery(query)
                .then(getSucceeded); // caller to handle failure

            function getSucceeded(data) {
                var qType = data.XHR ? "remote" : "local";
                logger.log(qType + " " + queryType + " query succeeded");
                setCurrentSeries(queryType, data.results);
                $rootScope.$broadcast('seriesDataChange', queryType);
                return data.results;
            }
        }

        function dateAsString(date) {
            if (date instanceof Date)
                return date.toString("yyyy-MM-dd");
            return date;
        }

        function getEndDate(date, period) {
            switch (period) {
                case Periods.Day:
                    return date.clone().addDays(1);
                    break;
                case Periods.Week:
                    return date.clone().addDays(7);
                    break;
                case Periods.Month:
                    return date.clone().moveToLastDayOfMonth().addDays(1);
                    break;
                default:
                    break;
            }
        }

        function getDevices(forceRefresh) {
            var query = breeze.EntityQuery
                .from("Devices")
                .expand("Person");
            if (initializedDevices && !forceRefresh) {
                query = query.using(breeze.FetchStrategy.FromLocalCache);
            }
            return manager.executeQuery(query)
                .then(getDevicesSucceeded); // caller to handle failure

            function getDevicesSucceeded(data) {
                var qType = data.XHR ? "remote" : "local";
                logger.log(qType + " devices query succeeded");
                initializedDevices = true;
                return data.results;
            }
        }

        function getPeople(forceRefresh) {
            var query = breeze.EntityQuery
                .from("People")
                //.expand("Devices")
                .orderBy("personId");
            if (initializedPeople && !forceRefresh) {
                query = query.using(breeze.FetchStrategy.FromLocalCache);
            }
            return manager.executeQuery(query)
                .then(getPeopleSucceeded); // caller to handle failure

            function getPeopleSucceeded(data) {
                var qType = data.XHR ? "remote" : "local";
                logger.log(qType + " people query succeeded");
                initializedPeople = true;
                return data.results;
            }
        }

        function createDevice() {
            return manager.createEntity("Device");
        }

        function createPerson() {
            return manager.createEntity("Person");
        }

        function removeDevice(device) {
            var person = device.person;
            person.removeDevice(device);
        }

        function deletePerson(person) {
            // Neither breeze nor server cascade deletes so we have to do it
            //var devices = person.devices.slice(); // iterate over copy
            // don't cascade
            //devices.forEach(function (entity) { entity.person = null; });
            person.entityAspect.setDeleted();
            return saveEntity(person);
        }

        function deleteDevice(device) {
            device.entityAspect.setDeleted();
            return saveEntity(device);
        }

        function saveEntity(masterEntity) {
            // if nothing to save, return a resolved promise
            if (!manager.hasChanges()) { return Q(); }

            if (masterEntity.birthdate !== undefined && masterEntity.birthdate !== null) {
                try {
                    var birthdate = Date.parse(masterEntity.birthdate);
                    masterEntity.birthdate = birthdate.toString('M/d/yyyy');
                } catch (e) { }
            }
            var description = describeSaveOperation(masterEntity);
            return manager.saveChanges().then(saveSucceeded).fail(saveFailed);

            function saveSucceeded(data) {
                logger.log("saved " + description);
                return data;
            }

            function saveFailed(error) {
                var msg = "Error saving " +
                    description + ": " +
                    getErrorMessage(error);

                masterEntity.errorMessage = msg;
                logger.log(msg, 'error');
                // Let user see invalid value briefly before reverting
                $timeout(function () { manager.rejectChanges(); }, 1000);
                throw error; // so caller can see failure
            }
        }
        function describeSaveOperation(entity) {
            var statename = entity.entityAspect.entityState.name.toLowerCase();
            var typeName = entity.entityType.shortName;
            var name = entity.name;
            name = name ? (" '" + name + "'") : "";
            return statename + " " + typeName + name;
        }
        function getErrorMessage(error) {
            var reason = error.message;
            if (reason.match(/validation error/i)) {
                reason = getValidationErrorMessage(error);
            }
            return reason;
        }
        function getValidationErrorMessage(error) {
            try { // return the first error message
                var firstItem = error.entitiesWithErrors[0];
                var firstError = firstItem.entityAspect.getValidationErrors()[0];
                return firstError.errorMessage;
            } catch (e) { // ignore problem extracting error message 
                return "validation error";
            }
        }

        function configureBreeze() {
            // configure to use the model library for Angular
            breeze.config.initializeAdapterInstance("modelLibrary", "backingStore", true);

            // configure to use camelCase
            breeze.NamingConvention.camelCase.setAsDefault();

            // configure to resist CSRF attack
            var antiForgeryToken = $("#antiForgeryToken").val();
            if (antiForgeryToken) {
                // get the current default Breeze AJAX adapter & add header
                var ajaxAdapter = breeze.config.getAdapterInstance("ajax");
                ajaxAdapter.defaultSettings = {
                    headers: {
                        'RequestVerificationToken': antiForgeryToken
                    },
                };
            }
        }
        //#endregion
    }]);