<?php

header("Access-Control-Allow-Origin: *");
header("Access-Control-Allow-Headers", "origin, x-requested-with, content-type");
header("Access-Control-Allow-Methods", "PUT, GET, POST, DELETE, OPTIONS");

$dsn = "mysql:host=154.12.235.119;dbname=cryptoboss_win;charset=utf8";
$connect = new PDO($dsn, 'cryptoboss_win', '0puelJv3s71TfynV');

$action = $_POST["Action"]; // Create, Update, Get, Complete

date_default_timezone_set('UTC');

$active = 0;
$chipCount = 2;
$guid = "test";
$sessionName = "test session";
$date = date('Y-m-d H:i:s'); // sql понимает только 'Y-m-d H:i:s'


if ($action == "Create") { // При инициализации сессии производит запись в бд стартовых значений
    $currentTypeId = setSessionType($sessionName, $chipCount, $connect); // Инициализация session_type
    setSession("session ". currentTypeId, $date, $currentTypeId, $connect); // Создание session 
}





// CREATE
function setSessionType($sessionName, $chipCount, $connect)
{
    $sql = 'INSERT INTO `session_types` (`name`, `count_chip`) VALUES (:nm, :cnt)';
    $command = $connect->prepare($sql);
    $command -> execute(['nm' => $sessionName, 'cnt' => $chipCount]);

    $sql = 'SELECT MAX(`id`) FROM `session_types`';
    $command = $connect->prepare($sql);
    $command -> execute();
    $result = $command->fetchAll(PDO::FETCH_ASSOC);

    return $result[0]["MAX(`id`)"];
}

function setSession($guid, $dateTimeStart, $sessionTypeId, $connect)
{
    $active = 1;
    $sql = 'INSERT INTO `sessions` (`guid`, `date_time_start`, `date_time_finish`, `is_active`, `session_type_id`) VALUES (:guid, :dts, :dtf, :active, :sessionId)';
    $command = $connect->prepare($sql);
    $command -> execute(['guid' => $guid, 'dts' => $dateTimeStart, 'dtf' => $dateTimeStart, 'active' => $active, 'sessionId' => $sessionTypeId]);
}


// UPDATE

?>