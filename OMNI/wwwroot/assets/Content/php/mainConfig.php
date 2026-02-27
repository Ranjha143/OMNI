<?php
    $conn = oci_connect('reportuser','report','localhost/RPROODS');
    if (!$conn) {
        $e = oci_error();
        trigger_error(htmlentities($e['message'], ENT_QUOTES), E_USER_ERROR);
        die();
    } else {
        ;
    }
?>
 