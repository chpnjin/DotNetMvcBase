DROP TABLE IF EXISTS SYS_NAVIGATION;

CREATE TABLE `SYS_NAVIGATION` (
  `GUID` varchar(38)  NOT NULL DEFAULT 'uuid()' COMMENT '[基本欄] 唯一值',
  `INSERT_USER` varchar(50)  NOT NULL DEFAULT 'SYSTEM' COMMENT '[基本欄] 新增者',
  `INSERT_TIME` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '[基本欄] 新增時間',
  `UPDATE_USER` varchar(50)  DEFAULT NULL COMMENT '[基本欄] 更新者',
  `UPDATE_TIME` varchar(50)  DEFAULT NULL COMMENT '[基本欄] 更新時間',
  `ENABLE` tinyint DEFAULT '1' COMMENT '[基本欄] 資料有效性',
  `URL` varchar(300)  DEFAULT NULL COMMENT '網頁完整路徑',
  `INDEX` float4 COMMENT '導覽項目排序序號',
  `DOC_KEY` varchar(50)  DEFAULT NULL COMMENT '多語系對應字典檔的data-lngKey',
  UNIQUE KEY `GUID_UNIQUE` (`GUID`)
) ENGINE=InnoDB DEFAULT CHARSET=UTF8MB4 COLLATE utf8mb4_unicode_ci 
COMMENT='右上前台下拉選單按鈕項目';