﻿/*
 * Copyright (c) 2019 Google LLC.
 *
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not
 * use this file except in compliance with the License. You may obtain a copy of
 * the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
 * License for the specific language governing permissions and limitations under
 * the License.
 */

using CloudSql.Settings;
using Google.Cloud.Diagnostics.AspNetCore;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace CloudSql
{
    public class Program
    {
        public static AppSettings AppSettings { get; private set; }

        public static void Main(string[] args)
        {
            BuildWebHost(args).Build().Run();
        }

        public static IWebHostBuilder BuildWebHost(string[] args)
        {
            ReadAppSettings();

            string port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
            // hostingUrl is for hosting by Cloud Run and App Engine.
            string hostingUrl = $"http://0.0.0.0:{port}";
            // localhostUrl is for running locally and running tests.
            // localhostUrl port should be different from hostingUrl port
            // to prevent "Address already in use" error when deploying
            // to Cloud Run or App Engine.
            string localhostUrl = "http://localhost:5567";
            string urls = $"{localhostUrl};{hostingUrl}";
            return WebHost.CreateDefaultBuilder(args)
                .UseGoogleDiagnostics(AppSettings.GoogleCloudSettings.ProjectId,
                        AppSettings.GoogleCloudSettings.ServiceName,
                        AppSettings.GoogleCloudSettings.Version)
                .UseStartup<Startup>().UseUrls(urls);
        }

        /// <summary>
        /// Read application settings from appsettings.json. 
        /// </summary>
        private static void ReadAppSettings()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            // Read json config into AppSettings.
            AppSettings = new AppSettings();
            config.Bind(AppSettings);
        }
    }
}
