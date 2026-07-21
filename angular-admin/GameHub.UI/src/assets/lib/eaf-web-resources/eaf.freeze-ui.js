var eaf = eaf || {}; // NOSONAR
(function () {

    eaf.ui.setBusy = function (element, text, freezeDelay) {
        FreezeUI({ element: element, text: text ? text : ' ', freezeDelay: freezeDelay }); // NOSONAR
    };

    eaf.ui.clearBusy = function (element, freezeDelay) {
        UnFreezeUI({ element: element,freezeDelay: freezeDelay });
    };

})();
