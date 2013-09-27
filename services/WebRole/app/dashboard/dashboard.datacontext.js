/* datacontext: data access and model management layer */

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
        var currentSeries = [];

        configureBreeze();
        var manager = new breeze.EntityManager("api/dashboard");
        manager.enableSaveQueuing(true);

        var datacontext = {
            addEventHandler:  addEventHandler,
            metadataStore:    manager.metadataStore,
            getCurrentDate:   getCurrentDate,
            setCurrentDate:   setCurrentDate,
            setCurrentPeriod: setCurrentPeriod,
            moveForward:      moveForward,
            moveBack:         moveBack,
            getCurrentSeries: getCurrentSeries,
            setCurrentSeries: setCurrentSeries,
            getWebSessions:   getWebSessions,
            getAggSessions:   getAggSessions,
            getCatTotals:     getCatTotals,
            getDevices:       getDevices,
            getPeople:        getPeople,
            createPerson:     createPerson,
            createDevice:     createDevice,
            removeDevice:     removeDevice,
            deletePerson:     deletePerson,
            saveEntity:       saveEntity
        };
        model.initialize(datacontext);
        return datacontext;

        //#region private members

        function addEventHandler(handler) {
            eventHandlers.push(handler);
        }

        function getCurrentDate() {
            return currentDate;
        }

        function setCurrentDate(date) {
            currentDate = date;
        }

        function setCurrentPeriod(period) {
            currentPeriod = period;
            switch (period) {
                case 'day':
                    break;
                case 'week':
                    // move to the previous Sunday
                    currentDate.moveToDayOfWeek(0, -1);
                    break;
                case 'month':
                    // move to the first day of the current month
                    currentDate.moveToFirstDayOfMonth();
                    break;
                default:
                    break;
            }
        }

        function moveForward() {
            switch (currentPeriod) {
                case 'day':
                    currentDate.addDays(1);
                    break;
                case 'week':
                    currentDate.addDays(7);
                    break;
                case 'month':
                    // move to the first day of the next month
                    currentDate.addMonths(1).moveToFirstDayOfMonth();
                    break;
                default:
                    break;
            }
        }

        function moveBack() {
            switch (currentPeriod) {
                case 'day':
                    currentDate.addDays(-1);
                    break;
                case 'week':
                    currentDate.addDays(-7);
                    break;
                case 'month':
                    // move to the first day of the next month
                    currentDate.addMonths(-1).moveToFirstDayOfMonth();
                    break;
                default:
                    break;
            }
        }

        function getCurrentSeries() {
            return currentSeries;
        }

        function setCurrentSeries(series) {
            currentSeries = series;
        }

        function getWebSessions(start, end, personId) {
            return getData("WebSessions", start, end, personId);
        }

        function getAggSessions(start, end, personId) {
            return getData("ConsolidatedWebSessions", start, end, personId);
        }

        function getCatTotals() {
            start = dateAsString(currentDate);
            end = dateAsString(getEndDate(currentDate, currentPeriod));

            var query = breeze.EntityQuery
                .from("CategoryTotals")
                .withParameters({ Start: start, End: end });

            return manager.executeQuery(query)
                .then(getCatTotalsSucceeded); // caller to handle failure
        }

        function getData(type, start, end, personId) {
            start = dateAsString(start);
            end = dateAsString(end);

            var query = breeze.EntityQuery
                .from(type)
                .where(createPredicate(personId))
                .withParameters({ Start: start, End: end });

            return manager.executeQuery(query)
                .then(getSucceeded); // caller to handle failure
        }

        function dateAsString(date) {
            if (date instanceof Date)
                return date.toString("yyyy-MM-dd");
            return date;
        }

        function getEndDate(date, period) {
            switch (period) {
                case 'day':
                    return date.clone().addDays(1);
                    break;
                case 'week':
                    return date.clone().addDays(7);
                    break;
                case 'month':
                    return date.clone().moveToLastDayOfMonth().addDays(1);
                    break;
                default:
                    break;
            }
        }

        function createPredicate(personId) {
            var predicate = breeze.Predicate("category", "!=", null);
            if (personId !== undefined)
                predicate = predicate.and("personId", "==", personId);
            return predicate;
        }

        function getDevices(forceRefresh) {

            var query = breeze.EntityQuery
                .from("Devices");
                //.expand("People")
                //.orderBy("personId desc");
                //.orderBy("timestamp");

            if (initializedDevices && !forceRefresh) {
                query = query.using(breeze.FetchStrategy.FromLocalCache);
            }

            return manager.executeQuery(query)
                .then(getDevicesSucceeded); // caller to handle failure
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
        }

        function getCatTotalsSucceeded(data) {
            var qType = data.XHR ? "remote" : "local";
            logger.log(qType + " category totals query succeeded");
            setCurrentSeries(data.results);
            $rootScope.$broadcast('seriesDataChange');
            return data.results;
        }

        function getPeopleSucceeded(data) {
            var qType = data.XHR ? "remote" : "local";
            logger.log(qType + " people query succeeded");
            initializedPeople = true;
            return data.results;
        }

        function getDevicesSucceeded(data) {
            var qType = data.XHR ? "remote" : "local";
            logger.log(qType + " devices query succeeded");
            initializedDevices = true;
            return data.results;
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

        function saveEntity(masterEntity) {
            // if nothing to save, return a resolved promise
            if (!manager.hasChanges()) { return Q(); }

            if (masterEntity.birthDateString !== undefined && masterEntity.birthDateString !== null) {
                masterEntity.birthdate = Date.parse(masterEntity.birthDateString);
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