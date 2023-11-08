// <copyright file="Program.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

#pragma warning disable SA1200
using System.Reflection;
using LastSeenDemo;
#pragma warning restore SA1200

// Global Application Services
var dateTimeProvider = new DateTimeProvider();
var loader = new Loader();
var detector = new OnlineDetector(dateTimeProvider);
var predictor = new Predictor(detector);
var userLoader = new UserLoader(loader, "sef.podkolzin.consulting/api/users/lastSeen");
var application = new LastSeenApplication(userLoader);
var userTransformer = new UserTransformer(dateTimeProvider);
var allUsersTransformer = new AllUsersTransformer(userTransformer);
var worker = new Worker(userLoader, allUsersTransformer);
var userMinMaxCalculator = new UserMinMaxCalculator(detector);

// End Global Application Services
var reports = new Dictionary<string, Report>();

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
Task.Run(worker.LoadDataPeriodically); // Launch collecting data in background
#pragma warning restore CS4014

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// APIs
var app = builder.Build();

app.MapGet("/", () => "Hello!"); // Just Demo Endpoint
app.MapGet(
    "/version",
    () => new
    {
        Version = 2,
        Assembly = Assembly.GetAssembly(typeof(Program))?.Location,
        Modified = File.GetLastWriteTime(Assembly.GetAssembly(typeof(Program))!.Location),
    });

Setup2ndAssignmentsEndpoints();
Setup3rdAssignmentsEndpoints();
Setup4ThAssignmentsEndpoints();
SetupReportsEndpoints();

app.UseSwagger();
app.UseSwaggerUI();

app.Run();

void Setup2ndAssignmentsEndpoints()
{
    app.MapGet("/formatted", () => application.Show(DateTimeOffset.Now)); // Assignment#2 in API form
}

void Setup3rdAssignmentsEndpoints()
{
    // Feature#1 - Implement endpoint that returns historical data for all users
    app.MapGet("/api/stats/users/", (DateTimeOffset date) => new { usersOnline = detector.CountOnline(worker.Users, date) });

    // Feature#2 - Implement endpoint that returns historical data for a concrete user
    app.MapGet("/api/stats/user", (DateTimeOffset date, Guid userId) =>
    {
        if (!worker.Users.ContainsKey(userId))
        {
            return Results.NotFound(new { userId });
        }

        var user = worker.Users[userId];
        return Results.Json(new
        {
            wasUserOnline = detector.Detect(user, date),
            nearestOnlineTime = detector.GetClosestOnlineTime(user, date),
        });
    });

    // Feature#3 - Implement endpoint that returns historical data for a concrete user
    app.MapGet(
        "/api/predictions/users",
        (DateTimeOffset date) => new { onlineUsers = predictor.PredictUsersOnline(worker.Users, date) });

    // Feature#4 - Implement a prediction mechanism based on a historical data for concrete user
    app.MapGet("/api/predictions/user", (Guid userId, DateTimeOffset date, float tolerance) =>
    {
        if (!worker.Users.TryGetValue(userId, out var user))
        {
            return Results.NotFound(new { userId });
        }

        var onlineChance = predictor.PredictUserOnline(user, date);
        return Results.Json(new { onlineChance, willBeOnline = onlineChance > tolerance });
    });
}

void Setup4ThAssignmentsEndpoints()
{
    // Feature#1 - Implement an endpoint that returns total time that user was online
    app.MapGet("/api/stats/user/total", (Guid userId) =>
    {
        if (!worker.Users.TryGetValue(userId, out var user))
        {
            return Results.NotFound(new { userId });
        }

        return Results.Json(new { totalTime = detector.CalculateTotalTimeForUser(user) });
    });

    // Feature#2 - Implement endpoints that returns average daily/weekly time for the specified user
    app.MapGet("/api/stats/user/average", (Guid userId) =>
    {
        if (!worker.Users.TryGetValue(userId, out var user))
        {
            return Results.NotFound(new { userId });
        }

        return Results.Json(new
        {
            dailyAverage = detector.CalculateDailyAverageForUser(user),
            weeklyAverage = detector.CalculateWeeklyAverageForUser(user),
        });
    });

    // Feature#3 - Implement endpoint to follow the EU regulator rules - GDPR - right to be forgotten
    app.MapPost("/api/user/forget", (Guid userId) =>
    {
        if (!worker.Users.ContainsKey(userId))
        {
            return Results.NotFound(new { userId });
        }

        worker.Forget(userId);
        return Results.Ok();
    });
}

void SetupReportsEndpoints()
{
    var userGuids = new List<Guid>
    {
        new Guid("2fba2529-c166-8574-2da2-eac544d82634"),
        new Guid("8b0b5db6-19d6-d777-575e-915c2a77959a"),
        new Guid("e13412b2-fe46-7149-6593-e47043f39c91"),
        new Guid("cbf0d80b-8532-070b-0df6-a0279e65d0b2"),
        new Guid("de5b8815-1689-7c78-44e1-33375e7e2931"),
    };

    // Feature#1 - Implement reports functionality
    app.MapPost("/api/report/{reportName}", (string reportName, ReportRequest request) =>
    {
        if (reports.ContainsKey(reportName))
        {
            return Results.BadRequest("Report already exists");
        }

        var report = new Report(reportName, request.Users, request.Metrics, worker, detector);
        reports.Add(reportName, report);
        return Results.NoContent();
    });

    app.MapGet("/api/reports", () => Results.Ok(reports.Values));

    app.MapGet("/api/report/{reportName}", (string reportName, DateTimeOffset from, DateTimeOffset to) =>
    {
        var report = new List<Dictionary<string, object>>();

        foreach (var userId in userGuids)
        {
            if (worker.Users.TryGetValue(userId, out var user))
            {
                try
                {
                    var userReport = new Dictionary<string, object> { { "UserId", userId } };

                    userReport["Total"] = detector.CalculateTotalTimeForUser(user);
                    userReport["DailyAverage"] = detector.CalculateDailyAverageForUser(user);
                    userReport["WeeklyAverage"] = detector.CalculateWeeklyAverageForUser(user);
                    var (min, max) = userMinMaxCalculator.CalculateMinMax(user, from, to);
                    userReport["Min"] = min;
                    userReport["Max"] = max;
                    report.Add(userReport);
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                }
            }
        }

        return Results.Ok(report);
    });
}
