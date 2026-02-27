<?php
    // include 'mainConfig.php';
    function newcount(){
        include 'mainConfig.php';
        $pendingCount = [];
        $query = "select to_char(doc.SID),doc.order_subtotal_with_tax,doc.total_line_item,st.STATUS_CODE,doc.POS_FLAG1,doc.UDF1_STRING,doc.UDF2_STRING 
        from rps.document doc , order_status_c os, status_type_c st 
        where doc.sbs_no =2 
        and to_char(doc.SID) = os.order_retail_id
        and doc.order_type = 0 
        and doc.pos_flag1 is not null 
        and os.status = st.status_number
        and os.status = 300";
        $mainQuery = oci_parse($conn,$query);
        oci_execute($mainQuery);
        while (($row = oci_fetch_assoc($mainQuery)) != false ) {
            array_push($pendingCount,$row);
        }
        echo count($pendingCount);
    }
	 function courierCountdby(){//daybeforeyesterday
        include 'mainConfig.php';
        $dbyCountOrders = [];
        $query = "select to_char(doc.SID),st.STATUS_CODE,doc.POS_FLAG1,doc.UDF1_STRING,doc.UDF1_STRING  
        from rps.document doc , order_status_c os, status_type_c st 
        where doc.sbs_no =2 
        and to_char(doc.SID) = os.order_retail_id
        and doc.order_type = 0 
        and doc.pos_flag1 is not null 
        and os.status = st.status_number
        and os.status = 305
        and trunc(os.modified_datetime) <=  trunc(sysdate)
        and trunc(os.modified_datetime) > trunc(TO_DATE(sysdate - 7))";
        $mainQuery = oci_parse($conn,$query);
        oci_execute($mainQuery);
        while (($row = oci_fetch_assoc($mainQuery)) != false ) {
            array_push($dbyCountOrders,$row);
        }
        echo count($dbyCountOrders);
    }
	function courierCountyesterday(){//daybeforeyesterday
        include 'mainConfig.php';
        $yesterdayCountOrders = [];
        $query = "select to_char(doc.SID),st.STATUS_CODE,doc.POS_FLAG1,doc.UDF1_STRING,doc.UDF1_STRING  
        from rps.document doc , order_status_c os, status_type_c st 
        where doc.sbs_no =2 
        and to_char(doc.SID) = os.order_retail_id
        and doc.order_type = 0 
        and doc.pos_flag1 is not null 
        and os.status = st.status_number
        and os.status = 305
        and trunc(os.modified_datetime) <=  trunc(sysdate-7)
        and trunc(os.modified_datetime) > trunc(TO_DATE(sysdate - 14))";
        $mainQuery = oci_parse($conn,$query);
        oci_execute($mainQuery);
        while (($row = oci_fetch_assoc($mainQuery)) != false ) {
            array_push($yesterdayCountOrders,$row);
        }
        echo count($yesterdayCountOrders);
    }
	
	function pickCountdby(){//daybeforeyesterday
        include 'mainConfig.php';
        $dbyCountOrders = [];
        $query = "select to_char(doc.SID),st.STATUS_CODE,doc.POS_FLAG1,doc.UDF1_STRING,doc.UDF1_STRING  
        from rps.document doc , order_status_c os, status_type_c st 
        where doc.sbs_no =2 
        and to_char(doc.SID) = os.order_retail_id
        and doc.order_type = 0 
        and doc.pos_flag1 is not null 
        and os.status = st.status_number
        and os.status = 303
        and trunc(os.modified_datetime) <=  trunc(sysdate)
        and trunc(os.modified_datetime) > trunc(TO_DATE(sysdate - 7))";
        $mainQuery = oci_parse($conn,$query);
        oci_execute($mainQuery);
        while (($row = oci_fetch_assoc($mainQuery)) != false ) {
            array_push($dbyCountOrders,$row);
        }
        echo count($dbyCountOrders);
    }
    function pickCountyesterday(){//daybeforeyesterday
        include 'mainConfig.php';
        $yesterdayCountOrders = [];
        $query = "select to_char(doc.SID),st.STATUS_CODE,doc.POS_FLAG1,doc.UDF1_STRING,doc.UDF1_STRING  
        from rps.document doc , order_status_c os, status_type_c st 
        where doc.sbs_no =2 
        and to_char(doc.SID) = os.order_retail_id
        and doc.order_type = 0 
        and doc.pos_flag1 is not null 
        and os.status = st.status_number
        and os.status = 303
        and trunc(os.modified_datetime) <=  trunc(sysdate-7)
        and trunc(os.modified_datetime) > trunc(TO_DATE(sysdate - 14))";
        $mainQuery = oci_parse($conn,$query);
        oci_execute($mainQuery);
        while (($row = oci_fetch_assoc($mainQuery)) != false ) {
            array_push($yesterdayCountOrders,$row);
        }
        echo count($yesterdayCountOrders);
    }
    function pickCountold(){//daybeforeyesterday
        include 'mainConfig.php';
        $oldpickCountOrders = [];
        $query = "select to_char(doc.SID),st.STATUS_CODE,doc.POS_FLAG1,doc.UDF1_STRING,doc.UDF1_STRING  
        from rps.document doc , order_status_c os, status_type_c st 
        where doc.sbs_no =2 
        and to_char(doc.SID) = os.order_retail_id
        and doc.order_type = 0 
        and doc.pos_flag1 is not null 
        and os.status = st.status_number
        and os.status = 303
        and trunc(os.modified_datetime) <=  trunc(sysdate-14)";
        $mainQuery = oci_parse($conn,$query);
        oci_execute($mainQuery);
        while (($row = oci_fetch_assoc($mainQuery)) != false ) {
            array_push($oldpickCountOrders,$row);
        }
        echo count($oldpickCountOrders);
    }
	  function courierCountold(){//daybeforeyesterday
        include 'mainConfig.php';
        $oldcourierCountOrders = [];
        $query = "select to_char(doc.SID),st.STATUS_CODE,doc.POS_FLAG1,doc.UDF1_STRING,doc.UDF1_STRING  
        from rps.document doc , order_status_c os, status_type_c st 
        where doc.sbs_no =2 
        and to_char(doc.SID) = os.order_retail_id
        and doc.order_type = 0 
        and doc.pos_flag1 is not null 
        and os.status = st.status_number
        and os.status = 305
        and trunc(os.modified_datetime) <=  trunc(sysdate-14)";
        $mainQuery = oci_parse($conn,$query);
        oci_execute($mainQuery);
        while (($row = oci_fetch_assoc($mainQuery)) != false ) {
            array_push($oldcourierCountOrders,$row);
        }
        echo count($oldcourierCountOrders);
    }
    function lastOrderDate(){//last Order date
        include 'mainConfig.php';
        $lastOrderDate = [];
        $query = "select max(doc.post_date) as postdate from rps.document doc where doc.sbs_no =2 
        and doc.order_type = 0 
        and doc.pos_flag1 is not null 
        and doc.so_cancel_flag = 0 
        and doc.status = 4 
        and doc.notes_general is not null";
        $mainQuery = oci_parse($conn,$query);
        oci_execute($mainQuery);
        $row = oci_fetch_assoc($mainQuery);
        $date = new DateTime($row['POSTDATE']);
        echo date_format($date,"d-m-Y H:i A");
    }
    function confirmedCount(){
        include 'mainConfig.php';
        $confirmedCount = [];
        $query = "select to_char(doc.SID),st.STATUS_CODE,doc.POS_FLAG1,doc.UDF1_STRING,doc.UDF2_STRING  
        from rps.document doc , order_status_c os, status_type_c st 
        where doc.sbs_no =2 
        and to_char(doc.SID) = os.order_retail_id
        and doc.order_type = 0 
        and doc.pos_flag1 is not null
        and os.status = st.status_number 
        and os.status = 301";
        $mainQuery = oci_parse($conn,$query);
        oci_execute($mainQuery);
        while (($row = oci_fetch_assoc($mainQuery)) != false ) {
            array_push($confirmedCount,$row);
        }
        echo count($confirmedCount);
    }
    function storeAssignCount(){
        include 'mainConfig.php';
        $confirmedCount = [];
        $query = "select to_char(doc.SID),st.STATUS_CODE,doc.POS_FLAG1,doc.UDF1_STRING,doc.UDF2_STRING  
        from rps.document doc , order_status_c os, status_type_c st 
        where doc.sbs_no =2 
        and to_char(doc.SID) = os.order_retail_id
        and doc.order_type = 0 
        and doc.pos_flag1 is not null
        and os.status = st.status_number 
        and os.status = 300";
        $mainQuery = oci_parse($conn,$query);
        oci_execute($mainQuery);
        while (($row = oci_fetch_assoc($mainQuery)) != false ) {
            array_push($confirmedCount,$row);
        }
        echo count($confirmedCount);
    }
    function pickCount(){
        include 'mainConfig.php';
        $confirmedCount = [];
        $query = "select to_char(doc.SID),st.STATUS_CODE,doc.POS_FLAG1,doc.UDF1_STRING,doc.UDF2_STRING  
        from rps.document doc , order_status_c os, status_type_c st 
        where doc.sbs_no =2 
        and to_char(doc.SID) = os.order_retail_id
        and doc.order_type = 0 
        and doc.pos_flag1 is not null
        and os.status = st.status_number 
        and os.status = 303";
        $mainQuery = oci_parse($conn,$query);
        oci_execute($mainQuery);
        while (($row = oci_fetch_assoc($mainQuery)) != false ) {
            array_push($confirmedCount,$row);
        }
        echo count($confirmedCount);
    }
    function packCount(){
        include 'mainConfig.php';
        $confirmedCount = [];
        $query = "select to_char(doc.SID),st.STATUS_CODE,doc.POS_FLAG1,doc.UDF1_STRING,doc.UDF2_STRING  
        from rps.document doc , order_status_c os, status_type_c st 
        where doc.sbs_no =2 
        and to_char(doc.SID) = os.order_retail_id
        and doc.order_type = 0 
        and doc.pos_flag1 is not null
        and os.status = st.status_number 
        and os.status = 304";
        $mainQuery = oci_parse($conn,$query);
        oci_execute($mainQuery);
        while (($row = oci_fetch_assoc($mainQuery)) != false ) {
            array_push($confirmedCount,$row);
        }
        echo count($confirmedCount);
    }
    function courierCount(){
        include 'mainConfig.php';
        $confirmedCount = [];
        $query = "select to_char(doc.SID),st.STATUS_CODE,doc.POS_FLAG1,doc.UDF1_STRING,doc.UDF1_STRING  
        from rps.document doc , order_status_c os, status_type_c st 
        where doc.sbs_no =2 
        and to_char(doc.SID) = os.order_retail_id
        and doc.order_type = 0 
        and doc.pos_flag1 is not null
        and os.status = st.status_number 
        and os.status = 305";
        $mainQuery = oci_parse($conn,$query);
        oci_execute($mainQuery);
        while (($row = oci_fetch_assoc($mainQuery)) != false ) {
            array_push($confirmedCount,$row);
        }
        echo count($confirmedCount);
    }
    function dispatchCount(){
        include 'mainConfig.php';
        $dispatchedCount = [];
        $query = "select to_char(doc.SID),st.STATUS_CODE,doc.POS_FLAG1,doc.UDF1_STRING,doc.UDF1_STRING  
        from rps.document doc , order_status_c os, status_type_c st 
        where doc.sbs_no =2 
        and to_char(doc.SID) = os.order_retail_id
        and doc.order_type = 0 
        and doc.pos_flag1 is not null 
        and os.status = st.status_number
        and os.status = 306";
        $mainQuery = oci_parse($conn,$query);
        oci_execute($mainQuery);
        while (($row = oci_fetch_assoc($mainQuery)) != false ) {
            array_push($dispatchedCount,$row);
        }
        echo count($dispatchedCount);
    }
    function dispatchedCount(){
        include 'mainConfig.php';
        $dispatchedCount = [];
        $query = "select to_char(doc.SID),st.STATUS_CODE,doc.POS_FLAG1,doc.UDF1_STRING,doc.UDF1_STRING  
        from rps.document doc , order_status_c os, status_type_c st 
        where doc.sbs_no =2 
        and to_char(doc.SID) = os.order_retail_id
        and doc.order_type = 0 
        and doc.pos_flag1 is not null 
        and os.status = st.status_number
        and os.status = 312";
        $mainQuery = oci_parse($conn,$query);
        oci_execute($mainQuery);
        while (($row = oci_fetch_assoc($mainQuery)) != false ) {
            array_push($dispatchedCount,$row);
        }
        echo count($dispatchedCount);
    }
    function deliveredCount(){
        include 'mainConfig.php';
        $deliveredCount = [];
        $query = "select to_char(doc.SID),st.STATUS_CODE,doc.POS_FLAG1,doc.UDF1_STRING,doc.UDF1_STRING  
        from rps.document doc , order_status_c os, status_type_c st 
        where doc.sbs_no =2 
        and to_char(doc.SID) = os.order_retail_id
        and doc.order_type = 0 
        and doc.pos_flag1 is not null 
        and os.status = st.status_number
        and os.status = 307";
        $mainQuery = oci_parse($conn,$query);
        oci_execute($mainQuery);
        while (($row = oci_fetch_assoc($mainQuery)) != false ) {
            array_push($deliveredCount,$row);
        }
        echo count($deliveredCount);
    }
    function undeliveredCount(){
        include 'mainConfig.php';
        $deliveredCount = [];
        $query = "select to_char(doc.SID),st.STATUS_CODE,doc.POS_FLAG1,doc.UDF1_STRING,doc.UDF1_STRING  
        from rps.document doc , order_status_c os, status_type_c st 
        where doc.sbs_no =2 
        and to_char(doc.SID) = os.order_retail_id
        and doc.order_type = 0 
        and doc.pos_flag1 is not null 
        and os.status = st.status_number
        and os.status = 308";
        $mainQuery = oci_parse($conn,$query);
        oci_execute($mainQuery);
        while (($row = oci_fetch_assoc($mainQuery)) != false ) {
            array_push($deliveredCount,$row);
        }
        echo count($deliveredCount);
    }
    function returnCount(){
        include 'mainConfig.php';
        $returnCount = [];
        $query = "select to_char(doc.SID),st.STATUS_CODE,doc.POS_FLAG1,doc.UDF1_STRING,doc.UDF1_STRING  
        from rps.document doc , order_status_c os, status_type_c st 
        where doc.sbs_no =2 
        and to_char(doc.SID) = os.order_retail_id
        and doc.order_type = 0 
        and doc.pos_flag1 is not null 
        and os.status = st.status_number
        and os.status = 309";
        $mainQuery = oci_parse($conn,$query);
        oci_execute($mainQuery);
        while (($row = oci_fetch_assoc($mainQuery)) != false ) {
            array_push($returnCount,$row);
        }
        echo count($returnCount);
    }
    function cancelCount(){
        include 'mainConfig.php';
        $cancelCount = [];
        $query = "select to_char(doc.SID),st.STATUS_CODE,doc.POS_FLAG1,doc.UDF1_STRING,doc.UDF1_STRING  
        from rps.document doc , order_status_c os, status_type_c st 
        where doc.sbs_no =2 
        and to_char(doc.SID) = os.order_retail_id
        and doc.order_type = 0 
        and doc.pos_flag1 is not null 
        and os.status = st.status_number
        and os.status = 310";
        $mainQuery = oci_parse($conn,$query);
        oci_execute($mainQuery);
        while (($row = oci_fetch_assoc($mainQuery)) != false ) {
            array_push($cancelCount,$row);
        }
        echo count($cancelCount);
    }
    function stuckCount(){
        include 'mainConfig.php';
        $stuckCount = [];
        $query = "select to_char(doc.SID),st.STATUS_CODE,doc.POS_FLAG1,doc.UDF1_STRING,doc.UDF1_STRING  
        from rps.document doc , order_status_c os, status_type_c st 
        where doc.sbs_no =2 
        and to_char(doc.SID) = os.order_retail_id
        and doc.order_type = 0 
        and doc.pos_flag1 is not null 
        and os.status = st.status_number
        and os.status = 311";
        $mainQuery = oci_parse($conn,$query);
        oci_execute($mainQuery);
        while (($row = oci_fetch_assoc($mainQuery)) != false ) {
            array_push($stuckCount,$row);
        }
        echo count($stuckCount);
    }
    function returnCourierCount(){
        include 'mainConfig.php';
        $stuckCount = [];
        $query = "select to_char(doc.SID),st.STATUS_CODE,doc.POS_FLAG1,doc.UDF1_STRING,doc.UDF1_STRING  
        from rps.document doc , order_status_c os, status_type_c st 
        where doc.sbs_no =2 
        and to_char(doc.SID) = os.order_retail_id
        and doc.order_type = 0 
        and doc.pos_flag1 is not null 
        and os.status = st.status_number
        and os.status = 314";
        $mainQuery = oci_parse($conn,$query);
        oci_execute($mainQuery);
        while (($row = oci_fetch_assoc($mainQuery)) != false ) {
            array_push($stuckCount,$row);
        }
        echo count($stuckCount);
    }
    function allCount(){
        include 'mainConfig.php';
        $allCountOrders = [];
        $query = "select to_char(doc.SID),st.STATUS_CODE,doc.POS_FLAG1,doc.UDF1_STRING,doc.UDF1_STRING  
        from rps.document doc , order_status_c os, status_type_c st 
        where doc.sbs_no =2 
        and to_char(doc.SID) = os.order_retail_id
        and doc.order_type = 0 
        and doc.pos_flag1 is not null 
        and os.status = st.status_number";
        $mainQuery = oci_parse($conn,$query);
        oci_execute($mainQuery);
        while (($row = oci_fetch_assoc($mainQuery)) != false ) {
            array_push($allCountOrders,$row);
        }
        echo count($allCountOrders);
    }
    function unverified(){
        include 'mainConfig.php';
        $pendingCount = [];
        $query = "select to_char(doc.SID),doc.order_subtotal_with_tax,doc.total_line_item,st.STATUS_CODE,doc.POS_FLAG1,doc.UDF1_STRING,doc.UDF2_STRING 
        from rps.document doc , order_status_c os, status_type_c st 
        where doc.sbs_no =2 
        and to_char(doc.SID) = os.order_retail_id
        and doc.order_type = 0 
        and doc.pos_flag1 is not null 
        and os.status = st.status_number
        and os.status = 300";
        $mainQuery = oci_parse($conn,$query);
        oci_execute($mainQuery);
        while (($row = oci_fetch_assoc($mainQuery)) != false ) {
            array_push($pendingCount,$row);
        }
        echo count($pendingCount);
    }
    
    
    
?>