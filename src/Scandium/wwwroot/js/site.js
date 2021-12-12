// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function loadJobList() {
    $("#job-list-result").empty();
    $("#job-list #loading-spinner").show();
    
    $.ajax({
        type: 'GET',
        url: '/job-list',
        success: function (data) {
            $("#job-list-result").html(data);
            $("#job-list #loading-spinner").hide();
        }
    });
}

function hideSplashScreen() {
    $("#splash-screen").css('display', 'none').removeClass("d-flex");
}

function startJob() {
    let numberOfWords = $("#NumberOfWords").val();

    let dataModel = {
        numberOfWords
    };

    let xhr = $.ajax({
        type: "GET",
        data: dataModel,
        contentType: "application/json",
        traditional: true,
        url: "/generate",
        success: function (result) {
            $("#toast-message").html(result);
            new bootstrap.Toast($("#status-toast")).show();
            $("#btn-load #loading-spinner").hide();
            $("#btn-load").prop('disabled', false);
            loadJobList();
        },
        error: function (xhr, status, error) {
            $("#toast-message").html(xhr.responseText);
            new bootstrap.Toast($("#status-toast")).show();
            $("#btn-load #loading-spinner").hide();
            $("#btn-load").prop('disabled', false);
        }
    });
    
    bindAbort(xhr);
}

function setProgress(progress) {
    $("#div-loading .progress-bar")
        .attr("style", `width:${progress}%;`)
        .attr("aria-valuenow", progress)
        .text(`${progress}% `);
}

function connectProgressBar() {
    let connection =
        new signalR.HubConnectionBuilder()
            .configureLogging(signalR.LogLevel.Debug)
            .withUrl("/progress-report/loading-bar-progress")
            .build();

    connection.on("updateLoadingBar",
        (response) => {
            let progress = Math.round(response * 100);
            setProgress(progress);
            if ($("#div-loading .progress-bar").text() >= 100) {
                location.reload();
            }
        });

    connection.on("canceledLoading",
        (response) => {
            location.reload();
        });

    connection.on("doneLoading",
        (response) => {
            location.reload();
        });

    let url = $(location).attr('href'),
        parts = url.split("/"), 
        lastPart = parts[parts.length - 1];
    
    connection
        .start()
        .then(function () {
            $.ajax({
                type: "POST",
                data: {
                    jobId: lastPart,
                    connectionId: connection.connectionId
                },
                url: "/progress-report/subscribe",
                success: function (result) {
                }
            });
        })
}

function bindAbort(jqXHR) {
    $(document).on("click", "#loading-cancel", function () {
        jqXHR.abort();
    });
}

var loadingWithProgressAndAbort = (function () {

    function startLoading() {
        $("#div-loading").show();
    }

    function setProgress(progress) {
        $("#div-loading .progress-bar")
            .attr("style", `width:${progress}%;`)
            .attr("aria-valuenow", progress)
            .text(`${progress}% `);
    }

    function stopLoading() {
        $("#div-loading").hide();
        setProgress("#div-loading", 0);
    }

    return {
        withSignalR: function (ajaxOptions) {
            
            var connection =
                new signalR.HubConnectionBuilder()
                    .configureLogging(signalR.LogLevel.Debug)
                    .withUrl("/progress-report/loading-bar-progress")
                    .build();

            connection.on("updateLoadingBar",
                (perc) => {
                    var progress = Math.round(perc * 100);
                    setProgress(progress);
                });

            connection
                .start()
                .then(function () {
                    const connectionId = connection.connectionId;

                    if (ajaxOptions.data === undefined) {
                        ajaxOptions["data"] = {};
                    }
                    ajaxOptions.data["connectionId"] = connectionId;

                    const xhr = $.ajax(ajaxOptions);
                    bindAbort(xhr);
                    startLoading();

                    xhr.always(function () {
                        connection.stop();
                        stopLoading();
                    });
                });
        }
    };

})();
