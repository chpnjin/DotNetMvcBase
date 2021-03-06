#USE web
SET SQL_SAFE_UPDATES=0;
#清除原本資料
DELETE FROM SYS_USER;
DELETE FROM SYS_GROUP;
DELETE FROM SYS_FUNCTION;
DELETE FROM SYS_NAVIGATION;
DELETE FROM SYS_PARAM;
DELETE FROM SYS_USER_GROUP;
DELETE FROM SYS_GROUP_FUNCTION;
DELETE FROM SYS_GROUP_NAVIGATION;
DELETE FROM HR_DEPARTMENT;
DELETE FROM HR_TITLE;
DELETE FROM HR_EMPLOYEE;
DELETE FROM HR_EMPLOYEE_TITLE;
DELETE FROM HR_EMPLOYEE_DEPARTMENT;

#系統管理員基本資料
INSERT INTO SYS_USER (GUID,INSERT_USER, ID, PASSWORD, EMPLOYEE_GUID, REMARK) VALUES (UUID(),N'SYSTEM', N'admin', N'0DPiKuNIrrVmD8IUCuw1hQxNqZc=', N'41DA5287-04E1-4758-A505-4471CEE68C73', N'預設使用者');
INSERT INTO SYS_GROUP (GUID,INSERT_USER, ID, NAME, REMARK) VALUES (uuid(),N'SYSTEM', N'ADMIN', N'管理者', N'最高權限');
INSERT INTO HR_DEPARTMENT (GUID,INSERT_USER, ID, NAME, REMARK) VALUES (N'4292D9C7-D265-4F18-80F3-0896A9D8986A',N'SYSTEM', N'A1', N'管理部', NULL);
INSERT INTO HR_TITLE (GUID,INSERT_USER, ID, NAME, REMARK) VALUES (N'72D8EA8A-92C6-449A-9856-67D32D3AE5A4',N'SYSTEM', N'M1', N'資訊長', N'測試');
INSERT INTO HR_EMPLOYEE (GUID,INSERT_USER, ID, NAME, EMAIL, PHONE, REMARK) VALUES (N'41DA5287-04E1-4758-A505-4471CEE68C73', N'SYSTEM', N'0000', N'系統管理員', N'test@aaa.com', N'00000000', N'預設管理員帳號資訊');
INSERT INTO HR_EMPLOYEE_TITLE(INSERT_USER,EMPLOYEE_GUID,TITLE_GUID)VALUES(N'SYSTEM',N'41DA5287-04E1-4758-A505-4471CEE68C73',N'72D8EA8A-92C6-449A-9856-67D32D3AE5A4');
INSERT INTO HR_EMPLOYEE_DEPARTMENT(INSERT_USER,EMPLOYEE_GUID,DEPARTMENT_GUID)VALUES(N'SYSTEM',N'41DA5287-04E1-4758-A505-4471CEE68C73',N'4292D9C7-D265-4F18-80F3-0896A9D8986A');

#功能導覽列項目
INSERT SYS_FUNCTION (GUID, INSERT_USER, PARENT_GUID, `INDEX`, LEVEL, FUNCTION_ID, DOC_KEY, ICON_KEY) VALUES (N'bc45ddd7-b3aa-4b03-8dff-3ff46cfaeb53', N'SYSTEM', NULL, 2, 1, N'#', N'accountManage', N'fas fa-user-cog');

