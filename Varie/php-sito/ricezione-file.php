<?php
// Nelle versioni di PHP precedenti alla 4.1.0 si deve utilizzare  $HTTP_POST_FILES anzichè
// $_FILES.

try {

	$uploaddir = $_SERVER['DOCUMENT_ROOT'] . "ricez/files/"; 

	$uploadfile = $uploaddir . $_FILES['file']['name'];


	// echo '<pre>';

	if( move_uploaded_file( $_FILES['file']['tmp_name'], $uploadfile ) ) {
		echo "OK";
	} else {
		echo "ERROR";
	}

	// echo 'Alcune informazioni di debug:';
	// print_r($_FILES);

	// print "</pre>";
		
} catch( Exception $ee ) {

	echo "ERROR";
	// echo $ee->getMessage();
}


?>