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
        device.isEditingDeviceName = false;
    }

    function personInitializer(person) {
        person.errorMessage = "";
        person.isEditingPersonName = false;
        person.birthDateString = person.birthdate !== null ? person.birthdate.toString("M/d/yyyy") : null;
    }

    function Person() {
        this.name = "New Person"; // defaults
        this.userId = "to be replaced";
    }
    
    function extendPerson() {
        Person.prototype.addDevice = function (device) {
            device.person = this;
            datacontext.saveEntity(device);
        };

        Person.prototype.removeDevice = function (device) {
            device.person = null;
            return datacontext.deleteDevice(device);
        };
    }
    //#endregion
});