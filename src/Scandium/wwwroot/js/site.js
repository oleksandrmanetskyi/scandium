// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function loadJobList() {
    $("#job-list").empty();
    $("div#loading").show();
    
    $.ajax({
        type: 'GET',
        url: '/job-list',
        success: function (data) {
            $("#job-list").html(data);
            $("div#loading").hide();
        }
    });
}

var loadingBar = (function () {

    $(document).ready(function () {

        $(document).on("click", "#btn-load", function () {

            $("#final-result").empty();

            let numberOfWords = $("#NumberOfWords").val();

            let dataModel = {
                numberOfWords
            };

            let ajaxOptions = {
                type: "GET",
                data: dataModel,
                contentType: "application/json",
                traditional: true,
                url: "/generate",
                success: function (result) {
                    $("#final-result").html(result);
                }
            };

            loadingWithProgressAndAbort.withSignalR(ajaxOptions);
        });
    });

})();


var loadingWithProgressAndAbort = (function () {

    function bindAbort(jqXHR) {
        $(document).on("click", "#loading-cancel", function () {
            jqXHR.abort();
            $("#final-result").html("Cancelled");
        });
    }

    function startLoading() {
        $("#div-loading").show();
    }

    function setProgress(progress) {
        $("#div-loading .progress-bar").attr("style", `width:${progress}%;`);
        $("#div-loading .progress-bar").attr("aria-valuenow", progress);
        $("#div-loading .progress-bar").text(`${progress}% `);
    }

    function stopLoading() {
        $("#div-loading").hide();
        setProgress("#div-loading", 0);
    }

    function hideGenerateSection() {
        $("#div-generate").hide();
    }

    function showGenerateSection () {
        $("#div-generate").show();
    }

    return {
        withSignalR: function (ajaxOptions) {

            hideGenerateSection();
            
            var connection =
                new signalR.HubConnectionBuilder()
                    .configureLogging(signalR.LogLevel.Debug)
                    .withUrl("/loading-bar-progress")
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
