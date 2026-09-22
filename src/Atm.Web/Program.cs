using Atm.Application.Accounts;
using Atm.Application.Ports;
using Atm.Application.Transactions;
using Atm.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

builder.Services.AddAtmPersistence(
    builder.Configuration.GetConnectionString("Atm") ?? "Data Source=atm.db");
builder.Services.AddSingleton<IClock, SystemClock>();
builder.Services.AddScoped<DepositHandler>();
builder.Services.AddScoped<WithdrawHandler>();
builder.Services.AddScoped<TransferHandler>();
builder.Services.AddScoped<GetAccountsQuery>();
builder.Services.AddScoped<GetTransactionHistoryQuery>();

var app = builder.Build();

await app.Services.InitializeAtmDatabaseAsync();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

await app.RunAsync();

public partial class Program;
