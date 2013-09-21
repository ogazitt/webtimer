/* model: extend server-supplied metadata with client-side entity model members */
dashboard.factory('model', function () {

    var datacontext;
    
    //extendDevice();
    extendPerson();

    var model = {
        initialize: initialize
    };
    
    return model;
  
    //#region private members
    function initialize(context) {
        datacontext = context;
        var store = datacontext.metadataStore;
        store.registerEntityTypeCtor("Device", null, deviceInitializer);
        store.registerEntityTypeCtor("Person", Person, personInitializer);
    }
    
    function deviceInitializer(device) {
        device.errorMessage = "";
    }

    function personInitializer(person) {
        person.errorMessage = "";
        person.newDeviceTitle = "";
        person.isEditingListTitle = false;
    }

    function Person() {
        this.title = "New Person"; // defaults
        this.userId = "to be replaced";
    }
    
    function extendPerson() {
        Person.prototype.addDevice = function () {
            var person = this;
            var title = person.newDeviceTitle;
            if (title) { // need a title to save
                var device = datacontext.createDevice();
                device.title = title;
                device.person = person;
                datacontext.saveEntity(device);
                person.newDeviceTitle = ""; // clear UI title box
            }
        };

        Person.prototype.deleteDevice = function (device) {
            return datacontext.deleteDevice(device);
        };
    }
    //#endregion
});