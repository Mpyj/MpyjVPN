using System;
using MpyjCore.Services;

var service = new ConfigService();
service.LogMessage += (msg, type) => Console.WriteLine($"[{type}] {msg}");

Console.WriteLine("=== Testing SetSystemProxy ===");
service.SetSystemProxy();

Console.WriteLine("\n=== Testing DisableSystemProxy ===");
service.DisableSystemProxy();
