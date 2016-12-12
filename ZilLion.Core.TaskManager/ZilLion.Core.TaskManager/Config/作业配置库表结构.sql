/*task 配置表*/
CREATE TABLE task_config (
taskid VARCHAR(36),
taskname  VARCHAR(20),
taskremark VARCHAR(100),
taskmodule VARCHAR(50),
taskaction VARCHAR(50),
taskparam VARCHAR(1000),
taskronexpression VARCHAR(200),
taskstatus TINYINT,
isdeleted TINYINT
PRIMARY KEY (taskid)
)
/*运行状态表*/
CREATE TABLE task_run_log (

taskserverid VARCHAR(36),
pcip VARCHAR(15),
pcmac VARCHAR(50),
pcname VARCHAR(50),
taskid VARCHAR(36),
taskname  VARCHAR(20),
taskstatus TINYINT,
tasknextruntime DATETIME,
tasklastruntime DATETIME,
taskremark VARCHAR(100),

PRIMARY KEY (taskserverid,taskid)
)