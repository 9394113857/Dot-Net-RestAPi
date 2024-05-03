var builder = WebApplication.CreateBuilder(args); // Create a new instance of the WebApplicationBuilder
var app = builder.Build(); // Build the application using the provided configuration

// Configure the HTTP request pipeline
app.UseRouting();

// Add a route for the homepage
app.MapGet("/", (HttpContext context) =>
{
    // Display the form to input the username
    return context.Response.WriteAsync(@"
        <!DOCTYPE html>
        <html>
        <head>
            <title>My Web App</title>
        </head>
        <body>
            <h1>Welcome to My Web App</h1>

            <!-- Form to input the username -->
            <form method='post'>
                <label for='userName'>Enter your name:</label><br>
                <input type='text' id='userName' name='userName'><br><br>
                <button type='submit'>Submit</button>
            </form>
        </body>
        </html>
    ");
});

// Add a route for handling form submissions
app.MapPost("/", (HttpContext context) =>
{
    // Retrieve the username from the form submission
    var userName = context.Request.Form["userName"];

    // Display the username entered by the user
    return context.Response.WriteAsync($@"
        <!DOCTYPE html>
        <html>
        <head>
            <title>My Web App</title>
        </head>
        <body>
            <h1>Welcome to My Web App</h1>

            <!-- Display the username entered by the user -->
            <p>Hello, {userName}!</p>

            <!-- Back link to the form -->
            <a href='/'>Back to Form</a>
        </body>
        </html>
    ");
});

app.Run(); // Start the application
