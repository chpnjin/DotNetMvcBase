DROP TABLE IF EXISTS SYS_USER;

CREATE TABLE `SYS_USER` (
  `GUID` varchar(38)  NOT NULL DEFAULT '00000' COMMENT '[基本欄] 唯一值',
  `INSERT_USER` varchar(50)  NOT NULL DEFAULT 'SYSTEM' COMMENT '[基本欄] 新增者',
  `INSERT_TIME` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '[基本欄] 新增時間',
  `UPDATE_USER` varchar(50)  DEFAULT NULL COMMENT '[基本欄] 更新者',
  `UPDATE_TIME` varchar(50)  DEFAULT NULL COMMENT '[基本欄] 更新時間',
  `ENABLE` tinyint DEFAULT '1' COMMENT '[基本欄] 資料有效性',
  `ID` varchar(50)  DEFAULT NULL COMMENT '使用者帳號',
  `PASSWORD` varchar(50)  DEFAULT NULL COMMENT '使用者密碼',
  `EMPLOYEE_GUID` varchar(38)  DEFAULT NULL COMMENT '綁定員工設定(HR_EMPLOYEE.GUID)',
  `REMARK` TEXT  DEFAULT NULL COMMENT '備註',
  UNIQUE KEY `GUID_UNIQUE` (`GUID`)
) ENGINE=InnoDB DEFAULT CHARSET=UTF8MB4 COLLATE utf8mb4_unicode_ci 
COMMENT='基本使用者帳號';