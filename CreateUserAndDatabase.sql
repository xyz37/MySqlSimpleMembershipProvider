grant all privileges on 
	SimpleMembershipTest.* to 
	'dev'@'%' identified by 
	'thePassword' with grant option;

flush privileges;

use mysql;
select host, user from user where user = 'dev';
select host, user, db from db where user = 'dev';



/*
>mysql --user=root -p
Enter password: ******
Welcome to the MySQL monitor.  Commands end with ; or \g.
Your MySQL connection id is 38
Server version: 5.5.28 MySQL Community Server (GPL)

Copyright (c) 2000, 2012, Oracle and/or its affiliates. All rights reserved.

Oracle is a registered trademark of Oracle Corporation and/or its
affiliates. Other names may be trademarks of their respective
owners.

Type 'help;' or '\h' for help. Type '\c' to clear the current input statement.

mysql> grant all privileges on
	->  SimpleMembershipTest.* to
	->  'dev'@'%' identified by
	->  'thePassword' with grant option;
Query OK, 0 rows affected (0.00 sec)

mysql>
mysql> flush privileges;
Query OK, 0 rows affected (0.00 sec)

mysql>
mysql> use mysql;
Database changed
mysql> select host, user from user where user = 'dev';
+------+------+
| host | user |
+------+------+
| %    | dev  |
+------+------+
1 row in set (0.00 sec)

mysql> select host, user, db from db where user = 'dev';
+------+------+----------------------+
| host | user | db                   |
+------+------+----------------------+
| %    | dev  | simplemembershiptest |
+------+------+----------------------+
1 row in set (0.00 sec)

mysql>
*/