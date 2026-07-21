var eaf = eaf || {}; // NOSONAR
(function () {
    if (!moment || !moment.tz) { // NOSONAR
        return;
    }

    /* DEFAULTS *************************************************/

    eaf.timing = eaf.timing || {};

    /* FUNCTIONS **************************************************/

    eaf.timing.convertToUserTimezone = function (date) {
        var momentDate = moment(date); // NOSONAR
        var targetDate = momentDate.clone().tz(eaf.timing.timeZoneInfo.iana.timeZoneId); // NOSONAR
        return targetDate;
    };

})();