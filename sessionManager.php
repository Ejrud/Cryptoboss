<?php

header("Access-Control-Allow-Origin: *");
header("Access-Control-Allow-Headers", "origin, x-requested-with, content-type");
header("Access-Control-Allow-Methods", "PUT, GET, POST, DELETE, OPTIONS");

date_default_timezone_set('UTC');

$dsn = "mysql:host=154.12.235.119;dbname=cryptoboss_win;charset=utf8";
$connect = new PDO($dsn, 'cryptoboss_win', '0puelJv3s71TfynV');
///

$jsonData = $_POST["JsonData"]; // Передача id сессии, имени (режима сессии), названия фишек, награда босси, рейтинг, количество фишек
$date = date('Y-m-d H:i:s'); // sql понимает только 'Y-m-d H:i:s'
$session = json_decode($jsonData, true); // true - ассоциативный массив session (id, action, name, chips (хранит guid, рейтинг, bossy))

switch ($session["action"]) {
    case "Create":
        $currentTypeId = setSessionType($session["name"], count($session["chips"]), $connect); // Инициализация session_type
        $sessionId = setSession("Session #". $currentTypeId, $date, $currentTypeId, $connect); // Создание session 
        echo $sessionId; // Возвращает id сессии (необходим для дальнейших обновлений в бд)
        break;

    case "Leave":
        playerLeave($session["id"], $session["chips"], $connect);
        closeSession($session["id"], $date, $connect);
        break;

    case "Complete":
        completeSession($session["id"], $session["chips"], $connect);
        closeSession($session["id"], $date, $connect);
        break;
}

////////////////////////////////////////////////////////////////
// CREATE
function setSessionType($sessionName, $chipCount, $connect)
{
    $sql = 'INSERT INTO `session_types` (`name`, `count_chip`) VALUES (:name, :cnt)'; // Инициализация таблицы session_type
    $command = $connect->prepare($sql);
    $command -> execute(['name' => $sessionName,'cnt' => $chipCount]);

    $sql = 'SELECT MAX(`id`) FROM `session_types`'; // Поиск id инициализированного session_type
    $command = $connect->prepare($sql);
    $command -> execute();
    $result = $command->fetchAll(PDO::FETCH_ASSOC); // fetch выдает страшный результат
    $sessionTypeGuid = "Session #" . $result[0]["MAX(`id`)"];

    return $result[0]["MAX(`id`)"];
}

function setSession($guid, $dateTimeStart, $sessionTypeId, $connect)
{
    $active = 1; 
    $sql = 'INSERT INTO `sessions` (`guid`, `date_time_start`, `date_time_finish`, `is_active`, `session_type_id`) VALUES (:guid, :dts, :dtf, :active, :sessionId)';
    $command = $connect->prepare($sql);
    $command -> execute(['guid' => $guid, 'dts' => $dateTimeStart, 'dtf' => $dateTimeStart, 'active' => $active, 'sessionId' => $sessionTypeId]);

    $sql = 'SELECT MAX(`id`) FROM `sessions`'; // Поиск id инициализированного session_type
    $command = $connect->prepare($sql);
    $command -> execute();
    $result = $command->fetchAll(PDO::FETCH_ASSOC); // fetch выдает страшный результат

    return $result[0]["MAX(`id`)"];
}


// LEAVE (Заполнить только session_players)
function playerLeave($sessionId, $chipIds, $connect)
{
    $lossRaiting = 8; // Если будет меняться то брать из бд
    for ($i = 0; $i < count($chipIds); $i++)
    {
        $sql = 'SELECT `id` FROM `chips` WHERE `guid` = :gid';
        $command = $connect->prepare($sql);
        $command->execute(['gid' => $chipIds[$i]["guid"]]);
        $chipId = $command->fetchAll(PDO::FETCH_ASSOC);
        
        $sql = 'INSERT INTO `session_players` (`session_id`, `loss_count_wall_street`, `chip_id`) VALUES (:sid, :los, :cId)';
        $command = $connect->prepare($sql);
        $command -> execute(['sid' => $sessionId, 'los' => $lossRaiting, 'cId' => $chipId[0]["id"]]);
        
        echo $chipIds[$i]["guid"];
    }
}

// COMPLETE
function completeSession($sessionId, $chipIds, $connect)
{
    for ($i = 0; $i < count($chipIds); $i++)
    {
        $sql = 'SELECT `id` FROM `chips` WHERE `guid` = :gid';
        $command = $connect->prepare($sql);
        $command->execute(['gid' => $chipIds[$i]["guid"]]);
        $chipId = $command->fetchAll(PDO::FETCH_ASSOC);
        
        if ($i < count($chipIds) / 2)
        {
            $sql = 'INSERT INTO `session_winners` (`session_id`, `win_count_bossy`, `win_count_wall_street`, `chip_id`) VALUES (:id, :boss, :rating, :chipId)';
            $command = $connect -> prepare($sql);
            $command -> execute(['id' => $sessionId, 'boss' => $chipIds[$i]["bossy"], 'rating' => $chipIds[$i]["rating"], 'chipId' => $chipId[0]["id"]]);
        }
        else 
        {
            $sql = 'INSERT INTO `session_players` (`session_id`, `loss_count_wall_street`, `chip_id`) VALUES (:id, :loss, :chipId)';
            $command = $connect -> prepare($sql);
            $command -> execute(['id' => $sessionId, 'loss' => $chipIds[$i]["rating"], 'chipId' => $chipId[0]["id"]]);
        }
    }
}

// Close the session
function closeSession($sessionId, $date, $connect)
{
    $active = 0; 
    $sql = 'UPDATE `sessions` SET `date_time_finish` = :dtf, `is_active` = :active WHERE `id` = :sessionId';
    $command = $connect->prepare($sql);
    $command -> execute(['dtf' => $date, 'active' => $active, 'sessionId' => $sessionId]);
}

?>