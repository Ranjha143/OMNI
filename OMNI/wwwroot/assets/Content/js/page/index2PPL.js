"use strict";

var draw = Chart.controllers.line.prototype.draw;
Chart.controllers.lineShadow = Chart.controllers.line.extend({
    draw: function() {
        draw.apply(this, arguments);
        var ctx = this.chart.chart.ctx;
        var _stroke = ctx.stroke;
        ctx.stroke = function() {
            ctx.save();
            ctx.shadowColor = '#00000075';
            ctx.shadowBlur = 10;
            ctx.shadowOffsetX = 8;
            ctx.shadowOffsetY = 8;
            _stroke.apply(this, arguments)
            ctx.restore();
        }
    }
});

$.ajax({
    url: "assets/php/dounet1PPL.php",
    type: "GET",
    success: function(data) {

        console.log(data);
        var data = JSON.parse(data);
        var values = {
            count: [],
            status: []
        };
        var len = data.length;
        for (var i = 0; i < len; i++) {
            values.count.push(data[i].COUNT);
            values.status.push(data[i].STATUS_CODE);
        }
        console.log(values.count);
        console.log(values.city);
        // for (var i = 0; i <= data.length; i++) {
        var ctx1 = document.getElementById("myChart31").getContext('2d');
        var myChart1 = new Chart(ctx1, {
            type: 'doughnut',
            data: {
                datasets: [{
                    data: values.count,
                    backgroundColor: [
                        "#DAF7A6",
                        // "#FFC300",
                        "#FF5733",
                        "#C70039",
                        "#900C3F",
                        "#581845",
                        "#C0C0C0",
                        "#808080",
                        "#800000",
                        "#FFFF00",
                        "#FF0000",
                    ],
                    label: 'Dataset 1'
                }],
                labels: values.status,
            },
            options: {
                responsive: true,
                legend: {
                    position: 'bottom',
                },
            }
        });
        // }
    },
})
$.ajax({
    url: "assets/php/dounet2PPL.php",
    type: "GET",
    success: function(data) {

        console.log(data);
        var data = JSON.parse(data);
        var values = {
            amount: [],
            dName: []
        };
        var len = data.length;
        for (var i = 0; i < len; i++) {
            values.amount.push(data[i].AMOUNT);
            values.dName.push(data[i].D_NAME);
        }
        console.log(values.amount);
        console.log(values.dName);
        // for (var i = 0; i <= data.length; i++) {
        var ctx2 = document.getElementById("myChart32").getContext('2d');
        var myChart2 = new Chart(ctx2, {
            type: 'doughnut',
            data: {
                datasets: [{
                    data: values.amount,
                    backgroundColor: [
                        "#DAF7A6",
                        // "#FFC300",
                        "#FF5733",
                        "#C70039",
                        "#900C3F",
                        "#581845",
                        "#C0C0C0",
                        "#808080",
                        "#800000",
                        "#FFFF00",
                        "#FF0000",
                    ],
                    label: 'Dataset 1'
                }],
                labels: values.dName,
            },
            options: {
                responsive: true,
                legend: {
                    position: 'bottom',
                },
            }
        });
        // }
    },
})
$.ajax({
    url: "assets/php/dounet3PPL.php",
    type: "GET",
    success: function(data) {

        console.log(data);
        var data = JSON.parse(data);
        var values = {
            count: [],
            city: []
        };
        var len = data.length;
        for (var i = 0; i < len; i++) {
            values.count.push(data[i].COUNT);
            values.city.push(data[i].CITY);
        }
        console.log(values.count);
        console.log(values.city);
        // for (var i = 0; i <= data.length; i++) {
        var ctx3 = document.getElementById("myChart33").getContext('2d');
        var myChart3 = new Chart(ctx3, {
            type: 'doughnut',
            data: {
                datasets: [{
                    data: values.count,
                    backgroundColor: [
                        "#DAF7A6",
                        // "#FFC300",
                        "#FF5733",
                        "#C70039",
                        "#900C3F",
                        "#581845",
                        "#C0C0C0",
                        "#808080",
                        "#800000",
                        "#FFFF00",
                        "#FF0000",
                    ],
                    label: 'Dataset 1'
                }],
                labels: values.city,
            },
            options: {
                responsive: true,
                legend: {
                    position: 'bottom',
                },
            }
        });
        // }
    },
})
$.ajax({
    url: "assets/php/dounet4PPL.php",
    type: "GET",
    success: function(data) {

        console.log(data);
        var data = JSON.parse(data);
        var values = {
            count: [],
            country: []
        };
        var len = data.length;
        for (var i = 0; i < len; i++) {
            values.count.push(data[i].COUNT);
            values.country.push(data[i].COUNTRY);
        }
        var countryData = {

        }
        console.log(values.count);
        console.log(values.country);
        // for (var i = 0; i <= data.length; i++) {
        var ctx4 = document.getElementById("myChart34").getContext('2d');
        var myChart4 = new Chart(ctx4, {
            type: 'doughnut',
            data: {
                datasets: [{
                    data: values.count,
                    backgroundColor: [
                        "#DAF7A6",
                        // "#FFC300",
                        "#FF5733",
                        "#C70039",
                        "#900C3F",
                        "#581845",
                        "#C0C0C0",
                        "#808080",
                        "#800000",
                        "#FFFF00",
                        "#FF0000",
                    ],
                    label: 'Dataset 1'
                }],
                labels: values.country,
            },
            options: {
                responsive: true,
                legend: {
                    position: 'bottom',
                },
            }
        });
        // }
    },
})
$.ajax({
    url: "assets/php/dounet5PPL.php",
    type: "GET",
    success: function(data) {

        console.log(data);
        var data = JSON.parse(data);
        var values = {
            count: [],
            status: []
        };
        var len = data.length;
        for (var i = 0; i < len; i++) {
            values.count.push(data[i].COUNT);
            values.status.push(data[i].STATUS_CODE);
        }
        console.log(values.count);
        var ctx5 = document.getElementById("myChart35").getContext('2d');
        var myChart35 = new Chart(ctx5, {
            type: 'bar',
            data: {
                datasets: [{
                    data: values.count,
                    backgroundColor: [
                        'rgba(255, 99, 132, 0.2)',
                        'rgba(54, 162, 235, 0.2)',
                        'rgba(255, 206, 86, 0.2)',
                        'rgba(75, 192, 192, 0.2)',
                        'rgba(153, 102, 255, 0.2)',
                        'rgba(255, 159, 64, 0.2)',
                        'rgba(255, 99, 132, 0.2)',
                        'rgba(54, 162, 235, 0.2)',
                        'rgba(255, 206, 86, 0.2)',
                        'rgba(75, 192, 192, 0.2)',
                        'rgba(153, 102, 255, 0.2)',
                        'rgba(255, 159, 64, 0.2)'
                    ],
                    borderColor: [
                        'rgba(255,99,132,1)',
                        'rgba(54, 162, 235, 1)',
                        'rgba(255, 206, 86, 1)',
                        'rgba(75, 192, 192, 1)',
                        'rgba(153, 102, 255, 1)',
                        'rgba(255, 159, 64, 1)',
                        'rgba(255,99,132,1)',
                        'rgba(54, 162, 235, 1)',
                        'rgba(255, 206, 86, 1)',
                        'rgba(75, 192, 192, 1)',
                        'rgba(153, 102, 255, 1)',
                        'rgba(255, 159, 64, 1)'
                    ],
                    borderWidth: 1,
                    label: 'New'
                }],
                labels: values.status,
            },
            options: {
                responsive: true,
                legend: {
                    position: 'bottom',
                },
            }
        });
    },
})