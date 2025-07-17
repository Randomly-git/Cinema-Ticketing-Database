using Oracle.ManagedDataAccess.Client;
using System;
class Program
{
    static void Main()
    {
        string connectionString= "Data Source=//8.148.76.54:1524/orclpdb1;User Id=SYS;Password=123456;DBA Privilege=SYSDBA";
        using (OracleConnection connection = new OracleConnection(connectionString))
        {
            try
            {
                connection.Open();
                Console.WriteLine("Connection to Oracle database established successfully.");
            }
            catch (OracleException ex)
            {
                Console.WriteLine("Oracle error code: " + ex.Number);
                Console.WriteLine("Oracle error message: " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }
        }
        //删除表
        /*string sql = "drop table discounts";                
        using (OracleConnection connection = new OracleConnection(connectionString))
        {
            try
            {
                connection.Open();
                using (OracleCommand command = new OracleCommand(sql, connection))
                {
                    command.ExecuteNonQuery();
                    Console.WriteLine("Table dropped successfully.");
                }
            }
            catch (OracleException ex)
            {
                Console.WriteLine("Oracle error code: " + ex.Number);
                Console.WriteLine("Oracle error message: " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }
        }*/





        //建立film表
        /*string sql2= "create table film(filmName varchar(50) primary key,genre varchar(20),filmLength int,normalPrice number(5,2),releaseDate date,endDate date,admissions int,boxOffice int,score int)";
        using (OracleConnection connection = new OracleConnection(connectionString))
        {
            try
            {
                connection.Open();
                using (OracleCommand command = new OracleCommand(sql2, connection))
                {
                    command.ExecuteNonQuery();
                    Console.WriteLine("Table film created successfully.");
                }
            }
            catch (OracleException ex)
            {
                Console.WriteLine("Oracle error code: " + ex.Number);
                Console.WriteLine("Oracle error message: " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }
        }*/





        //建立customer表
        /*string sql1 = "create table customer(customerID varchar(20) primary key,name varchar(20),phoneNum varchar(11),vipLevel int)";
        using (OracleConnection connection = new OracleConnection(connectionString))
        {
            try
            {
                connection.Open();
                using (OracleCommand command = new OracleCommand(sql1, connection))
                {
                    command.ExecuteNonQuery();
                    Console.WriteLine("Table customer created successfully.");
                }
            }
            catch (OracleException ex)
            {
                Console.WriteLine("Oracle error code: " + ex.Number);
                Console.WriteLine("Oracle error message: " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }
        }*/




        //建立cast表
        /*string sql2= "create table cast(memberName varchar(20) primary key,role varchar(20),filmName varchar(50),foreign key(filmName) references film(filmName))";
        using (OracleConnection connection = new OracleConnection(connectionString))
        {
            try
            {
                connection.Open();
                using (OracleCommand command = new OracleCommand(sql2, connection))
                {
                    command.ExecuteNonQuery();
                    Console.WriteLine("Table cast created successfully.");
                }
            }
            catch (OracleException ex)
            {
                Console.WriteLine("Oracle error code: " + ex.Number);
                Console.WriteLine("Oracle error message: " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }
        }*/




        //建立moviehall表
        //这里将原文件的column改为了columns，因为"column"是Oracle的保留字
        /*string sql3 = "create table moviehall(hallNo int primary key,lines int,columns int,category varchar(20))";
        using (OracleConnection connection = new OracleConnection(connectionString))
        {
            try
            {
                connection.Open();
                using (OracleCommand command = new OracleCommand(sql3, connection))
                {
                    command.ExecuteNonQuery();
                    Console.WriteLine("Table moviehall created successfully.");
                }
            }
            catch (OracleException ex)
            {
                Console.WriteLine("Oracle error code: " + ex.Number);
                Console.WriteLine("Oracle error message: " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }
        }*/





        //建立seathall表
        /*string sql4 = "create table seathall(hallNo int,lineNo int,columnNo int,category varchar(1),foreign key(hallNo) references moviehall(hallNo),primary key (hallNo,lineNo,columnNo))";
        using (OracleConnection connection = new OracleConnection(connectionString))
        {
            try
            {
                connection.Open();
                using (OracleCommand command = new OracleCommand(sql4, connection))
                {
                    command.ExecuteNonQuery();
                    Console.WriteLine("Table seathall created successfully.");
                }
            }
            catch (OracleException ex)
            {
                Console.WriteLine("Oracle error code: " + ex.Number);
                Console.WriteLine("Oracle error message: " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }
        }*/





        //建立timeslot表
        //这里由于oracle中不包含只记录时间而不记录日期的数据类型，因此这里暂时采用date类型对时间进行记录，会同时记录时间和日期。
        /*string sql5 = "create table timeslot(timeID varchar(20) primary key,startTime date,endTime date)";
        using (OracleConnection connection = new OracleConnection(connectionString))
        {
            try
            {
                connection.Open();
                using (OracleCommand command = new OracleCommand(sql5, connection))
                {
                    command.ExecuteNonQuery();
                    Console.WriteLine("Table timeslot created successfully.");
                }
            }
            catch (OracleException ex)
            {
                Console.WriteLine("Oracle error code: " + ex.Number);
                Console.WriteLine("Oracle error message: " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }
        }*/





        //建立section表
        /*string sql6 = "create table section(sectionID int primary key,filmName varchar(50),hallNo int,timeID varchar(20),day date,foreign key(filmName) references film(filmName),foreign key(hallNo) references moviehall(hallNo),foreign key(timeID) references timeslot(timeID))";
        using (OracleConnection connection = new OracleConnection(connectionString))
        {
            try
            {
                connection.Open();
                using (OracleCommand command = new OracleCommand(sql6, connection))
                {
                    command.ExecuteNonQuery();
                    Console.WriteLine("Table section created successfully.");
                }
            }
            catch (OracleException ex)
            {
                Console.WriteLine("Oracle error code: " + ex.Number);
                Console.WriteLine("Oracle error message: " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }
        }*/





        //建立ticket表
        /*string sql7= "create table ticket(ticketID varchar(20) primary key,price int,rating int,sectionID int,lineNo int,columnNo int,state varchar(10),foreign key(sectionID) references section(sectionID))";
        using (OracleConnection connection = new OracleConnection(connectionString))
        {
            try
            {
                connection.Open();
                using (OracleCommand command = new OracleCommand(sql7, connection))
                {
                    command.ExecuteNonQuery();
                    Console.WriteLine("Table ticket created successfully.");
                }
            }
            catch (OracleException ex)
            {
                Console.WriteLine("Oracle error code: " + ex.Number);
                Console.WriteLine("Oracle error message: " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }
        }*/





        //建立orderfortickets表
        //由于oracle中不支持p-method这一名称，因此将其改为pmethod
        /*string sql8= "create table orderfortickets(orderID int primary key,customerID varchar(20),ticketID varchar(20),day date,state varchar(10),pmethod varchar(20),price int,foreign key(customerID) references customer(customerID),foreign key(ticketID) references ticket(ticketID))";
        using (OracleConnection connection = new OracleConnection(connectionString))
        {
            try
            {
                connection.Open();
                using (OracleCommand command = new OracleCommand(sql8, connection))
                {
                    command.ExecuteNonQuery();
                    Console.WriteLine("Table orderfortickets created successfully.");
                }
            }
            catch (OracleException ex)
            {
                Console.WriteLine("Oracle error code: " + ex.Number);
                Console.WriteLine("Oracle error message: " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }
        }*/





        //建立relatedproduct表
        //oracle中number为保留字，因此将其改为productnumber
        /*string sql9 = "create table relatedproduct(productname varchar(20) primary key,price int,productnumber int)";
        using (OracleConnection connection = new OracleConnection(connectionString))
        {
            try
            {
                connection.Open();
                using (OracleCommand command = new OracleCommand(sql9, connection))
                {
                    command.ExecuteNonQuery();
                    Console.WriteLine("Table relatedproduct created successfully.");
                }
            }
            catch (OracleException ex)
            {
                Console.WriteLine("Oracle error code: " + ex.Number);
                Console.WriteLine("Oracle error message: " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }
        }*/





        //建立orderforproducts表
        //p-method改为pmethod,number(购买数量)改为purchasenum
        /*string sql10 = "create table orderforproducts(orderID int primary key,customerID varchar(20),productname varchar(20),purchasenum int,day date,state varchar(10),pmethod varchar(20),price int,foreign key(customerID) references customer(customerID),foreign key(productname) references relatedproduct(productname))";
        using (OracleConnection connection = new OracleConnection(connectionString))
        {
            try
            {
                connection.Open();
                using (OracleCommand command = new OracleCommand(sql10, connection))
                {
                    command.ExecuteNonQuery();
                    Console.WriteLine("Table orderforproducts created successfully.");
                }
            }
            catch (OracleException ex)
            {
                Console.WriteLine("Oracle error code: " + ex.Number);
                Console.WriteLine("Oracle error message: " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }
        }*/




        //建立customerprotrait表
        /*string sql11 = "create table customerprotrait(customerID varchar(20),genre varchar(20),primary key (customerID,genre),foreign key(customerID) references customer(customerID))";
        using (OracleConnection connection = new OracleConnection(connectionString))
        {
            try
            {
                connection.Open();
                using (OracleCommand command = new OracleCommand(sql11, connection))
                {
                    command.ExecuteNonQuery();
                    Console.WriteLine("Table customerprotrait created successfully.");
                }
            }
            catch (OracleException ex)
            {
                Console.WriteLine("Oracle error code: " + ex.Number);
                Console.WriteLine("Oracle error message: " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }
        }*/




        //建立VIPcard表
        //原文件内的pionts改为points
        /*string sql12= "create table VIPcard(customerID varchar(20) primary key,points int,foreign key(customerID) references customer(customerID))";
        using (OracleConnection connection = new OracleConnection(connectionString))
        {
            try
            {
                connection.Open();
                using (OracleCommand command = new OracleCommand(sql12, connection))
                {
                    command.ExecuteNonQuery();
                    Console.WriteLine("Table VIPcard created successfully.");
                }
            }
            catch (OracleException ex)
            {
                Console.WriteLine("Oracle error code: " + ex.Number);
                Console.WriteLine("Oracle error message: " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }
        }*/




        //建立discounts表
        /*string sql13 = "create table discounts(timeID varchar(20) primary key,discount decimal(2,1),foreign key(timeID) references timeslot(timeID))";
        using (OracleConnection connection = new OracleConnection(connectionString))
        {
            try
            {
                connection.Open();
                using (OracleCommand command = new OracleCommand(sql13, connection))
                {
                    command.ExecuteNonQuery();
                    Console.WriteLine("Table discounts created successfully.");
                }
            }
            catch (OracleException ex)
            {
                Console.WriteLine("Oracle error code: " + ex.Number);
                Console.WriteLine("Oracle error message: " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }
        }*/


        Console.WriteLine("Database setup completed successfully.");
    }
}