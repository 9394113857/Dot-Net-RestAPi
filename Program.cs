using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json;
using System.Collections.Generic;
using System;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Enable CORS
        services.AddCors();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            // Show detailed error page in development environment
            app.UseDeveloperExceptionPage();
        }

        app.UseRouting();

        // Allow CORS
        app.UseCors(builder =>
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader()
        );

        app.UseEndpoints(endpoints =>
        {
            // Default route
            endpoints.MapGet("/", async context =>
            {
                await context.Response.WriteAsync("Hello World!");
            });

            // Route to get all mobiles
            endpoints.MapGet("/mobiles", async context =>
            {
                var mobiles = GetMobiles();
                await context.Response.WriteAsync(JsonConvert.SerializeObject(mobiles, Formatting.Indented));
            });

            // Route to get a mobile by ID
            endpoints.MapGet("/mobiles/{id}", async context =>
            {
                var idString = context.Request.RouteValues["id"].ToString();
                if (!int.TryParse(idString, out int id))
                {
                    context.Response.StatusCode = 400; // Bad request
                    await context.Response.WriteAsync(JsonConvert.SerializeObject(new { message = "Invalid id" }));
                    return;
                }

                var mobile = GetMobileById(id);
                if (mobile != null)
                {
                    await context.Response.WriteAsync(JsonConvert.SerializeObject(mobile, Formatting.Indented));
                }
                else
                {
                    context.Response.StatusCode = 404; // Not found
                    await context.Response.WriteAsync(JsonConvert.SerializeObject(new { message = "Mobile not found" }));
                }
            });

            // Route to add a new mobile
            endpoints.MapPost("/mobiles", async context =>
            {
                var data = await context.Request.ReadFromJsonAsync<Mobile>();
                AddMobile(data);
                context.Response.StatusCode = 201; // Created
                await context.Response.WriteAsync(JsonConvert.SerializeObject(new { message = "Mobile added successfully" }));
            });

            // Route to update an existing mobile
            endpoints.MapPut("/mobiles/{id}", async context =>
            {
                var idString = context.Request.RouteValues["id"].ToString();
                if (!int.TryParse(idString, out int id))
                {
                    context.Response.StatusCode = 400; // Bad request
                    await context.Response.WriteAsync(JsonConvert.SerializeObject(new { message = "Invalid id" }));
                    return;
                }

                var data = await context.Request.ReadFromJsonAsync<Mobile>();
                UpdateMobile(id, data);
                await context.Response.WriteAsync(JsonConvert.SerializeObject(new { message = "Mobile updated successfully" }));
            });

            // Route to delete a mobile by ID
            endpoints.MapDelete("/mobiles/{id}", async context =>
            {
                var idString = context.Request.RouteValues["id"].ToString();
                if (!int.TryParse(idString, out int id))
                {
                    context.Response.StatusCode = 400; // Bad request
                    await context.Response.WriteAsync(JsonConvert.SerializeObject(new { message = "Invalid id" }));
                    return;
                }

                if (DeleteMobile(id))
                {
                    await context.Response.WriteAsync(JsonConvert.SerializeObject(new { message = "Mobile deleted successfully" }));
                }
                else
                {
                    context.Response.StatusCode = 404; // Not found
                    await context.Response.WriteAsync(JsonConvert.SerializeObject(new { message = "Mobile not found for deletion" }));
                }
            });
        });
    }

    // Method to retrieve all mobiles from the SQLite database
    private static List<Mobile> GetMobiles()
    {
        var mobiles = new List<Mobile>();
        using (var connection = new SqliteConnection("Data Source=mobiles.db"))
        {
            connection.Open();
            var selectCmd = connection.CreateCommand();
            selectCmd.CommandText = "SELECT * FROM mobiles";
            using (var reader = selectCmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    mobiles.Add(new Mobile
                    {
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        Price = reader.IsDBNull(2) ? (double?)null : reader.GetDouble(2),
                        RAM = reader.IsDBNull(3) ? null : reader.GetString(3),
                        Storage = reader.IsDBNull(4) ? null : reader.GetString(4)
                    });
                }
            }
        }
        return mobiles;
    }

    // Method to retrieve a mobile by ID from the SQLite database
    private static Mobile? GetMobileById(int id)
    {
        Mobile? mobile = null;
        using (var connection = new SqliteConnection("Data Source=mobiles.db"))
        {
            connection.Open();
            var selectCmd = connection.CreateCommand();
            selectCmd.CommandText = "SELECT * FROM mobiles WHERE id = @id";
            selectCmd.Parameters.AddWithValue("@id", id);
            using (var reader = selectCmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    mobile = new Mobile
                    {
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        Price = reader.IsDBNull(2) ? (double?)null : reader.GetDouble(2),
                        RAM = reader.IsDBNull(3) ? null : reader.GetString(3),
                        Storage = reader.IsDBNull(4) ? null : reader.GetString(4)
                    };
                }
            }
        }
        return mobile;
    }

    // Method to add a new mobile to the SQLite database
    private static void AddMobile(Mobile mobile)
    {
        using (var connection = new SqliteConnection("Data Source=mobiles.db"))
        {
            connection.Open();
            var insertCmd = connection.CreateCommand();
            insertCmd.CommandText = "INSERT INTO mobiles (name, price, ram, storage) VALUES (@name, @price, @ram, @storage)";
            insertCmd.Parameters.AddWithValue("@name", mobile.Name);
            insertCmd.Parameters.AddWithValue("@price", mobile.Price ?? (object)DBNull.Value);
            insertCmd.Parameters.AddWithValue("@ram", mobile.RAM ?? (object)DBNull.Value);
            insertCmd.Parameters.AddWithValue("@storage", mobile.Storage ?? (object)DBNull.Value);
            insertCmd.ExecuteNonQuery();
        }
    }

    // Method to update an existing mobile in the SQLite database
    private static void UpdateMobile(int id, Mobile mobile)
    {
        using (var connection = new SqliteConnection("Data Source=mobiles.db"))
        {
            connection.Open();
            var updateCmd = connection.CreateCommand();
            updateCmd.CommandText = "UPDATE mobiles SET name = @name, price = @price, ram = @ram, storage = @storage WHERE id = @id";
            updateCmd.Parameters.AddWithValue("@id", id);
            updateCmd.Parameters.AddWithValue("@name", mobile.Name);
            updateCmd.Parameters.AddWithValue("@price", mobile.Price ?? (object)DBNull.Value);
            updateCmd.Parameters.AddWithValue("@ram", mobile.RAM ?? (object)DBNull.Value);
            updateCmd.Parameters.AddWithValue("@storage", mobile.Storage ?? (object)DBNull.Value);
            updateCmd.ExecuteNonQuery();
        }
    }

    // Method to delete a mobile by ID from the SQLite database
    private static bool DeleteMobile(int id)
    {
        using (var connection = new SqliteConnection("Data Source=mobiles.db"))
        {
            connection.Open();
            var deleteCmd = connection.CreateCommand();
            deleteCmd.CommandText = "DELETE FROM mobiles WHERE id = @id";
            deleteCmd.Parameters.AddWithValue("@id", id);
            return deleteCmd.ExecuteNonQuery() > 0;
        }
    }
}

// Mobile class representing the structure of a mobile device
public class Mobile
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public double? Price { get; set; }
    public string? RAM { get; set; }
    public string? Storage { get; set; }
}

// Main program class
public class Program
{
    public static void Main(string[] args)
    {
        // Initialize SQLite database
        InitializeDatabase();
        
        // Build and run the web host
        CreateHostBuilder(args).Build().Run();
    }

    // Method to initialize the SQLite database
    private static void InitializeDatabase()
    {
        using (var connection = new SqliteConnection("Data Source=mobiles.db"))
        {
            connection.Open();

            var createTableCmd = connection.CreateCommand();
            createTableCmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS mobiles (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    name TEXT NOT NULL,
                    price REAL,
                    ram TEXT,
                    storage TEXT
                )";
            createTableCmd.ExecuteNonQuery();
        }
    }

    // Method to create a new instance of the web host
    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>(); 
            });
}
