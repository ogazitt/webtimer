/* datacontext: data access and model management layer */

// create and add datacontext to the Ng injector
// constructor function relies on Ng injector
// to provide service dependencies
dashboard.factory('datacontext',
    ['breeze', 'Q', 'model', 'logger', '$timeout',
    function (breeze, Q, model, logger, $timeout) {

        logger.log("creating datacontext");
        var initializedSessions, initializedPeople, initializedDevices;

        configureBreeze();
        var manager = new breeze.EntityManager("api/dashboard");
        manager.enableSaveQueuing(true);

        var datacontext = {
            metadataStore:  manager.metadataStore,
            getWebSessions: getWebSessions,
            getDevices:     getDevices,
            getPeople:      getPeople,
            createPerson:   createPerson,
            createDevice:   createDevice,
            deleteDevice:   deleteDevice,
            deletePerson:   deletePerson,
            saveEntity:     saveEntity
        };
        model.initialize(datacontext);
        return datacontext;

        //#region private members

        function getWebSessions(forceRefresh) {

            var query = breeze.EntityQuery
                .from("WebSessions")
                //.expand("Devices")
                //.orderBy("personId desc");
                .orderBy("timestamp");

            if (initializedSessions && !forceRefresh) {
                query = query.using(breeze.FetchStrategy.FromLocalCache);
            }
            initializedSessions = true;

            return manager.executeQuery(query)
                .then(getSucceeded); // caller to handle failure
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
            initializedDevices = true;

            return manager.executeQuery(query)
                .then(getSucceeded); // caller to handle failure
        }

        function getPeople(forceRefresh) {

            var query = breeze.EntityQuery
                .from("People")
                //.expand("Devices")
                .orderBy("personId");

            if (initializedPeople && !forceRefresh) {
                query = query.using(breeze.FetchStrategy.FromLocalCache);
            }
            initializedPeople = true;

            return manager.executeQuery(query)
                .then(getSucceeded); // caller to handle failure
        }

        function getSucceeded(data) {
            var qType = data.XHR ? "remote" : "local";
            logger.log(qType + " query succeeded");
            return data.results;
        }

        function createDevice() {
            return manager.createEntity("Device");
        }

        function createPerson() {
            return manager.createEntity("Person");
        }

        function deleteDevice(device) {
            device.entityAspect.setDeleted();
            return saveEntity(device);
        }

        function deletePerson(person) {
            // Neither breeze nor server cascade deletes so we have to do it
            var devices = person.devices.slice(); // iterate over copy
            // don't cascade
            //devices.forEach(function (entity) { entity.entityAspect.setDeleted(); });
            person.entityAspect.setDeleted();
            return saveEntity(person);
        }

        function saveEntity(masterEntity) {
            // if nothing to save, return a resolved promise
            if (!manager.hasChanges()) { return Q(); }

            var description = describeSaveOperation(masterEntity);
            return manager.saveChanges().then(saveSucceeded).fail(saveFailed);

            function saveSucceeded() {
                logger.log("saved " + description);
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
            var title = entity.title;
            title = title ? (" '" + title + "'") : "";
            return statename + " " + typeName + title;
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