//----------------------------------------------------------
// Copyright (C) WebTimer LLC. All rights reserved.
//----------------------------------------------------------
var Control = function Control$() { };

// modal message
Control.alert = function Control$alert(message, header, handlerClose) {
    var $modalMessage = $('#modalMessage');
    if ($modalMessage.length == 0) {
        message.replace('<br/>', '\r');
        alert(message);
        if (handlerClose != null) { handlerClose(); }
    } else {
        if (header == null) { header = "Warning!"; }
        $modalMessage.find('.modal-header h3').html(header);
        $modalMessage.find('.modal-body p').html(message);
        $modalMessage.modal('show');
        if (handlerClose != null) {
            $modalMessage.find('.modal-header .close').unbind('click').click(function () { handlerClose(); });
            $modalMessage.find('.modal-footer .btn-primary').unbind('click').click(function () { handlerClose(); });
        }
    }
}

// modal prompt
Control.confirm = function Control$confirm(message, header, handlerOK, handlerCancel) {
    var $modalPrompt = $('#modalPrompt');
    if ($modalPrompt.length == 0) {
        message.replace('<br\>', '\r');
        if (confirm(message) == true) {
            if (handlerOK != null) { handlerOK(); }
        } else {
            if (handlerCancel != null) { handlerCancel(); }
        }
    } else {
        if (header == null) { header = 'Confirm?'; }
        $modalPrompt.find('.modal-header h3').html(header);
        $modalPrompt.find('.modal-body p').html(message);
        $modalPrompt.modal({ backdrop: 'static', keyboard: false });
        $modalPrompt.find('.modal-footer .btn-primary').unbind('click').click(function () {
            $modalPrompt.modal('hide');
            if (handlerOK != null) { handlerOK(); }
        });
        $modalPrompt.find('.modal-footer .btn-cancel').unbind('click').click(function () {
            $modalPrompt.modal('hide');
            if (handlerCancel != null) { handlerCancel(); }
        });
    }
}