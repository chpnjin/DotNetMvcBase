DROP TABLE IF EXISTS LOG_NOTICE;
DROP TABLE IF EXISTS LOG_NOTICE_HAVE_READ;

CREATE TABLE `LOG_NOTICE` (
  `GUID` varchar(38)  NOT NULL DEFAULT '' COMMENT '[基本欄] 唯一值',
  `INSERT_USER` varchar(50) NOT NULL DEFAULT 'SYSTEM' COMMENT '[基本欄] 新增者',
  `INSERT_TIME` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '[基本欄] 新增時間',
  `TYPE` varchar(50) DEFAULT NULL COMMENT '通知類型(NORMAL,ALARM)',
  `TITLE` varchar(50) DEFAULT NULL COMMENT '標題',
  `CONTENT` TEXT DEFAULT NULL COMMENT '內文',
  UNIQUE KEY `GUID_UNIQUE` (`GUID`)
) ENGINE=InnoDB DEFAULT CHARSET=UTF8MB4 COLLATE=utf8mb4_unicode_ci 
COMMENT='通知紀錄';

CREATE TABLE `LOG_NOTICE_HAVE_READ`(
  `GUID` varchar(38)  NOT NULL DEFAULT '' COMMENT '[基本欄] 唯一值',
  `INSERT_USER` varchar(50)  NOT NULL DEFAULT 'SYSTEM' COMMENT '[基本欄] 新增者',
  `INSERT_TIME` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '[基本欄] 新增時間',
  `NOTICE_GUID` varchar(38) NOT NULL COMMENT '通知訊息GUID',
  `USER_GUID` varchar(38) NOT NULL COMMENT '已讀訊息使用者GUID',
  UNIQUE KEY `GUID_UNIQUE` (`GUID`)
) ENGINE=InnoDB DEFAULT CHARSET=UTF8MB4 COLLATE=utf8mb4_unicode_ci 
COMMENT='使用者已讀取通知紀錄';

-- 紀錄某人已讀取某項通知
-- INSERT INTO `log_notice_have_read`
-- (`GUID`,
-- `INSERT_TIME`,
-- `NOTICE_GUID`,
-- `USER_GUID`)
-- VALUES
-- (UUID(),
-- NOW(),
-- '[LOG_NOTICE.GUID]',
-- '[SYS_USER.GUID]');

-- 尋找某人讀取訊息狀態
-- SET @USER_GUID = (SELECT GUID FROM SYS_USER WHERE ID = `admin`);
-- SELECT * FROM(
-- 	-- 未讀取
-- 	SELECT N.GUID,N.INSERT_TIME,N.`TYPE`,N.TITLE,N.CONTENT,'N' AS 'READED'
-- 	FROM LOG_NOTICE N
-- 	WHERE N.GUID NOT IN (
-- 		SELECT NR.NOTICE_GUID FROM LOG_NOTICE_HAVE_READ
-- 		INNER JOIN LOG_NOTICE_HAVE_READ NR ON NR.NOTICE_GUID = N.GUID
-- 		INNER JOIN sys_user U ON U.GUID = NR.USER_GUID
-- 		WHERE NR.USER_GUID = @USER_GUID
-- 	)
-- 	UNION 
-- 	-- 今日已讀取
-- 	SELECT N.GUID,N.INSERT_TIME,N.TYPE,N.TITLE,N.CONTENT,'Y' AS 'READED'
-- 	FROM LOG_NOTICE N
-- 	INNER JOIN LOG_NOTICE_HAVE_READ NR ON NR.NOTICE_GUID = N.GUID
-- 	INNER JOIN sys_user U ON U.GUID = NR.USER_GUID
-- 	WHERE NR.USER_GUID = @USER_GUID
-- 	AND N.INSERT_TIME BETWEEN TIMESTAMP(CURDATE()) AND date_add(CURDATE(), interval 24*60*60 - 1 second)
-- ) a ORDER BY INSERT_TIME DESC
-- ;