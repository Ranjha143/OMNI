"use strict";

$("[data-checkboxes]").each(function() {
    var me = $(this),
        group = me.data('checkboxes'),
        role = me.data('checkbox-role');

    me.change(function() {
        var all = $('[data-checkboxes="' + group + '"]:not([data-checkbox-role="dad"])'),
            checked = $('[data-checkboxes="' + group + '"]:not([data-checkbox-role="dad"]):checked'),
            dad = $('[data-checkboxes="' + group + '"][data-checkbox-role="dad"]'),
            total = all.length,
            checked_length = checked.length;

        if (role == 'dad') {
            if (me.is(':checked')) {
                all.prop('checked', true);
            } else {
                all.prop('checked', false);
            }
        } else {
            if (checked_length >= total) {
                dad.prop('checked', true);
            } else {
                dad.prop('checked', false);
            }
        }
    });
});

$("#table-1").dataTable({
    "columnDefs": [
        { "sortable": false, "targets": [2, 3] }
    ]
});
$("#table-2").dataTable({
    "columnDefs": [
        { "sortable": false, "targets": [0, 2, 3] }
    ],
    order: [
            [1, "asc"]
        ] //column indexes is zero based

});
$('#save-stage').DataTable({
    "scrollX": true,
    stateSave: true
});
$('#save-stage1').DataTable({
    "scrollX": true,
    stateSave: true
});
$('#tableExportSyncedBT').DataTable({
    dom: 'Bfrtip',
    buttons: [
        'copy', 'csv', 'excel', {
            extend: 'pdfHtml5',
            orientation: 'landscape',
            pageSize: 'LEGAL'
        }, 'print'
    ],
    ajax: {
        "url": 'assets/php/productDataSyncedBT.php',
        "dataSrc": ''
    },
    "deferRender": true,
    columns: [
        { data: 'LOCAL_UPC' },
        { data: 'SHOPIFY_ID' },
        { data: 'DESCRIPTION1' },
        { data: 'DESCRIPTION2' },
        { data: 'SIZ' },
        { data: 'ATTR' },
        { data: 'BASE_PRICE', className: "text-right" },
        { data: 'RETAIL_PRICE', className: "text-right" },
        { data: 'OH_QTY', className: "text-right" },
        { data: 'SO_QTY', className: "text-right" },
        { data: 'AVAILABLE_QTY', className: "text-right" },
        { data: 'QTY_UPDATE', type: 'date-dd-mmm-yyyy h:mm A', targets: 0 },
        { data: 'PRICE_UPDATE', type: 'date-dd-mmm-yyyy h:mm A', targets: 0 }
    ]
});
$('#tableExportSyncedBT1').DataTable({
    dom: 'Bfrtip',
    buttons: [
        'copy', 'csv', 'excel', {
            extend: 'pdfHtml5',
            orientation: 'landscape',
            pageSize: 'LEGAL'
        }, 'print'
    ],
    ajax: {
        "url": 'assets/php/productDataSyncedPPL.php',
        "dataSrc": ''
    },
    "deferRender": true,
    columns: [
        { data: 'LOCAL_UPC' },
        { data: 'SHOPIFY_ID' },
        { data: 'DESCRIPTION1' },
        { data: 'DESCRIPTION2' },
        { data: 'SIZ' },
        { data: 'ATTR' },
        { data: 'BASE_PRICE', className: "text-right" },
        { data: 'RETAIL_PRICE', className: "text-right" },
        { data: 'OH_QTY', className: "text-right" },
        { data: 'SO_QTY', className: "text-right" },
        { data: 'AVAILABLE_QTY', className: "text-right" },
        { data: 'QTY_UPDATE', type: 'date-dd-mmm-yyyy h:mm A', targets: 0 },
        { data: 'PRICE_UPDATE', type: 'date-dd-mmm-yyyy h:mm A', targets: 0 }
    ]
});
// window.setTimeout(function() {
$('#tableExportNewBT').DataTable({
    dom: 'Bfrtip',
    buttons: [
        'copy', 'csv', 'excel', {
            extend: 'pdfHtml5',
            orientation: 'landscape',
            pageSize: 'LEGAL'
        }, 'print'
    ],
    ajax: {
        "url": 'assets/php/productDataNewBT.php',
        "dataSrc": ''
    },
    "deferRender": true,
    columns: [
        { data: 'LOCAL_UPC' },
        { data: 'DESCRIPTION1' },
        { data: 'DESCRIPTION2' },
        { data: 'SIZ' },
        { data: 'ATTR' },
        { data: 'BASE_PRICE', className: "text-right" },
        { data: 'RETAIL_PRICE', className: "text-right" },
        { data: 'OH_QTY', className: "text-right" },
        { data: 'SO_QTY', className: "text-right" },
        { data: 'AVAILABLE_QTY', className: "text-right" },
        { data: 'QTY_UPDATE' },
        { data: 'PRICE_UPDATE' }
    ]
});
// }, 5000);
window.setTimeout(function() {
    $('#tableExportNewBT1').DataTable({
        dom: 'Bfrtip',
        buttons: [
            'copy', 'csv', 'excel', {
                extend: 'pdfHtml5',
                orientation: 'landscape',
                pageSize: 'LEGAL'
            }, 'print'
        ],
        ajax: {
            "url": 'assets/php/productDataNewPPL.php',
            "dataSrc": ''
        },
        "deferRender": true,
        columns: [
            { data: 'LOCAL_UPC' },
            { data: 'DESCRIPTION1' },
            { data: 'DESCRIPTION2' },
            { data: 'SIZ' },
            { data: 'ATTR' },
            { data: 'BASE_PRICE', className: "text-right" },
            { data: 'RETAIL_PRICE', className: "text-right" },
            { data: 'OH_QTY', className: "text-right" },
            { data: 'SO_QTY', className: "text-right" },
            { data: 'AVAILABLE_QTY', className: "text-right" },
            { data: 'QTY_UPDATE' },
            { data: 'PRICE_UPDATE' }
        ]
    });
}, 5000);
var tableNew = $('#tableExportNewOrder').DataTable({
    dom: 'Bfrtip',
    buttons: [
        'copy', 'csv', 'excel', {
            extend: 'pdfHtml5',
            orientation: 'landscape',
            pageSize: 'LEGAL'
        }, 'print'
    ],
    "processing": true,
    // "serverSide": true,
    ajax: {
        "url": 'assets/php/orderTableData.php',
        "type": "POST",
        "data": {
            "check": "1"
        },
        "dataSrc": ''
    },
    "deferRender": true,
    columns: [
        { data: 'ORDERID' },
        { data: 'SID' },
        { data: 'STATUS' },
        { data: 'TOTAL_PRICE' },
        { data: 'TOTAL_QUANTITY' },
    ]
});
var tableConfirm = $('#tableExportConfirmedOrder').DataTable({
    dom: 'Bfrtip',
    buttons: [
        'copy', 'csv', 'excel', {
            extend: 'pdfHtml5',
            orientation: 'landscape',
            pageSize: 'LEGAL'
        }, 'print'
    ],
    "processing": true,
    // "serverSide": true,
    ajax: {
        "url": 'assets/php/orderTableData.php',
        "type": "POST",
        "data": {
            "check": "2"
        },
        "dataSrc": ''
    },
    "deferRender": true,
    columns: [
        { data: 'ORDERID' },
        { data: 'SID' },
        { data: 'STATUS' },
        { data: 'TOTAL_PRICE' },
        { data: 'TOTAL_QUANTITY' },
    ]
});
var tableStoreAssign = $('#tableExportStoreAssignOrder').DataTable({
    dom: 'Bfrtip',
    buttons: [
        'copy', 'csv', 'excel', {
            extend: 'pdfHtml5',
            orientation: 'landscape',
            pageSize: 'LEGAL'
        }, 'print'
    ],
    "processing": true,
    // "serverSide": true,
    ajax: {
        "url": 'assets/php/orderTableData.php',
        "type": "POST",
        "data": {
            "check": "3"
        },
        "dataSrc": ''
    },
    "deferRender": true,
    columns: [
        { data: 'ORDERID' },
        { data: 'SID' },
        { data: 'STATUS' },
        { data: 'TOTAL_PRICE' },
        { data: 'TOTAL_QUANTITY' },
    ]
});
var tablePickOrder = $('#tableExportPickOrder').DataTable({
    dom: 'Bfrtip',
    buttons: [
        'copy', 'csv', 'excel', {
            extend: 'pdfHtml5',
            orientation: 'landscape',
            pageSize: 'LEGAL'
        }, 'print'
    ],
    "processing": true,
    // "serverSide": true,
    ajax: {
        "url": 'assets/php/orderTableData.php',
        "type": "POST",
        "data": {
            "check": "4"
        },
        "dataSrc": ''
    },
    "deferRender": true,
    columns: [
        { data: 'ORDERID' },
        { data: 'SID' },
        { data: 'STATUS' },
        { data: 'TOTAL_PRICE' },
        { data: 'TOTAL_QUANTITY' },
    ]
});
var tablePackOrder = $('#tableExportPackOrder').DataTable({
    dom: 'Bfrtip',
    buttons: [
        'copy', 'csv', 'excel', {
            extend: 'pdfHtml5',
            orientation: 'landscape',
            pageSize: 'LEGAL'
        }, 'print'
    ],
    "processing": true,
    // "serverSide": true,
    ajax: {
        "url": 'assets/php/orderTableData.php',
        "type": "POST",
        "data": {
            "check": "5"
        },
        "dataSrc": ''
    },
    "deferRender": true,
    columns: [
        { data: 'ORDERID' },
        { data: 'SID' },
        { data: 'STATUS' },
        { data: 'TOTAL_PRICE' },
        { data: 'TOTAL_QUANTITY' },
    ]
});
var tableExportCourier = $('#tableExportCourierOrder').DataTable({
    dom: 'Bfrtip',
    buttons: [
        'copy', 'csv', 'excel', {
            extend: 'pdfHtml5',
            orientation: 'landscape',
            pageSize: 'LEGAL'
        }, 'print'
    ],
    "processing": true,
    // "serverSide": true,
    ajax: {
        "url": 'assets/php/orderTableData.php',
        "type": "POST",
        "data": {
            "check": "6"
        },
        "dataSrc": ''
    },
    "deferRender": true,
    columns: [
        { data: 'ORDERID' },
        { data: 'SID' },
        { data: 'STATUS' },
        { data: 'TOTAL_PRICE' },
        { data: 'TOTAL_QUANTITY' },
    ]
});
var tableDispatch = $('#tableExportDispatchedOrder').DataTable({
    dom: 'Bfrtip',
    buttons: [
        'copy', 'csv', 'excel', {
            extend: 'pdfHtml5',
            orientation: 'landscape',
            pageSize: 'LEGAL'
        }, 'print'
    ],
    "processing": true,
    // "serverSide": true,
    ajax: {
        "url": 'assets/php/orderTableData.php',
        "type": "POST",
        "data": {
            "check": "7"
        },
        "dataSrc": ''
    },
    "deferRender": true,
    columns: [
        { data: 'ORDERID' },
        { data: 'SID' },
        { data: 'STATUS' },
        { data: 'TOTAL_PRICE' },
        { data: 'TOTAL_QUANTITY' },
    ]
});
var tableDelivered = $('#tableExportDeliveredOrder').DataTable({
    dom: 'Bfrtip',
    buttons: [
        'copy', 'csv', 'excel', {
            extend: 'pdfHtml5',
            orientation: 'landscape',
            pageSize: 'LEGAL'
        }, 'print'
    ],
    "processing": true,
    // "serverSide": true,
    ajax: {
        "url": 'assets/php/orderTableData.php',
        "type": "POST",
        "data": {
            "check": "8"
        },
        "dataSrc": ''
    },
    "deferRender": true,
    columns: [
        { data: 'ORDERID' },
        { data: 'SID' },
        { data: 'STATUS' },
        { data: 'TOTAL_PRICE' },
        { data: 'TOTAL_QUANTITY' },
    ]
});
var tableUndelivered = $('#tableExportUndeliveredOrder').DataTable({
    dom: 'Bfrtip',
    buttons: [
        'copy', 'csv', 'excel', {
            extend: 'pdfHtml5',
            orientation: 'landscape',
            pageSize: 'LEGAL'
        }, 'print'
    ],
    "processing": true,
    // "serverSide": true,
    ajax: {
        "url": 'assets/php/orderTableData.php',
        "type": "POST",
        "data": {
            "check": "9"
        },
        "dataSrc": ''
    },
    "deferRender": true,
    columns: [
        { data: 'ORDERID' },
        { data: 'SID' },
        { data: 'STATUS' },
        { data: 'TOTAL_PRICE' },
        { data: 'TOTAL_QUANTITY' },
    ]
});
var tableReturn = $('#tableExportReturnOrder').DataTable({
    dom: 'Bfrtip',
    buttons: [
        'copy', 'csv', 'excel', {
            extend: 'pdfHtml5',
            orientation: 'landscape',
            pageSize: 'LEGAL'
        }, 'print'
    ],
    "processing": true,
    // "serverSide": true,
    ajax: {
        "url": 'assets/php/orderTableData.php',
        "type": "POST",
        "data": {
            "check": "10"
        },
        "dataSrc": ''
    },
    "deferRender": true,
    columns: [
        { data: 'ORDERID' },
        { data: 'SID' },
        { data: 'STATUS' },
        { data: 'TOTAL_PRICE' },
        { data: 'TOTAL_QUANTITY' },
    ]
});
var tableCancel = $('#tableExportCancelOrder').DataTable({
    dom: 'Bfrtip',
    buttons: [
        'copy', 'csv', 'excel', {
            extend: 'pdfHtml5',
            orientation: 'landscape',
            pageSize: 'LEGAL'
        }, 'print'
    ],
    "processing": true,
    // "serverSide": true,
    ajax: {
        "url": 'assets/php/orderTableData.php',
        "type": "POST",
        "data": {
            "check": "11"
        },
        "dataSrc": ''
    },
    "deferRender": true,
    columns: [
        { data: 'ORDERID' },
        { data: 'SID' },
        { data: 'STATUS' },
        { data: 'TOTAL_PRICE' },
        { data: 'TOTAL_QUANTITY' },
    ]
});
$('#tableExportNewOrder').on('click', 'tbody tr', function() {
    var sid = tableNew.row(this).data()['SID'];
    $('#model').show();
    $.ajax({
        url: 'assets/php/orderDetailData.php',
        type: "POST",
        data: {
            sid: sid,
            check: "1"
        },
        success: function(dataResult) {
            var dataResult = JSON.parse(dataResult);
            console.log(dataResult[0].orderId);
            if (document.getElementById('orderId').innerHTML = dataResult[0].orderId) {
                document.getElementById('orderId').innerHTML = dataResult[0].orderId;
            } else {
                document.getElementById('orderId').innerHTML = "N/A";
            }
            if (document.getElementById('orderDate').innerHTML = dataResult[0].date) {
                document.getElementById('orderDate').innerHTML = dataResult[0].date;
            } else {
                document.getElementById('orderDate').innerHTML = "N/A";
            }
            if (document.getElementById('orderStatus').innerHTML = dataResult[0].status) {
                document.getElementById('orderStatus').innerHTML = dataResult[0].status;
            } else {
                document.getElementById('orderStatus').innerHTML = "N/A";
            }
            if (document.getElementById('orderPaymentMethod').innerHTML = dataResult[0].tenderName) {
                document.getElementById('orderPaymentMethod').innerHTML = dataResult[0].tenderName;
            } else {
                document.getElementById('orderPaymentMethod').innerHTML = "N/A";
            }
            if (document.getElementById('custName').innerHTML = dataResult[0].fullName) {
                document.getElementById('custName').innerHTML = dataResult[0].fullName;
            } else {
                document.getElementById('custName').innerHTML = "N/A";
            }
            if (document.getElementById('custEmail').innerHTML = dataResult[0].email) {
                document.getElementById('custEmail').innerHTML = dataResult[0].email;
            } else {
                document.getElementById('custEmail').innerHTML = "N/A";
            }
            if (document.getElementById('custNumber').innerHTML = dataResult[0].number) {
                document.getElementById('custNumber').innerHTML = dataResult[0].number;
            } else {
                document.getElementById('custNumber').innerHTML = "N/A";
            }
            if (document.getElementById('custAddress').innerHTML = dataResult[0].address) {
                document.getElementById('custAddress').innerHTML = dataResult[0].address;
            } else {
                document.getElementById('custAddress').innerHTML = "N/A";
            }


        }
    });
    $.ajax({
        url: 'assets/php/orderItemDetailData.php',
        type: "POST",
        data: {
            sid: sid,
            check: "1"
        },
        success: function(dataResult1) {
            var dataResult1 = JSON.parse(dataResult1);
            console.log(dataResult1);
            table(dataResult1);

        }

    });
    $.ajax({
        url: 'assets/php/orderHistoryDetailData.php',
        type: "POST",
        data: {
            sid: sid,
            check: "1"
        },
        success: function(dataResult2) {
            var dataResult2 = JSON.parse(dataResult2);
            console.log(dataResult2);
            table1(dataResult2);

        }

    });

    function table(dataResult1) {
        var eTable = "<table class='table table-stripped'><thead><tr><th>SKU/Item</th><th>Price</th><th>QTY</th><th>Total</th></tr></thead><tbody>"
        for (var i = 0; i < dataResult1.length; i++) {
            eTable += "<tr style='border-bottom: 1px solid black;'>";
            eTable += "<td>" + "<strong>SKU:</strong>" + dataResult1[i]['sku'] + "<br>" + "<strong>Size:</strong>" + dataResult1[i]['itemSize'] + "<br>" + "<strong>Color:</strong>" + dataResult1[i]['attribute'] + "</td>";
            eTable += "<td>" + dataResult1[i]['price'] + "</td>";
            eTable += "<td>" + dataResult1[i]['qty'] + "</td>";
            eTable += "<td>" + dataResult1[i]['totalItem'] + "</td>";
            eTable += "</tr>";

        }
        eTable += "</tbody></table>";
        $('#forTable').html(eTable);
        var fTable = "<div class='container'><div class='row'><div class='col-4 align-left'>";
        fTable += "<strong>Discount Amount:</strong><br>";
        fTable += "<strong >Shipping Amount:</strong><br>";
        fTable += "<strong >Tax Amount:</strong><br>";
        fTable += "<strong >Total Amount:</strong><br>";
        fTable += "</div>";
        fTable += "<div class='col-4'></div>";
        fTable += "<div class='col-4 align-right'>";

        if (!dataResult1[0]['docDiscount']) {
            fTable += "<strong>PKR 0</strong><br>";
        } else {
            fTable += "<strong>PKR " + dataResult1[0]['docDiscount'] + "</strong><br>";
        }
        if (!dataResult1[0]['shippingAmt']) {
            fTable += "<strong>PKR 0</strong><br>";
        } else {
            fTable += "<strong>" + dataResult1[0]['shippingAmt'] + "</strong><br>";
        }
        fTable += "<strong>PKR " + dataResult1[0]['tax'] + "</strong><br>";
        fTable += "<strong>PKR " + dataResult1[0]['total'] + "</strong><br>";
        fTable += "</div>";
        fTable += "</div>";
        fTable += "</div>";
        $('#forTable1').html(fTable);
    }

    function table1(dataResult2) {
        var eTable = "<div class='container'><div class='row'><div class='col-4 align-left'>";
        eTable += "<strong> Shopify Order Id </strong><br>";
        eTable += "<br>";
        for (var i = 0; i < dataResult2.length; i++) {
            eTable += "<strong>" + dataResult2[i]['orderShopifyNumber'] + "</strong><br>";
        }
        eTable += "</div>";
        eTable += "<div class='col-4'></div>";
        eTable += "<div class='col-4 align-right'>";
        eTable += "<strong> Status </strong><br>";
        eTable += "<br>";
        for (var j = 0; j < dataResult2.length; j++) {
            eTable += "<strong>" + dataResult2[j]['status'] + "</strong><br>";
        }
        eTable += "</div>";
        $('#forTable2').html(eTable);
    }


})
$('#tableExportConfirmedOrder').on('click', 'tbody tr', function() {
    var sid = tableConfirm.row(this).data()['SID'];
    $('#model').show();
    $.ajax({
        url: 'assets/php/orderDetailData.php',
        type: "POST",
        data: {
            sid: sid,
            check: "2"
        },
        success: function(dataResult) {
            var dataResult = JSON.parse(dataResult);
            console.log(dataResult[0].orderId);
            if (document.getElementById('orderId').innerHTML = dataResult[0].orderId) {
                document.getElementById('orderId').innerHTML = dataResult[0].orderId;
            } else {
                document.getElementById('orderId').innerHTML = "N/A";
            }
            if (document.getElementById('orderDate').innerHTML = dataResult[0].date) {
                document.getElementById('orderDate').innerHTML = dataResult[0].date;
            } else {
                document.getElementById('orderDate').innerHTML = "N/A";
            }
            if (document.getElementById('orderStatus').innerHTML = dataResult[0].status) {
                document.getElementById('orderStatus').innerHTML = dataResult[0].status;
            } else {
                document.getElementById('orderStatus').innerHTML = "N/A";
            }
            if (document.getElementById('orderPaymentMethod').innerHTML = dataResult[0].tenderName) {
                document.getElementById('orderPaymentMethod').innerHTML = dataResult[0].tenderName;
            } else {
                document.getElementById('orderPaymentMethod').innerHTML = "N/A";
            }
            if (document.getElementById('custName').innerHTML = dataResult[0].fullName) {
                document.getElementById('custName').innerHTML = dataResult[0].fullName;
            } else {
                document.getElementById('custName').innerHTML = "N/A";
            }
            if (document.getElementById('custEmail').innerHTML = dataResult[0].email) {
                document.getElementById('custEmail').innerHTML = dataResult[0].email;
            } else {
                document.getElementById('custEmail').innerHTML = "N/A";
            }
            if (document.getElementById('custNumber').innerHTML = dataResult[0].number) {
                document.getElementById('custNumber').innerHTML = dataResult[0].number;
            } else {
                document.getElementById('custNumber').innerHTML = "N/A";
            }
            if (document.getElementById('custAddress').innerHTML = dataResult[0].address) {
                document.getElementById('custAddress').innerHTML = dataResult[0].address;
            } else {
                document.getElementById('custAddress').innerHTML = "N/A";
            }


        }
    });
    $.ajax({
        url: 'assets/php/orderItemDetailData.php',
        type: "POST",
        data: {
            sid: sid,
            check: "2"
        },
        success: function(dataResult1) {
            var dataResult1 = JSON.parse(dataResult1);
            console.log(dataResult1);
            table(dataResult1);

        }

    });
    $.ajax({
        url: 'assets/php/orderHistoryDetailData.php',
        type: "POST",
        data: {
            sid: sid,
            check: "1"
        },
        success: function(dataResult2) {
            var dataResult2 = JSON.parse(dataResult2);
            console.log(dataResult2);
            table1(dataResult2);

        }

    });

    function table(dataResult1) {
        var eTable = "<table class='table table-stripped'><thead><tr><th>SKU/Item</th><th>Price</th><th>QTY</th><th>Total</th></tr></thead><tbody>"
        for (var i = 0; i < dataResult1.length; i++) {
            eTable += "<tr style='border-bottom: 1px solid black;'>";
            eTable += "<td>" + "<strong>SKU:</strong>" + dataResult1[i]['sku'] + "<br>" + "<strong>Size:</strong>" + dataResult1[i]['itemSize'] + "<br>" + "<strong>Color:</strong>" + dataResult1[i]['attribute'] + "</td>";
            eTable += "<td>" + dataResult1[i]['price'] + "</td>";
            eTable += "<td>" + dataResult1[i]['qty'] + "</td>";
            eTable += "<td>" + dataResult1[i]['totalItem'] + "</td>";
            eTable += "</tr>";

        }
        eTable += "</tbody></table>";
        $('#forTable').html(eTable);
        var fTable = "<div class='container'><div class='row'><div class='col-4 align-left'>";
        fTable += "<strong>Discount Amount:</strong><br>";
        fTable += "<strong >Shipping Amount:</strong><br>";
        fTable += "<strong >Tax Amount:</strong><br>";
        fTable += "<strong >Total Amount:</strong><br>";
        fTable += "</div>";
        fTable += "<div class='col-4'></div>";
        fTable += "<div class='col-4 align-right'>";

        if (!dataResult1[0]['docDiscount']) {
            fTable += "<strong>PKR 0</strong><br>";
        } else {
            fTable += "<strong>PKR " + dataResult1[0]['docDiscount'] + "</strong><br>";
        }
        if (!dataResult1[0]['shippingAmt']) {
            fTable += "<strong>PKR 0</strong><br>";
        } else {
            fTable += "<strong>" + dataResult1[0]['shippingAmt'] + "</strong><br>";
        }
        if (!dataResult1[0]['tax']) {
            fTable += "<strong>PKR 0</strong><br>";
        } else {
            fTable += "<strong>PKR " + dataResult1[0]['tax'] + "</strong><br>";
        }
        if (!dataResult1[0]['total']) {
            fTable += "<strong>PKR 0</strong><br>";
        } else {
            fTable += "<strong>PKR " + dataResult1[0]['total'] + "</strong><br>";
        }
        fTable += "</div>";
        fTable += "</div>";
        fTable += "</div>";
        $('#forTable1').html(fTable);
    }

    function table1(dataResult2) {
        var eTable = "<div class='container'><div class='row'><div class='col-4 align-left'>";
        eTable += "<strong> Shopify Order Id </strong><br>";
        eTable += "<br>";
        for (var i = 0; i < dataResult2.length; i++) {
            eTable += "<strong>" + dataResult2[i]['orderShopifyNumber'] + "</strong><br>";
        }
        eTable += "</div>";
        eTable += "<div class='col-4'></div>";
        eTable += "<div class='col-4 align-right'>";
        eTable += "<strong> Status </strong><br>";
        eTable += "<br>";
        for (var j = 0; j < dataResult2.length; j++) {
            eTable += "<strong>" + dataResult2[j]['status'] + "</strong><br>";
        }
        eTable += "</div>";
        $('#forTable2').html(eTable);
    }

})
$('#tableExportStoreAssignOrder').on('click', 'tbody tr', function() {
    var sid = tableStoreAssign.row(this).data()['SID'];
    $('#model').show();
    $.ajax({
        url: 'assets/php/orderDetailData.php',
        type: "POST",
        data: {
            sid: sid,
            check: "3"
        },
        success: function(dataResult) {
            var dataResult = JSON.parse(dataResult);
            console.log(dataResult[0].orderId);
            if (document.getElementById('orderId').innerHTML = dataResult[0].orderId) {
                document.getElementById('orderId').innerHTML = dataResult[0].orderId;
            } else {
                document.getElementById('orderId').innerHTML = "N/A";
            }
            if (document.getElementById('orderDate').innerHTML = dataResult[0].date) {
                document.getElementById('orderDate').innerHTML = dataResult[0].date;
            } else {
                document.getElementById('orderDate').innerHTML = "N/A";
            }
            if (document.getElementById('orderStatus').innerHTML = dataResult[0].status) {
                document.getElementById('orderStatus').innerHTML = dataResult[0].status;
            } else {
                document.getElementById('orderStatus').innerHTML = "N/A";
            }
            if (document.getElementById('orderPaymentMethod').innerHTML = dataResult[0].tenderName) {
                document.getElementById('orderPaymentMethod').innerHTML = dataResult[0].tenderName;
            } else {
                document.getElementById('orderPaymentMethod').innerHTML = "N/A";
            }
            if (document.getElementById('custName').innerHTML = dataResult[0].fullName) {
                document.getElementById('custName').innerHTML = dataResult[0].fullName;
            } else {
                document.getElementById('custName').innerHTML = "N/A";
            }
            if (document.getElementById('custEmail').innerHTML = dataResult[0].email) {
                document.getElementById('custEmail').innerHTML = dataResult[0].email;
            } else {
                document.getElementById('custEmail').innerHTML = "N/A";
            }
            if (document.getElementById('custNumber').innerHTML = dataResult[0].number) {
                document.getElementById('custNumber').innerHTML = dataResult[0].number;
            } else {
                document.getElementById('custNumber').innerHTML = "N/A";
            }
            if (document.getElementById('custAddress').innerHTML = dataResult[0].address) {
                document.getElementById('custAddress').innerHTML = dataResult[0].address;
            } else {
                document.getElementById('custAddress').innerHTML = "N/A";
            }


        }
    });
    $.ajax({
        url: 'assets/php/orderItemDetailData.php',
        type: "POST",
        data: {
            sid: sid,
            check: "3"
        },
        success: function(dataResult1) {
            var dataResult1 = JSON.parse(dataResult1);
            console.log(dataResult1);
            table(dataResult1);

        }

    });
    $.ajax({
        url: 'assets/php/orderHistoryDetailData.php',
        type: "POST",
        data: {
            sid: sid,
            check: "1"
        },
        success: function(dataResult2) {
            var dataResult2 = JSON.parse(dataResult2);
            console.log(dataResult2);
            table1(dataResult2);

        }

    });

    function table(dataResult1) {
        var eTable = "<table class='table table-stripped'><thead><tr><th>SKU/Item</th><th>Price</th><th>QTY</th><th>Total</th></tr></thead><tbody>"
        for (var i = 0; i < dataResult1.length; i++) {
            eTable += "<tr style='border-bottom: 1px solid black;'>";
            eTable += "<td>" + "<strong>SKU:</strong>" + dataResult1[i]['sku'] + "<br>" + "<strong>Size:</strong>" + dataResult1[i]['itemSize'] + "<br>" + "<strong>Color:</strong>" + dataResult1[i]['attribute'] + "</td>";
            eTable += "<td>" + dataResult1[i]['price'] + "</td>";
            eTable += "<td>" + dataResult1[i]['qty'] + "</td>";
            eTable += "<td>" + dataResult1[i]['totalItem'] + "</td>";
            eTable += "</tr>";

        }
        eTable += "</tbody></table>";
        $('#forTable').html(eTable);
        var fTable = "<div class='container'><div class='row'><div class='col-4 align-left'>";
        fTable += "<strong>Discount Amount:</strong><br>";
        fTable += "<strong >Shipping Amount:</strong><br>";
        fTable += "<strong >Tax Amount:</strong><br>";
        fTable += "<strong >Total Amount:</strong><br>";
        fTable += "</div>";
        fTable += "<div class='col-4'></div>";
        fTable += "<div class='col-4 align-right'>";

        if (!dataResult1[0]['docDiscount']) {
            fTable += "<strong>PKR 0</strong><br>";
        } else {
            fTable += "<strong>PKR " + dataResult1[0]['docDiscount'] + "</strong><br>";
        }
        if (!dataResult1[0]['shippingAmt']) {
            fTable += "<strong>PKR 0</strong><br>";
        } else {
            fTable += "<strong>" + dataResult1[0]['shippingAmt'] + "</strong><br>";
        }
        if (!dataResult1[0]['tax']) {
            fTable += "<strong>PKR 0</strong><br>";
        } else {
            fTable += "<strong>PKR " + dataResult1[0]['tax'] + "</strong><br>";
        }
        if (!dataResult1[0]['total']) {
            fTable += "<strong>PKR 0</strong><br>";
        } else {
            fTable += "<strong>PKR " + dataResult1[0]['total'] + "</strong><br>";
        }
        fTable += "</div>";
        fTable += "</div>";
        fTable += "</div>";
        $('#forTable1').html(fTable);
    }

    function table1(dataResult2) {
        var eTable = "<div class='container'><div class='row'><div class='col-4 align-left'>";
        eTable += "<strong> Shopify Order Id </strong><br>";
        eTable += "<br>";
        for (var i = 0; i < dataResult2.length; i++) {
            eTable += "<strong>" + dataResult2[i]['orderShopifyNumber'] + "</strong><br>";
        }
        eTable += "</div>";
        eTable += "<div class='col-4'></div>";
        eTable += "<div class='col-4 align-right'>";
        eTable += "<strong> Status </strong><br>";
        eTable += "<br>";
        for (var j = 0; j < dataResult2.length; j++) {
            eTable += "<strong>" + dataResult2[j]['status'] + "</strong><br>";
        }
        eTable += "</div>";
        $('#forTable2').html(eTable);
    }

})
$('#tableExportPickOrder').on('click', 'tbody tr', function() {
    var sid = tablePickOrder.row(this).data()['SID'];
    $('#model').show();
    $.ajax({
        url: 'assets/php/orderDetailData.php',
        type: "POST",
        data: {
            sid: sid,
            check: "4"
        },
        success: function(dataResult) {
            var dataResult = JSON.parse(dataResult);
            console.log(dataResult[0].orderId);
            if (document.getElementById('orderId').innerHTML = dataResult[0].orderId) {
                document.getElementById('orderId').innerHTML = dataResult[0].orderId;
            } else {
                document.getElementById('orderId').innerHTML = "N/A";
            }
            if (document.getElementById('orderDate').innerHTML = dataResult[0].date) {
                document.getElementById('orderDate').innerHTML = dataResult[0].date;
            } else {
                document.getElementById('orderDate').innerHTML = "N/A";
            }
            if (document.getElementById('orderStatus').innerHTML = dataResult[0].status) {
                document.getElementById('orderStatus').innerHTML = dataResult[0].status;
            } else {
                document.getElementById('orderStatus').innerHTML = "N/A";
            }
            if (document.getElementById('orderPaymentMethod').innerHTML = dataResult[0].tenderName) {
                document.getElementById('orderPaymentMethod').innerHTML = dataResult[0].tenderName;
            } else {
                document.getElementById('orderPaymentMethod').innerHTML = "N/A";
            }
            if (document.getElementById('custName').innerHTML = dataResult[0].fullName) {
                document.getElementById('custName').innerHTML = dataResult[0].fullName;
            } else {
                document.getElementById('custName').innerHTML = "N/A";
            }
            if (document.getElementById('custEmail').innerHTML = dataResult[0].email) {
                document.getElementById('custEmail').innerHTML = dataResult[0].email;
            } else {
                document.getElementById('custEmail').innerHTML = "N/A";
            }
            if (document.getElementById('custNumber').innerHTML = dataResult[0].number) {
                document.getElementById('custNumber').innerHTML = dataResult[0].number;
            } else {
                document.getElementById('custNumber').innerHTML = "N/A";
            }
            if (document.getElementById('custAddress').innerHTML = dataResult[0].address) {
                document.getElementById('custAddress').innerHTML = dataResult[0].address;
            } else {
                document.getElementById('custAddress').innerHTML = "N/A";
            }


        }
    });
    $.ajax({
        url: 'assets/php/orderItemDetailData.php',
        type: "POST",
        data: {
            sid: sid,
            check: "4"
        },
        success: function(dataResult1) {
            var dataResult1 = JSON.parse(dataResult1);
            console.log(dataResult1);
            table(dataResult1);

        }

    });
    $.ajax({
        url: 'assets/php/orderHistoryDetailData.php',
        type: "POST",
        data: {
            sid: sid,
            check: "1"
        },
        success: function(dataResult2) {
            var dataResult2 = JSON.parse(dataResult2);
            console.log(dataResult2);
            table1(dataResult2);

        }

    });

    function table(dataResult1) {
        var eTable = "<table class='table table-stripped'><thead><tr><th>SKU/Item</th><th>Price</th><th>QTY</th><th>Total</th></tr></thead><tbody>"
        for (var i = 0; i < dataResult1.length; i++) {
            eTable += "<tr style='border-bottom: 1px solid black;'>";
            eTable += "<td>" + "<strong>SKU:</strong>" + dataResult1[i]['sku'] + "<br>" + "<strong>Size:</strong>" + dataResult1[i]['itemSize'] + "<br>" + "<strong>Color:</strong>" + dataResult1[i]['attribute'] + "</td>";
            eTable += "<td>" + dataResult1[i]['price'] + "</td>";
            eTable += "<td>" + dataResult1[i]['qty'] + "</td>";
            eTable += "<td>" + dataResult1[i]['totalItem'] + "</td>";
            eTable += "</tr>";

        }
        eTable += "</tbody></table>";
        $('#forTable').html(eTable);
        var fTable = "<div class='container'><div class='row'><div class='col-4 align-left'>";
        fTable += "<strong>Discount Amount:</strong><br>";
        fTable += "<strong >Shipping Amount:</strong><br>";
        fTable += "<strong >Tax Amount:</strong><br>";
        fTable += "<strong >Total Amount:</strong><br>";
        fTable += "</div>";
        fTable += "<div class='col-4'></div>";
        fTable += "<div class='col-4 align-right'>";

        if (!dataResult1[0]['docDiscount']) {
            fTable += "<strong>PKR 0</strong><br>";
        } else {
            fTable += "<strong>PKR " + dataResult1[0]['docDiscount'] + "</strong><br>";
        }
        if (!dataResult1[0]['shippingAmt']) {
            fTable += "<strong>PKR 0</strong><br>";
        } else {
            fTable += "<strong>" + dataResult1[0]['shippingAmt'] + "</strong><br>";
        }
        if (!dataResult1[0]['tax']) {
            fTable += "<strong>PKR 0</strong><br>";
        } else {
            fTable += "<strong>PKR " + dataResult1[0]['tax'] + "</strong><br>";
        }
        if (!dataResult1[0]['total']) {
            fTable += "<strong>PKR 0</strong><br>";
        } else {
            fTable += "<strong>PKR " + dataResult1[0]['total'] + "</strong><br>";
        }
        fTable += "</div>";
        fTable += "</div>";
        fTable += "</div>";
        $('#forTable1').html(fTable);
    }

    function table1(dataResult2) {
        var eTable = "<div class='container'><div class='row'><div class='col-4 align-left'>";
        eTable += "<strong> Shopify Order Id </strong><br>";
        eTable += "<br>";
        for (var i = 0; i < dataResult2.length; i++) {
            eTable += "<strong>" + dataResult2[i]['orderShopifyNumber'] + "</strong><br>";
        }
        eTable += "</div>";
        eTable += "<div class='col-4'></div>";
        eTable += "<div class='col-4 align-right'>";
        eTable += "<strong> Status </strong><br>";
        eTable += "<br>";
        for (var j = 0; j < dataResult2.length; j++) {
            eTable += "<strong>" + dataResult2[j]['status'] + "</strong><br>";
        }
        eTable += "</div>";
        $('#forTable2').html(eTable);
    }

})
$('#tableExportPackOrder').on('click', 'tbody tr', function() {
    var sid = tablePackOrder.row(this).data()['SID'];
    $('#model').show();
    $.ajax({
        url: 'assets/php/orderDetailData.php',
        type: "POST",
        data: {
            sid: sid,
            check: "5"
        },
        success: function(dataResult) {
            var dataResult = JSON.parse(dataResult);
            console.log(dataResult[0].orderId);
            if (document.getElementById('orderId').innerHTML = dataResult[0].orderId) {
                document.getElementById('orderId').innerHTML = dataResult[0].orderId;
            } else {
                document.getElementById('orderId').innerHTML = "N/A";
            }
            if (document.getElementById('orderDate').innerHTML = dataResult[0].date) {
                document.getElementById('orderDate').innerHTML = dataResult[0].date;
            } else {
                document.getElementById('orderDate').innerHTML = "N/A";
            }
            if (document.getElementById('orderStatus').innerHTML = dataResult[0].status) {
                document.getElementById('orderStatus').innerHTML = dataResult[0].status;
            } else {
                document.getElementById('orderStatus').innerHTML = "N/A";
            }
            if (document.getElementById('orderPaymentMethod').innerHTML = dataResult[0].tenderName) {
                document.getElementById('orderPaymentMethod').innerHTML = dataResult[0].tenderName;
            } else {
                document.getElementById('orderPaymentMethod').innerHTML = "N/A";
            }
            if (document.getElementById('custName').innerHTML = dataResult[0].fullName) {
                document.getElementById('custName').innerHTML = dataResult[0].fullName;
            } else {
                document.getElementById('custName').innerHTML = "N/A";
            }
            if (document.getElementById('custEmail').innerHTML = dataResult[0].email) {
                document.getElementById('custEmail').innerHTML = dataResult[0].email;
            } else {
                document.getElementById('custEmail').innerHTML = "N/A";
            }
            if (document.getElementById('custNumber').innerHTML = dataResult[0].number) {
                document.getElementById('custNumber').innerHTML = dataResult[0].number;
            } else {
                document.getElementById('custNumber').innerHTML = "N/A";
            }
            if (document.getElementById('custAddress').innerHTML = dataResult[0].address) {
                document.getElementById('custAddress').innerHTML = dataResult[0].address;
            } else {
                document.getElementById('custAddress').innerHTML = "N/A";
            }


        }
    });
    $.ajax({
        url: 'assets/php/orderItemDetailData.php',
        type: "POST",
        data: {
            sid: sid,
            check: "5"
        },
        success: function(dataResult1) {
            var dataResult1 = JSON.parse(dataResult1);
            console.log(dataResult1);
            table(dataResult1);

        }

    });
    $.ajax({
        url: 'assets/php/orderHistoryDetailData.php',
        type: "POST",
        data: {
            sid: sid,
            check: "1"
        },
        success: function(dataResult2) {
            var dataResult2 = JSON.parse(dataResult2);
            console.log(dataResult2);
            table1(dataResult2);

        }

    });

    function table(dataResult1) {
        var eTable = "<table class='table table-stripped'><thead><tr><th>SKU/Item</th><th>Price</th><th>QTY</th><th>Total</th></tr></thead><tbody>"
        for (var i = 0; i < dataResult1.length; i++) {
            eTable += "<tr style='border-bottom: 1px solid black;'>";
            eTable += "<td>" + "<strong>SKU:</strong>" + dataResult1[i]['sku'] + "<br>" + "<strong>Size:</strong>" + dataResult1[i]['itemSize'] + "<br>" + "<strong>Color:</strong>" + dataResult1[i]['attribute'] + "</td>";
            eTable += "<td>" + dataResult1[i]['price'] + "</td>";
            eTable += "<td>" + dataResult1[i]['qty'] + "</td>";
            eTable += "<td>" + dataResult1[i]['totalItem'] + "</td>";
            eTable += "</tr>";

        }
        eTable += "</tbody></table>";
        $('#forTable').html(eTable);
        var fTable = "<div class='container'><div class='row'><div class='col-4 align-left'>";
        fTable += "<strong>Discount Amount:</strong><br>";
        fTable += "<strong >Shipping Amount:</strong><br>";
        fTable += "<strong >Tax Amount:</strong><br>";
        fTable += "<strong >Total Amount:</strong><br>";
        fTable += "</div>";
        fTable += "<div class='col-4'></div>";
        fTable += "<div class='col-4 align-right'>";

        if (!dataResult1[0]['docDiscount']) {
            fTable += "<strong>PKR 0</strong><br>";
        } else {
            fTable += "<strong>PKR " + dataResult1[0]['docDiscount'] + "</strong><br>";
        }
        if (!dataResult1[0]['shippingAmt']) {
            fTable += "<strong>PKR 0</strong><br>";
        } else {
            fTable += "<strong>" + dataResult1[0]['shippingAmt'] + "</strong><br>";
        }
        if (!dataResult1[0]['tax']) {
            fTable += "<strong>PKR 0</strong><br>";
        } else {
            fTable += "<strong>PKR " + dataResult1[0]['tax'] + "</strong><br>";
        }
        if (!dataResult1[0]['total']) {
            fTable += "<strong>PKR 0</strong><br>";
        } else {
            fTable += "<strong>PKR " + dataResult1[0]['total'] + "</strong><br>";
        }
        fTable += "</div>";
        fTable += "</div>";
        fTable += "</div>";
        $('#forTable1').html(fTable);
    }

    function table1(dataResult2) {
        var eTable = "<div class='container'><div class='row'><div class='col-4 align-left'>";
        eTable += "<strong> Shopify Order Id </strong><br>";
        eTable += "<br>";
        for (var i = 0; i < dataResult2.length; i++) {
            eTable += "<strong>" + dataResult2[i]['orderShopifyNumber'] + "</strong><br>";
        }
        eTable += "</div>";
        eTable += "<div class='col-4'></div>";
        eTable += "<div class='col-4 align-right'>";
        eTable += "<strong> Status </strong><br>";
        eTable += "<br>";
        for (var j = 0; j < dataResult2.length; j++) {
            eTable += "<strong>" + dataResult2[j]['status'] + "</strong><br>";
        }
        eTable += "</div>";
        $('#forTable2').html(eTable);
    }


})
$('#tableExportCourierOrder').on('click', 'tbody tr', function() {
    var sid = tableExportCourier.row(this).data()['SID'];
    $('#model').show();
    $.ajax({
        url: 'assets/php/orderDetailData.php',
        type: "POST",
        data: {
            sid: sid,
            check: "6"
        },
        success: function(dataResult) {
            var dataResult = JSON.parse(dataResult);
            console.log(dataResult[0].orderId);
            if (document.getElementById('orderId').innerHTML = dataResult[0].orderId) {
                document.getElementById('orderId').innerHTML = dataResult[0].orderId;
            } else {
                document.getElementById('orderId').innerHTML = "N/A";
            }
            if (document.getElementById('orderDate').innerHTML = dataResult[0].date) {
                document.getElementById('orderDate').innerHTML = dataResult[0].date;
            } else {
                document.getElementById('orderDate').innerHTML = "N/A";
            }
            if (document.getElementById('orderStatus').innerHTML = dataResult[0].status) {
                document.getElementById('orderStatus').innerHTML = dataResult[0].status;
            } else {
                document.getElementById('orderStatus').innerHTML = "N/A";
            }
            if (document.getElementById('orderPaymentMethod').innerHTML = dataResult[0].tenderName) {
                document.getElementById('orderPaymentMethod').innerHTML = dataResult[0].tenderName;
            } else {
                document.getElementById('orderPaymentMethod').innerHTML = "N/A";
            }
            if (document.getElementById('custName').innerHTML = dataResult[0].fullName) {
                document.getElementById('custName').innerHTML = dataResult[0].fullName;
            } else {
                document.getElementById('custName').innerHTML = "N/A";
            }
            if (document.getElementById('custEmail').innerHTML = dataResult[0].email) {
                document.getElementById('custEmail').innerHTML = dataResult[0].email;
            } else {
                document.getElementById('custEmail').innerHTML = "N/A";
            }
            if (document.getElementById('custNumber').innerHTML = dataResult[0].number) {
                document.getElementById('custNumber').innerHTML = dataResult[0].number;
            } else {
                document.getElementById('custNumber').innerHTML = "N/A";
            }
            if (document.getElementById('custAddress').innerHTML = dataResult[0].address) {
                document.getElementById('custAddress').innerHTML = dataResult[0].address;
            } else {
                document.getElementById('custAddress').innerHTML = "N/A";
            }


        }
    });
    $.ajax({
        url: 'assets/php/orderItemDetailData.php',
        type: "POST",
        data: {
            sid: sid,
            check: "6"
        },
        success: function(dataResult1) {
            var dataResult1 = JSON.parse(dataResult1);
            console.log(dataResult1);
            table(dataResult1);

        }

    });
    $.ajax({
        url: 'assets/php/orderHistoryDetailData.php',
        type: "POST",
        data: {
            sid: sid,
            check: "1"
        },
        success: function(dataResult2) {
            var dataResult2 = JSON.parse(dataResult2);
            console.log(dataResult2);
            table1(dataResult2);

        }

    });

    function table(dataResult1) {
        var eTable = "<table class='table table-stripped'><thead><tr><th>SKU/Item</th><th>Price</th><th>QTY</th><th>Total</th></tr></thead><tbody>"
        for (var i = 0; i < dataResult1.length; i++) {
            eTable += "<tr style='border-bottom: 1px solid black;'>";
            eTable += "<td>" + "<strong>SKU:</strong>" + dataResult1[i]['sku'] + "<br>" + "<strong>Size:</strong>" + dataResult1[i]['itemSize'] + "<br>" + "<strong>Color:</strong>" + dataResult1[i]['attribute'] + "</td>";
            eTable += "<td>" + dataResult1[i]['price'] + "</td>";
            eTable += "<td>" + dataResult1[i]['qty'] + "</td>";
            eTable += "<td>" + dataResult1[i]['totalItem'] + "</td>";
            eTable += "</tr>";

        }
        eTable += "</tbody></table>";
        $('#forTable').html(eTable);
        var fTable = "<div class='container'><div class='row'><div class='col-4 align-left'>";
        fTable += "<strong>Discount Amount:</strong><br>";
        fTable += "<strong >Shipping Amount:</strong><br>";
        fTable += "<strong >Tax Amount:</strong><br>";
        fTable += "<strong >Total Amount:</strong><br>";
        fTable += "</div>";
        fTable += "<div class='col-4'></div>";
        fTable += "<div class='col-4 align-right'>";

        if (!dataResult1[0]['docDiscount']) {
            fTable += "<strong>PKR 0</strong><br>";
        } else {
            fTable += "<strong>PKR " + dataResult1[0]['docDiscount'] + "</strong><br>";
        }
        if (!dataResult1[0]['shippingAmt']) {
            fTable += "<strong>PKR 0</strong><br>";
        } else {
            fTable += "<strong>" + dataResult1[0]['shippingAmt'] + "</strong><br>";
        }
        if (!dataResult1[0]['tax']) {
            fTable += "<strong>PKR 0</strong><br>";
        } else {
            fTable += "<strong>PKR " + dataResult1[0]['tax'] + "</strong><br>";
        }
        if (!dataResult1[0]['total']) {
            fTable += "<strong>PKR 0</strong><br>";
        } else {
            fTable += "<strong>PKR " + dataResult1[0]['total'] + "</strong><br>";
        }
        fTable += "</div>";
        fTable += "</div>";
        fTable += "</div>";
        $('#forTable1').html(fTable);
    }

    function table1(dataResult2) {
        var eTable = "<div class='container'><div class='row'><div class='col-4 align-left'>";
        eTable += "<strong> Shopify Order Id </strong><br>";
        eTable += "<br>";
        for (var i = 0; i < dataResult2.length; i++) {
            eTable += "<strong>" + dataResult2[i]['orderShopifyNumber'] + "</strong><br>";
        }
        eTable += "</div>";
        eTable += "<div class='col-4'></div>";
        eTable += "<div class='col-4 align-right'>";
        eTable += "<strong> Status </strong><br>";
        eTable += "<br>";
        for (var j = 0; j < dataResult2.length; j++) {
            eTable += "<strong>" + dataResult2[j]['status'] + "</strong><br>";
        }
        eTable += "</div>";
        $('#forTable2').html(eTable);
    }

})
$('#tableExportDispatchedOrder').on('click', 'tbody tr', function() {
    var sid = tableDispatch.row(this).data()['SID'];
    $('#model').show();
    $.ajax({
        url: 'assets/php/orderDetailData.php',
        type: "POST",
        data: {
            sid: sid,
            check: "7"
        },
        success: function(dataResult) {
            var dataResult = JSON.parse(dataResult);
            console.log(dataResult[0].orderId);
            if (document.getElementById('orderId').innerHTML = dataResult[0].orderId) {
                document.getElementById('orderId').innerHTML = dataResult[0].orderId;
            } else {
                document.getElementById('orderId').innerHTML = "N/A";
            }
            if (document.getElementById('orderDate').innerHTML = dataResult[0].date) {
                document.getElementById('orderDate').innerHTML = dataResult[0].date;
            } else {
                document.getElementById('orderDate').innerHTML = "N/A";
            }
            if (document.getElementById('orderStatus').innerHTML = dataResult[0].status) {
                document.getElementById('orderStatus').innerHTML = dataResult[0].status;
            } else {
                document.getElementById('orderStatus').innerHTML = "N/A";
            }
            if (document.getElementById('orderPaymentMethod').innerHTML = dataResult[0].tenderName) {
                document.getElementById('orderPaymentMethod').innerHTML = dataResult[0].tenderName;
            } else {
                document.getElementById('orderPaymentMethod').innerHTML = "N/A";
            }
            if (document.getElementById('custName').innerHTML = dataResult[0].fullName) {
                document.getElementById('custName').innerHTML = dataResult[0].fullName;
            } else {
                document.getElementById('custName').innerHTML = "N/A";
            }
            if (document.getElementById('custEmail').innerHTML = dataResult[0].email) {
                document.getElementById('custEmail').innerHTML = dataResult[0].email;
            } else {
                document.getElementById('custEmail').innerHTML = "N/A";
            }
            if (document.getElementById('custNumber').innerHTML = dataResult[0].number) {
                document.getElementById('custNumber').innerHTML = dataResult[0].number;
            } else {
                document.getElementById('custNumber').innerHTML = "N/A";
            }
            if (document.getElementById('custAddress').innerHTML = dataResult[0].address) {
                document.getElementById('custAddress').innerHTML = dataResult[0].address;
            } else {
                document.getElementById('custAddress').innerHTML = "N/A";
            }


        }
    });
    $.ajax({
        url: 'assets/php/orderItemDetailData.php',
        type: "POST",
        data: {
            sid: sid,
            check: "7"
        },
        success: function(dataResult1) {
            var dataResult1 = JSON.parse(dataResult1);
            console.log(dataResult1);
            table(dataResult1);

        }

    });
    $.ajax({
        url: 'assets/php/orderHistoryDetailData.php',
        type: "POST",
        data: {
            sid: sid,
            check: "1"
        },
        success: function(dataResult2) {
            var dataResult2 = JSON.parse(dataResult2);
            console.log(dataResult2);
            table1(dataResult2);

        }

    });

    function table(dataResult1) {
        var eTable = "<table class='table table-stripped'><thead><tr><th>SKU/Item</th><th>Price</th><th>QTY</th><th>Total</th></tr></thead><tbody>"
        for (var i = 0; i < dataResult1.length; i++) {
            eTable += "<tr style='border-bottom: 1px solid black;'>";
            eTable += "<td>" + "<strong>SKU:</strong>" + dataResult1[i]['sku'] + "<br>" + "<strong>Size:</strong>" + dataResult1[i]['itemSize'] + "<br>" + "<strong>Color:</strong>" + dataResult1[i]['attribute'] + "</td>";
            eTable += "<td>" + dataResult1[i]['price'] + "</td>";
            eTable += "<td>" + dataResult1[i]['qty'] + "</td>";
            eTable += "<td>" + dataResult1[i]['totalItem'] + "</td>";
            eTable += "</tr>";

        }
        eTable += "</tbody></table>";
        $('#forTable').html(eTable);
        var fTable = "<div class='container'><div class='row'><div class='col-4 align-left'>";
        fTable += "<strong>Discount Amount:</strong><br>";
        fTable += "<strong >Shipping Amount:</strong><br>";
        fTable += "<strong >Tax Amount:</strong><br>";
        fTable += "<strong >Total Amount:</strong><br>";
        fTable += "</div>";
        fTable += "<div class='col-4'></div>";
        fTable += "<div class='col-4 align-right'>";

        if (!dataResult1[0]['docDiscount']) {
            fTable += "<strong>PKR 0</strong><br>";
        } else {
            fTable += "<strong>PKR " + dataResult1[0]['docDiscount'] + "</strong><br>";
        }
        if (!dataResult1[0]['shippingAmt']) {
            fTable += "<strong>PKR 0</strong><br>";
        } else {
            fTable += "<strong>" + dataResult1[0]['shippingAmt'] + "</strong><br>";
        }
        if (!dataResult1[0]['tax']) {
            fTable += "<strong>PKR 0</strong><br>";
        } else {
            fTable += "<strong>PKR " + dataResult1[0]['tax'] + "</strong><br>";
        }
        if (!dataResult1[0]['total']) {
            fTable += "<strong>PKR 0</strong><br>";
        } else {
            fTable += "<strong>PKR " + dataResult1[0]['total'] + "</strong><br>";
        }
        fTable += "</div>";
        fTable += "</div>";
        fTable += "</div>";
        $('#forTable1').html(fTable);
    }

    function table1(dataResult2) {
        var eTable = "<div class='container'><div class='row'><div class='col-4 align-left'>";
        eTable += "<strong> Shopify Order Id </strong><br>";
        eTable += "<br>";
        for (var i = 0; i < dataResult2.length; i++) {
            eTable += "<strong>" + dataResult2[i]['orderShopifyNumber'] + "</strong><br>";
        }
        eTable += "</div>";
        eTable += "<div class='col-4'></div>";
        eTable += "<div class='col-4 align-right'>";
        eTable += "<strong> Status </strong><br>";
        eTable += "<br>";
        for (var j = 0; j < dataResult2.length; j++) {
            eTable += "<strong>" + dataResult2[j]['status'] + "</strong><br>";
        }
        eTable += "</div>";
        $('#forTable2').html(eTable);
    }

})
$('#tableExportDeliveredOrder').on('click', 'tbody tr', function() {
    var sid = tableDelivered.row(this).data()['SID'];
    $('#model').show();
    $.ajax({
        url: 'assets/php/orderDetailData.php',
        type: "POST",
        data: {
            sid: sid,
            check: "8"
        },
        success: function(dataResult) {
            var dataResult = JSON.parse(dataResult);
            console.log(dataResult[0].orderId);
            if (document.getElementById('orderId').innerHTML = dataResult[0].orderId) {
                document.getElementById('orderId').innerHTML = dataResult[0].orderId;
            } else {
                document.getElementById('orderId').innerHTML = "N/A";
            }
            if (document.getElementById('orderDate').innerHTML = dataResult[0].date) {
                document.getElementById('orderDate').innerHTML = dataResult[0].date;
            } else {
                document.getElementById('orderDate').innerHTML = "N/A";
            }
            if (document.getElementById('orderStatus').innerHTML = dataResult[0].status) {
                document.getElementById('orderStatus').innerHTML = dataResult[0].status;
            } else {
                document.getElementById('orderStatus').innerHTML = "N/A";
            }
            if (document.getElementById('orderPaymentMethod').innerHTML = dataResult[0].tenderName) {
                document.getElementById('orderPaymentMethod').innerHTML = dataResult[0].tenderName;
            } else {
                document.getElementById('orderPaymentMethod').innerHTML = "N/A";
            }
            if (document.getElementById('custName').innerHTML = dataResult[0].fullName) {
                document.getElementById('custName').innerHTML = dataResult[0].fullName;
            } else {
                document.getElementById('custName').innerHTML = "N/A";
            }
            if (document.getElementById('custEmail').innerHTML = dataResult[0].email) {
                document.getElementById('custEmail').innerHTML = dataResult[0].email;
            } else {
                document.getElementById('custEmail').innerHTML = "N/A";
            }
            if (document.getElementById('custNumber').innerHTML = dataResult[0].number) {
                document.getElementById('custNumber').innerHTML = dataResult[0].number;
            } else {
                document.getElementById('custNumber').innerHTML = "N/A";
            }
            if (document.getElementById('custAddress').innerHTML = dataResult[0].address) {
                document.getElementById('custAddress').innerHTML = dataResult[0].address;
            } else {
                document.getElementById('custAddress').innerHTML = "N/A";
            }


        }
    });
    $.ajax({
        url: 'assets/php/orderItemDetailData.php',
        type: "POST",
        data: {
            sid: sid,
            check: "8"
        },
        success: function(dataResult1) {
            var dataResult1 = JSON.parse(dataResult1);
            console.log(dataResult1);
            table(dataResult1);

        }

    });
    $.ajax({
        url: 'assets/php/orderHistoryDetailData.php',
        type: "POST",
        data: {
            sid: sid,
            check: "1"
        },
        success: function(dataResult2) {
            var dataResult2 = JSON.parse(dataResult2);
            console.log(dataResult2);
            table1(dataResult2);

        }

    });

    function table(dataResult1) {
        var eTable = "<table class='table table-stripped'><thead><tr><th>SKU/Item</th><th>Price</th><th>QTY</th><th>Total</th></tr></thead><tbody>"
        for (var i = 0; i < dataResult1.length; i++) {
            eTable += "<tr style='border-bottom: 1px solid black;'>";
            eTable += "<td>" + "<strong>SKU:</strong>" + dataResult1[i]['sku'] + "<br>" + "<strong>Size:</strong>" + dataResult1[i]['itemSize'] + "<br>" + "<strong>Color:</strong>" + dataResult1[i]['attribute'] + "</td>";
            eTable += "<td>" + dataResult1[i]['price'] + "</td>";
            eTable += "<td>" + dataResult1[i]['qty'] + "</td>";
            eTable += "<td>" + dataResult1[i]['totalItem'] + "</td>";
            eTable += "</tr>";

        }
        eTable += "</tbody></table>";
        $('#forTable').html(eTable);
        var fTable = "<div class='container'><div class='row'><div class='col-4 align-left'>";
        fTable += "<strong>Discount Amount:</strong><br>";
        fTable += "<strong >Shipping Amount:</strong><br>";
        fTable += "<strong >Tax Amount:</strong><br>";
        fTable += "<strong >Total Amount:</strong><br>";
        fTable += "</div>";
        fTable += "<div class='col-4'></div>";
        fTable += "<div class='col-4 align-right'>";

        if (!dataResult1[0]['docDiscount']) {
            fTable += "<strong>PKR 0</strong><br>";
        } else {
            fTable += "<strong>PKR " + dataResult1[0]['docDiscount'] + "</strong><br>";
        }
        if (!dataResult1[0]['shippingAmt']) {
            fTable += "<strong>PKR 0</strong><br>";
        } else {
            fTable += "<strong>" + dataResult1[0]['shippingAmt'] + "</strong><br>";
        }
        if (!dataResult1[0]['tax']) {
            fTable += "<strong>PKR 0</strong><br>";
        } else {
            fTable += "<strong>PKR " + dataResult1[0]['tax'] + "</strong><br>";
        }
        if (!dataResult1[0]['total']) {
            fTable += "<strong>PKR 0</strong><br>";
        } else {
            fTable += "<strong>PKR " + dataResult1[0]['total'] + "</strong><br>";
        }
        fTable += "</div>";
        fTable += "</div>";
        fTable += "</div>";
        $('#forTable1').html(fTable);
    }

    function table1(dataResult2) {
        var eTable = "<div class='container'><div class='row'><div class='col-4 align-left'>";
        eTable += "<strong> Shopify Order Id </strong><br>";
        eTable += "<br>";
        for (var i = 0; i < dataResult2.length; i++) {
            eTable += "<strong>" + dataResult2[i]['orderShopifyNumber'] + "</strong><br>";
        }
        eTable += "</div>";
        eTable += "<div class='col-4'></div>";
        eTable += "<div class='col-4 align-right'>";
        eTable += "<strong> Status </strong><br>";
        eTable += "<br>";
        for (var j = 0; j < dataResult2.length; j++) {
            eTable += "<strong>" + dataResult2[j]['status'] + "</strong><br>";
        }
        eTable += "</div>";
        $('#forTable2').html(eTable);
    }


})
$('#tableExportUndeliveredOrder').on('click', 'tbody tr', function() {
    var sid = tableUndelivered.row(this).data()['SID'];
    $('#model').show();
    $.ajax({
        url: 'assets/php/orderDetailData.php',
        type: "POST",
        data: {
            sid: sid,
            check: "9"
        },
        success: function(dataResult) {
            var dataResult = JSON.parse(dataResult);
            console.log(dataResult[0].orderId);
            if (document.getElementById('orderId').innerHTML = dataResult[0].orderId) {
                document.getElementById('orderId').innerHTML = dataResult[0].orderId;
            } else {
                document.getElementById('orderId').innerHTML = "N/A";
            }
            if (document.getElementById('orderDate').innerHTML = dataResult[0].date) {
                document.getElementById('orderDate').innerHTML = dataResult[0].date;
            } else {
                document.getElementById('orderDate').innerHTML = "N/A";
            }
            if (document.getElementById('orderStatus').innerHTML = dataResult[0].status) {
                document.getElementById('orderStatus').innerHTML = dataResult[0].status;
            } else {
                document.getElementById('orderStatus').innerHTML = "N/A";
            }
            if (document.getElementById('orderPaymentMethod').innerHTML = dataResult[0].tenderName) {
                document.getElementById('orderPaymentMethod').innerHTML = dataResult[0].tenderName;
            } else {
                document.getElementById('orderPaymentMethod').innerHTML = "N/A";
            }
            if (document.getElementById('custName').innerHTML = dataResult[0].fullName) {
                document.getElementById('custName').innerHTML = dataResult[0].fullName;
            } else {
                document.getElementById('custName').innerHTML = "N/A";
            }
            if (document.getElementById('custEmail').innerHTML = dataResult[0].email) {
                document.getElementById('custEmail').innerHTML = dataResult[0].email;
            } else {
                document.getElementById('custEmail').innerHTML = "N/A";
            }
            if (document.getElementById('custNumber').innerHTML = dataResult[0].number) {
                document.getElementById('custNumber').innerHTML = dataResult[0].number;
            } else {
                document.getElementById('custNumber').innerHTML = "N/A";
            }
            if (document.getElementById('custAddress').innerHTML = dataResult[0].address) {
                document.getElementById('custAddress').innerHTML = dataResult[0].address;
            } else {
                document.getElementById('custAddress').innerHTML = "N/A";
            }


        }
    });
    $.ajax({
        url: 'assets/php/orderItemDetailData.php',
        type: "POST",
        data: {
            sid: sid,
            check: "9"
        },
        success: function(dataResult1) {
            var dataResult1 = JSON.parse(dataResult1);
            console.log(dataResult1);
            table(dataResult1);

        }

    });
    $.ajax({
        url: 'assets/php/orderHistoryDetailData.php',
        type: "POST",
        data: {
            sid: sid,
            check: "1"
        },
        success: function(dataResult2) {
            var dataResult2 = JSON.parse(dataResult2);
            console.log(dataResult2);
            table1(dataResult2);

        }

    });

    function table(dataResult1) {
        var eTable = "<table class='table table-stripped'><thead><tr><th>SKU/Item</th><th>Price</th><th>QTY</th><th>Total</th></tr></thead><tbody>"
        for (var i = 0; i < dataResult1.length; i++) {
            eTable += "<tr style='border-bottom: 1px solid black;'>";
            eTable += "<td>" + "<strong>SKU:</strong>" + dataResult1[i]['sku'] + "<br>" + "<strong>Size:</strong>" + dataResult1[i]['itemSize'] + "<br>" + "<strong>Color:</strong>" + dataResult1[i]['attribute'] + "</td>";
            eTable += "<td>" + dataResult1[i]['price'] + "</td>";
            eTable += "<td>" + dataResult1[i]['qty'] + "</td>";
            eTable += "<td>" + dataResult1[i]['totalItem'] + "</td>";
            eTable += "</tr>";

        }
        eTable += "</tbody></table>";
        $('#forTable').html(eTable);
        var fTable = "<div class='container'><div class='row'><div class='col-4 align-left'>";
        fTable += "<strong>Discount Amount:</strong><br>";
        fTable += "<strong >Shipping Amount:</strong><br>";
        fTable += "<strong >Tax Amount:</strong><br>";
        fTable += "<strong >Total Amount:</strong><br>";
        fTable += "</div>";
        fTable += "<div class='col-4'></div>";
        fTable += "<div class='col-4 align-right'>";

        if (!dataResult1[0]['docDiscount']) {
            fTable += "<strong>PKR 0</strong><br>";
        } else {
            fTable += "<strong>PKR " + dataResult1[0]['docDiscount'] + "</strong><br>";
        }
        if (!dataResult1[0]['shippingAmt']) {
            fTable += "<strong>PKR 0</strong><br>";
        } else {
            fTable += "<strong>" + dataResult1[0]['shippingAmt'] + "</strong><br>";
        }
        if (!dataResult1[0]['tax']) {
            fTable += "<strong>PKR 0</strong><br>";
        } else {
            fTable += "<strong>PKR " + dataResult1[0]['tax'] + "</strong><br>";
        }
        if (!dataResult1[0]['total']) {
            fTable += "<strong>PKR 0</strong><br>";
        } else {
            fTable += "<strong>PKR " + dataResult1[0]['total'] + "</strong><br>";
        }
        fTable += "</div>";
        fTable += "</div>";
        fTable += "</div>";
        $('#forTable1').html(fTable);
    }

    function table1(dataResult2) {
        var eTable = "<div class='container'><div class='row'><div class='col-4 align-left'>";
        eTable += "<strong> Shopify Order Id </strong><br>";
        eTable += "<br>";
        for (var i = 0; i < dataResult2.length; i++) {
            eTable += "<strong>" + dataResult2[i]['orderShopifyNumber'] + "</strong><br>";
        }
        eTable += "</div>";
        eTable += "<div class='col-4'></div>";
        eTable += "<div class='col-4 align-right'>";
        eTable += "<strong> Status </strong><br>";
        eTable += "<br>";
        for (var j = 0; j < dataResult2.length; j++) {
            eTable += "<strong>" + dataResult2[j]['status'] + "</strong><br>";
        }
        eTable += "</div>";
        $('#forTable2').html(eTable);
    }



})
$('#tableExportReturnOrder').on('click', 'tbody tr', function() {
    var sid = tableReturn.row(this).data()['SID'];
    $('#model').show();
    $.ajax({
        url: 'assets/php/orderDetailData.php',
        type: "POST",
        data: {
            sid: sid,
            check: "10"
        },
        success: function(dataResult) {
            var dataResult = JSON.parse(dataResult);
            console.log(dataResult[0].orderId);
            if (document.getElementById('orderId').innerHTML = dataResult[0].orderId) {
                document.getElementById('orderId').innerHTML = dataResult[0].orderId;
            } else {
                document.getElementById('orderId').innerHTML = "N/A";
            }
            if (document.getElementById('orderDate').innerHTML = dataResult[0].date) {
                document.getElementById('orderDate').innerHTML = dataResult[0].date;
            } else {
                document.getElementById('orderDate').innerHTML = "N/A";
            }
            if (document.getElementById('orderStatus').innerHTML = dataResult[0].status) {
                document.getElementById('orderStatus').innerHTML = dataResult[0].status;
            } else {
                document.getElementById('orderStatus').innerHTML = "N/A";
            }
            if (document.getElementById('orderPaymentMethod').innerHTML = dataResult[0].tenderName) {
                document.getElementById('orderPaymentMethod').innerHTML = dataResult[0].tenderName;
            } else {
                document.getElementById('orderPaymentMethod').innerHTML = "N/A";
            }
            if (document.getElementById('custName').innerHTML = dataResult[0].fullName) {
                document.getElementById('custName').innerHTML = dataResult[0].fullName;
            } else {
                document.getElementById('custName').innerHTML = "N/A";
            }
            if (document.getElementById('custEmail').innerHTML = dataResult[0].email) {
                document.getElementById('custEmail').innerHTML = dataResult[0].email;
            } else {
                document.getElementById('custEmail').innerHTML = "N/A";
            }
            if (document.getElementById('custNumber').innerHTML = dataResult[0].number) {
                document.getElementById('custNumber').innerHTML = dataResult[0].number;
            } else {
                document.getElementById('custNumber').innerHTML = "N/A";
            }
            if (document.getElementById('custAddress').innerHTML = dataResult[0].address) {
                document.getElementById('custAddress').innerHTML = dataResult[0].address;
            } else {
                document.getElementById('custAddress').innerHTML = "N/A";
            }


        }
    });
    $.ajax({
        url: 'assets/php/orderItemDetailData.php',
        type: "POST",
        data: {
            sid: sid,
            check: "10"
        },
        success: function(dataResult1) {
            var dataResult1 = JSON.parse(dataResult1);
            console.log(dataResult1);
            table(dataResult1);

        }

    });
    $.ajax({
        url: 'assets/php/orderHistoryDetailData.php',
        type: "POST",
        data: {
            sid: sid,
            check: "1"
        },
        success: function(dataResult2) {
            var dataResult2 = JSON.parse(dataResult2);
            console.log(dataResult2);
            table1(dataResult2);

        }

    });

    function table(dataResult1) {
        var eTable = "<table class='table table-stripped'><thead><tr><th>SKU/Item</th><th>Price</th><th>QTY</th><th>Total</th></tr></thead><tbody>"
        for (var i = 0; i < dataResult1.length; i++) {
            eTable += "<tr style='border-bottom: 1px solid black;'>";
            eTable += "<td>" + "<strong>SKU:</strong>" + dataResult1[i]['sku'] + "<br>" + "<strong>Size:</strong>" + dataResult1[i]['itemSize'] + "<br>" + "<strong>Color:</strong>" + dataResult1[i]['attribute'] + "</td>";
            eTable += "<td>" + dataResult1[i]['price'] + "</td>";
            eTable += "<td>" + dataResult1[i]['qty'] + "</td>";
            eTable += "<td>" + dataResult1[i]['totalItem'] + "</td>";
            eTable += "</tr>";

        }
        eTable += "</tbody></table>";
        $('#forTable').html(eTable);
        var fTable = "<div class='container'><div class='row'><div class='col-4 align-left'>";
        fTable += "<strong>Discount Amount:</strong><br>";
        fTable += "<strong >Shipping Amount:</strong><br>";
        fTable += "<strong >Tax Amount:</strong><br>";
        fTable += "<strong >Total Amount:</strong><br>";
        fTable += "</div>";
        fTable += "<div class='col-4'></div>";
        fTable += "<div class='col-4 align-right'>";

        if (!dataResult1[0]['docDiscount']) {
            fTable += "<strong>PKR 0</strong><br>";
        } else {
            fTable += "<strong>PKR " + dataResult1[0]['docDiscount'] + "</strong><br>";
        }
        if (!dataResult1[0]['shippingAmt']) {
            fTable += "<strong>PKR 0</strong><br>";
        } else {
            fTable += "<strong>" + dataResult1[0]['shippingAmt'] + "</strong><br>";
        }
        if (!dataResult1[0]['tax']) {
            fTable += "<strong>PKR 0</strong><br>";
        } else {
            fTable += "<strong>PKR " + dataResult1[0]['tax'] + "</strong><br>";
        }
        if (!dataResult1[0]['total']) {
            fTable += "<strong>PKR 0</strong><br>";
        } else {
            fTable += "<strong>PKR " + dataResult1[0]['total'] + "</strong><br>";
        }
        fTable += "</div>";
        fTable += "</div>";
        fTable += "</div>";
        $('#forTable1').html(fTable);
    }

    function table1(dataResult2) {
        var eTable = "<div class='container'><div class='row'><div class='col-4 align-left'>";
        eTable += "<strong> Shopify Order Id </strong><br>";
        eTable += "<br>";
        for (var i = 0; i < dataResult2.length; i++) {
            eTable += "<strong>" + dataResult2[i]['orderShopifyNumber'] + "</strong><br>";
        }
        eTable += "</div>";
        eTable += "<div class='col-4'></div>";
        eTable += "<div class='col-4 align-right'>";
        eTable += "<strong> Status </strong><br>";
        eTable += "<br>";
        for (var j = 0; j < dataResult2.length; j++) {
            eTable += "<strong>" + dataResult2[j]['status'] + "</strong><br>";
        }
        eTable += "</div>";
        $('#forTable2').html(eTable);
    }


})
$('#tableExportCancelOrder').on('click', 'tbody tr', function() {
    var sid = tableCancel.row(this).data()['SID'];
    $('#model').show();
    $.ajax({
        url: 'assets/php/orderDetailData.php',
        type: "POST",
        data: {
            sid: sid,
            check: "11"
        },
        success: function(dataResult) {
            var dataResult = JSON.parse(dataResult);
            console.log(dataResult[0].orderId);
            if (document.getElementById('orderId').innerHTML = dataResult[0].orderId) {
                document.getElementById('orderId').innerHTML = dataResult[0].orderId;
            } else {
                document.getElementById('orderId').innerHTML = "N/A";
            }
            if (document.getElementById('orderDate').innerHTML = dataResult[0].date) {
                document.getElementById('orderDate').innerHTML = dataResult[0].date;
            } else {
                document.getElementById('orderDate').innerHTML = "N/A";
            }
            if (document.getElementById('orderStatus').innerHTML = dataResult[0].status) {
                document.getElementById('orderStatus').innerHTML = dataResult[0].status;
            } else {
                document.getElementById('orderStatus').innerHTML = "N/A";
            }
            if (document.getElementById('orderPaymentMethod').innerHTML = dataResult[0].tenderName) {
                document.getElementById('orderPaymentMethod').innerHTML = dataResult[0].tenderName;
            } else {
                document.getElementById('orderPaymentMethod').innerHTML = "N/A";
            }
            if (document.getElementById('custName').innerHTML = dataResult[0].fullName) {
                document.getElementById('custName').innerHTML = dataResult[0].fullName;
            } else {
                document.getElementById('custName').innerHTML = "N/A";
            }
            if (document.getElementById('custEmail').innerHTML = dataResult[0].email) {
                document.getElementById('custEmail').innerHTML = dataResult[0].email;
            } else {
                document.getElementById('custEmail').innerHTML = "N/A";
            }
            if (document.getElementById('custNumber').innerHTML = dataResult[0].number) {
                document.getElementById('custNumber').innerHTML = dataResult[0].number;
            } else {
                document.getElementById('custNumber').innerHTML = "N/A";
            }
            if (document.getElementById('custAddress').innerHTML = dataResult[0].address) {
                document.getElementById('custAddress').innerHTML = dataResult[0].address;
            } else {
                document.getElementById('custAddress').innerHTML = "N/A";
            }


        }
    });
    $.ajax({
        url: 'assets/php/orderItemDetailData.php',
        type: "POST",
        data: {
            sid: sid,
            check: "11"
        },
        success: function(dataResult1) {
            var dataResult1 = JSON.parse(dataResult1);
            console.log(dataResult1);
            table(dataResult1);

        }

    });
    $.ajax({
        url: 'assets/php/orderHistoryDetailData.php',
        type: "POST",
        data: {
            sid: sid,
            check: "1"
        },
        success: function(dataResult2) {
            var dataResult2 = JSON.parse(dataResult2);
            console.log(dataResult2);
            table1(dataResult2);

        }

    });

    function table(dataResult1) {
        var eTable = "<table class='table table-stripped'><thead><tr><th>SKU/Item</th><th>Price</th><th>QTY</th><th>Total</th></tr></thead><tbody>"
        for (var i = 0; i < dataResult1.length; i++) {
            eTable += "<tr style='border-bottom: 1px solid black;'>";
            eTable += "<td>" + "<strong>SKU:</strong>" + dataResult1[i]['sku'] + "<br>" + "<strong>Size:</strong>" + dataResult1[i]['itemSize'] + "<br>" + "<strong>Color:</strong>" + dataResult1[i]['attribute'] + "</td>";
            eTable += "<td>" + dataResult1[i]['price'] + "</td>";
            eTable += "<td>" + dataResult1[i]['qty'] + "</td>";
            eTable += "<td>" + dataResult1[i]['totalItem'] + "</td>";
            eTable += "</tr>";

        }
        eTable += "</tbody></table>";
        $('#forTable').html(eTable);
        var fTable = "<div class='container'><div class='row'><div class='col-4 align-left'>";
        fTable += "<strong>Discount Amount:</strong><br>";
        fTable += "<strong >Shipping Amount:</strong><br>";
        fTable += "<strong >Tax Amount:</strong><br>";
        fTable += "<strong >Total Amount:</strong><br>";
        fTable += "</div>";
        fTable += "<div class='col-4'></div>";
        fTable += "<div class='col-4 align-right'>";

        if (!dataResult1[0]['docDiscount']) {
            fTable += "<strong>PKR 0</strong><br>";
        } else {
            fTable += "<strong>PKR " + dataResult1[0]['docDiscount'] + "</strong><br>";
        }
        if (!dataResult1[0]['shippingAmt']) {
            fTable += "<strong>PKR 0</strong><br>";
        } else {
            fTable += "<strong>" + dataResult1[0]['shippingAmt'] + "</strong><br>";
        }
        if (!dataResult1[0]['tax']) {
            fTable += "<strong>PKR 0</strong><br>";
        } else {
            fTable += "<strong>PKR " + dataResult1[0]['tax'] + "</strong><br>";
        }
        if (!dataResult1[0]['total']) {
            fTable += "<strong>PKR 0</strong><br>";
        } else {
            fTable += "<strong>PKR " + dataResult1[0]['total'] + "</strong><br>";
        }
        fTable += "</div>";
        fTable += "</div>";
        fTable += "</div>";
        $('#forTable1').html(fTable);
    }

    function table1(dataResult2) {
        var eTable = "<div class='container'><div class='row'><div class='col-4 align-left'>";
        eTable += "<strong> Shopify Order Id </strong><br>";
        eTable += "<br>";
        for (var i = 0; i < dataResult2.length; i++) {
            eTable += "<strong>" + dataResult2[i]['orderShopifyNumber'] + "</strong><br>";
        }
        eTable += "</div>";
        eTable += "<div class='col-4'></div>";
        eTable += "<div class='col-4 align-right'>";
        eTable += "<strong> Status </strong><br>";
        eTable += "<br>";
        for (var j = 0; j < dataResult2.length; j++) {
            eTable += "<strong>" + dataResult2[j]['status'] + "</strong><br>";
        }
        eTable += "</div>";
        $('#forTable2').html(eTable);
    }


})