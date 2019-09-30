(function () {

    function getTableCellClass(val) {

        if (val !== 0 && val < 75) {

            if (val < 50) {
                return "text-danger";
            } else {
                return "text-warning";
            }
        }

        return "";
    }

    $(document).ready(function () {

        $.ajax({
            type: 'GET',
            url: "/api/vsts/teams",
            dataType: 'json'
        }).done(function (data) {

            var teamSelect = $("#team-dropdown");
            teamSelect.empty();
            teamSelect.append("<option value=\"\" selected disabled>No Team Selected</option>");

            data.forEach(function (team) {
                teamSelect.append("<option value='" + team.name + "' data-project='" + team.projectID + "' data-id='" + team.id + "'>" + team.formattedName + "</option");
            });

        });

        $("#team-dropdown").change(function (event) {

            if (!$(event.target).val())
                return;

            var iterationSelect = $("#iteration-dropdown");
            iterationSelect.empty();
            iterationSelect.append("<option value=\"\" selected disabled>Loading...</option>");

            var selectedItem = $(event.target).find(":selected");

            $.ajax({
                type: 'GET',
                url: "/api/vsts/iterations",
                dataType: 'json',
                data: {
                    teamID: selectedItem.data("id"),
                    projectID: selectedItem.data("project")
                }
            }).done(function (data) {

                iterationSelect.empty();
                iterationSelect.append("<option value=\"\" selected disabled>No Sprint Selected</option>");

                data.forEach(function (iteration) {
                    iterationSelect.append("<option value='" + iteration.name + "' data-iteration='" + iteration.id + "'>" + iteration.name + "</option");
                });

            });

        });

        $("#iteration-dropdown").change(function (event) {

            var iterationID = $(event.target).find(":selected").data("iteration");

            if (!iterationID)
                return;

            $("#no-selection-overlay").fadeOut();

            $("#metrics-grid").fadeOut("slow", function () {

                $("#loading-overlay").css("display", "flex").hide().fadeIn("slow", function () {

                    $.ajax({
                        url: "/api/metrics/sprintmetrics",
                        dataType: "json",
                        data: {
                            projectID: $("#team-dropdown").find(":selected").data("project"),
                            teamID: $("#team-dropdown").find(":selected").data("id"),
                            iterationID: $(event.target).find(":selected").data("iteration")
                        }
                    }).done(function (response) {

                        var rows = [];
                        var metricsTableBody = $("#metrics-grid > tbody");
                        metricsTableBody.empty();

                        response.forEach(item => {

                            var rowHTML = "<tr>" +
                                "<td>" + (item.closed ? "<i id=\"closedIndicator\" data-toggle=\"tooltip\" data-delay=\"300\" title=\"Closed\" class=\"fas fa-check\"></i>" : "") + "</td>" +
                                "<td>" + item.workItemTitle + "</td>" +
                                "<td class='" + getTableCellClass(item.developmentAccuracy) + "'>" + item.formattedDevelopment + "</td>" +
                                "<td class='" + getTableCellClass(item.testingAccuracy) + "'>" + item.formattedTesting + "</td>" +
                                "<td class='" + getTableCellClass(item.overallAccuracy) + "'>" + item.formattedOverall + "</td>" +
                                "<td><button data-toggle=\"modal\" data-target=\"#all-sprints-metrics-modal\" data-work-item-id=\"" + item.workItemID + "\" class=\"btn btn-outline-dark\" data-toggle=\"tooltip\" data-delay=\"500\" title=\"View Metrics Across All Sprints\"><i class=\"fas fa-external-link-alt\"></i></button></td>" +
                                "</tr>";
                            rows.push(rowHTML);

                        });

                        metricsTableBody.html(rows.join(''));

                        $("#loading-overlay").fadeOut("slow", function () {
                            $("#metrics-grid").fadeIn();
                            $('[data-toggle="tooltip"]').tooltip();
                        });

                    });
                });
            });
        });

        $("#all-sprints-metrics-modal").on('show.bs.modal', function (event) {

            $("#chartCanvas").hide();
            $("#chartLoadingImage").show();

            //The overlay interferes with the chart tooltips so adjust the z-index as the overlay is shown/hidden to allow the
            //tooltips to appear
            $("#chartLoadingOverlay").css("z-index", "100");

            $("#totalTimeMetric").text("N/A");
            $("#totalEstimatedTimeMetric").text("N/A");
            $("#devTimeMetric").text("N/A");
            $("#reworkTimeMetric").text("N/A");
            $("#testingTimeMetric").text("N/A");
            $("#numBugsMetric").text("N/A");
            $("#numOpenBugsMetric").text("N/A");
            $("#numIterationsMetric").text("N/A");

        });

        $("#all-sprints-metrics-modal").on('shown.bs.modal', function (event) {

            var parentButton = $(event.relatedTarget);
            var modal = this;

            var chartContainer = $("#chart-container");

            var chartCanvas = $("#chartCanvas");
            chartCanvas.width(chartContainer.width());
            chartCanvas.height(chartContainer.height());

            $.ajax({
                url: '/api/metrics/crosssprintmetrics',
                dataType: 'json',
                data: {
                    iterationID: $("#iteration-dropdown").find(":selected").data("iteration"), 
                    workItemID: parentButton.data('work-item-id')
                }
            }).done(function (response) {

                $("#totalTimeMetric").text(response.totalTime);
                $("#totalEstimatedTimeMetric").text(response.totalEstimatedTime);
                $("#devTimeMetric").text(response.timeDeveloping);
                $("#reworkTimeMetric").text(response.timeBugFixing.toString().concat(' (', (response.bugFixingPercentageOfDev * 100).toFixed(2), '%)'));
                $("#testingTimeMetric").text(response.timeTesting);
                $("#numBugsMetric").text(response.numberOfBugs);
                $("#numOpenBugsMetric").text(response.numberOfOpenBugs);
                $("#numIterationsMetric").text(response.numberOfIterations);
                $(".modal-title").text(response.workItemTitle);

                var iterations = [];
                var devPerIteration = [];
                var testPerIteration = [];

                for (var i = 0; i < response.numberOfIterations; i++) {

                    var itm = response.iterationMetrics[i];

                    iterations.push(itm.iteration);
                    devPerIteration.push(itm.timeDeveloping);
                    testPerIteration.push(itm.timeTesting);
                }

                var chartData = {
                    labels: iterations,
                    datasets: [{
                        label: 'Development',
                        backgroundColor: "rgb(63, 127, 191, 0.7)",
                        data: devPerIteration
                    }, {
                        label: 'Testing',
                        backgroundColor: "rgba(222, 16, 16, 0.7)",
                        data: testPerIteration
                    }]
                };

                modal.chartLoading = true;

                if (modal.chart) {

                    modal.chart.clear();
                    modal.chart.data.labels = chartData.labels;
                    modal.chart.data.datasets = chartData.datasets;
                    modal.chart.update();

                } else {

                    modal.chart = new Chart(document.getElementById("chartCanvas").getContext("2d"), {
                        type: 'bar',
                        data: chartData,
                        options: {
                            legend: {
                                position: "bottom"
                            },

                            tooltips: {
                                enabled: true
                            },
                            animation: {
                                onProgress: function () {

                                    if (modal.chartLoading) {

                                        $("#chartLoadingImage").fadeOut("slow");
                                        $("#chartLoadingOverlay").css("z-index", "-1");
                                        $("#chartCanvas").fadeIn("slow");

                                        modal.chartLoading = false;
                                    }
                                }
                            },
                            responsive: false,
                            scales: {
                                xAxes: [{
                                    stacked: true
                                }],
                                yAxes: [{
                                    stacked: true
                                }]
                            }
                        }
                    });
                }

            });
        });

        $("#all-sprints-metrics-modal").on('hidden.bs.modal', function (event) {

            $(".modal-title").text("Loading...");
            $("#allSprintMetricsContent").hide();
            $("#allSprintMetricsLoading").show();
        });

    });
})();