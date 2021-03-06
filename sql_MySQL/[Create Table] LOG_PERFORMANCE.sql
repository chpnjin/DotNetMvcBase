DROP TABLE IF EXISTS LOG_PERFORMANCE;

CREATE TABLE `LOG_PERFORMANCE` (
  `GUID` varchar(38)  NOT NULL DEFAULT 'uuid()' COMMENT '[基本欄] 唯一值',
  `INSERT_USER` varchar(50)  NOT NULL DEFAULT 'SYSTEM' COMMENT '[基本欄] 新增者',
  `INSERT_TIME` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '[基本欄] 新增時間',
  `USER_ID` varchar(50)  DEFAULT NULL COMMENT '使用者所屬權限群組ID',
  `GROUP_ID` varchar(50)  DEFAULT NULL COMMENT '使用者所屬權限群組ID',
  `FUNCTION_ID` varchar(50)  DEFAULT NULL COMMENT '功能頁面代碼',
  `ACTION` varchar(50)  DEFAULT NULL COMMENT '資料動作',
  `SQL` varchar(500)  DEFAULT NULL COMMENT 'SQL字串',
  `ELAPSED` INT DEFAULT NULL COMMENT 'SQL執行時間(毫秒)',
  UNIQUE KEY `GUID_UNIQUE` (`GUID`)
) ENGINE=InnoDB DEFAULT CHARSET=UTF8MB4 COLLATE utf8mb4_unicode_ci 
COMMENT='DB效能紀錄';