#accountManage
INSERT SYS_FUNCTION (GUID, INSERT_USER, PARENT_GUID, `INDEX`, LEVEL, FUNCTION_ID, DOC_KEY,HELP_FILE_NAME) VALUES (uuid(), N'SYSTEM', N'bc45ddd7-b3aa-4b03-8dff-3ff46cfaeb53', 1, 2, N'User', N'account_user','user_demo.pdf');
INSERT SYS_FUNCTION (GUID, INSERT_USER, PARENT_GUID, `INDEX`, LEVEL, FUNCTION_ID, DOC_KEY) VALUES (uuid(), N'SYSTEM', N'bc45ddd7-b3aa-4b03-8dff-3ff46cfaeb53', 2, 2, N'GroupUser', N'account_groupUser');
INSERT SYS_FUNCTION (GUID, INSERT_USER, PARENT_GUID, `INDEX`, LEVEL, FUNCTION_ID, DOC_KEY) VALUES (uuid(), N'SYSTEM', N'bc45ddd7-b3aa-4b03-8dff-3ff46cfaeb53', 3, 2, N'GroupFunction', N'account_groupFunction');
INSERT SYS_FUNCTION (GUID, INSERT_USER, PARENT_GUID, `INDEX`, LEVEL, FUNCTION_ID, DOC_KEY) VALUES (uuid(), N'SYSTEM', N'bc45ddd7-b3aa-4b03-8dff-3ff46cfaeb53', 4, 2, N'Title', N'account_title');
INSERT SYS_FUNCTION (GUID, INSERT_USER, PARENT_GUID, `INDEX`, LEVEL, FUNCTION_ID, DOC_KEY) VALUES (uuid(), N'SYSTEM', N'bc45ddd7-b3aa-4b03-8dff-3ff46cfaeb53', 5, 2, N'Department', N'account_department');
INSERT SYS_FUNCTION (GUID, INSERT_USER, PARENT_GUID, `INDEX`, LEVEL, FUNCTION_ID, DOC_KEY) VALUES (uuid(), N'SYSTEM', N'bc45ddd7-b3aa-4b03-8dff-3ff46cfaeb53', 6, 2, N'Employee', N'account_employee');

#前台下拉按鈕選項
INSERT INTO SYS_NAVIGATION (GUID,INSERT_USER,URL,`INDEX`,DOC_KEY) 
VALUE (uuid(),'SYSTEM','[Please input website URL]',1,'frontOffice_map');

#預設系統參數
INSERT INTO SYS_PARAM (GUID,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,`FUNCTION`,FILTER_KEY,`TEXT`,`VALUE`,`TYPE`,`REMARK`) VALUES (uuid(),N'SYSTEM', NOW(), NULL, NULL, 1, N'GLOBAL', N'TimeFormat', N'時間格式', N'HH:mm:ss', N'nvarchar', NULL);
INSERT INTO SYS_PARAM (GUID,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,`FUNCTION`,FILTER_KEY,`TEXT`,`VALUE`,`TYPE`,`REMARK`) VALUES (uuid(),N'SYSTEM', NOW(), NULL, NULL, 1, N'GLOBAL', N'DateTimeFormat', N'日期+時間格式', N'yyyy-MM-dd HH:mm:ss', N'nvarchar', NULL);
INSERT INTO SYS_PARAM (GUID,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,`FUNCTION`,FILTER_KEY,`TEXT`,`VALUE`,`TYPE`,`REMARK`) VALUES (uuid(),N'SYSTEM', NOW(), NULL, NULL, 1, N'GLOBAL', N'WordLength', N'在Grid中單格顯示的字數上限', N'20', N'int', NULL);
INSERT INTO SYS_PARAM (GUID,INSERT_USER,INSERT_TIME,UPDATE_USER,UPDATE_TIME,`ENABLE`,`FUNCTION`,FILTER_KEY,`TEXT`,`VALUE`,`TYPE`,`REMARK`) VALUES (uuid(),N'SYSTEM', NOW(), NULL, NULL, 1, N'GLOBAL', N'DateFormat', N'日期格式', N'yyyy-MM-dd', N'nvarchar', NULL);

#預設管理者權限
INSERT INTO SYS_USER_GROUP(GUID,INSERT_USER,USER_GUID,GROUP_GUID)
SELECT uuid(),'SYSTEM',U.GUID,G.GUID
FROM SYS_USER U
CROSS JOIN SYS_GROUP G
WHERE U.ID = 'ADMIN' AND G.ID = 'ADMIN'
;

INSERT INTO SYS_GROUP_FUNCTION(GUID,INSERT_USER,GROUP_GUID,FUNCTION_GUID)
SELECT uuid(),'SYSTEM',G.GUID,F.GUID
FROM SYS_GROUP G
CROSS JOIN SYS_FUNCTION F
WHERE G.ID = 'ADMIN' AND F.FUNCTION_ID != '#'
;

INSERT INTO SYS_GROUP_NAVIGATION(GUID,INSERT_USER,GROUP_GUID,NAVIGATION_GUID)
SELECT uuid(),'SYSTEM',G.GUID,N.GUID
FROM SYS_GROUP G
CROSS JOIN SYS_NAVIGATION N
WHERE G.ID = 'ADMIN'
;