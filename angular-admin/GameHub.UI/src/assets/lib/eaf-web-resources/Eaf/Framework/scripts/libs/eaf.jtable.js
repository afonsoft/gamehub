/* Adapter for jTable (http://jtable.org) to ASP.NET Boilerplate (http://voegol.com.br)
 * by GOL Labs Admin (http://voegol.com.br).
 * by Afonsoft (www.afonsoft.com.br).
 */
(function ($) {

    if (!$ || !$.hik || !$.hik.jtable) {
        return;
    }

    //Reference to base object members
    var base = {
        _create: $.hik.jtable.prototype._create
    };

    //Extending jTable to adapt ASP.NET Boilerplate
    $.extend(true, $.hik.jtable.prototype, {

        //Override _create function to change actions according to Eaf system
        _create: function () {
            var self = this;
            base._create.apply(self, arguments);

            if (self.options.actions.listAction) {
                self._adaptListActionforEaf();
            }

            if (self.options.actions.createAction) {
                self._adaptCreateActionforEaf();
            }

            if (self.options.actions.updateAction) {
                self._adaptUpdateActionforEaf();
            }

            if (self.options.actions.deleteAction) {
                self._adaptDeleteActionforEaf();
            }
        },

        //LIST ACTION ADAPTER
        _adaptListActionforEaf: function () {
            var self = this;
            var originalListAction = self.options.actions.listAction;
            self.options.actions.listAction = function (postData, jtParams) {
                return $.Deferred(function ($dfd) {

                    var input = $.extend({}, postData, {
                        skipCount: jtParams.jtStartIndex,
                        maxResultCount: jtParams.jtPageSize,
                        sorting: jtParams.jtSorting
                    });

                    originalListAction.method(input)
                        .done(function (result) {
                            $dfd.resolve({
                                "Result": "OK",
                                "Records": result.items || result[originalListAction.recordsField],
                                "TotalRecordCount": result.totalCount,
                                originalResult: result
                            });
                        })
                        .fail(function (error) {
                            self._handlerForFailOnEafRequest($dfd, error);
                        });
                });
            };
        },

        //CREATE ACTION ADAPTER
        _adaptCreateActionforEaf: function () {
            var self = this;
            var originalCreateAction = self.options.actions.createAction;
            self.options.actions.createAction = function (postData) {
                return $.Deferred(function ($dfd) {

                    var input = $.extend({}, postData);

                    originalCreateAction.method(input)
                        .done(function (result) {
                            $dfd.resolve({
                                "Result": "OK",
                                "Record": originalCreateAction.recordField ? result[originalCreateAction.recordField] : result,
                                originalResult: result
                            });
                        })
                        .fail(function (error) {
                            self._handlerForFailOnEafRequest($dfd, error);
                        });
                });
            };
        },

        //UPDATE ACTION ADAPTER
        _adaptUpdateActionforEaf: function () {
            var self = this;
            var originalUpdateAction = self.options.actions.updateAction;
            self.options.actions.updateAction = function (postData) {
                return $.Deferred(function ($dfd) {

                    var input = $.extend({}, postData);

                    originalUpdateAction.method(input)
                        .done(function (result) {
                            var jtableResult = {
                                "Result": "OK",
                                originalResult: result
                            };

                            if (originalUpdateAction.returnsRecord) {
                                if (originalUpdateAction.recordField) {
                                    jtableResult.Record = result[originalUpdateAction.recordField];
                                } else {
                                    jtableResult.Record = result;
                                }
                            }

                            $dfd.resolve(jtableResult);
                        })
                        .fail(function (error) {
                            self._handlerForFailOnEafRequest($dfd, error);
                        });
                });
            };
        },

        //DELETE ACTION ADAPTER
        _adaptDeleteActionforEaf: function () {
            var self = this;
            var originalDeleteAction = self.options.actions.deleteAction;
            self.options.actions.deleteAction = function (postData) {
                return $.Deferred(function ($dfd) {

                    var input = $.extend({}, postData);

                    originalDeleteAction.method(input)
                        .done(function (result) {
                            $dfd.resolve({
                                "Result": "OK",
                                originalResult: result
                            });
                        })
                        .fail(function (error) {
                            self._handlerForFailOnEafRequest($dfd, error);
                        });
                });
            };
        },

        _handlerForFailOnEafRequest: function ($dfd, error) {
            if (error && error.message) {
                $dfd.resolve({
                    Result: "ERROR",
                    Message: error.message
                });
            } else {
                $dfd.reject(error);
            }
        },

        //Disable showing error messages
        _showError: function (message) {
            //do nothing since Eaf handles error messages!
        }

    });

    //Overriding some defaults
    $.extend(true, $.hik.jtable.prototype.options, {
        pageList: "minimal"
    });

})(jQuery);
