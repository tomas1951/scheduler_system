﻿using Npgsql;
using SharedResources.Enums;
using SharedResources.Messages;

namespace SchedulerServerApp.DBModule;

public class DBCommunication
{
    public NpgsqlDataSource DataSource { get; set; }
    public NpgsqlConnection Connection { get; set; }


    public DBCommunication(string host, string login, string pass,
        string myDatabase)
    {
        string connString = $"Host={host};Username={login};" +
            $"Password={pass};Database={myDatabase}";

        try
        {
            DataSource = NpgsqlDataSource.Create(connString);
            Connection = DataSource.OpenConnection();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"DB exception: {ex.GetType()}, {ex.Message}");
        }

        Console.WriteLine("DB connected.");
    }

    async public Task<List<string>> GetAllTasks()
    {
        // old
        await using var commandO = DataSource.CreateCommand(
            "SELECT * FROM tasks");
        await using var readerO = await commandO.ExecuteReaderAsync();

        // new
        await using var command = new NpgsqlCommand(
            "SELECT * FROM tasks", Connection);
        await using var reader = await command.ExecuteReaderAsync();

        // TODO - transform db into a list

        return new List<string>();
    }

    async public Task PrintAllTasks()
    {
        var command = DataSource.CreateCommand(
            "SELECT * FROM tasks");
        var reader = await command.ExecuteReaderAsync();

        int counter = 0;
        while (reader.Read())
        {
            counter++;
            for (int i = 0; i < reader.FieldCount; i++)
            {
                Console.Write($"{reader.GetName(i)}: {reader[i]} ");
            }
            Console.WriteLine();  // New line for each row
        }
        //Console.WriteLine($"Number of records: {counter}");
    }


    async public Task<TaskMessage?> GetHighestPriotityTask()
    {
        TaskMessage? output = null;
        var command = DataSource.CreateCommand(
            "SELECT * FROM tasks " +
            "WHERE status = 'Waiting' ORDER BY priority LIMIT 1;");
        var reader = await command.ExecuteReaderAsync();

        // TODO - hardcoded code, refactor in the future

        if (await reader.ReadAsync())
        {
            // TODO - refactor
            output = new TaskMessage
            {
                ID = (int)reader[0],
                Name = reader[1].ToString(),
                Description = reader[2].ToString(),
                Group = (int)reader[3],
                Status = (SchedulerTaskStatus)Enum.Parse(typeof(SchedulerTaskStatus), reader[4].ToString()),
                Priority = (Int32)(Int16)reader[5], // Because priority is of a datatype 'smallint'
                TimeCreated = DateTime.Now, // TODO - refactor
                ExeFilePath = reader[7].ToString(),
                InputFilesPath = reader[8].ToString(),
                OutputFilesPath = reader[9].ToString(),
                OperatingSystem = reader[10].ToString(),
                TimeCompleted = DateTime.Now,
                UserID = (int)reader[12]
            };
        }
        // TODO - what if there is no tasks in DB        
        return output; // TODO - NULL CHECK
    }


    async public void TestCreateRandomTask() // for autogenerated id
    {
        Console.WriteLine("Creating a task in database.");

        string name = "test_task";
        var command = DataSource.CreateCommand(
            "INSERT INTO " +
            $"tasks (name) VALUES ('{name}');");

        await command.ExecuteNonQueryAsync();
    }


    // TODO - check whether it is even called with await
    async public void SaveNewClientToDB(DBClientMachineModel cm)
    {
        // TODO - try catch block
        // TODO - add info columns
        var command = DataSource.CreateCommand(
            $"INSERT INTO client_machines " +
            $"(name, ip, status, last_status_msg_time) " +
            $"VALUES ('{cm.Client_name}', '{cm.IP}', 'Disconnected', @timestamp) " +
            $"ON CONFLICT (ip) DO NOTHING");

        command.Parameters.AddWithValue("@timestamp", NpgsqlTypes.NpgsqlDbType.Timestamp, DateTime.Now);
        await command.ExecuteNonQueryAsync();

        Console.WriteLine("New connection synced with the DB.");
    }


    // TODO - cant be async due to calling from another non async thread
    public void UpdateClientMachineStatus(string ip, string status, bool task_assigned)
    {
        var command = DataSource.CreateCommand(
            $"UPDATE client_machines " +
            $"SET status = '{status}', task_assigned = '{task_assigned}' " +
            $"WHERE ip = '{ip}'");

        command.ExecuteNonQuery();
        Console.WriteLine("Status of a client updated in DB.");
    }


    // TODO - this class is asking for a helping function for calling SQL queries in 
    // try - catch blocks

    async public Task<string?> GetFreeComputerClientAsync()
    {
        // TODO - but what do i need)? -- just IP
        var command = DataSource.CreateCommand(
            $"SELECT ip " +
            $"FROM client_machines " +
            $"WHERE status = 'Connected' AND task_assigned = '0' " +
            $"LIMIT 1");

        var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            string? output = reader[0].ToString();
            return output;
        }
        else
        {
            return null; // TODO - notify that there is no free client
        }
    }


    public void UpdateTaskStatus(string task_id, string status, DateTime? time_completed)
    {
        NpgsqlCommand command;

        if (time_completed is not null)
        {
            command = DataSource.CreateCommand(
            $"UPDATE tasks " +
            $"SET status = '{status}', time_completed = @timestamp " +
            $"WHERE id = '{task_id}'");

            command.Parameters.AddWithValue("@timestamp",
                NpgsqlTypes.NpgsqlDbType.Timestamp, time_completed);
        }
        else
        {
            command = DataSource.CreateCommand(
            $"UPDATE tasks " +
            $"SET status = '{status}' " +
            $"WHERE id = '{task_id}'");
        }

        command.ExecuteNonQuery();
        Console.WriteLine("Status of a task updated in DB.");
    }




    //public void TestGenerateTrainingTaskSet(int count)
    //{
    //    for (int i = 0; i <= count; i++)
    //    {
    //        string name = $"Task{i}";

    //    }
    //}


    //// TODO - More like an template than a function
    //public DateTime ConvertTimestampToDateTime(NpgsqlDataReader reader)
    //{
    //    DateTime output = reader.GetDateTime(0);
    //    return output;
    //}


    //// TODO - More like an template than a function
    //public NpgsqlCommand? ConvertDateTimeToTimestamp(DateTime datetime)
    //{
    //    var command = DataSource.CreateCommand(
    //        $"INSERT INTO table (timestampcolumn) VALUES ({datetime})");
    //    return command;
    //}

}
