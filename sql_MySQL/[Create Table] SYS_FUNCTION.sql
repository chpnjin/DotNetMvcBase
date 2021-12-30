DROP TABLE IF EXISTS SYS_FUNCTION;

CREATE TABLE `SYS_FUNCTION` (
  `GUID` varchar(38)  NOT NULL DEFAULT 'uuid()' COMMENT '[基本欄] 唯一值',
  `INSERT_USER` varchar(50)  NOT NULL DEFAULT 'SYSTEM' COMMENT '[基本欄] 新增者',
  `INSERT_TIME` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '[基本欄] 新增時間',
  `UPDATE_USER` varchar(50)  DEFAULT NULL COMMENT '[基本欄] 更新者',
  `UPDATE_TIME` varchar(50)  DEFAULT NULL COMMENT '[基本欄] 更新時間',
  `ENABLE` tinyint DEFAULT '1' COMMENT '[基本欄] 資料有效性',
  `FUNCTION_ID` varchar(50)  DEFAULT NULL COMMENT '導覽列項目對應的功能頁面ID',
  `LEVEL` float4 COMMENT '階層數(由1~3,當階層數不為1時須填寫對應父階層GUID)',
  `INDEX` float4 COMMENT '導覽項目排序序號',
  `DOC_KEY` varchar(50)  DEFAULT NULL COMMENT '多語系對應字典檔的data-lngKey',
  `ICON_KEY` TEXT  DEFAULT NULL COMMENT '項目對應要顯示的圖示關鍵字(css class 名稱,搭配前端套件)',
  `PARENT_GUID` varchar(38)  DEFAULT NULL COMMENT '對應父階層',
  `HELP_FILE_NAME` TEXT  DEFAULT NULL COMMENT '功能使用說明檔檔名',
  UNIQUE KEY `GUID_UNIQUE` (`GUID`)
) ENGINE=InnoDB DEFAULT CHARSET=UTF8MB4 COLLATE utf8mb4_unicode_ci 
COMMENT='左側功能頁面導覽列設定';