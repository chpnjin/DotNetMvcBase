DROP TABLE IF EXISTS LOG_ACTION;

CREATE TABLE `LOG_ACTION` (
  `GUID` varchar(38)  NOT NULL DEFAULT 'uuid()' COMMENT '[基本欄] 唯一值',
  `INSERT_USER` varchar(50)  NOT NULL DEFAULT 'SYSTEM' COMMENT '[基本欄] 新增者',
  `INSERT_TIME` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '[基本欄] 新增時間',
  `USER_ID` varchar(50)  DEFAULT NULL COMMENT '使用者代號',
  `USER_NAME` varchar(50)  DEFAULT NULL COMMENT '使用者名稱',
  `DEPARTMENT_ID` varchar(50)  DEFAULT NULL COMMENT '使用者所屬部門',
  `GROUP_ID` varchar(50)  DEFAULT NULL COMMENT '使用者權限群組',
  `FUNCTION_ID` varchar(50)  DEFAULT NULL COMMENT '功能頁面代碼',
  `TARGET_ELEMENT` varchar(50)  DEFAULT NULL COMMENT '動作目標元件',
  `ACTION` TEXT  DEFAULT NULL COMMENT '動作描述',
  UNIQUE KEY `GUID_UNIQUE` (`GUID`)
) ENGINE=InnoDB DEFAULT CHARSET=UTF8MB4 COLLATE utf8mb4_unicode_ci 
COMMENT='使用者動作紀錄';